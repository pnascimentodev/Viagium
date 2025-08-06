using Microsoft.AspNetCore.Mvc;
using Viagium.EntitiesDTO;
using Viagium.Services;
using Viagium.Services.Interfaces;

namespace Viagium.Controllers;

[ApiController]
[Route("api/roomtype")]
public class RoomTypeController : ControllerBase
{
    private readonly IRoomTypeService _roomTypeService;
    private readonly ImgbbService _imgbbService;

    public RoomTypeController(IRoomTypeService roomTypeService, ImgbbService imgbbService)
    {
        _roomTypeService = roomTypeService;
        _imgbbService = imgbbService;
    }

    /// <summary>
    /// Cria um novo tipo de quarto.
    /// </summary>
    /// <remarks>Exemplo: POST /api/roomtype</remarks>
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] RoomTypeCreateFormDTO formDto)
    {
        try
        {
            var dto = new RoomTypeCreateDTO
            {
                HotelId = formDto.HotelId,
                Name = formDto.Name,
                Description = formDto.Description,
                PricePerNight = formDto.PricePerNight,
                MaxOccupancy = formDto.MaxOccupancy,
                NumberOfRoomsAvailable = formDto.NumberOfRoomsAvailable,
                Amenities = formDto.Amenities ?? new List<int>(),
                RoomsNumber = formDto.RoomsNumber ?? new List<string>()
            };

            if (formDto.Image != null)
            {
                var imageUrl = await _imgbbService.UploadImageAsync(formDto.Image);
                dto.ImageUrl = imageUrl;
            }
            var result = await _roomTypeService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.RoomTypeId }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Busca um tipo de quarto pelo ID.
    /// </summary>
    /// <remarks>Exemplo: GET /api/roomtype/1</remarks>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _roomTypeService.GetByIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Lista todos os tipos de quarto cadastrados (ativos/inativos).
    /// </summary>
    /// <remarks>Exemplo: GET /api/roomtype</remarks>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _roomTypeService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Lista todos os tipos de quarto ativos.
    /// </summary>
    /// <remarks>Exemplo: GET /api/roomtype/active</remarks>
    [HttpGet("active")]
    public async Task<IActionResult> GetAllActive()
    {
        var result = await _roomTypeService.GetAllActiveAsync();
        return Ok(result);
    }

    /// <summary>
    /// Lista tipos de quarto por amenidades.
    /// </summary>
    /// <remarks>Exemplo: GET /api/roomtype/amenities?amenityIds=1&amp;amenityIds=2</remarks>
    [HttpGet("amenities")]
    public async Task<IActionResult> GetByAmenityId([FromQuery] List<int> amenityIds)
    {
        var result = await _roomTypeService.GetByAmenityId(amenityIds);
        return Ok(result);
    }

    /// <summary>
    /// Atualiza um tipo de quarto.
    /// </summary>
    /// <remarks>Exemplo: PUT /api/roomtype</remarks>
    [HttpPut]
    public async Task<IActionResult> Update([FromForm] RoomTypeUpdateDTO dto)
    {
        try
        {
            var result = await _roomTypeService.UpdateAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Desativa um tipo de quarto.
    /// </summary>
    /// <remarks>Exemplo: DELETE /api/roomtype/1</remarks>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Desativate(int id)
    {
        try
        {
            var result = await _roomTypeService.DesativateAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Ativa um tipo de quarto.
    /// </summary>
    /// <remarks>Exemplo: POST /api/roomtype/1/activate</remarks>
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        try
        {
            var result = await _roomTypeService.ActivateAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lista tipos de quarto que possuem apenas quartos disponíveis.
    /// </summary>
    /// <remarks>Exemplo: GET /api/roomtype/roomAvaliable</remarks>
    [HttpGet("roomAvaliable")]
    public async Task<IActionResult> GetRoomTypesWithAvailableRooms()
    {
        var result = await _roomTypeService.GetRoomTypesWithAvailableRoomsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Lista tipos de quarto que possuem apenas quartos que não estão disponíveis.
    /// </summary>
    /// <remarks>Exemplo: GET /api/roomtype/roomUnavailable</remarks>
    [HttpGet("roomUnavailable")]
    public async Task<IActionResult> GetRoomTypesWithUnavailableRooms()
    {
        var result = await _roomTypeService.GetRoomTypesWithUnavailableRoomsAsync();
        return Ok(result);
    }
}
