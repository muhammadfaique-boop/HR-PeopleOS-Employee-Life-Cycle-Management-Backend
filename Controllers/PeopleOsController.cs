using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using PeopleOS.Api.Data;
using PeopleOS.Api.Models;

namespace PeopleOS.Api.Controllers;

[ApiController]
[Route("api/peopleos")]
public class PeopleOsController(PeopleOsDbContext db) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> Dashboard()
    {
        var employees = await db.Employees.OrderBy(x => x.FullName).ToListAsync();
        var activeEmployee = employees.First(x => x.Email == "muhammad.faique@peopleos.dev");
        var pendingTasks = await db.ApprovalTasks.Where(x => x.Status != "Approved").ToListAsync();

        return Ok(new
        {
            metrics = new[]
            {
                new { label = "Active employees", value = employees.Count(x => x.LifecycleStatus == "Active").ToString(), accent = "teal" },
                new { label = "On probation", value = employees.Count(x => x.LifecycleStatus == "Probation").ToString(), accent = "amber" },
                new { label = "Pending approvals", value = pendingTasks.Count.ToString(), accent = "violet" },
                new { label = "Profile completion", value = $"{activeEmployee.ProfileCompletion}%", accent = "rose" }
            },
            activeEmployee,
            employees,
            lifecycle = await db.LifecycleStages.OrderBy(x => x.DueDate).ToListAsync(),
            approvals = pendingTasks.OrderBy(x => x.DueDate),
            recentActivity = new[]
            {
                "Leave request moved to manager approval",
                "Probation review opened for Bilal Raza",
                "Employment letter document marked ready",
                "Attendance sync completed for today"
            }
        });
    }

    [HttpGet("employees")]
    public async Task<ActionResult<List<Employee>>> Employees() =>
        await db.Employees.OrderBy(x => x.Department).ThenBy(x => x.FullName).ToListAsync();

    [HttpGet("employees/{id:int}")]
    public async Task<ActionResult<object>> EmployeeProfile(int id)
    {
        var employee = await db.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            employee,
            documents = await db.Documents.Where(x => x.EmployeeId == id).ToListAsync(),
            lifecycle = await db.LifecycleStages.Where(x => x.EmployeeId == id).ToListAsync(),
            leaveBalances = await db.LeaveBalances.Where(x => x.EmployeeId == id).ToListAsync()
        });
    }

    [HttpGet("attendance")]
    public async Task<ActionResult<object>> Attendance(int employeeId = 2)
    {
        return Ok(new
        {
            records = await db.AttendanceRecords.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.WorkDate).ToListAsync(),
            corrections = await db.AttendanceCorrections.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.WorkDate).ToListAsync()
        });
    }

    [HttpPost("attendance/corrections")]
    public async Task<ActionResult<AttendanceCorrection>> CreateAttendanceCorrection(AttendanceCorrectionRequest request)
    {
        var employee = await db.Employees.FindAsync(request.EmployeeId);
        if (employee is null)
        {
            return NotFound();
        }

        var correction = new AttendanceCorrection
        {
            Id = NextId(await db.AttendanceCorrections.Select(x => x.Id).ToListAsync()),
            EmployeeId = request.EmployeeId,
            WorkDate = request.WorkDate,
            RequestedChange = request.RequestedChange,
            Reason = request.Reason,
            Status = "Pending line manager",
            Approver = employee.Manager
        };

        db.AttendanceCorrections.Add(correction);
        db.ApprovalTasks.Add(new ApprovalTask
        {
            Id = await NextApprovalId(),
            Type = "Attendance Correction",
            Subject = $"{employee.FullName} - {request.WorkDate:MMM dd, yyyy}",
            Requester = employee.FullName,
            ApproverRole = $"Line Manager: {employee.Manager}",
            Status = "Pending",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2))
        });
        await db.SaveChangesAsync();

        return Ok(correction);
    }

    [HttpGet("attendance/download/{format}")]
    public async Task<IActionResult> DownloadAttendance(string format, int employeeId = 2)
    {
        var records = await db.AttendanceRecords.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.WorkDate).ToListAsync();
        var lines = records.Select(x => $"{x.WorkDate:yyyy-MM-dd},{x.CheckIn},{x.CheckOut},{x.Status},{x.Source}");
        var content = "Date,Check In,Check Out,Status,Source\r\n" + string.Join("\r\n", lines);

        if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
        {
            var pdfLike = Encoding.UTF8.GetBytes($"PeopleOS Attendance Log\r\n\r\n{content}");
            return File(pdfLike, "application/pdf", "attendance-log.pdf");
        }

        return File(Encoding.UTF8.GetBytes(content), "text/csv", "attendance-log.csv");
    }

    [HttpGet("leave")]
    public async Task<ActionResult<object>> Leave(int employeeId = 2)
    {
        return Ok(new
        {
            balances = await db.LeaveBalances.Where(x => x.EmployeeId == employeeId).ToListAsync(),
            requests = await db.LeaveRequests.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.FromDate).ToListAsync()
        });
    }

    [HttpPost("leave/requests")]
    public async Task<ActionResult<LeaveRequest>> CreateLeaveRequest(LeaveRequestDto request)
    {
        var employee = await db.Employees.FindAsync(request.EmployeeId);
        if (employee is null)
        {
            return NotFound();
        }

        var totalDays = Math.Max(1, request.ToDate.DayNumber - request.FromDate.DayNumber + 1);
        var leave = new LeaveRequest
        {
            Id = NextId(await db.LeaveRequests.Select(x => x.Id).ToListAsync()),
            EmployeeId = request.EmployeeId,
            LeaveType = request.LeaveType,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            TotalDays = totalDays,
            Reason = request.Reason,
            ContactDuringLeave = request.ContactDuringLeave,
            Status = "Pending line manager"
        };

        db.LeaveRequests.Add(leave);
        db.ApprovalTasks.Add(new ApprovalTask
        {
            Id = await NextApprovalId(),
            Type = "Leave",
            Subject = $"{request.LeaveType} - {employee.FullName}",
            Requester = employee.FullName,
            ApproverRole = $"Line Manager: {employee.Manager}",
            Status = "Pending",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        });
        await db.SaveChangesAsync();

        return Ok(leave);
    }

    [HttpGet("benefits")]
    public async Task<ActionResult<List<BenefitPlan>>> Benefits() =>
        await db.BenefitPlans.OrderBy(x => x.Category).ToListAsync();

    [HttpGet("expense")]
    public async Task<ActionResult<List<ExpenseClaim>>> ExpenseClaims(int employeeId = 2) =>
        await db.ExpenseClaims.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.ExpenseDate).ToListAsync();

    [HttpPost("expense/claims")]
    public async Task<ActionResult<ExpenseClaim>> CreateExpenseClaim(ExpenseClaimDto request)
    {
        var employee = await db.Employees.FindAsync(request.EmployeeId);
        if (employee is null)
        {
            return NotFound();
        }

        var claim = new ExpenseClaim
        {
            Id = NextId(await db.ExpenseClaims.Select(x => x.Id).ToListAsync()),
            EmployeeId = request.EmployeeId,
            ClaimType = request.ClaimType,
            Category = request.Category,
            Amount = request.Amount,
            ExpenseDate = request.ExpenseDate,
            Description = request.Description,
            Status = "Pending line manager",
            LineManager = employee.Manager
        };

        db.ExpenseClaims.Add(claim);
        db.ApprovalTasks.Add(new ApprovalTask
        {
            Id = await NextApprovalId(),
            Type = "Expense",
            Subject = $"{request.ClaimType} - {employee.FullName}",
            Requester = employee.FullName,
            ApproverRole = $"Line Manager: {employee.Manager}",
            Status = "Pending",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3))
        });
        await db.SaveChangesAsync();

        return Ok(claim);
    }

    [HttpGet("resignations")]
    public async Task<ActionResult<List<ResignationRequest>>> Resignations(int employeeId = 2) =>
        await db.ResignationRequests.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.ResignationDate).ToListAsync();

    [HttpPost("resignations")]
    public async Task<ActionResult<ResignationRequest>> CreateResignation(ResignationDto request)
    {
        var employee = await db.Employees.FindAsync(request.EmployeeId);
        if (employee is null)
        {
            return NotFound();
        }

        var resignation = new ResignationRequest
        {
            Id = NextId(await db.ResignationRequests.Select(x => x.Id).ToListAsync()),
            EmployeeId = request.EmployeeId,
            ResignationDate = DateOnly.FromDateTime(DateTime.Today),
            LastWorkingDate = request.LastWorkingDate,
            Reason = request.Reason,
            Status = "Pending line manager",
            LineManager = employee.Manager
        };

        db.ResignationRequests.Add(resignation);
        db.ApprovalTasks.Add(new ApprovalTask
        {
            Id = await NextApprovalId(),
            Type = "Resignation",
            Subject = $"Resignation request - {employee.FullName}",
            Requester = employee.FullName,
            ApproverRole = $"Line Manager: {employee.Manager}",
            Status = "Pending",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2))
        });
        await db.SaveChangesAsync();

        return Ok(resignation);
    }

    [HttpPatch("employees/{id:int}/profile")]
    public async Task<ActionResult<Employee>> UpdateProfile(int id, ProfileUpdateDto request)
    {
        var employee = await db.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        employee.PreferredLanguage = request.PreferredLanguage;
        employee.ProfileImageUrl = request.ProfileImageUrl;
        await db.SaveChangesAsync();

        return Ok(employee);
    }

    [HttpGet("policies")]
    public async Task<ActionResult<List<PolicyDocument>>> Policies() =>
        await db.Policies.OrderByDescending(x => x.PublishedOn).ToListAsync();

    [HttpGet("approvals")]
    public async Task<ActionResult<List<ApprovalTask>>> Approvals() =>
        await db.ApprovalTasks.OrderBy(x => x.DueDate).ToListAsync();

    private async Task<int> NextApprovalId() => NextId(await db.ApprovalTasks.Select(x => x.Id).ToListAsync());

    private static int NextId(List<int> ids) => ids.Count == 0 ? 1 : ids.Max() + 1;
}

public record AttendanceCorrectionRequest(int EmployeeId, DateOnly WorkDate, string RequestedChange, string Reason);
public record LeaveRequestDto(int EmployeeId, string LeaveType, DateOnly FromDate, DateOnly ToDate, string Reason, string ContactDuringLeave);
public record ExpenseClaimDto(int EmployeeId, string ClaimType, string Category, decimal Amount, DateOnly ExpenseDate, string Description);
public record ResignationDto(int EmployeeId, DateOnly LastWorkingDate, string Reason);
public record ProfileUpdateDto(string PreferredLanguage, string ProfileImageUrl);
