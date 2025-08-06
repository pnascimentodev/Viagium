using Microsoft.AspNetCore.Mvc;
using Viagium.EntitiesDTO.Auth;
using Viagium.Services.Interfaces;

namespace Viagium.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    /// <summary>
    /// Realiza o login do usuário (cliente).
    /// </summary>
    /// <remarks>Exemplo: POST /api/auth</remarks>
    // Removido: login padrão
}
