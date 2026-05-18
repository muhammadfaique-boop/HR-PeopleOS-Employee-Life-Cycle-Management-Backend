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
        var approvedLeaves = await db.LeaveRequests
            .Where(x => x.Status == "Approved" && x.ToDate >= DateOnly.FromDateTime(DateTime.Today))
            .OrderBy(x => x.FromDate)
            .ToListAsync();

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
            whoIsOut = approvedLeaves.Select(leave =>
            {
                var employee = employees.FirstOrDefault(x => x.Id == leave.EmployeeId);
                return new
                {
                    employeeName = employee?.FullName ?? "Employee",
                    leaveType = leave.LeaveType,
                    fromDate = leave.FromDate,
                    toDate = leave.ToDate,
                    department = employee?.Department ?? "Unassigned"
                };
            }),
            holidays = new[]
            {
                new { name = "Eid Holiday", date = new DateOnly(2026, 5, 27), type = "Public Holiday" },
                new { name = "Company Wellness Day", date = new DateOnly(2026, 6, 7), type = "Company Holiday" },
                new { name = "Independence Day", date = new DateOnly(2026, 8, 14), type = "Public Holiday" }
            },
            announcements = new[]
            {
                new { title = "Policy refresh", body = "Attendance and leave policy updates are available in Policies.", publishedOn = new DateOnly(2026, 5, 18), audience = "All employees" },
                new { title = "Probation cycle", body = "Managers should complete open probation reviews before due dates.", publishedOn = new DateOnly(2026, 5, 16), audience = "Managers" },
                new { title = "Document cleanup", body = "Please upload missing employment records from the Profile section.", publishedOn = new DateOnly(2026, 5, 14), audience = "Employees" }
            },
            quickActions = new[]
            {
                new { label = "Apply Leave", target = "leave" },
                new { label = "Correct Attendance", target = "attendance" },
                new { label = "Submit Expense", target = "expense" },
                new { label = "Open Policies", target = "policies" }
            },
            lifecycleSignals = new[]
            {
                new { label = "Open onboarding tasks", value = "2", status = "In progress" },
                new { label = "Documents pending", value = "1", status = "Needs attention" },
                new { label = "Probation reviews due", value = "1", status = "Pending" }
            },
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
            var employee = await db.Employees.FindAsync(employeeId);
            var title = $"PeopleOS Attendance Log - {employee?.FullName ?? "Employee"}";
            var body = title + "\n\n" + content.Replace(",", "    ");
            var pdf = BuildSimplePdf(body);
            return File(pdf, "application/pdf", $"Login_UserId_{employeeId}.Attendance log.pdf");
        }

        return File(Encoding.UTF8.GetBytes(content), "text/csv", $"Login_UserId_{employeeId}.Attendance log.csv");
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
            AttachmentFileName = request.AttachmentFileName ?? "",
            AttachmentDataUrl = request.AttachmentDataUrl ?? "",
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
            ReceiptFileName = request.ReceiptFileName ?? "",
            ReceiptDataUrl = request.ReceiptDataUrl ?? "",
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

    private static byte[] BuildSimplePdf(string text)
    {
        static string EscapePdf(string value) => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

        var lines = text.Replace("\r", "").Split('\n').Take(34).ToList();
        var contentBuilder = new StringBuilder("BT\n/F1 11 Tf\n50 780 Td\n14 TL\n");
        foreach (var line in lines)
        {
            contentBuilder.Append('(').Append(EscapePdf(line.Length > 95 ? line[..95] : line)).Append(") Tj\nT*\n");
        }
        contentBuilder.Append("ET");

        var stream = contentBuilder.ToString();
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}\nendstream"
        };

        var pdf = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int> { 0 };
        foreach (var item in objects.Select((value, index) => new { value, index }))
        {
            offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
            pdf.Append(item.index + 1).Append(" 0 obj\n").Append(item.value).Append("\nendobj\n");
        }

        var xrefOffset = Encoding.ASCII.GetByteCount(pdf.ToString());
        pdf.Append("xref\n0 ").Append(objects.Length + 1).Append("\n0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
        {
            pdf.Append(offset.ToString("D10")).Append(" 00000 n \n");
        }
        pdf.Append("trailer\n<< /Size ").Append(objects.Length + 1).Append(" /Root 1 0 R >>\nstartxref\n")
            .Append(xrefOffset).Append("\n%%EOF");

        return Encoding.ASCII.GetBytes(pdf.ToString());
    }
}

public record AttendanceCorrectionRequest(int EmployeeId, DateOnly WorkDate, string RequestedChange, string Reason);
public record LeaveRequestDto(int EmployeeId, string LeaveType, DateOnly FromDate, DateOnly ToDate, string Reason, string ContactDuringLeave, string? AttachmentFileName, string? AttachmentDataUrl);
public record ExpenseClaimDto(int EmployeeId, string ClaimType, string Category, decimal Amount, DateOnly ExpenseDate, string Description, string? ReceiptFileName, string? ReceiptDataUrl);
public record ResignationDto(int EmployeeId, DateOnly LastWorkingDate, string Reason);
public record ProfileUpdateDto(string PreferredLanguage, string ProfileImageUrl);
