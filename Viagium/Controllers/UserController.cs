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

    /// <summary>
    /// Cadastra um novo usuário.
    /// </summary>
    /// <remarks>Exemplo: POST /api/user</remarks>
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

    /// <summary>
    /// Realiza o login do usuário.
    /// </summary>
    /// <remarks>Exemplo: POST /api/user/auth</remarks>
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

    /// <summary>
    /// Busca um usuário pelo ID.
    /// </summary>
    /// <remarks>Exemplo: GET /api/user/1</remarks>
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

    /// <summary>
    /// Lista todos os usuários cadastrados.
    /// </summary>
    /// <remarks>Exemplo: GET /api/user</remarks>
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

    /// <summary>
    /// Atualiza os dados de um usuário.
    /// </summary>
    /// <remarks>Exemplo: PUT /api/user/1</remarks>
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
    
    /// <summary>
    /// Altera a senha de um usuário.
    /// </summary>
    /// <remarks>Exemplo: PUT /api/user/1/password</remarks>
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
    
    /// <summary>
    /// Recupera a senha de um usuário.
    /// </summary>
    /// <remarks>Exemplo: POST /api/user/1/forgot-password</remarks>
    // Recuperar senha de um usuário
    [HttpPost("{id}/forgot-password")]
    public async Task<IActionResult> ForgotPassword(int id, [FromBody] ForgotPasswordDto dto)
    {
        try
        {
            var user = await _userService.ForgotPasswordAsync(id, dto.NewPassword);
            if (user == null)
                return NotFound("Usuário não encontrado para recuperação de senha.");
            return Ok(user);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

    /// <summary>
    /// Desativa um usuário.
    /// </summary>
    /// <remarks>Exemplo: DELETE /api/user/1</remarks>
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

    /// <summary>
    /// Ativa um usuário.
    /// </summary>
    /// <remarks>Exemplo: POST /api/user/activate/1</remarks>
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
    
    /// <summary>
    /// Busca um usuário pelo e-mail.
    /// </summary>
    /// <remarks>Exemplo: GET /api/user/by-email?email=exemplo@teste.com</remarks>
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

    /// <summary>
    /// Envia e-mail de recuperação de senha para o usuário.
    /// </summary>
    /// <remarks>Exemplo: POST /api/user/forgot-password-email</remarks>
    [HttpPost("forgot-password-email")]
    public async Task<IActionResult> ForgotPasswordEmail([FromBody] string email)
    {
        try
        {
            await _userService.SendForgotPasswordEmailAsync(email);
            return Ok("Se o e-mail existir, uma mensagem foi enviada com instruções para redefinir a senha.");
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }
}
