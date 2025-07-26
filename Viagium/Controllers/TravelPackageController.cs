using Microsoft.AspNetCore.Mvc;
using Viagium.Services;
using AutoMapper;
using Viagium.EntitiesDTO;

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
    
    //Criar um pacote de viagem
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

    // Método para buscar pacote de viagem por ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var pacote = await _service.GetByIdAsync(id);
        if (pacote == null)
            return NotFound();
        return Ok(pacote);
    }


    // Método para buscar todos os pacotes de viagem
    [HttpGet]
    public async Task<IActionResult> GetAllTravelPackages()
    {
        var pacotes = await _service.GetAllAsync();
        return Ok(pacotes);
    }
    
    // Método para atualizar um pacote de viagem
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EditTravelPackageDTO dto)
    {
        try
        {
            ExceptionHandler.ValidateObject(dto, "pacote de viagem");
            var pacoteAtualizado = await _service.UpdateAsync(id, dto);
            return Ok(pacoteAtualizado);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }
    
    // Método para desativar um pacote de viagem
    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            var pacoteDesativado = await _service.DesativateAsync(id);
            if (pacoteDesativado == null)
                return NotFound("Pacote de viagem não encontrado para desativação.");
            return Ok(pacoteDesativado);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }
    
    // Método para ativar um pacote de viagem
    [HttpPost("activate/{id}")]
    public async Task<IActionResult> Activate(int id)
    {
        try
        {
            var pacoteAtivado = await _service.ActivateAsync(id);
            if (pacoteAtivado == null)
                return NotFound("Pacote de viagem não encontrado para ativação.");
            return Ok(pacoteAtivado);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    [HttpPost("active-promotion/{id}")]
    public async Task<IActionResult> ActivePromotion(int id, [FromBody] decimal discountPercentage)
    {
        try
        {
            var pacoteAtivado = await _service.ActivePromotionAsync(id, discountPercentage);
            if (pacoteAtivado == null)
                return NotFound("Pacote de viagem não encontrado para ativação de promoção.");
            return Ok(pacoteAtivado);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    [HttpGet("active-package")]
    public async Task<IActionResult> GetActivePackage()
    {
        try
        {
            var pacoteAtivo = await _service.GetActiveAsync();
            return Ok(pacoteAtivo);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }
}