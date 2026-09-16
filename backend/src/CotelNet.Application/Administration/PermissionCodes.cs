namespace CotelNet.Application.Administration;

public static class PermissionCodes
{
    public const string DashboardView = "dashboard.view";
    public const string UsersManage = "users.manage";
    public const string RolesManage = "roles.manage";
    public const string EstafetasManage = "estafetas.manage";
    public const string TerminalsManage = "terminals.manage";
    public const string ServicesManage = "services.manage";
    public const string VentasAccess = "ventas.access";
    public const string EnviosAccess = "envios.access";
    public const string CajaAccess = "caja.access";
    public const string InventarioAccess = "inventario.access";
    public const string ApartadosAccess = "apartados.access";
    public const string ReportesAccess = "reportes.access";

    public static readonly string[] All =
    [
        DashboardView, UsersManage, RolesManage, EstafetasManage, TerminalsManage, ServicesManage,
        VentasAccess, EnviosAccess, CajaAccess, InventarioAccess, ApartadosAccess, ReportesAccess
    ];
}
