using Microsoft.AspNetCore.Mvc;
using Viagium.Models;
using Viagium.Services;

namespace Viagium.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
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
    public IActionResult GetById(int id)
    {
        // Método placeholder para o CreatedAtAction funcionar
        return Ok();
    }
}