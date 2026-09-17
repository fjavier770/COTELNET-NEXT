using CotelNet.Application.Abstractions;
using CotelNet.Application.Administration;
using CotelNet.Domain.Estafetas;
using CotelNet.Domain.Users;
using CotelNet.Domain.Sales;
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
        await EnsurePrototypeSchemaCompatibilityAsync(db);

        var permissionDefinitions = new[]
        {
            (PermissionCodes.DashboardView, "Ver panel principal", "General"),
            (PermissionCodes.UsersManage, "Administrar usuarios", "Administración"),
            (PermissionCodes.RolesManage, "Administrar roles y permisos", "Administración"),
            (PermissionCodes.EstafetasManage, "Administrar estafetas", "Administración"),
            (PermissionCodes.TerminalsManage, "Administrar terminales", "Administración"),
            (PermissionCodes.ServicesManage, "Administrar servicios y tarifas", "Administración"),
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

        if (!await db.Terminals.AnyAsync())
        {
            db.Terminals.Add(new Terminal("T-000-01", "Caja principal", null, central.Id));
            await db.SaveChangesAsync();
        }

        if (!await db.PaymentMethods.AnyAsync())
        {
            db.PaymentMethods.AddRange(
                new PaymentMethod("EFECTIVO", "Efectivo", true),
                new PaymentMethod("TARJETA", "Tarjeta", false),
                new PaymentMethod("TRANSFERENCIA", "Transferencia", false));
        }

        if (!await db.Tariffs.AnyAsync())
        {
            var stamp = new Product("SELLO-050", "Sello postal B/.0.50");
            var envelope = new Product("SOBRE-M", "Sobre manila");
            var ordinary = new PostalService("CORR-NAC", "Correo nacional ordinario", "RT");
            var certified = new PostalService("CERT-NAC", "Correo nacional certificado", "RR");
            var express = new PostalService("EMS-NAC", "EMS nacional", "EE");
            db.Products.AddRange(stamp, envelope);
            db.PostalServices.AddRange(ordinary, certified, express);
            await db.SaveChangesAsync();
            db.Tariffs.AddRange(
                new Tariff("SELLO-050", stamp.Name, 0.50m, productId: stamp.Id),
                new Tariff("SOBRE-M", envelope.Name, 0.75m, productId: envelope.Id),
                new Tariff("CORR-NAC", ordinary.Name, 1.00m, postalServiceId: ordinary.Id),
                new Tariff("CERT-NAC", certified.Name, 2.50m, postalServiceId: certified.Id),
                new Tariff("EMS-NAC", express.Name, 5.00m, postalServiceId: express.Id));
        }
        await db.SaveChangesAsync();

        if (!await db.Destinations.AnyAsync())
        {
            db.Destinations.AddRange(
                new Destination("PA", "Panamá", "NACIONAL", true),
                new Destination("CR", "Costa Rica", "ZONA1", false),
                new Destination("CO", "Colombia", "ZONA1", false),
                new Destination("US", "Estados Unidos", "ZONA2", false),
                new Destination("ES", "España", "ZONA3", false));
        }
        if (!await db.SupplementaryServices.AnyAsync())
        {
            db.SupplementaryServices.AddRange(
                new SupplementaryService("AR", "Aviso de recibo", 1.50m),
                new SupplementaryService("CERT", "Certificación", 1.00m),
                new SupplementaryService("SEG", "Seguro básico", 2.00m));
        }
        await db.SaveChangesAsync();

        if (!await db.WeightTariffs.AnyAsync())
        {
            var services = await db.PostalServices.OrderBy(x => x.Id).ToListAsync();
            var zones = new[] { (Code: "NACIONAL", Factor: 1m), (Code: "ZONA1", Factor: 2m), (Code: "ZONA2", Factor: 3m), (Code: "ZONA3", Factor: 4m) };
            var bands = new[] { (Min: 0, Max: 500, Factor: 1m), (Min: 500, Max: 1000, Factor: 1.6m), (Min: 1000, Max: 2000, Factor: 2.4m), (Min: 2000, Max: 5000, Factor: 4m) };
            for (var serviceIndex = 0; serviceIndex < services.Count; serviceIndex++)
                foreach (var zone in zones)
                    foreach (var band in bands)
                        db.WeightTariffs.Add(new WeightTariff(services[serviceIndex].Id, zone.Code, band.Min, band.Max, decimal.Round((1m + serviceIndex * 1.5m) * zone.Factor * band.Factor, 2)));
            await db.SaveChangesAsync();
        }

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

    private static async Task EnsurePrototypeSchemaCompatibilityAsync(CotelNetDbContext db)
    {
        // EnsureCreated no modifica una base existente. Este bloque mantiene los entornos Docker
        // de desarrollo creados antes de incorporar el prefijo S10.
        await db.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('PostalServices', 'S10Prefix') IS NULL
                EXEC(N'ALTER TABLE PostalServices ADD S10Prefix nvarchar(2) NULL');
            EXEC(N'UPDATE PostalServices
                   SET S10Prefix = CASE WHEN Code LIKE ''EMS%'' THEN ''EE'' WHEN Code LIKE ''CERT%'' THEN ''RR'' ELSE ''RT'' END
                   WHERE S10Prefix IS NULL');
            EXEC(N'ALTER TABLE PostalServices ALTER COLUMN S10Prefix nvarchar(2) NULL');
            IF COL_LENGTH('PostalServices', 'LegacyServiceTypeId') IS NULL ALTER TABLE PostalServices ADD LegacyServiceTypeId int NULL;
            IF COL_LENGTH('PostalServices', 'LegacyRouteId') IS NULL ALTER TABLE PostalServices ADD LegacyRouteId int NULL;
            IF COL_LENGTH('Destinations', 'LegacyCountryId') IS NULL ALTER TABLE Destinations ADD LegacyCountryId int NULL;
            IF COL_LENGTH('Destinations', 'LegacyProvinceId') IS NULL ALTER TABLE Destinations ADD LegacyProvinceId int NULL;
            IF COL_LENGTH('WeightTariffs', 'LegacyTariffId') IS NULL ALTER TABLE WeightTariffs ADD LegacyTariffId int NULL;
            IF COL_LENGTH('SupplementaryServices', 'LegacyId') IS NULL ALTER TABLE SupplementaryServices ADD LegacyId int NULL;
            IF COL_LENGTH('SupplementaryServices', 'S10Prefix') IS NULL ALTER TABLE SupplementaryServices ADD S10Prefix nvarchar(2) NULL;
            IF OBJECT_ID('SenderProfiles', 'U') IS NULL
            BEGIN
                CREATE TABLE SenderProfiles (
                    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_SenderProfiles PRIMARY KEY,
                    DocumentKey nvarchar(40) NOT NULL,
                    DocumentNumber nvarchar(40) NOT NULL,
                    CountryCode nvarchar(2) NOT NULL CONSTRAINT DF_SenderProfiles_CountryCode DEFAULT 'PA',
                    Title nvarchar(30) NOT NULL CONSTRAINT DF_SenderProfiles_Title DEFAULT '',
                    IsMinor bit NOT NULL CONSTRAINT DF_SenderProfiles_IsMinor DEFAULT 0,
                    FirstName nvarchar(80) NOT NULL,
                    MiddleName nvarchar(80) NOT NULL CONSTRAINT DF_SenderProfiles_MiddleName DEFAULT '',
                    FirstLastName nvarchar(80) NOT NULL,
                    SecondLastName nvarchar(80) NOT NULL CONSTRAINT DF_SenderProfiles_SecondLastName DEFAULT '',
                    PrimaryPhone nvarchar(40) NOT NULL CONSTRAINT DF_SenderProfiles_PrimaryPhone DEFAULT '',
                    SecondaryPhone nvarchar(40) NOT NULL CONSTRAINT DF_SenderProfiles_SecondaryPhone DEFAULT '',
                    Email nvarchar(160) NOT NULL CONSTRAINT DF_SenderProfiles_Email DEFAULT '',
                    Province nvarchar(100) NOT NULL CONSTRAINT DF_SenderProfiles_Province DEFAULT '',
                    City nvarchar(100) NOT NULL CONSTRAINT DF_SenderProfiles_City DEFAULT '',
                    PostalCode nvarchar(20) NOT NULL CONSTRAINT DF_SenderProfiles_PostalCode DEFAULT '',
                    Street nvarchar(160) NOT NULL CONSTRAINT DF_SenderProfiles_Street DEFAULT '',
                    HouseNumber nvarchar(40) NOT NULL CONSTRAINT DF_SenderProfiles_HouseNumber DEFAULT '',
                    Address nvarchar(300) NOT NULL,
                    Fax nvarchar(40) NOT NULL CONSTRAINT DF_SenderProfiles_Fax DEFAULT '',
                    CreatedAtUtc datetime2 NOT NULL CONSTRAINT DF_SenderProfiles_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
                    UpdatedAtUtc datetime2 NULL
                );
                CREATE UNIQUE INDEX IX_SenderProfiles_DocumentKey ON SenderProfiles(DocumentKey);
            END
            IF COL_LENGTH('SenderProfiles', 'CountryCode') IS NULL ALTER TABLE SenderProfiles ADD CountryCode nvarchar(2) NOT NULL CONSTRAINT DF_SenderProfiles_CountryCode DEFAULT 'PA';
            IF COL_LENGTH('Shipments', 'SenderProfileId') IS NULL ALTER TABLE Shipments ADD SenderProfileId int NULL;
            IF COL_LENGTH('Shipments', 'RecipientTitle') IS NULL ALTER TABLE Shipments ADD RecipientTitle nvarchar(30) NOT NULL CONSTRAINT DF_Shipments_RecipientTitle DEFAULT '';
            IF COL_LENGTH('Shipments', 'RecipientFirstName') IS NULL ALTER TABLE Shipments ADD RecipientFirstName nvarchar(80) NOT NULL CONSTRAINT DF_Shipments_RecipientFirstName DEFAULT '';
            IF COL_LENGTH('Shipments', 'RecipientMiddleName') IS NULL ALTER TABLE Shipments ADD RecipientMiddleName nvarchar(80) NOT NULL CONSTRAINT DF_Shipments_RecipientMiddleName DEFAULT '';
            IF COL_LENGTH('Shipments', 'RecipientFirstLastName') IS NULL ALTER TABLE Shipments ADD RecipientFirstLastName nvarchar(80) NOT NULL CONSTRAINT DF_Shipments_RecipientFirstLastName DEFAULT '';
            IF COL_LENGTH('Shipments', 'RecipientSecondLastName') IS NULL ALTER TABLE Shipments ADD RecipientSecondLastName nvarchar(80) NOT NULL CONSTRAINT DF_Shipments_RecipientSecondLastName DEFAULT '';
            IF COL_LENGTH('Shipments', 'RecipientSecondaryPhone') IS NULL ALTER TABLE Shipments ADD RecipientSecondaryPhone nvarchar(40) NOT NULL CONSTRAINT DF_Shipments_RecipientSecondaryPhone DEFAULT '';
            IF COL_LENGTH('Shipments', 'RecipientEmail') IS NULL ALTER TABLE Shipments ADD RecipientEmail nvarchar(160) NOT NULL CONSTRAINT DF_Shipments_RecipientEmail DEFAULT '';
            IF COL_LENGTH('Shipments', 'RecipientProvince') IS NULL ALTER TABLE Shipments ADD RecipientProvince nvarchar(100) NOT NULL CONSTRAINT DF_Shipments_RecipientProvince DEFAULT '';
            IF COL_LENGTH('Shipments', 'RecipientCity') IS NULL ALTER TABLE Shipments ADD RecipientCity nvarchar(100) NOT NULL CONSTRAINT DF_Shipments_RecipientCity DEFAULT '';
            IF COL_LENGTH('Shipments', 'RecipientPostalCode') IS NULL ALTER TABLE Shipments ADD RecipientPostalCode nvarchar(20) NOT NULL CONSTRAINT DF_Shipments_RecipientPostalCode DEFAULT '';
            IF COL_LENGTH('Shipments', 'RecipientStreet') IS NULL ALTER TABLE Shipments ADD RecipientStreet nvarchar(160) NOT NULL CONSTRAINT DF_Shipments_RecipientStreet DEFAULT '';
            IF COL_LENGTH('Shipments', 'RecipientHouseNumber') IS NULL ALTER TABLE Shipments ADD RecipientHouseNumber nvarchar(40) NOT NULL CONSTRAINT DF_Shipments_RecipientHouseNumber DEFAULT '';
            IF COL_LENGTH('Shipments', 'RecipientFax') IS NULL ALTER TABLE Shipments ADD RecipientFax nvarchar(40) NOT NULL CONSTRAINT DF_Shipments_RecipientFax DEFAULT '';
            IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Shipments_SenderProfiles_SenderProfileId')
                ALTER TABLE Shipments ADD CONSTRAINT FK_Shipments_SenderProfiles_SenderProfileId FOREIGN KEY (SenderProfileId) REFERENCES SenderProfiles(Id);
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Shipments_SenderProfileId' AND object_id = OBJECT_ID('Shipments'))
                CREATE INDEX IX_Shipments_SenderProfileId ON Shipments(SenderProfileId);
            IF OBJECT_ID('TariffSupplementaryOptions', 'U') IS NULL
            BEGIN
                CREATE TABLE TariffSupplementaryOptions (
                    LegacyTariffId int NOT NULL,
                    SupplementaryServiceId int NOT NULL,
                    PriceAdjustment decimal(12,2) NOT NULL,
                    CONSTRAINT PK_TariffSupplementaryOptions PRIMARY KEY (LegacyTariffId, SupplementaryServiceId),
                    CONSTRAINT FK_TariffSupplementaryOptions_SupplementaryServices FOREIGN KEY (SupplementaryServiceId) REFERENCES SupplementaryServices(Id)
                );
            END
            """);
    }
}
