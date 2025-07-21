using Microsoft.AspNetCore.Mvc;
using Viagium.EntitiesDTO.Auth;
using Viagium.Services.Auth;

namespace Viagium.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// Realiza login e retorna um token JWT.
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest)
    {
        try
        {
            var response = await _authService.LoginAsync(loginRequest);
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
}