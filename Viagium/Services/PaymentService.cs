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

    public async Task<Payment> AddPaymentAsync(AsaasPaymentDTO asaasPaymentDTO)
    {
        // Monta o payload apenas com dados do cliente já existente e do pagamento
        DateTime dataPagamento = DateTime.Now;
        if (asaasPaymentDTO.PaymentMethod == PaymentMethodType.PIX)
        {
            dataPagamento = DateTime.Now.AddDays(1);
        }
        if (asaasPaymentDTO.PaymentMethod == PaymentMethodType.CREDIT_CARD || asaasPaymentDTO.PaymentMethod == PaymentMethodType.DEBIT_CARD)
        {
            dataPagamento = DateTime.Now.AddDays(1);
        }
        if (asaasPaymentDTO.PaymentMethod == PaymentMethodType.BOLETO)
        {
            dataPagamento = DateTime.Now.AddDays(30);
        }

        var newPayment = new
        {
            customer = asaasPaymentDTO.AsaasApiId, // O cliente já deve existir na Asaas
            value = asaasPaymentDTO.TotalPrice,
            billingType = asaasPaymentDTO.PaymentMethod.ToString(),
            dueDate = dataPagamento,
            observation = $"Pagamento referente à reserva{asaasPaymentDTO.ReservationId}",
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
            ReservationId = asaasPaymentDTO.ReservationId,
            PaymentMethod = asaasPaymentDTO.PaymentMethod,
            Amount = asaasPaymentDTO.TotalPrice,
            CardLastFourDigits = asaasPaymentDTO.CardLastFourDigits,
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
            name = $"{user.FirstName} {user.LastName}",
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
            throw new Exception($"Erro {response.StatusCode} ao criar cliente no Asaas: {responseContent}");

        var doc = JsonDocument.Parse(responseContent);
        return doc.RootElement.GetProperty("id").GetString()!;
    }
    
    public async Task<string> GetPixQrCodeByCpfAsync(string documentNumber)
    {
        // 1. Buscar o cliente pelo CPF na API do Asaas
        var customerRequest = new HttpRequestMessage(HttpMethod.Get, $"{_asaasBaseUrl}/customers?cpfCnpj={documentNumber}");
        customerRequest.Headers.Add("access_token", _asaasApiKey);
        customerRequest.Headers.Add("User-Agent", "ViagiumApp/1.0");
        customerRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var customerResponse = await _httpClient.SendAsync(customerRequest);
        var customerContent = await customerResponse.Content.ReadAsStringAsync();
        if (!customerResponse.IsSuccessStatusCode)
            throw new Exception($"Erro ao buscar cliente: {customerContent}");
        var customerJson = JsonDocument.Parse(customerContent);
        var data = customerJson.RootElement.GetProperty("data");
        if (data.GetArrayLength() == 0)
            throw new Exception("Cliente não encontrado para o CPF informado.");
        var customerId = data[0].GetProperty("id").GetString();

        // 2. Buscar cobranças PIX pendentes
        var paymentsRequest = new HttpRequestMessage(HttpMethod.Get, $"{_asaasBaseUrl}/payments?customer={customerId}&billingType=PIX&status=PENDING");
        paymentsRequest.Headers.Add("access_token", _asaasApiKey);
        paymentsRequest.Headers.Add("User-Agent", "ViagiumApp/1.0");
        paymentsRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var paymentsResponse = await _httpClient.SendAsync(paymentsRequest);
        var paymentsContent = await paymentsResponse.Content.ReadAsStringAsync();
        if (!paymentsResponse.IsSuccessStatusCode)
            throw new Exception($"Erro ao buscar cobranças PIX: {paymentsContent}");
        var paymentsJson = JsonDocument.Parse(paymentsContent);
        var paymentsData = paymentsJson.RootElement.GetProperty("data");
        if (paymentsData.GetArrayLength() == 0)
            throw new Exception("Nenhuma cobrança PIX pendente encontrada para este cliente.");
        var paymentId = paymentsData[0].GetProperty("id").GetString();

        // 3. Buscar QR Code do pagamento PIX
        var qrCodeRequest = new HttpRequestMessage(HttpMethod.Get, $"{_asaasBaseUrl}/payments/{paymentId}/pixQrCode");
        qrCodeRequest.Headers.Add("access_token", _asaasApiKey);
        qrCodeRequest.Headers.Add("User-Agent", "ViagiumApp/1.0");
        qrCodeRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var qrCodeResponse = await _httpClient.SendAsync(qrCodeRequest);
        var qrCodeContent = await qrCodeResponse.Content.ReadAsStringAsync();
        if (!qrCodeResponse.IsSuccessStatusCode)
            throw new Exception($"Erro ao buscar QR Code do PIX: {qrCodeContent}");
        var qrCodeJson = JsonDocument.Parse(qrCodeContent);
        if (!qrCodeJson.RootElement.TryGetProperty("qrCode", out var qrCodeProp))
            throw new Exception("A resposta da API não contém o QR Code do PIX. Verifique se a cobrança realmente possui um QR Code gerado.");
        var qrCode = qrCodeProp.GetString();
        if (string.IsNullOrEmpty(qrCode))
            throw new Exception("O QR Code retornado está vazio. Verifique se a cobrança está correta e ativa.");
        return qrCode;
    }

    public async Task<string> GetBankSlipByDocumentNumber(string documentNumber)
    {
        // 1. Buscar o cliente pelo CPF/CNPJ na API do Asaas
        var customerRequest = new HttpRequestMessage(HttpMethod.Get, $"{_asaasBaseUrl}/customers?cpfCnpj={documentNumber}");
        customerRequest.Headers.Add("access_token", _asaasApiKey);
        customerRequest.Headers.Add("User-Agent", "ViagiumApp/1.0");
        customerRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var customerResponse = await _httpClient.SendAsync(customerRequest);
        var customerContent = await customerResponse.Content.ReadAsStringAsync();
        if (!customerResponse.IsSuccessStatusCode)
            throw new Exception($"Erro ao buscar cliente: {customerContent}");
        var customerJson = JsonDocument.Parse(customerContent);
        var data = customerJson.RootElement.GetProperty("data");
        if (data.GetArrayLength() == 0)
            throw new Exception("Cliente não encontrado para o documento informado.");
        var customerId = data[0].GetProperty("id").GetString();

        // 2. Buscar cobranças BOLETO pendentes
        var paymentsRequest = new HttpRequestMessage(HttpMethod.Get, $"{_asaasBaseUrl}/payments?customer={customerId}&billingType=BOLETO&status=PENDING");
        paymentsRequest.Headers.Add("access_token", _asaasApiKey);
        paymentsRequest.Headers.Add("User-Agent", "ViagiumApp/1.0");
        paymentsRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var paymentsResponse = await _httpClient.SendAsync(paymentsRequest);
        var paymentsContent = await paymentsResponse.Content.ReadAsStringAsync();
        if (!paymentsResponse.IsSuccessStatusCode)
            throw new Exception($"Erro ao buscar boletos: {paymentsContent}");
        var paymentsJson = JsonDocument.Parse(paymentsContent);
        var paymentsData = paymentsJson.RootElement.GetProperty("data");
        if (paymentsData.GetArrayLength() == 0)
            throw new Exception("Nenhum boleto pendente encontrado para este cliente.");

        // 3. Buscar o link do boleto para download
        var boletoUrl = paymentsData[0].GetProperty("bankSlipUrl").GetString();
        if (string.IsNullOrEmpty(boletoUrl))
            throw new Exception("Não foi possível obter o link do boleto para download.");
        
        return boletoUrl;

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

                    // Se o pagamento foi confirmado, atualizar a reserva para Confirmado
                    if (status == "RECEIVED")
                    {
                        var reserva = await _unitOfWork.ReservationRepository.GetByIdAsync(pagamentoLocal.ReservationId);
                        if (reserva != null)
                        {
                            reserva.Status = "Confirmado";
                            await _unitOfWork.ReservationRepository.UpdateAsync(reserva);
                        }
                    }
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