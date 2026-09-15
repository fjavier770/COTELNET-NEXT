using CotelNet.Application.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CotelNet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/administration")]
public sealed class AdministrationController(IAdministrationService service) : ControllerBase
{
    [HttpGet("users")]
    [Authorize(Policy = PermissionCodes.UsersManage)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken) => Ok(await service.GetUsersAsync(cancellationToken));

    [HttpPost("users")]
    [Authorize(Policy = PermissionCodes.UsersManage)]
    public Task<IActionResult> CreateUser(CreateUserRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => service.CreateUserAsync(request, cancellationToken));

    [HttpPut("users/{id:int}")]
    [Authorize(Policy = PermissionCodes.UsersManage)]
    public Task<IActionResult> UpdateUser(int id, UpdateUserRequest request, CancellationToken cancellationToken) =>
        ExecuteNullableAsync(() => service.UpdateUserAsync(id, request, cancellationToken));

    [HttpGet("roles")]
    [Authorize(Policy = PermissionCodes.RolesManage)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken) => Ok(await service.GetRolesAsync(cancellationToken));

    [HttpGet("users/roles")]
    [Authorize(Policy = PermissionCodes.UsersManage)]
    public async Task<IActionResult> GetUserRoles(CancellationToken cancellationToken) => Ok(await service.GetRolesAsync(cancellationToken));

    [HttpPost("roles")]
    [Authorize(Policy = PermissionCodes.RolesManage)]
    public Task<IActionResult> CreateRole(CreateRoleRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => service.CreateRoleAsync(request, cancellationToken));

    [HttpPut("roles/{id:int}")]
    [Authorize(Policy = PermissionCodes.RolesManage)]
    public Task<IActionResult> UpdateRole(int id, UpdateRoleRequest request, CancellationToken cancellationToken) =>
        ExecuteNullableAsync(() => service.UpdateRoleAsync(id, request, cancellationToken));

    [HttpGet("permissions")]
    [Authorize(Policy = PermissionCodes.RolesManage)]
    public async Task<IActionResult> GetPermissions(CancellationToken cancellationToken) => Ok(await service.GetPermissionsAsync(cancellationToken));

    [HttpGet("estafetas")]
    [Authorize(Policy = PermissionCodes.EstafetasManage)]
    public async Task<IActionResult> GetEstafetas(CancellationToken cancellationToken) => Ok(await service.GetEstafetasAsync(cancellationToken));

    [HttpGet("users/estafetas")]
    [Authorize(Policy = PermissionCodes.UsersManage)]
    public async Task<IActionResult> GetUserEstafetas(CancellationToken cancellationToken) => Ok(await service.GetEstafetasAsync(cancellationToken));

    [HttpGet("terminals/estafetas")]
    [Authorize(Policy = PermissionCodes.TerminalsManage)]
    public async Task<IActionResult> GetTerminalEstafetas(CancellationToken cancellationToken) => Ok(await service.GetEstafetasAsync(cancellationToken));

    [HttpPost("estafetas")]
    [Authorize(Policy = PermissionCodes.EstafetasManage)]
    public Task<IActionResult> CreateEstafeta(CreateEstafetaRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => service.CreateEstafetaAsync(request, cancellationToken));

    [HttpPut("estafetas/{id:int}")]
    [Authorize(Policy = PermissionCodes.EstafetasManage)]
    public Task<IActionResult> UpdateEstafeta(int id, UpdateEstafetaRequest request, CancellationToken cancellationToken) =>
        ExecuteNullableAsync(() => service.UpdateEstafetaAsync(id, request, cancellationToken));

    [HttpGet("terminals")]
    [Authorize(Policy = PermissionCodes.TerminalsManage)]
    public async Task<IActionResult> GetTerminals(CancellationToken cancellationToken) => Ok(await service.GetTerminalsAsync(cancellationToken));

    [HttpPost("terminals")]
    [Authorize(Policy = PermissionCodes.TerminalsManage)]
    public Task<IActionResult> CreateTerminal(CreateTerminalRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => service.CreateTerminalAsync(request, cancellationToken));

    [HttpPut("terminals/{id:int}")]
    [Authorize(Policy = PermissionCodes.TerminalsManage)]
    public Task<IActionResult> UpdateTerminal(int id, UpdateTerminalRequest request, CancellationToken cancellationToken) =>
        ExecuteNullableAsync(() => service.UpdateTerminalAsync(id, request, cancellationToken));

    private async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try { return Ok(await action()); }
        catch (InvalidOperationException exception) { return Conflict(new { message = exception.Message }); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }

    private async Task<IActionResult> ExecuteNullableAsync<T>(Func<Task<T?>> action) where T : class
    {
        try
        {
            var result = await action();
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException exception) { return Conflict(new { message = exception.Message }); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }
}
