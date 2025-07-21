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
        Console.WriteLine($"[LOG] Iniciando cadastro de usuário: {user?.Email}");
        try
        {
            ExceptionHandler.ValidateObject(user, "usuário");
            Console.WriteLine("[LOG] Validação concluída");
            var createdUser = await _userService.AddAync(user);
            Console.WriteLine($"[LOG] Usuário criado: {createdUser?.UserId}");
            return CreatedAtAction(nameof(GetById), new { id = createdUser.UserId }, createdUser);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOG] Erro no cadastro: {ex.Message}");
            return ExceptionHandler.HandleException(ex);
        }

    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] User user)
    {
        try
        {
            ExceptionHandler.ValidateObject(user, "usuário");
            var loggedUser = await _userService.LoginAsync(user);
            
            return Ok(loggedUser);
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