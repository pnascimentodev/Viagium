using Microsoft.AspNetCore.Mvc;
using Viagium.Models;
using Viagium.Services;
using Viagium.Services.Interfaces;
using Viagium.EntitiesDTO;
using AutoMapper;
using Viagium.EntitiesDTO.Auth;
using Viagium.Services.Auth.Affiliate;
using Viagium.EntitiesDTO.Affiliate;

namespace Viagium.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    
    // endpoint de login
    [HttpPost("login")]
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

}