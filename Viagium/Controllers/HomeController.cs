using Microsoft.AspNetCore.Mvc;

namespace Viagium.Controllers;

[ApiController]
[Route("/")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("API rodando!");
    
}