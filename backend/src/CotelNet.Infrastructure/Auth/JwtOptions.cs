namespace CotelNet.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public required string Secret { get; init; }
    public string Issuer { get; init; } = "CotelNet.Api";
    public string Audience { get; init; } = "CotelNet.Frontend";
    public int ExpirationMinutes { get; init; } = 480;
}

