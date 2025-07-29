using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Viagium.EntitiesDTO;
using Viagium.Services;

namespace Viagium.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly AddressService _service;

        public AddressController(AddressService service)
        {
            _service = service;
            
        }

        /// <summary>
        /// Cria um novo endereço.
        /// </summary>
        /// <remarks>Exemplo: POST /api/address</remarks>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddressDTO addressDto)
        {
            try
            {
                ExceptionHandler.ValidateObject(addressDto, "endereço");
                var createdAddress = await _service.AddAsync(addressDto);
                return CreatedAtAction(nameof(GetById), new { id = createdAddress.AddressNumber }, createdAddress);
            }
            catch (Exception ex)
            {
                return ExceptionHandler.HandleException(ex);
            }
        }

        /// <summary>
        /// Busca um endereço pelo ID.
        /// </summary>
        /// <remarks>Exemplo: GET /api/address/1</remarks>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var address = await _service.GetByIdAsync(id);
            if (address == null)
                return NotFound("Endereço não encontrado.");
            return Ok(address);
        }

        /// <summary>
        /// Lista todos os endereços cadastrados.
        /// </summary>
        /// <remarks>Exemplo: GET /api/address</remarks>
        [HttpGet]
        public async Task<IActionResult> GetAllAddresses()
        {
            var addresses = await _service.GetAllAsync();
            return Ok(addresses);
        }

        /// <summary>
        /// Atualiza um endereço existente.
        /// </summary>
        /// <remarks>Exemplo: PUT /api/address/1</remarks>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AddressDTO addressDto)
        {
            if (id != addressDto.AddressNumber)
                return BadRequest("Id da rota e do corpo não coincidem.");
            var updatedAddress = await _service.UpdateAsync(id, addressDto);
            return Ok(updatedAddress);
        }
    }
}
