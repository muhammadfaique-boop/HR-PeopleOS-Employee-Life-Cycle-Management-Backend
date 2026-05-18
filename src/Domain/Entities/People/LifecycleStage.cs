namespace PeopleOS.Api.Domain.Entities;

public class LifecycleStage
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Stage { get; set; } = "";
    public string Owner { get; set; } = "";
    public string Status { get; set; } = "";
    public DateOnly DueDate { get; set; }
    public string Summary { get; set; } = "";
}
