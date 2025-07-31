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

    /// <summary>
    /// Cria um novo pacote de viagem.
    /// </summary>
    /// <remarks>Exemplo: POST /api/TravelPackage/create</remarks>
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

    /// <summary>
    /// Lista todos os pacotes de viagem.
    /// </summary>
    /// <remarks>Exemplo: GET /api/TravelPackage/list</remarks>
    [HttpGet("list")]
    public async Task<IActionResult> ListAll()
    {
        var result = await _service.ListAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Atualiza um pacote de viagem.
    /// </summary>
    /// <remarks>Exemplo: PUT /api/TravelPackage/update</remarks>
    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] ResponseTravelPackageDTO dto)
    {
        try
        {
            var result = await _service.UpdateAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Aplica desconto em um pacote de viagem.
    /// </summary>
    /// <remarks>Exemplo: PUT /api/TravelPackage/discount</remarks>
    [HttpPut("discount")]
    public async Task<IActionResult> CreateDiscount(int travelPackageId, decimal discountPercentage)
    {
        try
        {
            var result = await _service.CreateDiscountAsync(travelPackageId, discountPercentage, DateTime.MinValue, DateTime.MinValue);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Remove desconto de um pacote de viagem.
    /// </summary>
    /// <remarks>Exemplo: PUT /api/TravelPackage/discount/deactivate</remarks>
    [HttpPut("discount/deactivate")]
    public async Task<IActionResult> DesactivateDiscount(int travelPackageId)
    {
        try
        {
            var result = await _service.DesactivateDiscountAsync(travelPackageId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Desativa um pacote de viagem.
    /// </summary>
    /// <remarks>Exemplo: POST /api/TravelPackage/deactivate</remarks>
    [HttpPost("deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            var result = await _service.DesactivateAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Ativa um pacote de viagem.
    /// </summary>
    /// <remarks>Exemplo: POST /api/TravelPackage/activate</remarks>
    [HttpPost("activate")]
    public async Task<IActionResult> Activate(int id)
    {
        try
        {
            var result = await _service.ActivateAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Busca um pacote de viagem por ID.
    /// </summary>
    /// <remarks>Exemplo: GET /api/TravelPackage/getById/1</remarks>
    [HttpGet("getById/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Busca um pacote de viagem por nome.
    /// </summary>
    /// <remarks>Exemplo: GET /api/TravelPackage/getByName/Natal</remarks>
    [HttpGet("getByName/{name}")]
    public async Task<IActionResult> GetByName(string name)
    {
        var result = await _service.GetByNameAsync(name);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Busca um pacote de viagem por cidade e país de destino.
    /// </summary>
    /// <remarks>Exemplo: GET /api/TravelPackage/getByCityAndCountry?city=Garanhuns&amp;country=Brasil</remarks>
    [HttpGet("getByCityAndCountry")]
    public async Task<IActionResult> GetByCityAndCountry(string city, string country)
    {
        var result = await _service.GetByCityAndCountryAsync(city, country);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Atualiza o cupom e o valor de desconto de um pacote de viagem já registrado.
    /// </summary>
    /// <remarks>Exemplo: PUT /api/TravelPackage/cupom</remarks>
    [HttpPut("cupom")]
    public async Task<IActionResult> PutCupom([FromQuery] int travelPackageId, [FromQuery] string cupom, [FromQuery] decimal discountValue)
    {
        try
        {
            var result = await _service.UpdateCupomAsync(travelPackageId, cupom, discountValue);
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Retorna o valor do desconto do cupom informado para um pacote.
    /// </summary>
    /// <remarks>Exemplo: GET /api/TravelPackage/cupom-discount?travelPackageId=1&amp;cupom=MEUCUPOM</remarks>
    [HttpGet("cupom-discount")]
    public async Task<IActionResult> GetCupomDiscount([FromQuery] int travelPackageId, [FromQuery] string cupom)
    {
        var pacote = await _service.GetByIdAsync(travelPackageId);
        if (pacote == null)
            return NotFound("Pacote não encontrado.");
        if (string.IsNullOrWhiteSpace(pacote.CupomDiscount) || !string.Equals(pacote.CupomDiscount, cupom, StringComparison.OrdinalIgnoreCase))
            return Ok(0); // Cupom não existe ou não corresponde
        return Ok(pacote.DiscountValue);
    }
}