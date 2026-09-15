namespace CotelNet.Domain.Users;

public sealed class RolePermission
{
    private RolePermission() { }

    public RolePermission(int roleId, int permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }

    public int RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    public int PermissionId { get; private set; }
    public Permission Permission { get; private set; } = null!;
}

