using CotelNet.Domain.Common;

namespace CotelNet.Domain.Users;

public sealed class Role : Entity
{
    private Role() { }

    public Role(string name, string description)
    {
        Name = name.Trim();
        Description = description.Trim();
        Active = true;
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool Active { get; private set; }
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    public void Update(string name, string description, bool active)
    {
        Name = name.Trim();
        Description = description.Trim();
        Active = active;
        MarkUpdated();
    }
}

