namespace PeopleOS.Api.Domain.Entities;

public class EmployeeNotification
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string Tone { get; set; } = "";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
