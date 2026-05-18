using System.Text;
using PeopleOS.Api.Models;
using PeopleOS.Api.Models.Dtos;
using PeopleOS.Api.Repositories;

namespace PeopleOS.Api.Services;

public interface IAuthService
{
    Task<object?> LoginAsync(LoginRequest request);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
}

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync();
}

public interface IEmployeeService
{
    Task<List<Employee>> GetEmployeesAsync();
    Task<EmployeeProfileDto?> GetProfileAsync(int employeeId);
    Task<Employee?> UpdateProfileAsync(int employeeId, ProfileUpdateDto request);
}

public interface IAttendanceService
{
    Task<AttendanceDto> GetAttendanceAsync(int employeeId);
    Task<AttendanceCorrection?> CreateCorrectionAsync(AttendanceCorrectionRequest request);
    Task<AttendanceDownload> DownloadAsync(string format, int employeeId);
}

public interface ILeaveService
{
    Task<LeaveDto> GetLeaveAsync(int employeeId);
    Task<LeaveRequest?> CreateRequestAsync(LeaveRequestDto request);
}

public interface IBenefitsService
{
    Task<List<BenefitPlan>> GetBenefitsAsync();
}

public interface IExpenseService
{
    Task<List<ExpenseClaim>> GetClaimsAsync(int employeeId);
    Task<ExpenseClaim?> CreateClaimAsync(ExpenseClaimDto request);
}

public interface IResignationService
{
    Task<List<ResignationRequest>> GetResignationsAsync(int employeeId);
    Task<ResignationRequest?> CreateResignationAsync(ResignationDto request);
}

public interface IPolicyService
{
    Task<List<PolicyDocument>> GetPoliciesAsync();
}

public interface IApprovalService
{
    Task<List<ApprovalTask>> GetApprovalsAsync();
}

public class AuthService(IAuthRepository authRepository, IEmployeeRepository employeeRepository) : IAuthService
{
    public async Task<object?> LoginAsync(LoginRequest request)
    {
        var user = await authRepository.FindByCredentialsAsync(request.Email, request.Password);
        if (user is null)
        {
            return null;
        }

        var employee = await employeeRepository.GetByIdAsync(user.EmployeeId);
        return new
        {
            token = $"demo-token-{user.Id}",
            user.Email,
            user.Role,
            employee
        };
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var user = await authRepository.FindByEmailAsync(request.Email);
        if (user is null || user.Password != request.CurrentPassword)
        {
            return false;
        }

        user.Password = request.NewPassword;
        await authRepository.SaveChangesAsync();
        return true;
    }
}

public class DashboardService(
    IEmployeeRepository employees,
    ILifecycleRepository lifecycle,
    IApprovalRepository approvals,
    ILeaveRepository leaves,
    IReferenceDataRepository referenceData) : IDashboardService
{
    public async Task<DashboardDto> GetDashboardAsync()
    {
        var employeeList = await employees.GetAllAsync();
        var activeEmployee = await employees.GetByEmailAsync("muhammad.faique@peopleos.dev") ?? employeeList.First();
        var pendingTasks = await approvals.GetPendingAsync();
        var approvedLeaves = await leaves.GetApprovedUpcomingAsync(DateOnly.FromDateTime(DateTime.Today));

        var whoIsOut = approvedLeaves.Select(leave =>
        {
            var employee = employeeList.FirstOrDefault(x => x.Id == leave.EmployeeId);
            return new WhoIsOutItem(
                employee?.FullName ?? "Employee",
                leave.LeaveType,
                leave.FromDate,
                leave.ToDate,
                employee?.Department ?? "Unassigned");
        }).ToList();

        return new DashboardDto(
            new[]
            {
                new DashboardMetric("Active employees", employeeList.Count(x => x.LifecycleStatus == "Active").ToString(), "teal"),
                new DashboardMetric("On probation", employeeList.Count(x => x.LifecycleStatus == "Probation").ToString(), "amber"),
                new DashboardMetric("Pending approvals", pendingTasks.Count.ToString(), "violet"),
                new DashboardMetric("Profile completion", $"{activeEmployee.ProfileCompletion}%", "rose")
            },
            activeEmployee,
            employeeList,
            await lifecycle.GetAllAsync(),
            pendingTasks,
            whoIsOut,
            await referenceData.GetHolidaysAsync(),
            await referenceData.GetAnnouncementsAsync(),
            await referenceData.GetQuickActionsAsync(),
            await referenceData.GetLifecycleSignalsAsync(),
            (await referenceData.GetRecentActivityAsync()).Select(x => x.Message).ToList());
    }
}

public class EmployeeService(
    IEmployeeRepository employees,
    ILifecycleRepository lifecycle,
    IDocumentRepository documents,
    ILeaveRepository leaves) : IEmployeeService
{
    public Task<List<Employee>> GetEmployeesAsync() => employees.GetAllAsync();

    public async Task<EmployeeProfileDto?> GetProfileAsync(int employeeId)
    {
        var employee = await employees.GetByIdAsync(employeeId);
        if (employee is null)
        {
            return null;
        }

        return new EmployeeProfileDto(
            employee,
            await documents.GetByEmployeeAsync(employeeId),
            await lifecycle.GetByEmployeeAsync(employeeId),
            await leaves.GetBalancesAsync(employeeId));
    }

    public async Task<Employee?> UpdateProfileAsync(int employeeId, ProfileUpdateDto request)
    {
        var employee = await employees.GetByIdAsync(employeeId);
        if (employee is null)
        {
            return null;
        }

        employee.PreferredLanguage = request.PreferredLanguage;
        employee.ProfileImageUrl = request.ProfileImageUrl;
        await employees.SaveChangesAsync();
        return employee;
    }
}

public class AttendanceService(
    IAttendanceRepository attendance,
    IEmployeeRepository employees,
    IApprovalRepository approvals,
    IUnitOfWork unitOfWork) : IAttendanceService
{
    public async Task<AttendanceDto> GetAttendanceAsync(int employeeId) =>
        new(await attendance.GetRecordsAsync(employeeId), await attendance.GetCorrectionsAsync(employeeId));

    public async Task<AttendanceCorrection?> CreateCorrectionAsync(AttendanceCorrectionRequest request)
    {
        var employee = await employees.GetByIdAsync(request.EmployeeId);
        if (employee is null)
        {
            return null;
        }

        var correction = new AttendanceCorrection
        {
            Id = await attendance.NextCorrectionIdAsync(),
            EmployeeId = request.EmployeeId,
            WorkDate = request.WorkDate,
            RequestedChange = request.RequestedChange,
            Reason = request.Reason,
            Status = "Pending line manager",
            Approver = employee.Manager
        };

        await attendance.AddCorrectionAsync(correction);
        await approvals.AddAsync(new ApprovalTask
        {
            Id = await approvals.NextIdAsync(),
            Type = "Attendance Correction",
            Subject = $"{employee.FullName} - {request.WorkDate:MMM dd, yyyy}",
            Requester = employee.FullName,
            ApproverRole = $"Line Manager: {employee.Manager}",
            Status = "Pending",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2))
        });
        await unitOfWork.SaveChangesAsync();

        return correction;
    }

    public async Task<AttendanceDownload> DownloadAsync(string format, int employeeId)
    {
        var records = await attendance.GetRecordsAsync(employeeId);
        var lines = records.Select(x => $"{x.WorkDate:yyyy-MM-dd},{x.CheckIn},{x.CheckOut},{x.Status},{x.Source}");
        var content = "Date,Check In,Check Out,Status,Source\r\n" + string.Join("\r\n", lines);

        if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
        {
            var employee = await employees.GetByIdAsync(employeeId);
            var title = $"PeopleOS Attendance Log - {employee?.FullName ?? "Employee"}";
            var body = title + "\n\n" + content.Replace(",", "    ");
            return new AttendanceDownload($"Login_UserId_{employeeId}.Attendance log.pdf", "application/pdf", PdfBuilder.Build(body));
        }

        return new AttendanceDownload($"Login_UserId_{employeeId}.Attendance log.csv", "text/csv", Encoding.UTF8.GetBytes(content));
    }
}

public class LeaveService(
    ILeaveRepository leaves,
    IEmployeeRepository employees,
    IApprovalRepository approvals,
    IUnitOfWork unitOfWork) : ILeaveService
{
    public async Task<LeaveDto> GetLeaveAsync(int employeeId) =>
        new(await leaves.GetBalancesAsync(employeeId), await leaves.GetRequestsAsync(employeeId));

    public async Task<LeaveRequest?> CreateRequestAsync(LeaveRequestDto request)
    {
        var employee = await employees.GetByIdAsync(request.EmployeeId);
        if (employee is null)
        {
            return null;
        }

        var leave = new LeaveRequest
        {
            Id = await leaves.NextRequestIdAsync(),
            EmployeeId = request.EmployeeId,
            LeaveType = request.LeaveType,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            TotalDays = Math.Max(1, request.ToDate.DayNumber - request.FromDate.DayNumber + 1),
            Reason = request.Reason,
            ContactDuringLeave = request.ContactDuringLeave,
            AttachmentFileName = request.AttachmentFileName ?? "",
            AttachmentDataUrl = request.AttachmentDataUrl ?? "",
            Status = "Pending line manager"
        };

        await leaves.AddRequestAsync(leave);
        await approvals.AddAsync(new ApprovalTask
        {
            Id = await approvals.NextIdAsync(),
            Type = "Leave",
            Subject = $"{request.LeaveType} - {employee.FullName}",
            Requester = employee.FullName,
            ApproverRole = $"Line Manager: {employee.Manager}",
            Status = "Pending",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        });
        await unitOfWork.SaveChangesAsync();

        return leave;
    }
}

public class BenefitsService(IBenefitsRepository benefits) : IBenefitsService
{
    public Task<List<BenefitPlan>> GetBenefitsAsync() => benefits.GetAllAsync();
}

public class ExpenseService(IExpenseRepository expenses, IEmployeeRepository employees, IApprovalRepository approvals, IUnitOfWork unitOfWork) : IExpenseService
{
    public Task<List<ExpenseClaim>> GetClaimsAsync(int employeeId) => expenses.GetByEmployeeAsync(employeeId);

    public async Task<ExpenseClaim?> CreateClaimAsync(ExpenseClaimDto request)
    {
        var employee = await employees.GetByIdAsync(request.EmployeeId);
        if (employee is null)
        {
            return null;
        }

        var claim = new ExpenseClaim
        {
            Id = await expenses.NextIdAsync(),
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

        await expenses.AddAsync(claim);
        await approvals.AddAsync(new ApprovalTask
        {
            Id = await approvals.NextIdAsync(),
            Type = "Expense",
            Subject = $"{request.ClaimType} - {employee.FullName}",
            Requester = employee.FullName,
            ApproverRole = $"Line Manager: {employee.Manager}",
            Status = "Pending",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3))
        });
        await unitOfWork.SaveChangesAsync();
        return claim;
    }
}

public class ResignationService(IResignationRepository resignations, IEmployeeRepository employees, IApprovalRepository approvals, IUnitOfWork unitOfWork) : IResignationService
{
    public Task<List<ResignationRequest>> GetResignationsAsync(int employeeId) => resignations.GetByEmployeeAsync(employeeId);

    public async Task<ResignationRequest?> CreateResignationAsync(ResignationDto request)
    {
        var employee = await employees.GetByIdAsync(request.EmployeeId);
        if (employee is null)
        {
            return null;
        }

        var resignation = new ResignationRequest
        {
            Id = await resignations.NextIdAsync(),
            EmployeeId = request.EmployeeId,
            ResignationDate = DateOnly.FromDateTime(DateTime.Today),
            LastWorkingDate = request.LastWorkingDate,
            Reason = request.Reason,
            Status = "Pending line manager",
            LineManager = employee.Manager
        };

        await resignations.AddAsync(resignation);
        await approvals.AddAsync(new ApprovalTask
        {
            Id = await approvals.NextIdAsync(),
            Type = "Resignation",
            Subject = $"Resignation request - {employee.FullName}",
            Requester = employee.FullName,
            ApproverRole = $"Line Manager: {employee.Manager}",
            Status = "Pending",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2))
        });
        await unitOfWork.SaveChangesAsync();
        return resignation;
    }
}

public class PolicyService(IPolicyRepository policies) : IPolicyService
{
    public Task<List<PolicyDocument>> GetPoliciesAsync() => policies.GetAllAsync();
}

public class ApprovalService(IApprovalRepository approvals) : IApprovalService
{
    public Task<List<ApprovalTask>> GetApprovalsAsync() => approvals.GetAllAsync();
}

public static class PdfBuilder
{
    public static byte[] Build(string text)
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
