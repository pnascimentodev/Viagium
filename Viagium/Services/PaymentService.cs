using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Viagium.EntitiesDTO.ApiDTO;
using Viagium.EntitiesDTO.Payment;
using Viagium.Models;
using Viagium.Models.ENUM;
using Viagium.Repository.Interface;
using Viagium.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.Email;

namespace Viagium.Services;

public class PaymentService : IPaymentService
{
    private readonly string _asaasApiKey;
    private readonly string _asaasBaseUrl;
    private readonly HttpClient _httpClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _environment;

    public PaymentService(IHttpClientFactory httpClientFactory, IUnitOfWork unitOfWork, IConfiguration configuration, IEmailService emailService, IWebHostEnvironment environment)
    {
        _httpClient = httpClientFactory.CreateClient();
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _environment = environment;
        _asaasApiKey = configuration["Asaas:ApiKey"] ?? throw new InvalidOperationException("Asaas API Key não configurada");
        _asaasBaseUrl = configuration["Asaas:BaseUrl"] ?? throw new InvalidOperationException("Asaas Base URL não configurada");
    }

    public async Task<Payment> AddPaymentAsync(int reservationId, PaymentMethodType paymentMethod)
    {
        // Busca a reserva pelo ID
        var reservation = await _unitOfWork.ReservationRepository.GetByIdAsync(reservationId);
        if (reservation == null)
        {
            throw new Exception($"Reserva com ID {reservationId} não encontrada.");
        }

        // Busca o usuário da reserva
        var user = await _unitOfWork.UserRepository.GetByIdAsync(reservation.UserId);
        if (user == null)
        {
            throw new Exception($"Usuário da reserva {reservationId} não encontrado.");
        }

        if (string.IsNullOrEmpty(user.AsaasApiId))
        {
            throw new Exception($"Usuário {user.UserId} não possui AsaasApiId. É necessário criar o cliente na Asaas primeiro.");
        }

        // Monta o payload apenas com dados do cliente já existente e do pagamento
        DateTime dataPagamento = DateTime.Now;
        if (paymentMethod == PaymentMethodType.PIX)
        {
            dataPagamento = DateTime.Now.AddDays(1);
        }
        if (paymentMethod == PaymentMethodType.CREDIT_CARD)
        {
            dataPagamento = DateTime.Now.AddDays(1);
        }
        if (paymentMethod == PaymentMethodType.BOLETO)
        {
            dataPagamento = DateTime.Now.AddDays(30);
        }

        var newPayment = new
        {
            customer = user.AsaasApiId, // O cliente já deve existir na Asaas
            value = reservation.TotalPrice,
            billingType = paymentMethod.ToString(),
            dueDate = dataPagamento,
            observation = $"Pagamento referente à reserva {reservationId}"
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
            ReservationId = reservationId,
            PaymentMethod = paymentMethod,
            Amount = reservation.TotalPrice,
            PaymentIdAsaas = asaasPaymentId,
            Status = PaymentStatus.PENDING,
            PaidAt = null,
        };
        await _unitOfWork.PaymentRepository.AddAsync(payment);
        await _unitOfWork.SaveAsync();
        return payment;
    }

    public async Task<Payment> AddPaymentAsync(int reservationId, PaymentMethodType paymentMethod, CreditCardDTO? creditCard = null, string? remoteIp = null, AddressDTO? address = null)
    {
        // Busca a reserva pelo ID
        var reservation = await _unitOfWork.ReservationRepository.GetByIdAsync(reservationId);
        if (reservation == null)
        {
            throw new Exception($"Reserva com ID {reservationId} não encontrada.");
        }

        // Busca o usuário da reserva 
        // pode ser removido pois a reserva já deve ter o usuário carregado
        var user = await _unitOfWork.UserRepository.GetByIdAsync(reservation.UserId);
        if (user == null)
        {
            throw new Exception($"Usuário da reserva {reservationId} não encontrado.");
        }

        if (string.IsNullOrEmpty(user.AsaasApiId))
        {
            throw new Exception($"Usuário {user.UserId} não possui AsaasApiId. É necessário criar o cliente na Asaas primeiro.");
        }

        // Se um endereço foi fornecido e é um pagamento com cartão, salva/atualiza o endereço do usuário
        if (address != null && paymentMethod == PaymentMethodType.CREDIT_CARD)
        {
            await SaveOrUpdateUserAddressAsync(user, address);
        }

        // Define data de vencimento baseada no método de pagamento
        DateTime dataPagamento = DateTime.Now;
        if (paymentMethod == PaymentMethodType.PIX)
        {
            dataPagamento = DateTime.Now.AddDays(1);
        }
        if (paymentMethod == PaymentMethodType.CREDIT_CARD)
        {
            dataPagamento = DateTime.Now;
        }
        if (paymentMethod == PaymentMethodType.BOLETO)
        {
            dataPagamento = DateTime.Now.AddDays(30);
        }

        // Monta o payload base
        var paymentPayload = new
        {
            customer = user.AsaasApiId,
            value = reservation.TotalPrice,
            billingType = paymentMethod.ToString(),
            dueDate = dataPagamento,
            observation = $"Pagamento referente à reserva {reservationId}",
            // Campos condicionais para cartão de crédito
            creditCard = paymentMethod == PaymentMethodType.CREDIT_CARD && creditCard != null 
                ? new {
                    holderName = creditCard.HolderName,
                    number = creditCard.Number,
                    expiryMonth = creditCard.ExpiryMonth,
                    expiryYear = creditCard.ExpiryYear,
                    ccv = creditCard.Ccv
                } : null,
            creditCardHolderInfo = paymentMethod == PaymentMethodType.CREDIT_CARD && creditCard != null 
                ? await BuildCreditCardHolderInfoAsync(user, address) : null,
            remoteIp = !string.IsNullOrEmpty(remoteIp) ? remoteIp : null
        };

        var paymentJson = JsonSerializer.Serialize(paymentPayload, new JsonSerializerOptions 
        { 
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull 
        });
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
            // Para cartões, a API retorna 400 se a transação for negada
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && 
                paymentMethod == PaymentMethodType.CREDIT_CARD)
            {
                throw new Exception($"Transação com cartão negada: {responseBody}");
            }
            throw new Exception($"Erro ao criar pagamento: {responseBody}");
        }

        // Extrai informações da resposta da Asaas
        string? asaasPaymentId = null;
        PaymentStatus initialStatus = PaymentStatus.PENDING;
        DateTime? paidAt = null;

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("id", out var idProp))
            {
                asaasPaymentId = idProp.GetString();
            }

            // Para cartões, se chegou até aqui (HTTP 200), significa que foi autorizado
            if (paymentMethod == PaymentMethodType.CREDIT_CARD)
            {
                if (doc.RootElement.TryGetProperty("status", out var statusProp))
                {
                    var status = statusProp.GetString();
                    if (status == "CONFIRMED" || status == "RECEIVED")
                    {
                        initialStatus = PaymentStatus.RECEIVED;
                        paidAt = DateTime.Now;
                    }
                }
            }
        }
        catch (Exception)
        {
            throw new Exception($"Falha na comunicação com o serviço de pagamento: {responseBody}");
        }

        // Salva o pagamento localmente
        var payment = new Payment
        {
            ReservationId = reservationId,
            PaymentMethod = paymentMethod,
            Amount = reservation.TotalPrice,
            PaymentIdAsaas = asaasPaymentId,
            Status = initialStatus,
            PaidAt = paidAt,
        };

        await _unitOfWork.PaymentRepository.AddAsync(payment);
        await _unitOfWork.SaveAsync();

        // Atualiza a reserva com o ID do pagamento criado
        reservation.PaymentId = payment.PaymentId;
        await _unitOfWork.ReservationRepository.UpdateAsync(reservation);
        await _unitOfWork.SaveAsync();

        // Se o pagamento foi confirmado imediatamente, atualiza a reserva
        if (initialStatus == PaymentStatus.RECEIVED)
        {
            reservation.Status = "confirmed";
            await _unitOfWork.ReservationRepository.UpdateAsync(reservation);
            await _unitOfWork.SaveAsync();
            Console.WriteLine($"🎉 PAGAMENTO CARTÃO APROVADO! Reserva {reservationId} confirmada automaticamente!");
        }

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
    
    public async Task<object> GetPixQrCodeByCpfAsync(string documentNumber)
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

        // 2. Buscar cobranças PIX pendentes (ordenadas por data, mais recente primeiro)
        var paymentsRequest = new HttpRequestMessage(HttpMethod.Get, $"{_asaasBaseUrl}/payments?customer={customerId}&billingType=PIX&status=PENDING&sort=dueDate&order=desc");
        paymentsRequest.Headers.Add("access_token", _asaasApiKey);
        paymentsRequest.Headers.Add("User-Agent", "ViagiumApp/1.0");
        paymentsRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var paymentsResponse = await _httpClient.SendAsync(paymentsRequest);
        var paymentsContent = await paymentsResponse.Content.ReadAsStringAsync();
        if (!paymentsResponse.IsSuccessStatusCode)
            throw new Exception($"Erro ao buscar cobranças PIX: {paymentsContent}");
        var paymentsJson = JsonDocument.Parse(paymentsContent);
        var paymentsData = paymentsJson.RootElement.GetProperty("data");

        string paymentId;
        decimal paymentValue;

        // 3. Se não há cobrança PIX pendente, criar uma nova
        if (paymentsData.GetArrayLength() == 0)
        {
            
            // Buscar o usuário local pelo CPF para obter dados da reserva
            var localUser = await _unitOfWork.UserRepository.GetByDocumentNumberAsync(documentNumber);
            if (localUser == null)
                throw new Exception("Usuário não encontrado no sistema local. É necessário ter uma reserva ativa para gerar PIX.");

            // Buscar reservas pendentes do usuário
            var pendingReservations = await _unitOfWork.ReservationRepository.GetPendingReservationsByUserIdAsync(localUser.UserId);
            if (!pendingReservations.Any())
                throw new Exception("Nenhuma reserva pendente encontrada para este usuário.");

            // Pegar a reserva mais recente
            var latestReservation = pendingReservations.OrderByDescending(r => r.CreatedAt).First();

            // Criar nova cobrança PIX
            var newPixPayment = new
            {
                customer = customerId,
                value = latestReservation.TotalPrice,
                billingType = "PIX",
                dueDate = DateTime.Now.AddDays(1),
                observation = $"Pagamento PIX - Reserva {latestReservation.ReservationId}"
            };

            var pixPaymentJson = JsonSerializer.Serialize(newPixPayment);
            var content = new StringContent(pixPaymentJson, Encoding.UTF8, "application/json");

            var createPaymentRequest = new HttpRequestMessage(HttpMethod.Post, $"{_asaasBaseUrl}/payments")
            {
                Content = content
            };
            createPaymentRequest.Headers.Add("access_token", _asaasApiKey);
            createPaymentRequest.Headers.Add("User-Agent", "ViagiumApp/1.0");

            var createPaymentResponse = await _httpClient.SendAsync(createPaymentRequest);
            var createPaymentContent = await createPaymentResponse.Content.ReadAsStringAsync();
            
            if (!createPaymentResponse.IsSuccessStatusCode)
                throw new Exception($"Erro ao criar cobrança PIX: {createPaymentContent}");

            var createdPaymentJson = JsonDocument.Parse(createPaymentContent);
            paymentId = createdPaymentJson.RootElement.GetProperty("id").GetString()!;
            paymentValue = createdPaymentJson.RootElement.GetProperty("value").GetDecimal();

            // Salvar o pagamento localmente
            var localPayment = new Payment
            {
                ReservationId = latestReservation.ReservationId,
                PaymentMethod = PaymentMethodType.PIX,
                Amount = paymentValue,
                PaymentIdAsaas = paymentId,
                Status = PaymentStatus.PENDING,
                PaidAt = null,
            };
            await _unitOfWork.PaymentRepository.AddAsync(localPayment);
            await _unitOfWork.SaveAsync();

            
        }
        else
        {
            // Usar cobrança existente (mais recente)
            var existingPayment = paymentsData[0];
            paymentId = existingPayment.GetProperty("id").GetString()!;
            paymentValue = existingPayment.GetProperty("value").GetDecimal();
        }

        // 4. Buscar QR Code e código PIX copia e cola
        var qrCodeRequest = new HttpRequestMessage(HttpMethod.Get, $"{_asaasBaseUrl}/payments/{paymentId}/pixQrCode");
        qrCodeRequest.Headers.Add("access_token", _asaasApiKey);
        qrCodeRequest.Headers.Add("User-Agent", "ViagiumApp/1.0");
        qrCodeRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var qrCodeResponse = await _httpClient.SendAsync(qrCodeRequest);
        var qrCodeContent = await qrCodeResponse.Content.ReadAsStringAsync();
        
        if (!qrCodeResponse.IsSuccessStatusCode)
            throw new Exception($"Erro ao buscar QR Code do PIX: {qrCodeContent}");
        
        JsonDocument qrCodeJson;
        try
        {
            qrCodeJson = JsonDocument.Parse(qrCodeContent);
        }
        catch (JsonException ex)
        {
            throw new Exception($"Erro ao processar resposta da API PIX. Resposta recebida: {qrCodeContent}. Erro: {ex.Message}");
        }
        
        // A API do Asaas retorna 'encodedImage' para o QR Code e 'payload' para o código copia e cola  
        if (!qrCodeJson.RootElement.TryGetProperty("encodedImage", out var qrCodeProp))
        {
            throw new Exception("QR Code não encontrado na resposta da API.");
        }
        
        if (!qrCodeJson.RootElement.TryGetProperty("payload", out var payloadProp))
        {
            throw new Exception("Código PIX copia e cola não encontrado na resposta da API.");
        }

        var qrCodeBase64 = qrCodeProp.GetString();
        var pixCopiaCola = payloadProp.GetString();
        
        if (string.IsNullOrEmpty(qrCodeBase64) || string.IsNullOrEmpty(pixCopiaCola))
            throw new Exception("Dados do PIX estão incompletos. QR Code ou código copia e cola vazios.");

        // Retornar dados completos do PIX
        return new
        {
            paymentId = paymentId,
            valor = paymentValue,
            vencimento = DateTime.Now.AddDays(1),
            qrCode = $"data:image/png;base64,{qrCodeBase64}",
            pixCopiaCola = pixCopiaCola,
            mensagem = "PIX gerado com sucesso! Use o QR Code ou copie e cole o código.",
            instrucoes = new[]
            {
                "1. Abra o aplicativo do seu banco",
                "2. Escaneie o QR Code ou copie o código PIX",
                "3. Confirme os dados e efetue o pagamento",
                "4. O pagamento será processado automaticamente"
            }
        };
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

        // 2. Buscar cobranças BOLETO pendentes (ordenadas por data, mais recente primeiro)
        var paymentsRequest = new HttpRequestMessage(HttpMethod.Get, $"{_asaasBaseUrl}/payments?customer={customerId}&billingType=BOLETO&status=PENDING&sort=dueDate&order=desc");
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
        
        // 3. Pega o boleto mais recente (primeiro da lista ordenada)
        var mostRecentPayment = paymentsData[0];
        var paymentId = mostRecentPayment.GetProperty("id").GetString();

        // 4. Usa a função otimizada para buscar a URL do boleto
        var boletoUrl = await GetBoletoUrlByPaymentIdAsync(paymentId!);
        if (string.IsNullOrEmpty(boletoUrl))
            throw new Exception("Não foi possível obter o link do boleto para download.");
        
        return boletoUrl;
    }

    public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
    {
        return await _unitOfWork.PaymentRepository.GetPaymentByIdAsync(paymentId);
    }

    public async Task SynchronizePaymentsAsync()
    {
        try
        {
            Console.WriteLine("🔄 Iniciando sincronização de pagamentos...");
            
            // ✅ CORREÇÃO: Usar access_token ao invés de Authorization Bearer
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_asaasBaseUrl}/payments?limit=100&offset=0");
            request.Headers.Add("access_token", _asaasApiKey);
            request.Headers.Add("User-Agent", "ViagiumApp/1.0");
            
            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Erro HTTP {response.StatusCode}: {errorBody}");
                throw new Exception($"Erro ao consultar API Asaas: {response.StatusCode} - {errorBody}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var root = JsonDocument.Parse(json).RootElement;
            
            if (!root.TryGetProperty("data", out var pagamentos) || pagamentos.GetArrayLength() == 0)
            {
                Console.WriteLine("ℹ️ Nenhum pagamento encontrado na API Asaas para sincronizar.");
                // Mesmo sem pagamentos da Asaas, verifica reservas que devem ser finalizadas
                await CheckAndFinishExpiredReservationsAsync();
                return;
            }

            int pagamentosProcessados = 0;
            int pagamentosAtualizados = 0;

            foreach (var pagamento in pagamentos.EnumerateArray())
            {
                try
                {
                    var id = pagamento.GetProperty("id").GetString();
                    var statusString = pagamento.GetProperty("status").GetString();
                    var valor = pagamento.GetProperty("value").GetDecimal();
                    
                    // Busca pagamento local pelo ID da Asaas
                    var pagamentoLocal = await _unitOfWork.PaymentRepository.GetByAsaasIdAsync(id!);
                    if (pagamentoLocal == null)
                    {
                        Console.WriteLine($"⚠️ Pagamento {id} não encontrado localmente - ignorando.");
                        continue;
                    }

                    pagamentosProcessados++;

                    // Converte string para enum
                    if (Enum.TryParse<PaymentStatus>(statusString, true, out var novoStatus))
                    {
                        var statusAnterior = pagamentoLocal.Status;
                        
                        // Só atualiza se o status realmente mudou
                        if (statusAnterior != novoStatus)
                        {
                            Console.WriteLine($"📊 Pagamento {id}: {statusAnterior} → {novoStatus}");
                            
                            pagamentoLocal.Status = novoStatus;
                            pagamentoLocal.Amount = valor;

                            // Atualiza data de pagamento apenas quando confirmado pela primeira vez
                            if (novoStatus == PaymentStatus.RECEIVED && statusAnterior != PaymentStatus.RECEIVED)
                            {
                                pagamentoLocal.PaidAt = DateTime.Now;
                                Console.WriteLine($"💰 Pagamento {id} confirmado às {DateTime.Now:dd/MM/yyyy HH:mm}");
                            }

                            // Atualiza no banco
                            await _unitOfWork.PaymentRepository.FinalizePaymentAsync(pagamentoLocal);

                            // Busca e atualiza a reserva
                            var reserva = await _unitOfWork.ReservationRepository.GetByIdAsync(pagamentoLocal.ReservationId);
                            if (reserva != null)
                            {
                                await UpdateReservationStatusAsync(reserva, novoStatus, statusAnterior);
                            }
                            else
                            {
                                Console.WriteLine($"⚠️ Reserva {pagamentoLocal.ReservationId} não encontrada para o pagamento {id}");
                            }

                            pagamentosAtualizados++;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"❌ Status desconhecido recebido da Asaas: '{statusString}' para pagamento {id}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao processar pagamento individual: {ex.Message}");
                    // Continue processando outros pagamentos mesmo se um falhar
                }
            }

            // Verifica reservas que devem ser finalizadas independente de mudanças no pagamento
            await CheckAndFinishExpiredReservationsAsync();

            // Salva todas as alterações de uma vez
            await _unitOfWork.SaveAsync();
            
            Console.WriteLine($"✅ Sincronização concluída!");
            Console.WriteLine($"📈 Estatísticas:");
            Console.WriteLine($"   - Pagamentos processados: {pagamentosProcessados}");
            Console.WriteLine($"   - Pagamentos atualizados: {pagamentosAtualizados}");
            Console.WriteLine($"   - Última sincronização: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"🌐 Erro de conectividade com a API Asaas: {httpEx.Message}");
            throw new Exception("Falha na comunicação com o serviço de pagamentos. Tente novamente mais tarde.");
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine($"📄 Erro ao processar resposta da API Asaas: {jsonEx.Message}");
            throw new Exception("Formato de resposta inválido da API de pagamentos.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Erro geral ao sincronizar pagamentos: {ex.Message}");
            throw new Exception($"Erro interno na sincronização: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica e finaliza reservas confirmadas que já passaram da data de término
    /// </summary>
    private async Task CheckAndFinishExpiredReservationsAsync()
    {
        try
        {
            Console.WriteLine("🔍 Verificando reservas que devem ser finalizadas...");
            
            // Busca todas as reservas confirmadas que já passaram da data de fim
            var allReservations = await _unitOfWork.ReservationRepository.GetAllAsync();
            var expiredReservations = allReservations
                .Where(r => r.Status == "confirmed" && r.EndDate < DateTime.Now)
                .ToList();

            if (expiredReservations.Any())
            {
                Console.WriteLine($"📋 Encontradas {expiredReservations.Count} reservas para finalizar.");
                
                foreach (var reservation in expiredReservations)
                {
                    try
                    {
                        var statusAnterior = reservation.Status;
                        reservation.Status = "finished";
                        await _unitOfWork.ReservationRepository.UpdateAsync(reservation);
                        
                        Console.WriteLine($"🏁 RESERVA FINALIZADA! ID: {reservation.ReservationId} | Viagem concluída em {reservation.EndDate:dd/MM/yyyy}");
                        
                        // ✅ ENVIAR EMAIL DE SOLICITAÇÃO DE REVIEW
                        await SendReviewRequestEmailAsync(reservation);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Erro ao finalizar reserva {reservation.ReservationId}: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("✅ Nenhuma reserva precisa ser finalizada no momento.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao verificar reservas para finalização: {ex.Message}");
        }
    }

    private async Task UpdateReservationStatusAsync(Reservation reserva, PaymentStatus statusPagamento, PaymentStatus statusAnterior)
    {
        string statusAnteriorReserva = reserva.Status;
        string novoStatusReserva = statusPagamento switch
        {
            PaymentStatus.PENDING => "pending",
            PaymentStatus.RECEIVED => "confirmed",
            PaymentStatus.OVERDUE => "cancelled", 
            PaymentStatus.CANCELED => "cancelled",
            _ => reserva.Status // Mantém status atual se não reconhecido
        };

        // Verifica se a reserva deve ser marcada como finalizada
        // Se a data de fim já passou e o status é "confirmed", marca como "finished"
        if (reserva.EndDate < DateTime.Now && novoStatusReserva == "confirmed")
        {
            novoStatusReserva = "finished";
        }

        // Só atualiza se o status da reserva realmente mudou
        if (statusAnteriorReserva != novoStatusReserva)
        {
            reserva.Status = novoStatusReserva;
            await _unitOfWork.ReservationRepository.UpdateAsync(reserva);
            
            // Logs detalhados para auditoria
            Console.WriteLine($"🏨 Reserva {reserva.ReservationId}: '{statusAnteriorReserva}' → '{novoStatusReserva}'");
            
            // Log especial para confirmações
            if (novoStatusReserva == "confirmed")
            {
                Console.WriteLine($"🎉 RESERVA CONFIRMADA! ID: {reserva.ReservationId} | Valor: R$ {reserva.TotalPrice:F2}");
            }
            
            // Log para finalizações
            if (novoStatusReserva == "finished")
            {
                Console.WriteLine($"🏁 RESERVA FINALIZADA! ID: {reserva.ReservationId} | Viagem concluída em {reserva.EndDate:dd/MM/yyyy}");
                
                // ✅ ENVIAR EMAIL DE SOLICITAÇÃO DE REVIEW
                await SendReviewRequestEmailAsync(reserva);
            }
            
            // Log para problemas
            if (novoStatusReserva == "cancelled")
            {
                Console.WriteLine($"⚠️ ATENÇÃO: Reserva {reserva.ReservationId} foi cancelada - verificar necessidade de ação.");
            }
        }
    }

    /// <summary>
    /// Envia email de solicitação de review para o usuário quando a reserva é finalizada
    /// </summary>
    private async Task SendReviewRequestEmailAsync(Reservation reservation)
    {
        try
        {
            // Buscar dados do usuário se não estiverem carregados
            if (reservation.User == null)
            {
                reservation.User = await _unitOfWork.UserRepository.GetByIdAsync(reservation.UserId);
            }

            if (reservation.User == null)
            {
                Console.WriteLine($"⚠️ Usuário não encontrado para envio de email de review. ReservationId: {reservation.ReservationId}");
                return;
            }

            var userName = $"{reservation.User.FirstName} {reservation.User.LastName}";
            var userEmail = reservation.User.Email;

            // Carregar template de email de review
            var templatePath = Path.Combine(_environment.ContentRootPath, "EmailTemplates", "User", "ReviewSolicited.html");
            
            if (!File.Exists(templatePath))
            {
                Console.WriteLine($"⚠️ Template de email não encontrado: {templatePath}");
                return;
            }

            var template = await File.ReadAllTextAsync(templatePath);
            
            // ✅ SUBSTITUIR PLACEHOLDERS NO TEMPLATE
            var emailBody = template
                .Replace("{NOME}", reservation.User.FirstName)
                .Replace("{RESERVATION_ID}", reservation.ReservationId.ToString());
            
            var emailDto = new SendEmailDTO
            {
                To = userEmail,
                Subject = "✨ Conte-nos sobre sua experiência - Viagium",
                HtmlBody = emailBody
            };

            // Enviar email usando o serviço existente
            await _emailService.SendEmailAsync(emailDto);

            Console.WriteLine($"📧 Email de solicitação de review enviado para: {userEmail} (Reserva: {reservation.ReservationId})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao enviar email de review para reserva {reservation.ReservationId}: {ex.Message}");
            // Não relança a exceção para não interromper o fluxo de sincronização
        }
    }

    private async Task<object> BuildCreditCardHolderInfoAsync(User user, AddressDTO? address)
    {
        // Usa endereço fornecido ou busca endereço salvo do usuário
        var addressInfo = address;
        if (addressInfo == null && user.AddressId.HasValue)
        {
            // Busca endereço salvo do usuário
            var userAddress = await _unitOfWork.AddressRepository.GetByIdAsync(user.AddressId.Value);
            if (userAddress != null)
            {
                addressInfo = new AddressDTO
                {
                    ZipCode = userAddress.ZipCode,
                    AddressNumber = userAddress.AddressNumber,
                    StreetName = userAddress.StreetName,
                    Neighborhood = userAddress.Neighborhood,
                    City = userAddress.City,
                    State = userAddress.State,
                    Country = userAddress.Country
                };
            }
        }
        
        return new
        {
            name = $"{user.FirstName} {user.LastName}",
            email = user.Email,
            cpfCnpj = user.DocumentNumber,
            postalCode = addressInfo?.ZipCode ?? "00000-000",
            addressNumber = addressInfo?.AddressNumber.ToString() ?? "0",
            addressComplement = (string?)null, // AddressDTO não tem Complement
            phone = user.Phone ?? "",
            mobilePhone = user.Phone ?? ""
        };
    }

    private async Task SaveOrUpdateUserAddressAsync(User user, AddressDTO addressDto)
    {
        try
        {
            Address? userAddress = null;
            
            // Se o usuário já tem um endereço, atualiza
            if (user.AddressId.HasValue)
            {
                userAddress = await _unitOfWork.AddressRepository.GetByIdAsync(user.AddressId.Value);
            }
            
            if (userAddress != null)
            {
                // Atualiza endereço existente
                userAddress.StreetName = addressDto.StreetName;
                userAddress.AddressNumber = addressDto.AddressNumber;
                userAddress.Neighborhood = addressDto.Neighborhood;
                userAddress.City = addressDto.City;
                userAddress.State = addressDto.State;
                userAddress.ZipCode = addressDto.ZipCode;
                userAddress.Country = addressDto.Country;
                
                await _unitOfWork.AddressRepository.UpdateAsync(userAddress);
                Console.WriteLine($"📍 Endereço do usuário {user.UserId} atualizado.");
            }
            else
            {
                // Cria novo endereço
                userAddress = new Address
                {
                    StreetName = addressDto.StreetName,
                    AddressNumber = addressDto.AddressNumber,
                    Neighborhood = addressDto.Neighborhood,
                    City = addressDto.City,
                    State = addressDto.State,
                    ZipCode = addressDto.ZipCode,
                    Country = addressDto.Country,
                    UserId = user.UserId,
                    CreatedAt = DateTime.Now
                };
                
                await _unitOfWork.AddressRepository.AddAsync(userAddress);
                await _unitOfWork.SaveAsync(); // Salva para obter o ID
                
                // Associa o endereço ao usuário
                user.AddressId = userAddress.AdressId;
                await _unitOfWork.UserRepository.UpdateAsync(user);
                
                Console.WriteLine($"📍 Novo endereço criado e associado ao usuário {user.UserId}.");
            }
            
            await _unitOfWork.SaveAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao salvar/atualizar endereço do usuário {user.UserId}: {ex.Message}");
            throw;
        }
    }

    public async Task<string?> GetBoletoUrlByPaymentIdAsync(string paymentIdAsaas)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_asaasBaseUrl}/payments/{paymentIdAsaas}");
            request.Headers.Add("access_token", _asaasApiKey);
            request.Headers.Add("User-Agent", "ViagiumApp/1.0");
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            
            if (json.RootElement.TryGetProperty("bankSlipUrl", out var boletoUrlProp))
            {
                return boletoUrlProp.GetString();
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Erro ao buscar URL do boleto: {ex.Message}");
            return null;
        }
    }
}
