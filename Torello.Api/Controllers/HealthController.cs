using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace Torello.Api.Controllers;

public class HealthController : ControllerBase
{
    [HttpGet("/_health", Name = nameof(GetHealth))]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IActionResult), 200)]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            CurrentUtcDateTime = DateTime.UtcNow,
        });
    }
}
