using Microsoft.EntityFrameworkCore;
using PeopleOS.Api.Application.Interfaces;
using PeopleOS.Api.Domain.Entities;
using PeopleOS.Api.Infrastructure.Database;

namespace PeopleOS.Api.Infrastructure.Repositories;

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

    public Task<AppRole?> GetRoleAsync(string roleName) =>
        db.Roles.FirstOrDefaultAsync(x => x.Name == roleName);

    public Task<List<AppRolePermission>> GetRolePermissionsAsync(string roleName) =>
        db.RolePermissions
            .Join(db.Roles, grant => grant.RoleId, role => role.Id, (grant, role) => new { grant, role })
            .Where(x => x.role.Name == roleName)
            .Select(x => x.grant)
            .ToListAsync();

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
    public Task<List<ApprovalTask>> GetPendingAsync() => db.ApprovalTasks.Where(x => x.Status != "Approved" && x.Status != "Rejected").OrderBy(x => x.DueDate).ToListAsync();
    public Task<List<ApprovalTask>> GetAllAsync() => db.ApprovalTasks.OrderBy(x => x.DueDate).ToListAsync();
    public Task<ApprovalTask?> GetByIdAsync(int id) => db.ApprovalTasks.FirstOrDefaultAsync(x => x.Id == id);
    public async Task AddAsync(ApprovalTask task) => await db.ApprovalTasks.AddAsync(task);
    public async Task<int> NextIdAsync() => await NextIdAsync(db.ApprovalTasks.Select(x => x.Id));

    private static async Task<int> NextIdAsync(IQueryable<int> ids) => await ids.AnyAsync() ? await ids.MaxAsync() + 1 : 1;
}

public class AttendanceRepository(PeopleOsDbContext db) : IAttendanceRepository
{
    public Task<List<AttendanceRecord>> GetRecordsAsync(int employeeId) => db.AttendanceRecords.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.WorkDate).ToListAsync();
    public Task<List<AttendanceCorrection>> GetCorrectionsAsync(int employeeId) => db.AttendanceCorrections.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.WorkDate).ToListAsync();
    public Task<AttendanceCorrection?> GetCorrectionByIdAsync(int id) => db.AttendanceCorrections.FirstOrDefaultAsync(x => x.Id == id);
    public async Task AddCorrectionAsync(AttendanceCorrection correction) => await db.AttendanceCorrections.AddAsync(correction);
    public async Task<int> NextCorrectionIdAsync() => await NextIdAsync(db.AttendanceCorrections.Select(x => x.Id));

    private static async Task<int> NextIdAsync(IQueryable<int> ids) => await ids.AnyAsync() ? await ids.MaxAsync() + 1 : 1;
}

public class LeaveRepository(PeopleOsDbContext db) : ILeaveRepository
{
    public Task<List<LeaveBalance>> GetBalancesAsync(int employeeId) => db.LeaveBalances.Where(x => x.EmployeeId == employeeId).ToListAsync();
    public Task<LeaveBalance?> GetBalanceAsync(int employeeId, string leaveType) =>
        db.LeaveBalances.FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.LeaveType == leaveType);
    public Task<List<LeaveRequest>> GetRequestsAsync(int employeeId) => db.LeaveRequests.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.FromDate).ToListAsync();
    public Task<LeaveRequest?> GetRequestByIdAsync(int id) => db.LeaveRequests.FirstOrDefaultAsync(x => x.Id == id);
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
    public Task<ExpenseClaim?> GetByIdAsync(int id) => db.ExpenseClaims.FirstOrDefaultAsync(x => x.Id == id);
    public async Task AddAsync(ExpenseClaim claim) => await db.ExpenseClaims.AddAsync(claim);
    public async Task<int> NextIdAsync() => await NextIdAsync(db.ExpenseClaims.Select(x => x.Id));

    private static async Task<int> NextIdAsync(IQueryable<int> ids) => await ids.AnyAsync() ? await ids.MaxAsync() + 1 : 1;
}

public class ResignationRepository(PeopleOsDbContext db) : IResignationRepository
{
    public Task<List<ResignationRequest>> GetByEmployeeAsync(int employeeId) => db.ResignationRequests.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.ResignationDate).ToListAsync();
    public Task<ResignationRequest?> GetByIdAsync(int id) => db.ResignationRequests.FirstOrDefaultAsync(x => x.Id == id);
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

public class NotificationRepository(PeopleOsDbContext db) : INotificationRepository
{
    public Task<List<EmployeeNotification>> GetByEmployeeAsync(int employeeId) =>
        db.EmployeeNotifications.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public async Task AddAsync(EmployeeNotification notification) => await db.EmployeeNotifications.AddAsync(notification);

    public async Task MarkAllReadAsync(int employeeId)
    {
        var notifications = await db.EmployeeNotifications.Where(x => x.EmployeeId == employeeId && !x.IsRead).ToListAsync();
        notifications.ForEach(notification => notification.IsRead = true);
    }

    public async Task<bool> ClearAsync(int employeeId, int notificationId)
    {
        var notification = await db.EmployeeNotifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.EmployeeId == employeeId);
        if (notification is null)
        {
            return false;
        }

        db.EmployeeNotifications.Remove(notification);
        return true;
    }

    public async Task<int> NextIdAsync() => await NextIdAsync(db.EmployeeNotifications.Select(x => x.Id));

    private static async Task<int> NextIdAsync(IQueryable<int> ids) => await ids.AnyAsync() ? await ids.MaxAsync() + 1 : 1;
}
