using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Viagium.EntitiesDTO.ApiDTO;
using Viagium.EntitiesDTO.User;
using Viagium.Models;
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
    


    [HttpPost("CreatePayment")] //envia a informação de pagamento para a api do Asaas
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> CreatePayment([FromForm] Reservation reservation)
    {
        try
        {
            var payment = await _paymentService.AddPaymentAsync(reservation);

            return Ok(new
            {
                mensagem = "Pagamento criado com sucesso!",
                pagamentoId = payment.PaymentId,
                status = payment.Status,
                metodo = payment.PaymentMethod,
                valor = payment.Amount
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
}