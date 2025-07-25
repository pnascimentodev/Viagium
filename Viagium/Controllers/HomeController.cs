using Microsoft.AspNetCore.Mvc;

namespace Viagium.Controllers;

[ApiController]
[Route("api/home")] // <- troque a rota
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("API rodando!");
}
