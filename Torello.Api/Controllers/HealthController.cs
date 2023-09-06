using Microsoft.AspNetCore.Mvc;
using Torello.Application.Common.Interfaces.Persistence;

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
