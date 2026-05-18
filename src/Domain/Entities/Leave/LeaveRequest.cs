namespace PeopleOS.Api.Domain.Entities;

public class LeaveRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string LeaveType { get; set; } = "";
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public decimal TotalDays { get; set; }
    public string Reason { get; set; } = "";
    public string ContactDuringLeave { get; set; } = "";
    public string AttachmentFileName { get; set; } = "";
    public string AttachmentDataUrl { get; set; } = "";
    public string Status { get; set; } = "";
}
