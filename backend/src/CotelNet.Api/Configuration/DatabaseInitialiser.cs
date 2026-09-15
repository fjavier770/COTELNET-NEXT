using CotelNet.Application.Abstractions;
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

        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        if (await users.AnyAsync(CancellationToken.None))
            return;

        var username = app.Configuration["Bootstrap:AdminUser"];
        var password = app.Configuration["Bootstrap:AdminPassword"];
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return;

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var result = hasher.Hash(password);
        await users.AddAsync(
            new User(username, "Administrador COTELNET", result.Hash, result.Salt, result.Iterations, "Administrador"),
            CancellationToken.None);
        await users.SaveChangesAsync(CancellationToken.None);
    }
}

