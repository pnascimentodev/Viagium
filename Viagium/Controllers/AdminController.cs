using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Viagium.Models;
using Viagium.Services.Interfaces;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.Auth;
using Viagium.Services; // Adicionado para ExceptionHandler

namespace Viagium.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IMapper _mapper;

    public AdminController(IAdminService adminService, IMapper mapper)
    {
        _adminService = adminService;
        _mapper = mapper;
    }

    // Registro de admin
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AdminRegisterDTO adminRegisterDto)
    {
        try
        {
            var createdUser = await _adminService.RegisterAsync(adminRegisterDto);
            var adminDto = _mapper.Map<AdminDTO>(createdUser);
            return CreatedAtAction(nameof(GetById), new { id = adminDto.UserId }, adminDto);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    // Login de admin/suporte
    [HttpPost("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest)
    {
        try
        {
            var response = await _adminService.LoginWithRoleAsync(loginRequest);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    // Buscar dados do admin
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var adminDto = await _adminService.GetByIdAsync(id);
            if (adminDto == null)
                return NotFound("Admin não encontrado.");
            return Ok(adminDto);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    // Atualizar dados do admin
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminUpdateDTO adminUpdateDto)
    {
        try
        {
            var adminDto = await _adminService.UpdateAsync(id, adminUpdateDto);
            return Ok(adminDto);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    // Buscar todos os admins
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var admins = await _adminService.GetAllAsync();
            return Ok(admins);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    // Buscar todos os admins ativos
    [HttpGet("active")]
    public async Task<IActionResult> GetAllActive()
    {
        try
        {
            var admins = await _adminService.GetAllActiveAsync();
            return Ok(admins);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    // Desativar usuário
    [HttpDelete("{id}")]
    public async Task<IActionResult> DesativateUser(int id)
    {
        try
        {
            var user = await _adminService.DesativateUserAsync(id);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    // Ativar usuário
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        try
        {
            var user = await _adminService.ActivateUserAsync(id);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }
}
