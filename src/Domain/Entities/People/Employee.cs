namespace PeopleOS.Api.Domain.Entities;

public class Employee
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Department { get; set; } = "";
    public string Position { get; set; } = "";
    public string Manager { get; set; } = "";
    public int? ManagerEmployeeId { get; set; }
    public string LifecycleStatus { get; set; } = "";
    public DateOnly JoiningDate { get; set; }
    public int ProfileCompletion { get; set; }
    public string WorkLocation { get; set; } = "";
    public string PreferredLanguage { get; set; } = "English";
    public string ProfileImageUrl { get; set; } = "";
}
