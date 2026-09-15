namespace CotelNet.Application.Administration;

public sealed record UserDto(int Id, string Username, string FullName, bool Active, int RoleId, string Role, int? EstafetaId, string? Estafeta);
public sealed record CreateUserRequest(string Username, string FullName, string Password, int RoleId, int? EstafetaId);
public sealed record UpdateUserRequest(string FullName, int RoleId, int? EstafetaId, bool Active);

public sealed record RoleDto(int Id, string Name, string Description, bool Active, IReadOnlyCollection<int> PermissionIds, IReadOnlyCollection<string> Permissions);
public sealed record CreateRoleRequest(string Name, string Description, IReadOnlyCollection<int>? PermissionIds);
public sealed record UpdateRoleRequest(string Name, string Description, bool Active, IReadOnlyCollection<int>? PermissionIds);
public sealed record PermissionDto(int Id, string Code, string Name, string Module);

public sealed record EstafetaDto(int Id, string Codigo, string Nombre, bool Activa);
public sealed record CreateEstafetaRequest(string Codigo, string Nombre);
public sealed record UpdateEstafetaRequest(string Codigo, string Nombre, bool Activa);

public sealed record TerminalDto(int Id, string Code, string Name, string? MacAddress, bool Active, int EstafetaId, string Estafeta);
public sealed record CreateTerminalRequest(string Code, string Name, string? MacAddress, int EstafetaId);
public sealed record UpdateTerminalRequest(string Code, string Name, string? MacAddress, int EstafetaId, bool Active);

public interface IAdministrationService
{
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken);
    Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken);
    Task<RoleDto> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken);
    Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<EstafetaDto>> GetEstafetasAsync(CancellationToken cancellationToken);
    Task<EstafetaDto> CreateEstafetaAsync(CreateEstafetaRequest request, CancellationToken cancellationToken);
    Task<EstafetaDto?> UpdateEstafetaAsync(int id, UpdateEstafetaRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<TerminalDto>> GetTerminalsAsync(CancellationToken cancellationToken);
    Task<TerminalDto> CreateTerminalAsync(CreateTerminalRequest request, CancellationToken cancellationToken);
    Task<TerminalDto?> UpdateTerminalAsync(int id, UpdateTerminalRequest request, CancellationToken cancellationToken);
}
