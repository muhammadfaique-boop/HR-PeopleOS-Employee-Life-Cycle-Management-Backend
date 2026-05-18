using Microsoft.EntityFrameworkCore;
using PeopleOS.Api.Domain.Entities;

namespace PeopleOS.Api.Infrastructure.Database;

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
    public DbSet<ExpenseClaim> ExpenseClaims => Set<ExpenseClaim>();
    public DbSet<ResignationRequest> ResignationRequests => Set<ResignationRequest>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<QuickAction> QuickActions => Set<QuickAction>();
    public DbSet<LifecycleSignal> LifecycleSignals => Set<LifecycleSignal>();
    public DbSet<ActivityFeedItem> ActivityFeedItems => Set<ActivityFeedItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var idProperty = entityType.FindProperty("Id");
            if (idProperty?.ClrType == typeof(int))
            {
                idProperty.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
            }
        }

        modelBuilder.Entity<LeaveBalance>().Property(x => x.AnnualEntitlement).HasPrecision(8, 2);
        modelBuilder.Entity<LeaveBalance>().Property(x => x.AvailableBalance).HasPrecision(8, 2);
        modelBuilder.Entity<LeaveRequest>().Property(x => x.TotalDays).HasPrecision(8, 2);
        modelBuilder.Entity<ExpenseClaim>().Property(x => x.Amount).HasPrecision(18, 2);
    }
}
