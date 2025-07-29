using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Viagium.EntitiesDTO.ApiDTO;
using Viagium.Models;
using Viagium.Models.ENUM;
using Viagium.Repository.Interface;
using Viagium.Services.Interfaces;

namespace Viagium.Services;

public class PaymentService : IPaymentService
{
    private readonly string _asaasApiKey;
    private readonly string _asaasBaseUrl;
    private readonly HttpClient _httpClient;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(IHttpClientFactory httpClientFactory, IUnitOfWork unitOfWork)
    {
        _httpClient = httpClientFactory.CreateClient();
        _unitOfWork = unitOfWork;
        _asaasApiKey = Environment.GetEnvironmentVariable("ASAAS_API_KEY") ?? string.Empty;
        _asaasBaseUrl = Environment.GetEnvironmentVariable("ASAAS_BASE_URL") ?? string.Empty;
    }

    public async Task<Payment> AddAsync(Reservation reservation)
    {
        // Monta o payload apenas com dados do cliente já existente e do pagamento
        DateTime dataPagamento = DateTime.Now;
        if (reservation.Payment?.PaymentMethod == PaymentMethodType.PIX)
        {
            dataPagamento = DateTime.Now.AddDays(1);
        }
        if (reservation.Payment?.PaymentMethod == PaymentMethodType.CREDIT_CARD)
        {
            // Para cartão de crédito, o pagamento é considerado imediato
            dataPagamento = DateTime.Now;
        }
        if (reservation.Payment?.PaymentMethod == PaymentMethodType.BOLETO)
        {
            dataPagamento = DateTime.Now.AddDays(30);
        }

        var newPayment = new
        {
            customer = reservation.User?.AsaasApiId, // O cliente já deve existir na Asaas
            value = reservation.TotalPrice,
            billingType = reservation.Payment?.PaymentMethod.ToString(),
            dueDate = dataPagamento,
            observation = "Pagamento referente à reserva{}",
            reservation.ReservationId
        };

        var paymentJson = JsonSerializer.Serialize(newPayment);
        var content = new StringContent(paymentJson, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_asaasBaseUrl}/payments")
        {
            Content = content
        };

        httpRequest.Headers.Add("access_token", _asaasApiKey);
        httpRequest.Headers.Add("User-Agent", "ViagiumApp/1.0");

        var response = await _httpClient.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Erro ao criar pagamento: {responseBody}");
        }

        // Extrai o id do pagamento do Asaas
        string? asaasPaymentId = null;
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("id", out var idProp))
            {
                asaasPaymentId = idProp.GetString();
            }
        }
        catch (Exception)
        {
            throw new Exception($"Falha na comunicação com o serviço de pagamento: {responseBody}");
        }

        // Salva o pagamento localmente após sucesso na API
        var payment = new Payment
        {
            ReservationId = reservation.ReservationId,
            PaymentMethod = reservation.Payment!.PaymentMethod,
            Amount = reservation.TotalPrice,
            CardLastFourDigits = reservation.Payment?.CardLastFourDigits,
            PaymentIdAsaas = asaasPaymentId,
            Status = "Pending",
            PaidAt = null,
        };
        await _unitOfWork.PaymentRepository.AddAsync(payment);
        await _unitOfWork.SaveAsync();
        return payment;
    }

    public async Task<string> CreateUserAsync(AsaasUserDTO user)
    {
        var novoCliente = new
        {
            name = user.FirstName + user.LastName,
            email = user.Email,
            mobilePhone = user.Phone,
            cpfCnpj = user.DocumentNumber
        };

        var jsonPayload = JsonSerializer.Serialize(novoCliente);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_asaasBaseUrl}/customers")
        {
            Content = content
        };

        request.Headers.Add("access_token", _asaasApiKey);
        request.Headers.Add("User-Agent", "ViagiumApp/1.0");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Erro ao criar cliente no Asaas: {responseContent}");

        var doc = JsonDocument.Parse(responseContent);
        return doc.RootElement.GetProperty("id").GetString()!;
    }

    public async Task SincronizarPagamentos()
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _asaasApiKey);
            var response = await _httpClient.GetAsync($"{_asaasBaseUrl}/payments");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement;
            if (!root.TryGetProperty("data", out var pagamentos) || pagamentos.GetArrayLength() == 0)
            {
                Console.WriteLine("Nenhum pagamento para sincronizar.");
                return;
            }

            foreach (var pagamento in pagamentos.EnumerateArray())
            {
                var id = pagamento.GetProperty("id").GetString();
                var status = pagamento.GetProperty("status").GetString();
                var valor = pagamento.GetProperty("value").GetDecimal();

                var pagamentoLocal = await _unitOfWork.PaymentRepository.GetByAsaasIdAsync(id!);
                if (pagamentoLocal != null)
                {
                    pagamentoLocal.Status = status!;
                    pagamentoLocal.Amount = valor;
                    await _unitOfWork.PaymentRepository.FinalizePaymentAsync(pagamentoLocal);
                }
            }
            await _unitOfWork.SaveAsync();
            Console.WriteLine("Pagamentos sincronizados com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao sincronizar pagamentos: {ex.Message}");
        }
    }
}