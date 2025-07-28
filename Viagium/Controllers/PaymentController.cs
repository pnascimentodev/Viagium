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
    public async Task<IActionResult> CreatePayment([FromBody] Reservation reservation)
    {
        try
        {
            var payment = await _paymentService.AddAsync(reservation);

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
    public async Task<IActionResult> CreateUser([FromBody] AsaasUserDTO user)
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
    
    
    
}