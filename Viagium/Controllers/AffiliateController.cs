using Microsoft.AspNetCore.Mvc;
using Viagium.Models;
using Viagium.Services;
using Viagium.Services.Interfaces;
using Viagium.EntitiesDTO;
using AutoMapper;
using Viagium.EntitiesDTO.Auth;
using Viagium.Services.Auth.Affiliate;
using Viagium.EntitiesDTO.Affiliate;
using Viagium.EntitiesDTO.User;
using Microsoft.AspNetCore.Authorization;

namespace Viagium.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AffiliateController : ControllerBase
{
    private IAuthAffiliateService _authAffiliateService;
    private readonly IAffiliateService _affiliateService;
    private readonly IMapper _mapper;

    public AffiliateController(IAffiliateService affiliateService, IMapper mapper, IAuthAffiliateService authAffiliateService)
    {
        _affiliateService = affiliateService;
        _mapper = mapper;
        _authAffiliateService = authAffiliateService;
    }
    

    /// <summary>
    /// Cria um novo afiliado.
    /// </summary>
    /// <remarks>Exemplo: POST /api/affiliate/create</remarks>
    [HttpPost("create")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateAffiliate([FromBody] AffiliateCreateDto affiliateCreateDto)
    {
        try
        {
            // Mapeia o DTO para User, exceto senha
            var affiliate = _mapper.Map<Affiliate>(affiliateCreateDto);
            affiliate.IsActive = true;
            var createdAffiliate = await _affiliateService.AddAsync(affiliateCreateDto, affiliateCreateDto.HashPassword); // Corrigido: passa o DTO
            return CreatedAtAction(nameof(GetById), new { id = createdAffiliate.AffiliateId }, createdAffiliate);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    /// <summary>
    /// Edita um afiliado existente.
    /// </summary>
    /// <remarks>Exemplo: PUT /api/affiliate/1</remarks>
    [HttpPut("{id}")]
    public async Task<IActionResult> Editar(int id, [FromBody] Affiliate affiliate)
    {
        try
        {
            affiliate.AffiliateId = id;
            var updatedAffiliate = await _affiliateService.UpdateAsync(affiliate);
            return Ok(updatedAffiliate);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    /// <summary>
    /// Busca um afiliado pelo ID.
    /// </summary>
    /// <remarks>Exemplo: GET /api/affiliate/1</remarks>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var affiliate = await _affiliateService.GetByIdAsync(id);
            return Ok(affiliate);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    /// <summary>
    /// Lista todos os afiliados cadastrados.
    /// </summary>
    /// <remarks>Exemplo: GET /api/affiliate</remarks>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var affiliates = await _affiliateService.GetAllAsync();
            return Ok(affiliates);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    /// <summary>
    /// Exclui um afiliado pelo ID.
    /// </summary>
    /// <remarks>Exemplo: DELETE /api/affiliate/1</remarks>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _affiliateService.DeleteAsync(id);
            if (result)
                return NoContent();
            return NotFound();
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }


    /// <summary>
    /// Busca afiliados por cidade.
    /// </summary>
    /// <remarks>Exemplo: GET /api/affiliate/ByCity/SaoPaulo</remarks>
    [HttpGet("ByCity/{city}")]
    public async Task<IActionResult> GetByCity(string city)
    {
        try
        {
            var affiliates = await _affiliateService.GetByCityAsync(city);
            var result = _mapper.Map<List<AffiliateListDTO>>(affiliates);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }
    
    /// <summary>
    /// Realiza login do afiliado.
    /// </summary>
    /// <remarks>Exemplo: POST /api/affiliate/login</remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest)
    {
        try
        {
            var response = await _authAffiliateService.LoginAsync(loginRequest);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    /// <summary>
    /// Busca afiliado pelo e-mail.
    /// </summary>
    /// <remarks>Exemplo: GET /api/affiliate/by-email?email=exemplo@email.com</remarks>
    [HttpGet("by-email")]
    public async Task<IActionResult> GetByEmail([FromQuery] string email)
    {
        try
        {
            var affiliate = await _affiliateService.GetByEmailAsync(email, false);
            if (affiliate == null)
                return NotFound("Email não encontrado.");
            var userDto = _mapper.Map<AffiliateDTO>(affiliate);
            return Ok(userDto);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }

    }
    /// <summary>
    /// Envia e-mail de recuperação de senha para o afiliado.
    /// </summary>
    /// <remarks>Exemplo: POST /api/affiliate/forgot-password</remarks>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> SendForgotPasswordEmailAsync([FromBody] string email)
    {
        try
        {
            await _affiliateService.SendForgotPasswordEmailAsync(email);
            return Ok(new { message = "Uma mensagem foi enviada com instruções para redefinir a senha." });
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    /// <summary>
    /// Atualiza a senha do afiliado.
    /// </summary>
    /// <remarks>Exemplo: PUT /api/affiliate/update-password/1</remarks>
    [HttpPut("update-password/{id}")]
   public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdatePasswordDto dto)
    {
        try
        {
            var updatedAffiliate = await _affiliateService.UpdatePasswordAsync(id, dto);
            return Ok("Senha atualizada com sucesso.");
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    /// <summary>
    /// Redefine a senha do afiliado.
    /// </summary>
    /// <remarks>Exemplo: POST /api/affiliate/forgot-password/1</remarks>
    [HttpPost("forgot-password/{id}")]
    public async Task<IActionResult> ForgotPassword(int id, [FromBody] ForgotPasswordDto dto)
    {
        try
        {
            var user = await _affiliateService.ForgotPasswordAsync(id, dto.NewPassword);
            if (user == null)
                return NotFound("Usuário não encontrado para recuperação de senha.");
            return Ok(user);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    /// <summary>
    /// Realiza logout do afiliado, invalidando o token JWT.
    /// </summary>
    /// <remarks>Exemplo: POST /api/affiliate/logout</remarks>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] string token)
    {
        try
        {
            var result = await _authAffiliateService.LogoutAsync(token);
            return Ok(new { message = "Logout realizado com sucesso." });
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

}