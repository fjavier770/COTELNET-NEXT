using CotelNet.Application.Abstractions;
using CotelNet.Application.Administration;
using CotelNet.Domain.Estafetas;
using CotelNet.Domain.Users;
using CotelNet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CotelNet.Api.Configuration;

public static class DatabaseInitialiser
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CotelNetDbContext>();
        await db.Database.EnsureCreatedAsync();

        var permissionDefinitions = new[]
        {
            (PermissionCodes.DashboardView, "Ver panel principal", "General"),
            (PermissionCodes.UsersManage, "Administrar usuarios", "Administración"),
            (PermissionCodes.RolesManage, "Administrar roles y permisos", "Administración"),
            (PermissionCodes.EstafetasManage, "Administrar estafetas", "Administración"),
            (PermissionCodes.TerminalsManage, "Administrar terminales", "Administración"),
            (PermissionCodes.VentasAccess, "Acceder a ventas", "Operación"),
            (PermissionCodes.EnviosAccess, "Acceder a envíos", "Operación"),
            (PermissionCodes.CajaAccess, "Acceder a caja", "Operación"),
            (PermissionCodes.InventarioAccess, "Acceder a inventario", "Operación"),
            (PermissionCodes.ApartadosAccess, "Acceder a apartados", "Operación"),
            (PermissionCodes.ReportesAccess, "Acceder a reportes", "Operación")
        };

        var existingCodes = await db.Permissions.Select(x => x.Code).ToListAsync();
        db.Permissions.AddRange(permissionDefinitions
            .Where(x => !existingCodes.Contains(x.Item1))
            .Select(x => new Permission(x.Item1, x.Item2, x.Item3)));
        await db.SaveChangesAsync();

        var administrator = await db.Roles.Include(x => x.RolePermissions).SingleOrDefaultAsync(x => x.Name == "Administrador");
        if (administrator is null)
        {
            administrator = new Role("Administrador", "Acceso completo a la plataforma");
            db.Roles.Add(administrator);
            await db.SaveChangesAsync();
        }

        var allPermissionIds = await db.Permissions.Select(x => x.Id).ToListAsync();
        var assignedPermissionIds = administrator.RolePermissions.Select(x => x.PermissionId).ToHashSet();
        db.RolePermissions.AddRange(allPermissionIds.Where(x => !assignedPermissionIds.Contains(x)).Select(x => new RolePermission(administrator.Id, x)));

        if (!await db.Roles.AnyAsync(x => x.Name == "Oficinista"))
        {
            var role = new Role("Oficinista", "Operación diaria de una estafeta");
            db.Roles.Add(role);
            await db.SaveChangesAsync();
            var operationalCodes = new[] { PermissionCodes.DashboardView, PermissionCodes.VentasAccess, PermissionCodes.EnviosAccess, PermissionCodes.CajaAccess, PermissionCodes.InventarioAccess, PermissionCodes.ApartadosAccess };
            var ids = await db.Permissions.Where(x => operationalCodes.Contains(x.Code)).Select(x => x.Id).ToListAsync();
            db.RolePermissions.AddRange(ids.Select(x => new RolePermission(role.Id, x)));
        }

        var central = await db.Estafetas.SingleOrDefaultAsync(x => x.Codigo == "000");
        if (central is null)
        {
            central = new Estafeta("000", "Administración Central");
            db.Estafetas.Add(central);
        }
        await db.SaveChangesAsync();

        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        if (await users.AnyAsync(CancellationToken.None)) return;

        var username = app.Configuration["Bootstrap:AdminUser"];
        var password = app.Configuration["Bootstrap:AdminPassword"];
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return;

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var result = hasher.Hash(password);
        var admin = new User(username, "Administrador COTELNET", result.Hash, result.Salt, result.Iterations, administrator.Id);
        admin.AssignEstafeta(central.Id);
        await users.AddAsync(admin, CancellationToken.None);
        await users.SaveChangesAsync(CancellationToken.None);
    }
}
