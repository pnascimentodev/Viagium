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
                Amenities = formDto.Amenities ?? new List<int>()
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _roomTypeService.GetByIdAsync(id);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _roomTypeService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("amenities")]
    public async Task<IActionResult> GetByAmenityId([FromQuery] List<int> amenityIds)
    {
        var result = await _roomTypeService.GetByAmenityId(amenityIds);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] RoomTypeUpdateDTO dto)
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
}
