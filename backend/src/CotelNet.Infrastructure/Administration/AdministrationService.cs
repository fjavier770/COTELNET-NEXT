using CotelNet.Application.Abstractions;
using CotelNet.Application.Administration;
using CotelNet.Domain.Estafetas;
using CotelNet.Domain.Users;
using CotelNet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CotelNet.Infrastructure.Administration;

public sealed class AdministrationService(CotelNetDbContext db, IPasswordHasher passwordHasher) : IAdministrationService
{
    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken)
    {
        var users = await db.Users.Include(x => x.Role).AsNoTracking().OrderBy(x => x.Username).ToListAsync(cancellationToken);
        var estafetas = await db.Estafetas.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Nombre, cancellationToken);
        return users.Select(x => ToDto(x, x.EstafetaId is int id && estafetas.TryGetValue(id, out var name) ? name : null)).ToList();
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        Require(request.Username, "El usuario");
        Require(request.FullName, "El nombre completo");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8) throw new InvalidOperationException("La contraseña debe tener al menos 8 caracteres.");
        if (await db.Users.AnyAsync(x => x.Username == request.Username.Trim().ToLower(), cancellationToken))
            throw new InvalidOperationException("El nombre de usuario ya existe.");
        var role = await db.Roles.FindAsync([request.RoleId], cancellationToken) ?? throw new InvalidOperationException("El rol seleccionado no existe.");
        var estafeta = await FindEstafetaAsync(request.EstafetaId, cancellationToken);
        var password = passwordHasher.Hash(request.Password);
        var user = new User(request.Username, request.FullName, password.Hash, password.Salt, password.Iterations, role.Id);
        user.AssignEstafeta(request.EstafetaId);
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return new UserDto(user.Id, user.Username, user.FullName, user.Active, role.Id, role.Name, user.EstafetaId, estafeta?.Nombre);
    }

    public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        Require(request.FullName, "El nombre completo");
        var user = await db.Users.FindAsync([id], cancellationToken);
        if (user is null) return null;
        var role = await db.Roles.FindAsync([request.RoleId], cancellationToken) ?? throw new InvalidOperationException("El rol seleccionado no existe.");
        var estafeta = await FindEstafetaAsync(request.EstafetaId, cancellationToken);
        user.Update(request.FullName, role.Id, request.EstafetaId, request.Active);
        await db.SaveChangesAsync(cancellationToken);
        return new UserDto(user.Id, user.Username, user.FullName, user.Active, role.Id, role.Name, user.EstafetaId, estafeta?.Nombre);
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken) =>
        (await db.Roles.Include(x => x.RolePermissions).ThenInclude(x => x.Permission).AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken))
            .Select(ToDto).ToList();

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        Require(request.Name, "El nombre del rol");
        if (await db.Roles.AnyAsync(x => x.Name == request.Name.Trim(), cancellationToken))
            throw new InvalidOperationException("Ya existe un rol con ese nombre.");
        var permissionIds = await ValidatePermissionIdsAsync(request.PermissionIds, cancellationToken);
        var role = new Role(request.Name, request.Description ?? string.Empty);
        db.Roles.Add(role);
        await db.SaveChangesAsync(cancellationToken);
        db.RolePermissions.AddRange(permissionIds.Select(x => new RolePermission(role.Id, x)));
        await db.SaveChangesAsync(cancellationToken);
        return (await GetRoleAsync(role.Id, cancellationToken))!;
    }

    public async Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        Require(request.Name, "El nombre del rol");
        var role = await db.Roles.Include(x => x.RolePermissions).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (role is null) return null;
        if (await db.Roles.AnyAsync(x => x.Id != id && x.Name == request.Name.Trim(), cancellationToken))
            throw new InvalidOperationException("Ya existe un rol con ese nombre.");
        var permissionIds = await ValidatePermissionIdsAsync(request.PermissionIds, cancellationToken);
        role.Update(request.Name, request.Description ?? string.Empty, request.Active);
        var currentIds = role.RolePermissions.Select(x => x.PermissionId).ToHashSet();
        db.RolePermissions.RemoveRange(role.RolePermissions.Where(x => !permissionIds.Contains(x.PermissionId)));
        db.RolePermissions.AddRange(permissionIds.Where(x => !currentIds.Contains(x)).Select(x => new RolePermission(role.Id, x)));
        await db.SaveChangesAsync(cancellationToken);
        return await GetRoleAsync(role.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken) =>
        await db.Permissions.AsNoTracking().OrderBy(x => x.Module).ThenBy(x => x.Name)
            .Select(x => new PermissionDto(x.Id, x.Code, x.Name, x.Module)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<EstafetaDto>> GetEstafetasAsync(CancellationToken cancellationToken) =>
        await db.Estafetas.AsNoTracking().OrderBy(x => x.Codigo)
            .Select(x => new EstafetaDto(x.Id, x.Codigo, x.Nombre, x.Activa)).ToListAsync(cancellationToken);

    public async Task<EstafetaDto> CreateEstafetaAsync(CreateEstafetaRequest request, CancellationToken cancellationToken)
    {
        if (await db.Estafetas.AnyAsync(x => x.Codigo == request.Codigo.Trim(), cancellationToken))
            throw new InvalidOperationException("Ya existe una estafeta con ese código.");
        var estafeta = new Estafeta(request.Codigo, request.Nombre);
        db.Estafetas.Add(estafeta);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(estafeta);
    }

    public async Task<EstafetaDto?> UpdateEstafetaAsync(int id, UpdateEstafetaRequest request, CancellationToken cancellationToken)
    {
        var estafeta = await db.Estafetas.FindAsync([id], cancellationToken);
        if (estafeta is null) return null;
        if (await db.Estafetas.AnyAsync(x => x.Id != id && x.Codigo == request.Codigo.Trim(), cancellationToken))
            throw new InvalidOperationException("Ya existe una estafeta con ese código.");
        estafeta.Update(request.Codigo, request.Nombre, request.Activa);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(estafeta);
    }

    public async Task<IReadOnlyList<TerminalDto>> GetTerminalsAsync(CancellationToken cancellationToken) =>
        await db.Terminals.Include(x => x.Estafeta).AsNoTracking().OrderBy(x => x.Code)
            .Select(x => new TerminalDto(x.Id, x.Code, x.Name, x.MacAddress, x.Active, x.EstafetaId, x.Estafeta.Nombre)).ToListAsync(cancellationToken);

    public async Task<TerminalDto> CreateTerminalAsync(CreateTerminalRequest request, CancellationToken cancellationToken)
    {
        await ValidateTerminalAsync(null, request.Code, request.MacAddress, request.EstafetaId, cancellationToken);
        var terminal = new Terminal(request.Code, request.Name, request.MacAddress, request.EstafetaId);
        db.Terminals.Add(terminal);
        await db.SaveChangesAsync(cancellationToken);
        var estafeta = await db.Estafetas.FindAsync([request.EstafetaId], cancellationToken);
        return ToDto(terminal, estafeta!.Nombre);
    }

    public async Task<TerminalDto?> UpdateTerminalAsync(int id, UpdateTerminalRequest request, CancellationToken cancellationToken)
    {
        var terminal = await db.Terminals.FindAsync([id], cancellationToken);
        if (terminal is null) return null;
        await ValidateTerminalAsync(id, request.Code, request.MacAddress, request.EstafetaId, cancellationToken);
        terminal.Update(request.Code, request.Name, request.MacAddress, request.EstafetaId, request.Active);
        await db.SaveChangesAsync(cancellationToken);
        var estafeta = await db.Estafetas.FindAsync([request.EstafetaId], cancellationToken);
        return ToDto(terminal, estafeta!.Nombre);
    }

    private async Task<RoleDto?> GetRoleAsync(int id, CancellationToken cancellationToken)
    {
        var role = await db.Roles.Include(x => x.RolePermissions).ThenInclude(x => x.Permission).AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return role is null ? null : ToDto(role);
    }

    private async Task<int[]> ValidatePermissionIdsAsync(IReadOnlyCollection<int>? ids, CancellationToken cancellationToken)
    {
        var distinct = (ids ?? []).Distinct().ToArray();
        var count = await db.Permissions.CountAsync(x => distinct.Contains(x.Id), cancellationToken);
        if (count != distinct.Length) throw new InvalidOperationException("Uno o más permisos no existen.");
        return distinct;
    }

    private async Task<Estafeta?> FindEstafetaAsync(int? id, CancellationToken cancellationToken)
    {
        if (id is null) return null;
        return await db.Estafetas.FindAsync([id.Value], cancellationToken) ?? throw new InvalidOperationException("La estafeta seleccionada no existe.");
    }

    private async Task ValidateTerminalAsync(int? id, string code, string? macAddress, int estafetaId, CancellationToken cancellationToken)
    {
        Require(code, "El código de terminal");
        if (!await db.Estafetas.AnyAsync(x => x.Id == estafetaId, cancellationToken)) throw new InvalidOperationException("La estafeta seleccionada no existe.");
        if (await db.Terminals.AnyAsync(x => x.Id != id && x.Code == code.Trim().ToUpper(), cancellationToken)) throw new InvalidOperationException("Ya existe una terminal con ese código.");
        var mac = string.IsNullOrWhiteSpace(macAddress) ? null : macAddress.Trim().ToUpper();
        if (mac is not null && await db.Terminals.AnyAsync(x => x.Id != id && x.MacAddress == mac, cancellationToken)) throw new InvalidOperationException("La dirección MAC ya está registrada.");
    }

    private static void Require(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException($"{field} es obligatorio.");
    }

    private static UserDto ToDto(User x, string? estafeta) => new(x.Id, x.Username, x.FullName, x.Active, x.RoleId, x.Role.Name, x.EstafetaId, estafeta);
    private static RoleDto ToDto(Role x) => new(x.Id, x.Name, x.Description, x.Active, x.RolePermissions.Select(p => p.PermissionId).ToArray(), x.RolePermissions.Select(p => p.Permission.Code).OrderBy(p => p).ToArray());
    private static EstafetaDto ToDto(Estafeta x) => new(x.Id, x.Codigo, x.Nombre, x.Activa);
    private static TerminalDto ToDto(Terminal x, string estafeta) => new(x.Id, x.Code, x.Name, x.MacAddress, x.Active, x.EstafetaId, estafeta);
}
