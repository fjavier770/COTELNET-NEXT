using CotelNet.Domain.Common;

namespace CotelNet.Domain.Users;

public sealed class User : Entity
{
    private User() { }

    public User(string username, string fullName, string passwordHash, string passwordSalt, int iterations, int roleId)
    {
        Username = username.Trim().ToLowerInvariant();
        FullName = fullName.Trim();
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        PasswordIterations = iterations;
        RoleId = roleId;
        Active = true;
    }

    public string Username { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string PasswordSalt { get; private set; } = string.Empty;
    public int PasswordIterations { get; private set; }
    public bool Active { get; private set; }
    public int? EstafetaId { get; private set; }
    public int RoleId { get; private set; }
    public Role Role { get; private set; } = null!;

    public void AssignEstafeta(int? estafetaId)
    {
        EstafetaId = estafetaId;
        MarkUpdated();
    }

    public void Update(string fullName, int roleId, int? estafetaId, bool active)
    {
        FullName = fullName.Trim();
        RoleId = roleId;
        EstafetaId = estafetaId;
        Active = active;
        MarkUpdated();
    }
}

