using System.Text;
using PeopleOS.Api.Application.DTOs;
using PeopleOS.Api.Application.Interfaces;
using PeopleOS.Api.Application.Mappers;
using PeopleOS.Api.Common.Utils;
using PeopleOS.Api.Domain.Entities;

namespace PeopleOS.Api.Application.Services;

public class AuthService(IAuthRepository authRepository, IEmployeeRepository employeeRepository) : IAuthService
{
    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await authRepository.FindByCredentialsAsync(request.Email, request.Password);
        if (user is null)
        {
            return null;
        }

        var employee = await employeeRepository.GetByIdAsync(user.EmployeeId);
        var roleName = NormalizeRole(user.Role);
        var role = await authRepository.GetRoleAsync(roleName);
        var permissions = await authRepository.GetRolePermissionsAsync(roleName);

        return new LoginResponseDto(
            $"demo-token-{user.Id}",
            user.Email,
            roleName,
            role?.DefaultScope ?? "own",
            permissions.Select(x => new PermissionGrantResponseDto(PermissionKeys[x.PermissionId], x.Scope)).ToList(),
            employee?.ToResponse());
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequestDto request)
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

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var user = await authRepository.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return false;
        }

        user.Password = request.NewPassword;
        await authRepository.SaveChangesAsync();
        return true;
    }

    private static string NormalizeRole(string role) => role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
        ? "Super Admin"
        : role;

    private static readonly IReadOnlyDictionary<int, string> PermissionKeys = new Dictionary<int, string>
    {
        [1] = "attendance.read",
        [2] = "attendance.create",
        [3] = "attendance.correct",
        [4] = "attendance.approve",
        [5] = "leave.read",
        [6] = "leave.create",
        [7] = "leave.approve",
        [8] = "employee.read",
        [9] = "employee.create",
        [10] = "employee.update",
        [11] = "benefit.read",
        [12] = "benefit.manage",
        [13] = "expense.read",
        [14] = "expense.create",
        [15] = "expense.approve",
        [16] = "resignation.read",
        [17] = "resignation.create",
        [18] = "resignation.approve",
        [19] = "policy.read",
        [20] = "policy.manage",
        [21] = "role.manage",
        [22] = "permission.manage",
        [23] = "audit.read",
        [24] = "report.read"
    };
}

public class DashboardService(
    IEmployeeRepository employees,
    ILifecycleRepository lifecycle,
    IApprovalRepository approvals,
    ILeaveRepository leaves,
    IReferenceDataRepository referenceData) : IDashboardService
{
    public async Task<DashboardResponseDto> GetDashboardAsync()
    {
        var employeeList = await employees.GetAllAsync();
        var activeEmployee = await employees.GetByEmailAsync("muhammad.faique@peopleos.dev") ?? employeeList.First();
        var pendingTasks = await approvals.GetPendingAsync();
        var approvedLeaves = await leaves.GetApprovedUpcomingAsync(DateOnly.FromDateTime(DateTime.Today));

        var whoIsOut = approvedLeaves.Select(leave =>
        {
            var employee = employeeList.FirstOrDefault(x => x.Id == leave.EmployeeId);
            return new WhoIsOutResponseDto(
                employee?.FullName ?? "Employee",
                leave.LeaveType,
                leave.FromDate,
                leave.ToDate,
                employee?.Department ?? "Unassigned");
        }).ToList();

        return new DashboardResponseDto(
            new[]
            {
                new DashboardMetricResponseDto("Active employees", employeeList.Count(x => x.LifecycleStatus == "Active").ToString(), "teal"),
                new DashboardMetricResponseDto("On probation", employeeList.Count(x => x.LifecycleStatus == "Probation").ToString(), "amber"),
                new DashboardMetricResponseDto("Pending approvals", pendingTasks.Count.ToString(), "violet"),
                new DashboardMetricResponseDto("Profile completion", $"{activeEmployee.ProfileCompletion}%", "rose")
            },
            activeEmployee.ToResponse(),
            employeeList.Select(x => x.ToResponse()).ToList(),
            (await lifecycle.GetAllAsync()).Select(x => x.ToResponse()).ToList(),
            pendingTasks.Select(x => x.ToResponse()).ToList(),
            whoIsOut,
            (await referenceData.GetHolidaysAsync()).Select(x => x.ToResponse()).ToList(),
            (await referenceData.GetAnnouncementsAsync()).Select(x => x.ToResponse()).ToList(),
            (await referenceData.GetQuickActionsAsync()).Select(x => x.ToResponse()).ToList(),
            (await referenceData.GetLifecycleSignalsAsync()).Select(x => x.ToResponse()).ToList(),
            (await referenceData.GetRecentActivityAsync()).Select(x => x.Message).ToList());
    }
}

public class EmployeeService(
    IEmployeeRepository employees,
    ILifecycleRepository lifecycle,
    IDocumentRepository documents,
    ILeaveRepository leaves) : IEmployeeService
{
    public async Task<List<EmployeeResponseDto>> GetEmployeesAsync() =>
        (await employees.GetAllAsync()).Select(x => x.ToResponse()).ToList();

    public async Task<EmployeeProfileResponseDto?> GetProfileAsync(int employeeId)
    {
        var employee = await employees.GetByIdAsync(employeeId);
        if (employee is null)
        {
            return null;
        }

        return new EmployeeProfileResponseDto(
            employee.ToResponse(),
            (await documents.GetByEmployeeAsync(employeeId)).Select(x => x.ToResponse()).ToList(),
            (await lifecycle.GetByEmployeeAsync(employeeId)).Select(x => x.ToResponse()).ToList(),
            (await leaves.GetBalancesAsync(employeeId)).Select(x => x.ToResponse()).ToList());
    }

    public async Task<EmployeeResponseDto?> UpdateProfileAsync(int employeeId, UpdateEmployeeProfileRequestDto request)
    {
        var employee = await employees.GetByIdAsync(employeeId);
        if (employee is null)
        {
            return null;
        }

        employee.PreferredLanguage = request.PreferredLanguage;
        employee.ProfileImageUrl = request.ProfileImageUrl;
        await employees.SaveChangesAsync();
        return employee.ToResponse();
    }
}

public class AttendanceService(
    IAttendanceRepository attendance,
    IEmployeeRepository employees,
    IApprovalRepository approvals,
    IUnitOfWork unitOfWork) : IAttendanceService
{
    public async Task<AttendanceResponseDto> GetAttendanceAsync(int employeeId) =>
        new(
            (await attendance.GetRecordsAsync(employeeId)).Select(x => x.ToResponse()).ToList(),
            (await attendance.GetCorrectionsAsync(employeeId)).Select(x => x.ToResponse()).ToList());

    public async Task<AttendanceCorrectionResponseDto?> CreateCorrectionAsync(CreateAttendanceCorrectionRequestDto request)
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
        await approvals.AddAsync(CreateApproval(await approvals.NextIdAsync(), "Attendance Correction", $"{employee.FullName} - {request.WorkDate:MMM dd, yyyy}", employee.FullName, employee.Manager, 2, "AttendanceCorrection", correction.Id));
        await unitOfWork.SaveChangesAsync();

        return correction.ToResponse();
    }

    public async Task<AttendanceDownloadResponseDto> DownloadAsync(string format, int employeeId)
    {
        var records = await attendance.GetRecordsAsync(employeeId);
        var lines = records.Select(x => $"{x.WorkDate:yyyy-MM-dd},{x.CheckIn},{x.CheckOut},{x.Status},{x.Source}");
        var content = "Date,Check In,Check Out,Status,Source\r\n" + string.Join("\r\n", lines);

        if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
        {
            var employee = await employees.GetByIdAsync(employeeId);
            var title = $"PeopleOS Attendance Log - {employee?.FullName ?? "Employee"}";
            var body = title + "\n\n" + content.Replace(",", "    ");
            return new AttendanceDownloadResponseDto($"Login_UserId_{employeeId}.Attendance log.pdf", "application/pdf", PdfBuilder.Build(body));
        }

        return new AttendanceDownloadResponseDto($"Login_UserId_{employeeId}.Attendance log.csv", "text/csv", Encoding.UTF8.GetBytes(content));
    }

    private static ApprovalTask CreateApproval(int id, string type, string subject, string requester, string manager, int dueInDays, string referenceType, int referenceId) =>
        new() { Id = id, Type = type, Subject = subject, Requester = requester, ApproverRole = $"Line Manager: {manager}", Status = "Pending", DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(dueInDays)), ReferenceType = referenceType, ReferenceId = referenceId };
}

public class LeaveService(
    ILeaveRepository leaves,
    IEmployeeRepository employees,
    IApprovalRepository approvals,
    IUnitOfWork unitOfWork) : ILeaveService
{
    public async Task<LeaveResponseDto> GetLeaveAsync(int employeeId) =>
        new(
            (await leaves.GetBalancesAsync(employeeId)).Select(x => x.ToResponse()).ToList(),
            (await leaves.GetRequestsAsync(employeeId)).Select(x => x.ToResponse()).ToList());

    public async Task<LeaveRequestResponseDto?> CreateRequestAsync(CreateLeaveRequestDto request)
    {
        var employee = await employees.GetByIdAsync(request.EmployeeId);
        if (employee is null)
        {
            return null;
        }

        var totalDays = Math.Max(1, request.ToDate.DayNumber - request.FromDate.DayNumber + 1);
        var leave = new LeaveRequest
        {
            Id = await leaves.NextRequestIdAsync(),
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

        var balance = await leaves.GetBalanceAsync(request.EmployeeId, request.LeaveType);
        if (balance is not null)
        {
            balance.AvailableBalance = Math.Max(0, balance.AvailableBalance - totalDays);
        }

        await leaves.AddRequestAsync(leave);
        await approvals.AddAsync(CreateApproval(await approvals.NextIdAsync(), "Leave", $"{request.LeaveType} - {employee.FullName}", employee.FullName, employee.Manager, 1, "LeaveRequest", leave.Id));
        await unitOfWork.SaveChangesAsync();

        return leave.ToResponse();
    }

    private static ApprovalTask CreateApproval(int id, string type, string subject, string requester, string manager, int dueInDays, string referenceType, int referenceId) =>
        new() { Id = id, Type = type, Subject = subject, Requester = requester, ApproverRole = $"Line Manager: {manager}", Status = "Pending", DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(dueInDays)), ReferenceType = referenceType, ReferenceId = referenceId };
}

public class BenefitsService(IBenefitsRepository benefits) : IBenefitsService
{
    public async Task<List<BenefitPlanResponseDto>> GetBenefitsAsync() =>
        (await benefits.GetAllAsync()).Select(x => x.ToResponse()).ToList();
}

public class ExpenseService(IExpenseRepository expenses, IEmployeeRepository employees, IApprovalRepository approvals, IUnitOfWork unitOfWork) : IExpenseService
{
    public async Task<List<ExpenseClaimResponseDto>> GetClaimsAsync(int employeeId) =>
        (await expenses.GetByEmployeeAsync(employeeId)).Select(x => x.ToResponse()).ToList();

    public async Task<ExpenseClaimResponseDto?> CreateClaimAsync(CreateExpenseClaimRequestDto request)
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
        await approvals.AddAsync(CreateApproval(await approvals.NextIdAsync(), "Expense", $"{request.ClaimType} - {employee.FullName}", employee.FullName, employee.Manager, 3, "ExpenseClaim", claim.Id));
        await unitOfWork.SaveChangesAsync();
        return claim.ToResponse();
    }

    private static ApprovalTask CreateApproval(int id, string type, string subject, string requester, string manager, int dueInDays, string referenceType, int referenceId) =>
        new() { Id = id, Type = type, Subject = subject, Requester = requester, ApproverRole = $"Line Manager: {manager}", Status = "Pending", DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(dueInDays)), ReferenceType = referenceType, ReferenceId = referenceId };
}

public class ResignationService(IResignationRepository resignations, IEmployeeRepository employees, IApprovalRepository approvals, IUnitOfWork unitOfWork) : IResignationService
{
    public async Task<List<ResignationResponseDto>> GetResignationsAsync(int employeeId) =>
        (await resignations.GetByEmployeeAsync(employeeId)).Select(x => x.ToResponse()).ToList();

    public async Task<ResignationResponseDto?> CreateResignationAsync(CreateResignationRequestDto request)
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
        await approvals.AddAsync(CreateApproval(await approvals.NextIdAsync(), "Resignation", $"Resignation request - {employee.FullName}", employee.FullName, employee.Manager, 2, "ResignationRequest", resignation.Id));
        await unitOfWork.SaveChangesAsync();
        return resignation.ToResponse();
    }

    private static ApprovalTask CreateApproval(int id, string type, string subject, string requester, string manager, int dueInDays, string referenceType, int referenceId) =>
        new() { Id = id, Type = type, Subject = subject, Requester = requester, ApproverRole = $"Line Manager: {manager}", Status = "Pending", DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(dueInDays)), ReferenceType = referenceType, ReferenceId = referenceId };
}

public class PolicyService(IPolicyRepository policies) : IPolicyService
{
    public async Task<List<PolicyDocumentResponseDto>> GetPoliciesAsync() =>
        (await policies.GetAllAsync()).Select(x => x.ToResponse()).ToList();
}

public class ApprovalService(
    IApprovalRepository approvals,
    IAttendanceRepository attendance,
    ILeaveRepository leaves,
    IExpenseRepository expenses,
    IResignationRepository resignations,
    INotificationRepository notifications,
    IUnitOfWork unitOfWork) : IApprovalService
{
    public async Task<List<ApprovalTaskResponseDto>> GetApprovalsAsync() =>
        (await approvals.GetAllAsync()).Select(x => x.ToResponse()).ToList();

    public async Task<ApprovalTaskResponseDto?> DecideAsync(int approvalId, ApprovalDecisionRequestDto request)
    {
        var decision = NormalizeDecision(request.Decision);
        if (decision is null)
        {
            return null;
        }

        var approval = await approvals.GetByIdAsync(approvalId);
        if (approval is null)
        {
            return null;
        }

        approval.Status = decision;
        approval.DecidedAt = DateTime.UtcNow;
        await ApplyDecisionToReferenceAsync(approval, decision);
        await unitOfWork.SaveChangesAsync();

        return approval.ToResponse();
    }

    private async Task ApplyDecisionToReferenceAsync(ApprovalTask approval, string decision)
    {
        if (approval.ReferenceId is null)
        {
            return;
        }

        switch (approval.ReferenceType)
        {
            case "AttendanceCorrection":
                var correction = await attendance.GetCorrectionByIdAsync(approval.ReferenceId.Value);
                if (correction is not null)
                {
                    correction.Status = decision;
                    await AddDecisionNotificationAsync(correction.EmployeeId, "Attendance correction", approval.Subject, decision);
                }
                break;
            case "LeaveRequest":
                var leave = await leaves.GetRequestByIdAsync(approval.ReferenceId.Value);
                if (leave is not null)
                {
                    await ApplyLeaveDecisionAsync(leave, decision);
                    await AddDecisionNotificationAsync(leave.EmployeeId, "Leave request", approval.Subject, decision);
                }
                break;
            case "ExpenseClaim":
                var claim = await expenses.GetByIdAsync(approval.ReferenceId.Value);
                if (claim is not null)
                {
                    claim.Status = decision;
                    await AddDecisionNotificationAsync(claim.EmployeeId, "Expense claim", approval.Subject, decision);
                }
                break;
            case "ResignationRequest":
                var resignation = await resignations.GetByIdAsync(approval.ReferenceId.Value);
                if (resignation is not null)
                {
                    resignation.Status = decision;
                    await AddDecisionNotificationAsync(resignation.EmployeeId, "Resignation request", approval.Subject, decision);
                }
                break;
        }
    }

    private async Task AddDecisionNotificationAsync(int employeeId, string titlePrefix, string subject, string decision)
    {
        await notifications.AddAsync(new EmployeeNotification
        {
            Id = await notifications.NextIdAsync(),
            EmployeeId = employeeId,
            Title = $"{titlePrefix} {decision.ToLowerInvariant()}",
            Body = $"{subject} has been {decision.ToLowerInvariant()} by your line manager.",
            Tone = decision.Equals("Rejected", StringComparison.OrdinalIgnoreCase) ? "urgent" : "info",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
    }

    private async Task ApplyLeaveDecisionAsync(LeaveRequest leave, string decision)
    {
        var previousStatus = leave.Status;
        leave.Status = decision;

        if (!decision.Equals("Rejected", StringComparison.OrdinalIgnoreCase) ||
            previousStatus.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var balance = await leaves.GetBalanceAsync(leave.EmployeeId, leave.LeaveType);
        if (balance is not null)
        {
            balance.AvailableBalance = Math.Min(balance.AnnualEntitlement, balance.AvailableBalance + leave.TotalDays);
        }
    }

    private static string? NormalizeDecision(string decision)
    {
        if (decision.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            return "Approved";
        }

        if (decision.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
        {
            return "Rejected";
        }

        return null;
    }
}

public class NotificationService(INotificationRepository notifications, IUnitOfWork unitOfWork) : INotificationService
{
    public async Task<List<EmployeeNotificationResponseDto>> GetNotificationsAsync(int employeeId) =>
        (await notifications.GetByEmployeeAsync(employeeId)).Select(x => x.ToResponse()).ToList();

    public async Task MarkAllReadAsync(int employeeId)
    {
        await notifications.MarkAllReadAsync(employeeId);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> ClearAsync(int employeeId, int notificationId)
    {
        var cleared = await notifications.ClearAsync(employeeId, notificationId);
        if (!cleared)
        {
            return false;
        }

        await unitOfWork.SaveChangesAsync();
        return true;
    }
}
