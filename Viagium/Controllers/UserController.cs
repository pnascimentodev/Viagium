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

    [HttpPost]
    public async Task<IActionResult> Cadastro([FromBody] User user)
    {
        try
        {
            ExceptionHandler.ValidateObject(user, "usuário");

            user.HashPassword = Viagium.Services.PasswordHelper.HashPassword(user.HashPassword);
            user.Role = Role.Client;
            var createdUser = await _userService.AddAync(user);

            return CreatedAtAction(nameof(GetById), new { id = createdUser.UserId }, createdUser);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }

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
            user.HashPassword = Viagium.Services.PasswordHelper.HashPassword(userUpdateDto.HashPassword);
            user.UpdatedAt = DateTime.Now;

            ExceptionHandler.ValidateObject(user, "usuário");
            await _userService.UpdateAsync(user);
            return Ok(user); 
        }
        catch (Exception ex)
        {
            return ExceptionHandler.HandleException(ex);
        }
    }
}