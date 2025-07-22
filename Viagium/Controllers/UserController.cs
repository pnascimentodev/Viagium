using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Viagium.Models;
using Viagium.Services;
using Viagium.EntitiesDTO;

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
            var createdUser = await _userService.AddAync(user, userCreateDto.Password); // Envia senha pura
            return CreatedAtAction(nameof(GetById), new { id = createdUser.UserId }, createdUser);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
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
            return Ok(user);
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
    public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDTO userUpdateDto)
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
            await _userService.UpdateAsync(user, userUpdateDto.Password); // Envia senha em texto puro
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
}