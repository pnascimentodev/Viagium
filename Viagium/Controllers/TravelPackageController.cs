using Microsoft.AspNetCore.Mvc;
using Viagium.Services;

namespace Viagium.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TravelPackageController : ControllerBase
{
    private readonly TravelPackageService _service;
    public TravelPackageController(TravelPackageService service)
    {
        _service = service;
    }
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Models.TravelPackage travelPackage)
    {
        try
        {
            ExceptionHandler.ValidateObject(travelPackage, "pacote de viagem");
            var createdPackage = await _service.AddAsync(travelPackage);
            return CreatedAtAction(nameof(GetById), new { id = createdPackage.TravelPackagesId }, createdPackage);
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
}