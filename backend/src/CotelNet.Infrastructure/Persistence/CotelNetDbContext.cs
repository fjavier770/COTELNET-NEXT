using CotelNet.Domain.Estafetas;
using CotelNet.Domain.Users;
using CotelNet.Domain.Sales;
using Microsoft.EntityFrameworkCore;

namespace CotelNet.Infrastructure.Persistence;

public sealed class CotelNetDbContext(DbContextOptions<CotelNetDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Estafeta> Estafetas => Set<Estafeta>();
    public DbSet<Terminal> Terminals => Set<Terminal>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<PostalService> PostalServices => Set<PostalService>();
    public DbSet<Tariff> Tariffs => Set<Tariff>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<CashSession> CashSessions => Set<CashSession>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleLine> SaleLines => Set<SaleLine>();
    public DbSet<SalePayment> SalePayments => Set<SalePayment>();
    public DbSet<Destination> Destinations => Set<Destination>();
    public DbSet<WeightTariff> WeightTariffs => Set<WeightTariff>();
    public DbSet<SupplementaryService> SupplementaryServices => Set<SupplementaryService>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentSupplementaryService> ShipmentSupplementaryServices => Set<ShipmentSupplementaryService>();
    public DbSet<TariffSupplementaryOption> TariffSupplementaryOptions => Set<TariffSupplementaryOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Username).IsUnique();
            entity.Property(x => x.Username).HasMaxLength(80).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PasswordSalt).HasMaxLength(256).IsRequired();
            entity.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Estafeta>().WithMany().HasForeignKey(x => x.EstafetaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(250).IsRequired();
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Module).HasMaxLength(80).IsRequired();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(x => new { x.RoleId, x.PermissionId });
            entity.HasOne(x => x.Role).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId);
            entity.HasOne(x => x.Permission).WithMany().HasForeignKey(x => x.PermissionId);
        });

        modelBuilder.Entity<Estafeta>(entity =>
        {
            entity.ToTable("Estafetas");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Codigo).IsUnique();
            entity.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<Terminal>(entity =>
        {
            entity.ToTable("Terminals");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.MacAddress).IsUnique().HasFilter("[MacAddress] IS NOT NULL");
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.MacAddress).HasMaxLength(30);
            entity.HasOne(x => x.Estafeta).WithMany().HasForeignKey(x => x.EstafetaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products"); entity.HasKey(x => x.Id); entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired(); entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
        });
        modelBuilder.Entity<PostalService>(entity =>
        {
            entity.ToTable("PostalServices"); entity.HasKey(x => x.Id); entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired(); entity.Property(x => x.Name).HasMaxLength(150).IsRequired(); entity.Property(x => x.S10Prefix).HasMaxLength(2);
            entity.HasIndex(x => new { x.LegacyServiceTypeId, x.LegacyRouteId }).IsUnique().HasFilter("[LegacyServiceTypeId] IS NOT NULL AND [LegacyRouteId] IS NOT NULL");
        });
        modelBuilder.Entity<Tariff>(entity =>
        {
            entity.ToTable("Tariffs"); entity.HasKey(x => x.Id); entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired(); entity.Property(x => x.Description).HasMaxLength(180).IsRequired(); entity.Property(x => x.Price).HasPrecision(12, 2);
            entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PostalService).WithMany().HasForeignKey(x => x.PostalServiceId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.ToTable("PaymentMethods"); entity.HasKey(x => x.Id); entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired(); entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        });
        modelBuilder.Entity<CashSession>(entity =>
        {
            entity.ToTable("CashSessions"); entity.HasKey(x => x.Id);
            entity.Property(x => x.OpeningAmount).HasPrecision(12, 2); entity.Property(x => x.ExpectedCash).HasPrecision(12, 2); entity.Property(x => x.DeclaredCash).HasPrecision(12, 2); entity.Property(x => x.Difference).HasPrecision(12, 2);
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Terminal>().WithMany().HasForeignKey(x => x.TerminalId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.ToTable("Sales"); entity.HasKey(x => x.Id); entity.HasIndex(x => x.InvoiceNumber).IsUnique();
            entity.Property(x => x.InvoiceNumber).HasMaxLength(40).IsRequired(); entity.Property(x => x.Total).HasPrecision(12, 2);
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Estafeta>().WithMany().HasForeignKey(x => x.EstafetaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CashSession>().WithMany().HasForeignKey(x => x.CashSessionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.SaleId);
            entity.HasMany(x => x.Payments).WithOne(x => x.Sale).HasForeignKey(x => x.SaleId);
        });
        modelBuilder.Entity<SaleLine>(entity =>
        {
            entity.ToTable("SaleLines"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired(); entity.Property(x => x.Description).HasMaxLength(180).IsRequired(); entity.Property(x => x.UnitPrice).HasPrecision(12, 2); entity.Property(x => x.Total).HasPrecision(12, 2);
            entity.HasOne<Tariff>().WithMany().HasForeignKey(x => x.TariffId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<SalePayment>(entity =>
        {
            entity.ToTable("SalePayments"); entity.HasKey(x => x.Id); entity.Property(x => x.Amount).HasPrecision(12, 2);
            entity.HasOne(x => x.PaymentMethod).WithMany().HasForeignKey(x => x.PaymentMethodId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Destination>(entity =>
        {
            entity.ToTable("Destinations"); entity.HasKey(x => x.Id); entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(20).IsRequired(); entity.Property(x => x.Name).HasMaxLength(120).IsRequired(); entity.Property(x => x.Zone).HasMaxLength(30).IsRequired();
            entity.HasIndex(x => x.LegacyCountryId).IsUnique().HasFilter("[LegacyCountryId] IS NOT NULL");
            entity.HasIndex(x => x.LegacyProvinceId).IsUnique().HasFilter("[LegacyProvinceId] IS NOT NULL");
        });
        modelBuilder.Entity<WeightTariff>(entity =>
        {
            entity.ToTable("WeightTariffs"); entity.HasKey(x => x.Id); entity.Property(x => x.DestinationZone).HasMaxLength(30).IsRequired(); entity.Property(x => x.Price).HasPrecision(12, 2);
            entity.HasIndex(x => new { x.PostalServiceId, x.DestinationZone, x.MinimumWeightGrams, x.MaximumWeightGrams }).IsUnique();
            entity.HasOne(x => x.PostalService).WithMany().HasForeignKey(x => x.PostalServiceId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<SupplementaryService>(entity =>
        {
            entity.ToTable("SupplementaryServices"); entity.HasKey(x => x.Id); entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired(); entity.Property(x => x.Name).HasMaxLength(140).IsRequired(); entity.Property(x => x.Price).HasPrecision(12, 2);
            entity.Property(x => x.S10Prefix).HasMaxLength(2); entity.HasIndex(x => x.LegacyId).IsUnique().HasFilter("[LegacyId] IS NOT NULL");
        });
        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.ToTable("Shipments"); entity.HasKey(x => x.Id); entity.HasIndex(x => x.SaleId).IsUnique(); entity.HasIndex(x => x.TrackingNumber).IsUnique();
            entity.Property(x => x.TrackingNumber).HasMaxLength(40).IsRequired(); entity.Property(x => x.BasePrice).HasPrecision(12, 2);
            entity.Property(x => x.SenderName).HasMaxLength(160).IsRequired(); entity.Property(x => x.SenderDocument).HasMaxLength(40); entity.Property(x => x.SenderPhone).HasMaxLength(40); entity.Property(x => x.SenderEmail).HasMaxLength(160); entity.Property(x => x.SenderAddress).HasMaxLength(300).IsRequired();
            entity.Property(x => x.RecipientName).HasMaxLength(160).IsRequired(); entity.Property(x => x.RecipientPhone).HasMaxLength(40); entity.Property(x => x.RecipientAddress).HasMaxLength(300).IsRequired();
            entity.HasOne(x => x.Sale).WithOne(x => x.Shipment).HasForeignKey<Shipment>(x => x.SaleId);
            entity.HasOne(x => x.PostalService).WithMany().HasForeignKey(x => x.PostalServiceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Destination).WithMany().HasForeignKey(x => x.DestinationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.SupplementaryServices).WithOne().HasForeignKey(x => x.ShipmentId);
        });
        modelBuilder.Entity<ShipmentSupplementaryService>(entity =>
        {
            entity.ToTable("ShipmentSupplementaryServices"); entity.HasKey(x => x.Id); entity.Property(x => x.Description).HasMaxLength(140).IsRequired(); entity.Property(x => x.Price).HasPrecision(12, 2);
            entity.HasOne<SupplementaryService>().WithMany().HasForeignKey(x => x.SupplementaryServiceId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<TariffSupplementaryOption>(entity =>
        {
            entity.ToTable("TariffSupplementaryOptions"); entity.HasKey(x => new { x.LegacyTariffId, x.SupplementaryServiceId });
            entity.Property(x => x.PriceAdjustment).HasPrecision(12, 2);
            entity.HasOne(x => x.SupplementaryService).WithMany().HasForeignKey(x => x.SupplementaryServiceId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
