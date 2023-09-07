using Microsoft.AspNetCore.Mvc;

namespace Torello.Api.Controllers;

public class HealthController : ControllerBase
{
    [HttpGet("/_health")]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            CurrentUtcDateTime = DateTime.UtcNow,
        });
    }
}
