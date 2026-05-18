namespace PeopleOS.Api.Domain.Entities;

public class ApprovalTask
{
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Requester { get; set; } = "";
    public string ApproverRole { get; set; } = "";
    public string Status { get; set; } = "";
    public DateOnly DueDate { get; set; }
}
