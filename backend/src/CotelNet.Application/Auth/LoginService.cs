using CotelNet.Application.Abstractions;

namespace CotelNet.Application.Auth;

public sealed class LoginService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtTokenService tokenService)
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await users.FindByUsernameAsync(request.Username.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || !user.Active)
            return null;

        var valid = passwordHasher.Verify(
            request.Password,
            user.PasswordHash,
            user.PasswordSalt,
            user.PasswordIterations);

        if (!valid)
            return null;

        var token = tokenService.Create(user);
        return new LoginResponse(
            token.AccessToken,
            token.ExpiresAtUtc,
            user.Username,
            user.FullName,
            user.Role,
            user.EstafetaId);
    }
}

public sealed record LoginRequest(string Username, string Password);

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string Username,
    string FullName,
    string Role,
    int? EstafetaId);

