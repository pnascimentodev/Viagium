using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.Auth;
using Viagium.Models;
using Viagium.Services; // Adicionado para ExceptionHandler
using Viagium.Services.Interfaces;

namespace Viagium.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IMapper _mapper;

    public AdminController(IAdminService adminService, IMapper mapper)
    {
        _adminService = adminService;
        _mapper = mapper;
    }

    /// <summary>
    /// Realiza o registro de um novo administrador.
    /// </summary>
    /// <remarks>Exemplo: POST /api/admin/register</remarks>
    [HttpPost("register")]
    [AllowAnonymous]
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

    /// <summary>
    /// Realiza o login de administrador/suporte.
    /// </summary>
    /// <remarks>Exemplo: POST /api/admin/auth</remarks>
    [HttpPost("auth")]
    [AllowAnonymous]
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

    /// <summary>
    /// Busca dados do administrador pelo ID.
    /// </summary>
    /// <remarks>Exemplo: GET /api/admin/1</remarks>
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

    /// <summary>
    /// Atualiza dados do administrador.
    /// </summary>
    /// <remarks>Exemplo: PUT /api/admin/1</remarks>
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

    /// <summary>
    /// Lista todos os administradores ativos e inativos.
    /// </summary>
    /// <remarks>Exemplo: GET /api/admin</remarks>
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

    /// <summary>
    /// Lista todos os administradores ativos.
    /// </summary>
    /// <remarks>Exemplo: GET /api/admin/active</remarks>
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

    /// <summary>
    /// Desativa um usuário administrador.
    /// </summary>
    /// <remarks>Exemplo: DELETE /api/admin/1</remarks>
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

    /// <summary>
    /// Ativa um usuário administrador.
    /// </summary>
    /// <remarks>Exemplo: POST /api/admin/1/activate</remarks>
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
