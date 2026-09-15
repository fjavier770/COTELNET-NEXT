using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CotelNet.Api.Controllers;

[ApiController]
[Route("api/v1/system")]
public sealed class SystemController : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new
    {
        username = User.Identity?.Name,
        fullName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
        role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
        estafetaId = User.FindFirst("estafeta_id")?.Value,
        permissions = User.FindAll("permission").Select(x => x.Value).OrderBy(x => x)
    });
}
