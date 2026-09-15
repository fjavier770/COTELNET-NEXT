using CotelNet.Domain.Users;

namespace CotelNet.Application.Abstractions;

public interface IJwtTokenService
{
    TokenResult Create(User user);
}

public sealed record TokenResult(string AccessToken, DateTime ExpiresAtUtc);

