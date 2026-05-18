using Microsoft.EntityFrameworkCore;
using PeopleOS.Api.Data;
using PeopleOS.Api.Models;

namespace PeopleOS.Api.Repositories;

public interface IAuthRepository
{
    Task<AppUser?> FindByCredentialsAsync(string email, string password);
    Task<AppUser?> FindByEmailAsync(string email);
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

public class EfUnitOfWork(PeopleOsDbContext db) : IUnitOfWork
{
    public Task SaveChangesAsync() => db.SaveChangesAsync();
}

public class AuthRepository(PeopleOsDbContext db) : IAuthRepository
{
    public Task<AppUser?> FindByCredentialsAsync(string email, string password) =>
        db.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower() && x.Password == password);

    public Task<AppUser?> FindByEmailAsync(string email) =>
        db.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}

public class EmployeeRepository(PeopleOsDbContext db) : IEmployeeRepository
{
    public Task<List<Employee>> GetAllAsync() => db.Employees.OrderBy(x => x.FullName).ToListAsync();
    public Task<Employee?> GetByIdAsync(int id) => db.Employees.FindAsync(id).AsTask();
    public Task<Employee?> GetByEmailAsync(string email) => db.Employees.FirstOrDefaultAsync(x => x.Email == email);
    public Task SaveChangesAsync() => db.SaveChangesAsync();
}

public class LifecycleRepository(PeopleOsDbContext db) : ILifecycleRepository
{
    public Task<List<LifecycleStage>> GetAllAsync() => db.LifecycleStages.OrderBy(x => x.DueDate).ToListAsync();
    public Task<List<LifecycleStage>> GetByEmployeeAsync(int employeeId) => db.LifecycleStages.Where(x => x.EmployeeId == employeeId).OrderBy(x => x.DueDate).ToListAsync();
}

public class DocumentRepository(PeopleOsDbContext db) : IDocumentRepository
{
    public Task<List<EmployeeDocument>> GetByEmployeeAsync(int employeeId) => db.Documents.Where(x => x.EmployeeId == employeeId).OrderBy(x => x.Category).ThenBy(x => x.Name).ToListAsync();
}

public class ApprovalRepository(PeopleOsDbContext db) : IApprovalRepository
{
    public Task<List<ApprovalTask>> GetPendingAsync() => db.ApprovalTasks.Where(x => x.Status != "Approved").OrderBy(x => x.DueDate).ToListAsync();
    public Task<List<ApprovalTask>> GetAllAsync() => db.ApprovalTasks.OrderBy(x => x.DueDate).ToListAsync();
    public async Task AddAsync(ApprovalTask task) => await db.ApprovalTasks.AddAsync(task);
    public async Task<int> NextIdAsync() => await NextIdAsync(db.ApprovalTasks.Select(x => x.Id));

    private static async Task<int> NextIdAsync(IQueryable<int> ids) => await ids.AnyAsync() ? await ids.MaxAsync() + 1 : 1;
}

public class AttendanceRepository(PeopleOsDbContext db) : IAttendanceRepository
{
    public Task<List<AttendanceRecord>> GetRecordsAsync(int employeeId) => db.AttendanceRecords.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.WorkDate).ToListAsync();
    public Task<List<AttendanceCorrection>> GetCorrectionsAsync(int employeeId) => db.AttendanceCorrections.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.WorkDate).ToListAsync();
    public async Task AddCorrectionAsync(AttendanceCorrection correction) => await db.AttendanceCorrections.AddAsync(correction);
    public async Task<int> NextCorrectionIdAsync() => await NextIdAsync(db.AttendanceCorrections.Select(x => x.Id));

    private static async Task<int> NextIdAsync(IQueryable<int> ids) => await ids.AnyAsync() ? await ids.MaxAsync() + 1 : 1;
}

public class LeaveRepository(PeopleOsDbContext db) : ILeaveRepository
{
    public Task<List<LeaveBalance>> GetBalancesAsync(int employeeId) => db.LeaveBalances.Where(x => x.EmployeeId == employeeId).ToListAsync();
    public Task<List<LeaveRequest>> GetRequestsAsync(int employeeId) => db.LeaveRequests.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.FromDate).ToListAsync();
    public Task<List<LeaveRequest>> GetApprovedUpcomingAsync(DateOnly fromDate) => db.LeaveRequests.Where(x => x.Status == "Approved" && x.ToDate >= fromDate).OrderBy(x => x.FromDate).ToListAsync();
    public async Task AddRequestAsync(LeaveRequest request) => await db.LeaveRequests.AddAsync(request);
    public async Task<int> NextRequestIdAsync() => await NextIdAsync(db.LeaveRequests.Select(x => x.Id));

    private static async Task<int> NextIdAsync(IQueryable<int> ids) => await ids.AnyAsync() ? await ids.MaxAsync() + 1 : 1;
}

public class BenefitsRepository(PeopleOsDbContext db) : IBenefitsRepository
{
    public Task<List<BenefitPlan>> GetAllAsync() => db.BenefitPlans.OrderBy(x => x.Category).ThenBy(x => x.Name).ToListAsync();
}

public class PolicyRepository(PeopleOsDbContext db) : IPolicyRepository
{
    public Task<List<PolicyDocument>> GetAllAsync() => db.Policies.OrderByDescending(x => x.PublishedOn).ToListAsync();
}

public class ExpenseRepository(PeopleOsDbContext db) : IExpenseRepository
{
    public Task<List<ExpenseClaim>> GetByEmployeeAsync(int employeeId) => db.ExpenseClaims.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.ExpenseDate).ToListAsync();
    public async Task AddAsync(ExpenseClaim claim) => await db.ExpenseClaims.AddAsync(claim);
    public async Task<int> NextIdAsync() => await NextIdAsync(db.ExpenseClaims.Select(x => x.Id));

    private static async Task<int> NextIdAsync(IQueryable<int> ids) => await ids.AnyAsync() ? await ids.MaxAsync() + 1 : 1;
}

public class ResignationRepository(PeopleOsDbContext db) : IResignationRepository
{
    public Task<List<ResignationRequest>> GetByEmployeeAsync(int employeeId) => db.ResignationRequests.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.ResignationDate).ToListAsync();
    public async Task AddAsync(ResignationRequest resignation) => await db.ResignationRequests.AddAsync(resignation);
    public async Task<int> NextIdAsync() => await NextIdAsync(db.ResignationRequests.Select(x => x.Id));

    private static async Task<int> NextIdAsync(IQueryable<int> ids) => await ids.AnyAsync() ? await ids.MaxAsync() + 1 : 1;
}

public class ReferenceDataRepository(PeopleOsDbContext db) : IReferenceDataRepository
{
    public Task<List<Holiday>> GetHolidaysAsync() => db.Holidays.OrderBy(x => x.Date).ToListAsync();
    public Task<List<Announcement>> GetAnnouncementsAsync() => db.Announcements.OrderByDescending(x => x.PublishedOn).ToListAsync();
    public Task<List<QuickAction>> GetQuickActionsAsync() => db.QuickActions.OrderBy(x => x.DisplayOrder).ToListAsync();
    public Task<List<LifecycleSignal>> GetLifecycleSignalsAsync() => db.LifecycleSignals.OrderBy(x => x.DisplayOrder).ToListAsync();
    public Task<List<ActivityFeedItem>> GetRecentActivityAsync() => db.ActivityFeedItems.OrderByDescending(x => x.ActivityDate).ThenByDescending(x => x.Id).ToListAsync();
}
