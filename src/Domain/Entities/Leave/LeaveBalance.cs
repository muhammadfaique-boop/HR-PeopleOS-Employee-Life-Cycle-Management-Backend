namespace PeopleOS.Api.Domain.Entities;

public class LeaveBalance
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string LeaveType { get; set; } = "";
    public decimal AnnualEntitlement { get; set; }
    public decimal AvailableBalance { get; set; }
}
