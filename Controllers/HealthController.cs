using Microsoft.AspNetCore.Mvc;

namespace learnflow_service.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy" });
}
