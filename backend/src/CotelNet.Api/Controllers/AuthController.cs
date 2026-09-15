using CotelNet.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CotelNet.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(LoginService loginService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Usuario y contraseña son obligatorios." });

        var response = await loginService.LoginAsync(request, cancellationToken);
        return response is null
            ? Unauthorized(new { message = "Credenciales inválidas." })
            : Ok(response);
    }
}

