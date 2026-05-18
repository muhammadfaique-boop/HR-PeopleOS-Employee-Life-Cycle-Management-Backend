namespace PeopleOS.Api.Domain.Entities;

public class AttendanceCorrection
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public string RequestedChange { get; set; } = "";
    public string Reason { get; set; } = "";
    public string Status { get; set; } = "";
    public string Approver { get; set; } = "";
}
