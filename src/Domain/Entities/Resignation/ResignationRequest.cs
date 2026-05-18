namespace PeopleOS.Api.Domain.Entities;

public class ResignationRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly ResignationDate { get; set; }
    public DateOnly LastWorkingDate { get; set; }
    public string Reason { get; set; } = "";
    public string Status { get; set; } = "";
    public string LineManager { get; set; } = "";
}
