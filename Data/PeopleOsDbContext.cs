using Microsoft.EntityFrameworkCore;
using PeopleOS.Api.Models;

namespace PeopleOS.Api.Data;

public class PeopleOsDbContext(DbContextOptions<PeopleOsDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<LifecycleStage> LifecycleStages => Set<LifecycleStage>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<AttendanceCorrection> AttendanceCorrections => Set<AttendanceCorrection>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<BenefitPlan> BenefitPlans => Set<BenefitPlan>();
    public DbSet<EmployeeDocument> Documents => Set<EmployeeDocument>();
    public DbSet<PolicyDocument> Policies => Set<PolicyDocument>();
    public DbSet<ApprovalTask> ApprovalTasks => Set<ApprovalTask>();
}
