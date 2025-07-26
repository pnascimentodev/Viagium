using Microsoft.AspNetCore.Mvc;
using Viagium.Services;
using Viagium.Services.Interfaces;

namespace Viagium.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AmenityController : ControllerBase
{
    private readonly IAmenityService _amenityService;

    public AmenityController(IAmenityService amenityService)
    {
        _amenityService = amenityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var amenities = await _amenityService.GetAllAsync();
            return Ok(amenities);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Erro ao buscar adcionais: " + ex.Message);
        }
    }
    
}