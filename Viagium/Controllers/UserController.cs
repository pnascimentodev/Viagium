using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Viagium.Models;
using Viagium.Services;
using Viagium.EntitiesDTO.User;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.Auth;

namespace Viagium.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IMapper _mapper;

    public UserController(UserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    // cadastra um usuário
    [HttpPost]
    public async Task<IActionResult> Cadastro([FromBody] UserCreateDTO userCreateDto)
    {
        try
        {
            // Mapeia o DTO para User, exceto senha
            var user = _mapper.Map<User>(userCreateDto);
            user.Role = Role.Client;
            user.IsActive = true;
            var createdUser = await _userService.AddAsync(userCreateDto, userCreateDto.Password); // Corrigido: passa o DTO
            return CreatedAtAction(nameof(GetById), new { id = createdUser.UserId }, createdUser);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    // endpoint de login
    [HttpPost("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest)
    {
        try
        {
            var response = await _userService.LoginAsync(loginRequest);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Erro interno ao processar a autenticação." });
        }
    }

    // busca um usuário por id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound("Id não encontrado.");
            var userDto = _mapper.Map<UserListDTO>(user);
            return Ok(userDto);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    // busca todos os usuários
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    // atualiza um usuário
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto userUpdateDto)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound("Id não encontrado.");

            // Mapeia os campos do DTO para o usuário existente, exceto DocumentNumber
            _mapper.Map(userUpdateDto, user);
            user.UpdatedAt = DateTime.Now;

            ExceptionHandler.ValidateObject(user, "usuário");
            await _userService.UpdateAsync(userUpdateDto, userUpdateDto.Password); // Corrigido: passa o DTO
            return Ok(user); 
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }
    
    //Alterar a senha de um usuário
    [HttpPut("{id}/password")]
    public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdatePasswordDto dto)
    {
        try
        {
            var user = await _userService.UpdatePasswordAsync(id, dto);
            if (user == null)
                return NotFound("Usuário não encontrado para atualização de senha.");
            return Ok(user);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    // Desativa um usuário
    [HttpDelete("{id}")]
    public async Task<IActionResult> Desactivate(int id)
    {
        try
        {
            var userDisabled = await _userService.DesativateAsync(id);
            if (userDisabled == null)
                return NotFound("Usuário não encontrado para desativação.");
            return Ok(userDisabled);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    // ativar um usuário
    [HttpPost("activate/{id}")]
    public async Task<IActionResult> Activate(int id)
    {
        try
        {
            var userActivated = await _userService.ActivateAsync(id);
            if (userActivated == null)
                return NotFound("Usuário não encontrado para ativação.");
            return Ok(userActivated);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }
    
    [HttpGet("by-email")]
    public async Task<IActionResult> GetByEmail([FromQuery] string email)
    {
        try
        {
            var user = await _userService.GetByEmailAsync(email, false);
            if (user == null)
                return NotFound("Usuário não encontrado com este e-mail.");
            var userDto = _mapper.Map<UserDTO>(user);
            return Ok(userDto);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    
}