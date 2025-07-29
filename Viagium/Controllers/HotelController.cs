using Microsoft.AspNetCore.Mvc;
using Viagium.EntitiesDTO;
using Viagium.Services;
using Viagium.Services.Interfaces;

namespace Viagium.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HotelController : ControllerBase
{
    private readonly IHotelService _hotelService;
    private readonly ImgbbService _imgbbService;
    
    public HotelController(IHotelService hotelService, ImgbbService imgbbService)
    {
        _hotelService = hotelService;
        _imgbbService = imgbbService;
    }
    
    /// <summary>
    /// Cria um novo hotel.
    /// </summary>
    /// <remarks>Exemplo: POST /api/hotel/create</remarks>
    [HttpPost ("create")]
    public async Task<IActionResult> Create([FromForm] HotelCreateFormDTO hotelCreateFormDTO)
    {
        try
        {
            if (hotelCreateFormDTO == null)
                return BadRequest("Dados do hotel não podem ser nulos.");

            if (hotelCreateFormDTO.Image != null)
            {
                var imageUrl = await _imgbbService.UploadImageAsync(hotelCreateFormDTO.Image);
                hotelCreateFormDTO.ImageUrl = imageUrl;
            }
            var createdHotel = await _hotelService.AddAsync(hotelCreateFormDTO);
            if (createdHotel == null)
                return StatusCode(500, "Erro ao criar hotel.");
            return CreatedAtAction(nameof(GetById), new { id = createdHotel.HotelId }, createdHotel);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao criar hotel: {ex.Message}");
        }
    }
    

    /// <summary>
    /// Atualiza os dados de um hotel existente.
    /// </summary>
    /// <remarks>Exemplo: PUT /api/hotel/1</remarks>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] HotelWithAddressDTO hotelWithAddressDTO)
    {
        try
        {
            if (id != hotelWithAddressDTO.HotelId)
                return BadRequest("Id do hotel não coincide com o id fornecido na rota.");
            
            await _hotelService.UpdateAsync(hotelWithAddressDTO);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao atualizar hotel: {ex.Message}");
        }
    }

    /// <summary>
    /// Desativa um hotel pelo ID.
    /// </summary>
    /// <remarks>Exemplo: DELETE /api/hotel/1/desactivate</remarks>
    [HttpDelete("{id}/desactivate")]
    public async Task<IActionResult> Desactivate(int id)
    {
        try
        {
            var result = await _hotelService.DesactivateAsync(id);
            if (!result)
                return NotFound("Hotel não encontrado ou já desativado.");
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao desativar hotel: {ex.Message}");
        }
    }

    /// <summary>
    /// Ativa um hotel pelo ID.
    /// </summary>
    /// <remarks>Exemplo: PUT /api/hotel/1/activate</remarks>
    [HttpPut("{id}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        try
        {
            var result = await _hotelService.ActivateAsync(id);
            if (!result)
                return NotFound("Hotel não encontrado ou já ativado.");
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao ativar hotel: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Busca um hotel pelo ID.
    /// </summary>
    /// <remarks>Exemplo: GET /api/hotel/1</remarks>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var hotel = await _hotelService.GetByIdAsync(id);
            if (hotel == null)
                return NotFound("Hotel não encontrado.");
            return Ok(hotel);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao buscar hotel: {ex.Message}");
        }
    }

    /// <summary>
    /// Lista todos os hotéis cadastrados.
    /// </summary>
    /// <remarks>Exemplo: GET /api/hotel</remarks>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var hotels = await _hotelService.GetAllAsync();
            if (hotels == null || !hotels.Any())
                return NotFound("Nenhum hotel encontrado.");
            return Ok(hotels);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao buscar hotéis: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Busca hotéis por cidade.
    /// </summary>
    /// <remarks>Exemplo: GET /api/hotel/by-city/SaoPaulo</remarks>
    [HttpGet("by-city/{city}")]
    public async Task<IActionResult> GetByCity(string city)
    {
        var hotels = await _hotelService.GetByCityAsync(city);
        return Ok(hotels);
    }

    /// <summary>
    /// Busca hotéis por adcionais.
    /// </summary>
    /// <remarks>Exemplo: GET /api/hotel/by-amenities?amenityIds=1&amenityIds=2</remarks>
    [HttpGet("by-amenities")]
    public async Task<IActionResult> GetByAmenities([FromQuery] List<int> amenityIds)
    {
        var hotels = await _hotelService.GetByAmenitiesAsync(amenityIds);
        return Ok(hotels);
    }
    
}