using CotelNet.Domain.Common;

namespace CotelNet.Domain.Users;

public sealed class User : Entity
{
    private User() { }

    public User(string username, string fullName, string passwordHash, string passwordSalt, int iterations, string role)
    {
        Username = username.Trim().ToLowerInvariant();
        FullName = fullName.Trim();
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        PasswordIterations = iterations;
        Role = role.Trim();
        Active = true;
    }

    public string Username { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string PasswordSalt { get; private set; } = string.Empty;
    public int PasswordIterations { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public bool Active { get; private set; }
    public int? EstafetaId { get; private set; }

    public void AssignEstafeta(int? estafetaId)
    {
        EstafetaId = estafetaId;
        MarkUpdated();
    }
}

