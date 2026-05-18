using PeopleOS.Api.Models;

namespace PeopleOS.Api.Models.Dtos;

public record LoginRequest(string Email, string Password);
public record ChangePasswordRequest(string Email, string CurrentPassword, string NewPassword);
public record AttendanceCorrectionRequest(int EmployeeId, DateOnly WorkDate, string RequestedChange, string Reason);
public record LeaveRequestDto(int EmployeeId, string LeaveType, DateOnly FromDate, DateOnly ToDate, string Reason, string ContactDuringLeave, string? AttachmentFileName, string? AttachmentDataUrl);
public record ExpenseClaimDto(int EmployeeId, string ClaimType, string Category, decimal Amount, DateOnly ExpenseDate, string Description, string? ReceiptFileName, string? ReceiptDataUrl);
public record ResignationDto(int EmployeeId, DateOnly LastWorkingDate, string Reason);
public record ProfileUpdateDto(string PreferredLanguage, string ProfileImageUrl);

public record DashboardMetric(string Label, string Value, string Accent);
public record WhoIsOutItem(string EmployeeName, string LeaveType, DateOnly FromDate, DateOnly ToDate, string Department);

public record DashboardDto(
    IReadOnlyList<DashboardMetric> Metrics,
    Employee ActiveEmployee,
    IReadOnlyList<Employee> Employees,
    IReadOnlyList<LifecycleStage> Lifecycle,
    IReadOnlyList<ApprovalTask> Approvals,
    IReadOnlyList<WhoIsOutItem> WhoIsOut,
    IReadOnlyList<Holiday> Holidays,
    IReadOnlyList<Announcement> Announcements,
    IReadOnlyList<QuickAction> QuickActions,
    IReadOnlyList<LifecycleSignal> LifecycleSignals,
    IReadOnlyList<string> RecentActivity);

public record EmployeeProfileDto(
    Employee Employee,
    IReadOnlyList<EmployeeDocument> Documents,
    IReadOnlyList<LifecycleStage> Lifecycle,
    IReadOnlyList<LeaveBalance> LeaveBalances);

public record AttendanceDto(
    IReadOnlyList<AttendanceRecord> Records,
    IReadOnlyList<AttendanceCorrection> Corrections);

public record LeaveDto(
    IReadOnlyList<LeaveBalance> Balances,
    IReadOnlyList<LeaveRequest> Requests);

public record AttendanceDownload(string FileName, string ContentType, byte[] Content);
