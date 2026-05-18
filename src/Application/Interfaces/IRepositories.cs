using PeopleOS.Api.Domain.Entities;

namespace PeopleOS.Api.Application.Interfaces;

public interface IAuthRepository
{
    Task<AppUser?> FindByCredentialsAsync(string email, string password);
    Task<AppUser?> FindByEmailAsync(string email);
    Task<List<AppRolePermission>> GetRolePermissionsAsync(string roleName);
    Task<AppRole?> GetRoleAsync(string roleName);
    Task SaveChangesAsync();
}

public interface IEmployeeRepository
{
    Task<List<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee?> GetByEmailAsync(string email);
    Task SaveChangesAsync();
}

public interface ILifecycleRepository
{
    Task<List<LifecycleStage>> GetAllAsync();
    Task<List<LifecycleStage>> GetByEmployeeAsync(int employeeId);
}

public interface IDocumentRepository
{
    Task<List<EmployeeDocument>> GetByEmployeeAsync(int employeeId);
}

public interface IApprovalRepository
{
    Task<List<ApprovalTask>> GetPendingAsync();
    Task<List<ApprovalTask>> GetAllAsync();
    Task AddAsync(ApprovalTask task);
    Task<int> NextIdAsync();
}

public interface IAttendanceRepository
{
    Task<List<AttendanceRecord>> GetRecordsAsync(int employeeId);
    Task<List<AttendanceCorrection>> GetCorrectionsAsync(int employeeId);
    Task AddCorrectionAsync(AttendanceCorrection correction);
    Task<int> NextCorrectionIdAsync();
}

public interface ILeaveRepository
{
    Task<List<LeaveBalance>> GetBalancesAsync(int employeeId);
    Task<List<LeaveRequest>> GetRequestsAsync(int employeeId);
    Task<List<LeaveRequest>> GetApprovedUpcomingAsync(DateOnly fromDate);
    Task AddRequestAsync(LeaveRequest request);
    Task<int> NextRequestIdAsync();
}

public interface IBenefitsRepository
{
    Task<List<BenefitPlan>> GetAllAsync();
}

public interface IPolicyRepository
{
    Task<List<PolicyDocument>> GetAllAsync();
}

public interface IExpenseRepository
{
    Task<List<ExpenseClaim>> GetByEmployeeAsync(int employeeId);
    Task AddAsync(ExpenseClaim claim);
    Task<int> NextIdAsync();
}

public interface IResignationRepository
{
    Task<List<ResignationRequest>> GetByEmployeeAsync(int employeeId);
    Task AddAsync(ResignationRequest resignation);
    Task<int> NextIdAsync();
}

public interface IReferenceDataRepository
{
    Task<List<Holiday>> GetHolidaysAsync();
    Task<List<Announcement>> GetAnnouncementsAsync();
    Task<List<QuickAction>> GetQuickActionsAsync();
    Task<List<LifecycleSignal>> GetLifecycleSignalsAsync();
    Task<List<ActivityFeedItem>> GetRecentActivityAsync();
}

public interface IUnitOfWork
{
    Task SaveChangesAsync();
}
