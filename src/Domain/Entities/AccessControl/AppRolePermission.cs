namespace PeopleOS.Api.Domain.Entities;

public class AppRolePermission
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public string Scope { get; set; } = "";
}
