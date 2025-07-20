using Microsoft.AspNetCore.Mvc;
using Viagium.EntitiesDTO;
using Viagium.Services;
using Microsoft.AspNetCore.Authorization;


namespace Viagium.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Adiciona autorização para todos os endpoints
public class TravelPackageController : ControllerBase
{
    private readonly ITravelPackage _service;
    private readonly ILogger<TravelPackageController> _logger;
    
    public TravelPackageController(ITravelPackage service, ILogger<TravelPackageController> logger)
    {
        _service = service;
        _logger = logger;
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateTravelPackageDTO travelPackageDto)
    {
        try
        {
            _logger.LogInformation("Iniciando criação de pacote de viagem para destino: {Destination}", travelPackageDto.DestinationAddress);

            var createdPackage = await _service.AddAsync(travelPackageDto);

            _logger.LogInformation("Pacote de viagem criado com sucesso. ID: {PackageId}", createdPackage.TravelPackagesId);

            return CreatedAtAction(nameof(GetById),
                new { id = createdPackage.TravelPackagesId },
                new
                {
                    id = createdPackage.TravelPackagesId,
                    title = createdPackage.Title,
                    destination = createdPackage.DestinationAddress?.ToString(),
                    price = createdPackage.Price,
                    createdAt = createdPackage.CreatedAt
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pacote de viagem");
            return ExceptionHandler.HandleException(ex);
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public IActionResult GetById(int id)
    {
        try
        {
            _logger.LogInformation("Buscando pacote de viagem com ID: {PackageId}", id);

            // TODO: Implementar busca real no serviço
            return Ok(new {
                message = "Endpoint em desenvolvimento",
                requestedId = id,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pacote com ID: {PackageId}", id);
            return ExceptionHandler.HandleException(ex);
        }
    }
}
