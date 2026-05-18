namespace PeopleOS.Api.Models;

public class AppUser
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "";
    public int EmployeeId { get; set; }
}

public class Employee
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Department { get; set; } = "";
    public string Position { get; set; } = "";
    public string Manager { get; set; } = "";
    public string LifecycleStatus { get; set; } = "";
    public DateOnly JoiningDate { get; set; }
    public int ProfileCompletion { get; set; }
    public string WorkLocation { get; set; } = "";
}

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

public class AttendanceRecord
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public TimeOnly? CheckIn { get; set; }
    public TimeOnly? CheckOut { get; set; }
    public string Status { get; set; } = "";
    public string Source { get; set; } = "";
}

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

public class LeaveBalance
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string LeaveType { get; set; } = "";
    public decimal AnnualEntitlement { get; set; }
    public decimal AvailableBalance { get; set; }
}

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
    public string Status { get; set; } = "";
}

public class BenefitPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Coverage { get; set; } = "";
    public string Status { get; set; } = "";
}

public class EmployeeDocument
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Status { get; set; } = "";
    public DateOnly UpdatedOn { get; set; }
}

public class PolicyDocument
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string Version { get; set; } = "";
    public DateOnly PublishedOn { get; set; }
}

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
