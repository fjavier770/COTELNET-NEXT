namespace CotelNet.Application.Abstractions;

public interface IPasswordHasher
{
    PasswordHashResult Hash(string password);
    bool Verify(string password, string hash, string salt, int iterations);
}

public sealed record PasswordHashResult(string Hash, string Salt, int Iterations);

