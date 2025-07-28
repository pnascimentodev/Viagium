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

    /// <summary>
    /// Lista todos os adicionais disponíveis.
    /// </summary>
    /// <remarks>Exemplo: GET /api/amenity</remarks>
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

    /// <summary>
    /// Lista os adicionais disponíveis para tipos de quarto.
    /// </summary>
    /// <remarks>Exemplo: GET /api/amenity/TypeRoom</remarks>
    [HttpGet("TypeRoom")]
    public async Task<IActionResult> GetRoomTypeAmenities()
    {
        try
        {
            var amenities = await _amenityService.GetRoomTypeAmenitiesAsync();
            return Ok(amenities);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Erro ao buscar amenities de tipo de quarto: " + ex.Message);
        }
    }

    /// <summary>
    /// Lista os adicionais disponíveis para hotéis.
    /// </summary>
    /// <remarks>Exemplo: GET /api/amenity/Hotel</remarks>
    [HttpGet("Hotel")]
    public async Task<IActionResult> GetHotelAmenities()
    {
        try
        {
            var amenities = await _amenityService.GetHotelAmenitiesAsync();
            return Ok(amenities);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Erro ao buscar amenities de hotel: " + ex.Message);
        }
    }
}