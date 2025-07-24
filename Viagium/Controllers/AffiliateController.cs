using Microsoft.AspNetCore.Mvc;
using Viagium.Models;
using Viagium.Services;
using Viagium.Services.Interfaces;
using Viagium.EntitiesDTO;
using AutoMapper;

namespace Viagium.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AffiliateController : ControllerBase
{
    private readonly IAffiliateService _affiliateService;
    private readonly IMapper _mapper;

    public AffiliateController(IAffiliateService affiliateService, IMapper mapper)
    {
        _affiliateService = affiliateService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Cadastro([FromBody] Affiliate affiliate)
    {
        try
        {
            ExceptionHandler.ValidateObject(affiliate, "afiliado");
            var createdAffiliate = await _affiliateService.AddAsync(affiliate);
            return CreatedAtAction(nameof(GetById), new { id = createdAffiliate.AffiliateId }, createdAffiliate);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

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
}