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

            return Ok(new
            {
                mensagem = "Pagamento criado com sucesso!",
                pagamentoId = payment.PaymentId,
                status = payment.Status.ToString(),
                statusDescricao = GetStatusDescription(payment.Status),
                metodo = payment.PaymentMethod.ToString(),
                valor = payment.Amount,
                processadoImediatamente = paymentMethod == PaymentMethodType.CREDIT_CARD,
                enderecoSalvo = address != null ? "Endereço salvo para o usuário" : "Nenhum endereço fornecido"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                erro = "Erro ao criar pagamento",
                detalhes = ex.Message
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
            var qrCode = await _paymentService.GetPixQrCodeByCpfAsync(cpf);
            return Ok(new { qrCode });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = "Erro ao obter QR Code do PIX", detalhes = ex.Message });
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