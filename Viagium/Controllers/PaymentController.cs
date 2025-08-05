using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.ApiDTO;
using Viagium.EntitiesDTO.Payment;
using Viagium.EntitiesDTO.User;
using Viagium.Models;
using Viagium.Models.ENUM;
using Viagium.Services;

namespace Viagium.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{

    private readonly PaymentService _paymentService;
    private readonly IMapper _mapper;

    public PaymentController(PaymentService paymentService, IMapper mapper)
    {
        _paymentService = paymentService;
        _mapper = mapper;
    }
    


    /// <summary>
    /// Cria um novo pagamento.
    /// </summary>
    /// <remarks>Exemplo: POST /api/payment/CreatePayment</remarks>
    [HttpPost("CreatePayment")] //envia a informação de pagamento para a api do Asaas
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> CreatePayment(
        [FromForm] int reservationId, 
        [FromForm] PaymentMethodType paymentMethod,
        [FromForm] string? holderName = null,
        [FromForm] string? cardNumber = null,
        [FromForm] string? expiryMonth = null,
        [FromForm] string? expiryYear = null,
        [FromForm] string? ccv = null,
        [FromForm] string? remoteIp = null,
        // Campos opcionais de endereço para pagamentos com cartão
        [FromForm] string? streetName = null,
        [FromForm] int? addressNumber = null,
        [FromForm] string? neighborhood = null,
        [FromForm] string? city = null,
        [FromForm] string? state = null,
        [FromForm] string? zipCode = null,
        [FromForm] string? country = null)
    {
        try
        {
            CreditCardDTO? creditCard = null;
            AddressDTO? address = null;

            // Se for cartão de crédito, valida e monta os dados do cartão
            if (paymentMethod == PaymentMethodType.CREDIT_CARD)
            {
                if (string.IsNullOrEmpty(holderName) || string.IsNullOrEmpty(cardNumber) || 
                    string.IsNullOrEmpty(expiryMonth) || string.IsNullOrEmpty(expiryYear) || 
                    string.IsNullOrEmpty(ccv))
                {
                    return BadRequest(new
                    {
                        erro = "Para pagamentos com cartão de crédito, todos os dados do cartão são obrigatórios",
                        camposObrigatorios = new[] { "holderName", "cardNumber", "expiryMonth", "expiryYear", "ccv" }
                    });
                }

                creditCard = new CreditCardDTO
                {
                    HolderName = holderName,
                    Number = cardNumber,
                    ExpiryMonth = expiryMonth,
                    ExpiryYear = expiryYear,
                    Ccv = ccv
                };

                // Monta dados do endereço se fornecidos (opcional para cartão)
                if (!string.IsNullOrEmpty(streetName) && addressNumber.HasValue && 
                    !string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(zipCode))
                {
                    address = new AddressDTO
                    {
                        StreetName = streetName,
                        AddressNumber = addressNumber.Value,
                        Neighborhood = neighborhood ?? "",
                        City = city,
                        State = state ?? "",
                        ZipCode = zipCode,
                        Country = country ?? "Brasil"
                    };
                }

                // Valida os dados do cartão
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
            }

            var payment = await _paymentService.AddPaymentAsync(reservationId, paymentMethod, creditCard, remoteIp, address);

            var response = new
            {
                mensagem = "Pagamento criado com sucesso!",
                pagamentoId = payment.PaymentId,
                status = payment.Status.ToString(),
                statusDescricao = GetStatusDescription(payment.Status),
                metodo = payment.PaymentMethod.ToString(),
                valor = payment.Amount,
                processadoImediatamente = paymentMethod == PaymentMethodType.CREDIT_CARD,
                enderecoSalvo = address != null ? "Endereço salvo para o usuário" : "Nenhum endereço fornecido"
            };

            // Se for boleto, busca o link de download e adiciona na resposta
            if (paymentMethod == PaymentMethodType.BOLETO && !string.IsNullOrEmpty(payment.PaymentIdAsaas))
            {
                try
                {
                    var boletoUrl = await _paymentService.GetBoletoUrlByPaymentIdAsync(payment.PaymentIdAsaas);
                    
                    if (!string.IsNullOrEmpty(boletoUrl))
                    {
                        return Ok(new
                        {
                            mensagem = "Pagamento criado com sucesso!",
                            pagamentoId = payment.PaymentId,
                            status = payment.Status.ToString(),
                            statusDescricao = GetStatusDescription(payment.Status),
                            metodo = payment.PaymentMethod.ToString(),
                            valor = payment.Amount,
                            processadoImediatamente = false,
                            enderecoSalvo = address != null ? "Endereço salvo para o usuário" : "Nenhum endereço fornecido",
                            boletoUrl = boletoUrl,
                            mensagemBoleto = "Link do boleto gerado com sucesso! Clique no link para fazer o download."
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Se houver erro ao buscar o boleto, apenas loga mas não falha a operação
                    Console.WriteLine($"⚠️ Erro ao buscar link do boleto: {ex.Message}");
                }
            }

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            // Erros de validação de argumentos
            return BadRequest(new
            {
                erro = "Dados inválidos",
                detalhes = ex.Message,
                tipoErro = "VALIDATION_ERROR"
            });
        }
        catch (InvalidOperationException ex)
        {
            // Erros de operação inválida (ex: usuário não encontrado, etc.)
            return BadRequest(new
            {
                erro = "Operação inválida",
                detalhes = ex.Message,
                tipoErro = "BUSINESS_RULE_ERROR"
            });
        }
        catch (HttpRequestException ex)
        {
            // Erros de comunicação com a API externa
            return StatusCode(502, new
            {
                erro = "Erro na comunicação com o serviço de pagamento",
                detalhes = "Serviço temporariamente indisponível. Tente novamente em alguns minutos.",
                tipoErro = "EXTERNAL_API_ERROR",
                detalhesInternos = ex.Message
            });
        }
        catch (Exception ex) when (ex.Message.Contains("Reserva com ID") && ex.Message.Contains("não encontrada"))
        {
            // Reserva não encontrada
            return NotFound(new
            {
                erro = "Reserva não encontrada",
                detalhes = "A reserva especificada não foi encontrada no sistema. Verifique se o ID da reserva está correto.",
                mensagemFront = "Ops! Não conseguimos encontrar sua reserva. Por favor, verifique os dados e tente novamente.",
                tipoErro = "NOT_FOUND_ERROR",
                recurso = "Reserva"
            });
        }
        catch (Exception ex) when (ex.Message.Contains("Usuário") && ex.Message.Contains("não encontrado"))
        {
            // Usuário não encontrado
            return NotFound(new
            {
                erro = "Usuário não encontrado",
                detalhes = "O usuário associado à reserva não foi encontrado no sistema.",
                mensagemFront = "Ops! Não conseguimos encontrar os dados do usuário. Entre em contato com o suporte.",
                tipoErro = "NOT_FOUND_ERROR",
                recurso = "Usuário"
            });
        }
        catch (Exception ex) when (ex.Message.Contains("AsaasApiId"))
        {
            // Usuário não possui conta no Asaas
            return BadRequest(new
            {
                erro = "Cliente não cadastrado no sistema de pagamento",
                detalhes = "É necessário criar uma conta de cliente antes de realizar pagamentos. Entre em contato com o suporte.",
                tipoErro = "ACCOUNT_NOT_FOUND_ERROR",
                detalhesInternos = ex.Message
            });
        }
        catch (Exception ex) when (ex.Message.Contains("Transação com cartão negada"))
        {
            // Transação com cartão negada
            return BadRequest(new
            {
                erro = "Transação negada",
                detalhes = "O pagamento com cartão de crédito foi negado. Verifique os dados do cartão ou tente outro cartão.",
                tipoErro = "PAYMENT_DECLINED_ERROR",
                detalhesInternos = ex.Message
            });
        }
        catch (Exception ex) when (ex.Message.Contains("Erro ao criar pagamento"))
        {
            // Erros específicos da API do Asaas
            return BadRequest(new
            {
                erro = "Erro ao processar pagamento",
                detalhes = "Não foi possível processar o pagamento. Verifique os dados informados e tente novamente.",
                tipoErro = "PAYMENT_PROCESSING_ERROR",
                detalhesInternos = ex.Message
            });
        }
        catch (Exception ex)
        {
            // Erro genérico não tratado
            return StatusCode(500, new
            {
                erro = "Erro interno do servidor",
                detalhes = "Ocorreu um erro inesperado. Tente novamente ou entre em contato com o suporte.",
                tipoErro = "INTERNAL_SERVER_ERROR",
                detalhesInternos = ex.Message
            });
        }
    }
    
    /// <summary>
    /// Cria um novo usuário.
    /// </summary>
    /// <remarks>Exemplo: POST /api/payment/CreateUser</remarks>
    [HttpPost("CreateUser")] //envia a informação de pagamento para a api do Asaas
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> CreateUser([FromForm] AsaasUserDTO user)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var clienteId = await _paymentService.CreateUserAsync(user);
            return Ok(new {
                mensagem = "Cliente criado com sucesso!",
                clienteId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {
                erro = "Erro ao criar cliente",
                detalhes = ex.Message
            });
        }
    }
    
    [HttpPost("GetPixQrCodeByCpf")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> GetPixQrCodeByCpf([FromForm] string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return BadRequest(new { erro = "O CPF é obrigatório." });
        try
        {
            var pixData = await _paymentService.GetPixQrCodeByCpfAsync(cpf);
            return Ok(pixData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = "Erro ao obter dados do PIX", detalhes = ex.Message });
        }
    }
    
    [HttpPost("GetBankSlipByDocumentNumber")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> GetBankSlipByDocumentNumber([FromForm] string documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
            return BadRequest(new { erro = "O número do documento é obrigatório." });
        try
        {
            var boletoUrl = await _paymentService.GetBankSlipByDocumentNumber(documentNumber);
            return Ok(new { boletoUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = "Erro ao obter boleto para download", detalhes = ex.Message });
        }
    }
    
    /// <summary>
    /// Consulta o status atual de um pagamento pelo ID interno.
    /// </summary>
    [HttpGet("status/{paymentId}")]
    public async Task<IActionResult> GetPaymentStatus(int paymentId)
    {
        try
        {
            var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
            if (payment == null)
                return NotFound(new { erro = "Pagamento não encontrado." });

            return Ok(new
            {
                pagamentoId = payment.PaymentId,
                status = payment.Status.ToString(),
                statusDescricao = GetStatusDescription(payment.Status),
                valor = payment.Amount,
                metodoPagamento = payment.PaymentMethod.ToString(),
                dataPagamento = payment.PaidAt,
                reservaId = payment.ReservationId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                erro = "Erro ao consultar status do pagamento",
                detalhes = ex.Message
            });
        }
    }

    /// <summary>
    /// Força a sincronização manual dos pagamentos com a API Asaas.
    /// </summary>
    [HttpPost("sincronizar")]
    public async Task<IActionResult> SincronizarPagamentos()
    {
        try
        {
            await _paymentService.SynchronizePaymentsAsync();
            return Ok(new { mensagem = "Sincronização executada com sucesso!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                erro = "Erro ao sincronizar pagamentos",
                detalhes = ex.Message
            });
        }
    }

    private static string GetStatusDescription(PaymentStatus status)
    {
        return status switch
        {
            PaymentStatus.PENDING => "Pagamento criado, aguardando",
            PaymentStatus.RECEIVED => "Pagamento confirmado/recebido",
            PaymentStatus.OVERDUE => "Pagamento vencido",
            PaymentStatus.CANCELED => "Pagamento cancelado",
            _ => "Status desconhecido"
        };
    }
}