using CotelNet.Domain.Estafetas;
using CotelNet.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CotelNet.Infrastructure.Persistence;

public sealed class CotelNetDbContext(DbContextOptions<CotelNetDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Estafeta> Estafetas => Set<Estafeta>();

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
            entity.Property(x => x.Role).HasMaxLength(80).IsRequired();
        });

        modelBuilder.Entity<Estafeta>(entity =>
        {
            entity.ToTable("Estafetas");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Codigo).IsUnique();
            entity.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        });
    }
}

