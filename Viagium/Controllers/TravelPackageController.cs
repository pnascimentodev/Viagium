using Microsoft.AspNetCore.Mvc;
using Viagium.Services;
using AutoMapper;

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
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Models.TravelPackage travelPackage)
    {
        try
        {
            ExceptionHandler.ValidateObject(travelPackage, "pacote de viagem");
            var createdPackage = await _service.AddAsync(travelPackage);
            var dto = _mapper.Map<EntitiesDTO.TravelPackageDTO>(createdPackage);
            return CreatedAtAction(nameof(GetById), new { id = createdPackage.TravelPackagesId }, dto);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var pacote = await _service.GetByIdAsync(id);
        if (pacote == null)
            return NotFound();
        return Ok(pacote);
    }


    [HttpGet]
    public async Task<IActionResult> GetAllTravelPackages()
    {
        var pacotes = await _service.GetAllAsync();
        return Ok(pacotes);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EntitiesDTO.EditTravelPackageDTO dto)
    {
        // Aqui você pode validar se o id bate com o que está no DTO, se necessário
        try
        {
            ExceptionHandler.ValidateObject(dto, "pacote de viagem");
            var travelPackage = _mapper.Map<Models.TravelPackage>(dto);
            travelPackage.TravelPackagesId = id; // Garante que o id da URL será usado
            var pacoteAtualizado = await _service.UpdateAsync(travelPackage);
            var dtoAtualizado = _mapper.Map<EntitiesDTO.EditTravelPackageDTO>(pacoteAtualizado);
            return Ok(dtoAtualizado);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }
}