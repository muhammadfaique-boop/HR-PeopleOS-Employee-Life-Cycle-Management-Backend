namespace PeopleOS.Api.Domain.Entities;

public class EmployeeDocument
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Status { get; set; } = "";
    public DateOnly UpdatedOn { get; set; }
}
