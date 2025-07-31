using Microsoft.AspNetCore.Mvc;
using Viagium.Services;
using AutoMapper;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.TravelPackageDTO;

namespace Viagium.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TravelPackageController : ControllerBase
{
    private readonly TravelPackageService _service;
    private readonly IMapper _mapper;
    public TravelPackageController(TravelPackageService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateTravelPackage([FromForm] CreateTravelPackageDTO create)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.AddAsync(create);
            return CreatedAtAction(nameof(CreateTravelPackage), new { title = result.Title }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao criar pacote de viagem: {ex.Message}");
        }
    }
}