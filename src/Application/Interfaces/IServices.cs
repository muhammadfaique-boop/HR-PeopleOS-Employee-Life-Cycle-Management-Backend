using PeopleOS.Api.Application.DTOs;

namespace PeopleOS.Api.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    Task<bool> ChangePasswordAsync(ChangePasswordRequestDto request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request);
}

public interface IDashboardService
{
    Task<DashboardResponseDto> GetDashboardAsync();
}

public interface IEmployeeService
{
    Task<List<EmployeeResponseDto>> GetEmployeesAsync();
    Task<EmployeeProfileResponseDto?> GetProfileAsync(int employeeId);
    Task<EmployeeResponseDto?> UpdateProfileAsync(int employeeId, UpdateEmployeeProfileRequestDto request);
}

public interface IAttendanceService
{
    Task<AttendanceResponseDto> GetAttendanceAsync(int employeeId);
    Task<AttendanceCorrectionResponseDto?> CreateCorrectionAsync(CreateAttendanceCorrectionRequestDto request);
    Task<AttendanceDownloadResponseDto> DownloadAsync(string format, int employeeId);
}

public interface ILeaveService
{
    Task<LeaveResponseDto> GetLeaveAsync(int employeeId);
    Task<LeaveRequestResponseDto?> CreateRequestAsync(CreateLeaveRequestDto request);
}

public interface IBenefitsService
{
    Task<List<BenefitPlanResponseDto>> GetBenefitsAsync();
}

public interface IExpenseService
{
    Task<List<ExpenseClaimResponseDto>> GetClaimsAsync(int employeeId);
    Task<ExpenseClaimResponseDto?> CreateClaimAsync(CreateExpenseClaimRequestDto request);
}

public interface IResignationService
{
    Task<List<ResignationResponseDto>> GetResignationsAsync(int employeeId);
    Task<ResignationResponseDto?> CreateResignationAsync(CreateResignationRequestDto request);
}

public interface IPolicyService
{
    Task<List<PolicyDocumentResponseDto>> GetPoliciesAsync();
}

public interface IApprovalService
{
    Task<List<ApprovalTaskResponseDto>> GetApprovalsAsync();
    Task<ApprovalTaskResponseDto?> DecideAsync(int approvalId, ApprovalDecisionRequestDto request);
}

public interface INotificationService
{
    Task<List<EmployeeNotificationResponseDto>> GetNotificationsAsync(int employeeId);
    Task MarkAllReadAsync(int employeeId);
    Task<bool> ClearAsync(int employeeId, int notificationId);
}
