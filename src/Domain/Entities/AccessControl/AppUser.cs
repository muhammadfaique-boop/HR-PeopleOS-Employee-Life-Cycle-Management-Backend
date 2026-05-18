namespace PeopleOS.Api.Domain.Entities;

public class AppUser
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "";
    public int EmployeeId { get; set; }
}
