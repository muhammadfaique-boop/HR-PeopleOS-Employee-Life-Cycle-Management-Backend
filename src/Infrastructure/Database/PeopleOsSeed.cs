using PeopleOS.Api.Domain.Entities;

namespace PeopleOS.Api.Infrastructure.Database;

public static class PeopleOsSeed
{
    public static void Seed(PeopleOsDbContext db)
    {
        if (!db.Users.Any())
        {
            SeedCoreData(db);
        }

        SeedRolesAndPermissions(db);
        SeedReferenceData(db);
        db.SaveChanges();
    }

    private static void SeedCoreData(PeopleOsDbContext db)
    {
        db.Employees.AddRange(
            new Employee { Id = 1, EmployeeCode = "EMP-1001", FullName = "Ayesha Khan", Email = "ayesha.khan@peopleos.dev", Department = "People Operations", Position = "HR Operations Lead", Manager = "Sara Ahmed", ManagerEmployeeId = null, LifecycleStatus = "Active", JoiningDate = new DateOnly(2023, 3, 6), ProfileCompletion = 96, WorkLocation = "Lahore" },
            new Employee { Id = 2, EmployeeCode = "EMP-1042", FullName = "Muhammad Faique", Email = "muhammad.faique@peopleos.dev", Department = "Engineering", Position = "Senior Software Engineer", Manager = "Ayesha Khan", ManagerEmployeeId = 1, LifecycleStatus = "Active", JoiningDate = new DateOnly(2026, 1, 16), ProfileCompletion = 88, WorkLocation = "Lahore", ProfileImageUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=240&q=80" },
            new Employee { Id = 3, EmployeeCode = "EMP-1077", FullName = "Bilal Raza", Email = "bilal.raza@peopleos.dev", Department = "Engineering", Position = "Frontend Engineer", Manager = "Muhammad Faique", ManagerEmployeeId = 2, LifecycleStatus = "Probation", JoiningDate = new DateOnly(2026, 4, 1), ProfileCompletion = 72, WorkLocation = "Karachi" });

        db.Users.AddRange(
            new AppUser { Id = 1, Email = "admin@peopleos.dev", Password = "Admin@123", Role = "Super Admin", EmployeeId = 1 },
            new AppUser { Id = 2, Email = "hr@peopleos.dev", Password = "Hr@123", Role = "HR", EmployeeId = 1 },
            new AppUser { Id = 3, Email = "employee@peopleos.dev", Password = "Employee@123", Role = "Employee", EmployeeId = 2 });

        db.LifecycleStages.AddRange(
            Stage(1, 1, "Pre-onboarding", "HR", "Done", 2026, 1, 10, "Offer packet, joining checklist and pre-start documents completed."),
            Stage(2, 2, "Official start", "HR", "Done", 2026, 1, 16, "Employee profile, welcome flow and ID setup completed."),
            Stage(3, 2, "Post-onboarding integration", "Manager", "In progress", 2026, 6, 1, "Role goals, team intro and first feedback cycle."),
            Stage(4, 3, "Probation review", "Manager", "Needs attention", 2026, 6, 28, "Manager feedback and confirmation decision pending."),
            Stage(5, 2, "Career growth", "Manager", "Planned", 2026, 12, 15, "Promotion readiness and skill plan tracking."),
            Stage(6, 2, "Lifecycle records", "HR", "In progress", 2026, 5, 30, "E-file, verification letters and document completeness."));

        db.AttendanceRecords.AddRange(
            Attendance(1, 2, 2026, 5, 18, 9, 27, 18, 12, "Present", "Biometric"),
            Attendance(2, 2, 2026, 5, 17, 9, 42, 18, 4, "Late", "Biometric"),
            Attendance(3, 2, 2026, 5, 16, 9, 20, 17, 52, "Present", "Biometric"),
            Attendance(4, 3, 2026, 5, 18, null, null, null, null, "Missing punch", "Device sync"));

        db.AttendanceCorrections.Add(new AttendanceCorrection { Id = 1, EmployeeId = 2, WorkDate = new DateOnly(2026, 5, 17), RequestedChange = "Update check-in to 09:15", Reason = "Device queue synced late", Status = "Pending manager review", Approver = "Ayesha Khan" });

        db.LeaveBalances.AddRange(
            Balance(1, 2, "Casual Leave", 12, 7),
            Balance(2, 2, "Sick Leave", 10, 9),
            Balance(3, 2, "Annual Leave", 18, 14),
            Balance(4, 2, "Work from Home", 24, 20));

        db.LeaveRequests.AddRange(
            new LeaveRequest { Id = 1, EmployeeId = 2, LeaveType = "Casual Leave", FromDate = new DateOnly(2026, 5, 14), ToDate = new DateOnly(2026, 5, 14), TotalDays = 1, Reason = "Family medical support", ContactDuringLeave = "Available on mobile", Status = "Approval Required" },
            new LeaveRequest { Id = 2, EmployeeId = 2, LeaveType = "Work from Home", FromDate = new DateOnly(2026, 5, 21), ToDate = new DateOnly(2026, 5, 21), TotalDays = 1, Reason = "Deep work day", ContactDuringLeave = "Teams and phone", Status = "Approved" });

        db.BenefitPlans.AddRange(
            new BenefitPlan { Id = 1, Name = "Health Insurance", Category = "Benefit", Coverage = "Employee, spouse and children", Status = "Active", Description = "Hospitalization, emergency and dependent coverage." },
            new BenefitPlan { Id = 2, Name = "Vehicle Benefit", Category = "Mobility", Coverage = "Role-based allowance categories", Status = "Policy managed", Description = "Mobility entitlement by role, location and approval policy." },
            new BenefitPlan { Id = 3, Name = "Medical OPD", Category = "Expense Category", Coverage = "OPD reimbursement", Status = "Active", Description = "Medical expense claim category for outpatient reimbursements." },
            new BenefitPlan { Id = 4, Name = "Business Expense", Category = "Expense Category", Coverage = "Approved business purchases", Status = "Active", Description = "Business expense claim category with line manager approval." });

        db.Documents.AddRange(
            Doc(1, 2, "CNIC copy", "Identity", "Verified", 2026, 1, 16),
            Doc(2, 2, "Education certificate", "Education", "Verified", 2026, 1, 18),
            Doc(3, 2, "Employment verification letter", "Letters", "Ready to request", 2026, 5, 1),
            Doc(4, 3, "Probation confirmation form", "Probation", "Pending", 2026, 5, 18));

        db.Policies.AddRange(
            new PolicyDocument { Id = 1, Title = "Attendance and Absence Policy", Category = "Attendance", Version = "v1.2", PublishedOn = new DateOnly(2026, 2, 1) },
            new PolicyDocument { Id = 2, Title = "Leave Policy", Category = "Leave", Version = "v2.0", PublishedOn = new DateOnly(2026, 3, 15) },
            new PolicyDocument { Id = 3, Title = "Promotion Cycle Guide", Category = "Career Growth", Version = "v1.0", PublishedOn = new DateOnly(2026, 4, 10) });

        db.ApprovalTasks.AddRange(
            new ApprovalTask { Id = 1, Type = "Leave", Subject = "Casual Leave - Muhammad Faique", Requester = "Muhammad Faique", ApproverRole = "Line Manager", Status = "Pending", DueDate = new DateOnly(2026, 5, 19), ReferenceType = "LeaveRequest", ReferenceId = 1 },
            new ApprovalTask { Id = 2, Type = "Probation", Subject = "Probation review - Bilal Raza", Requester = "People Operations", ApproverRole = "Line Manager", Status = "Pending", DueDate = new DateOnly(2026, 6, 28) },
            new ApprovalTask { Id = 3, Type = "Document", Subject = "Employment letter verification", Requester = "Muhammad Faique", ApproverRole = "HR", Status = "In review", DueDate = new DateOnly(2026, 5, 22) });

        db.ExpenseClaims.Add(new ExpenseClaim { Id = 1, EmployeeId = 2, ClaimType = "Medical Expense OPD", Category = "Medical OPD", Amount = 6500, ExpenseDate = new DateOnly(2026, 5, 10), Description = "Clinic consultation and medicine", Status = "Pending line manager", LineManager = "Ayesha Khan" });

        db.ResignationRequests.Add(new ResignationRequest { Id = 1, EmployeeId = 3, ResignationDate = new DateOnly(2026, 5, 1), LastWorkingDate = new DateOnly(2026, 5, 31), Reason = "Demo resignation workflow", Status = "Pending line manager", LineManager = "Muhammad Faique" });
    }

    private static void SeedRolesAndPermissions(PeopleOsDbContext db)
    {
        foreach (var admin in db.Users.Where(x => x.Role == "Admin"))
        {
            admin.Role = "Super Admin";
        }

        if (!db.Roles.Any())
        {
            db.Roles.AddRange(
                new AppRole { Id = 1, Name = "Super Admin", Description = "Owns system setup, roles, permissions, organization-wide settings, reporting and audit visibility.", DefaultScope = "organization" },
                new AppRole { Id = 2, Name = "HR", Description = "Manages employees, attendance policies, leave policies, holidays, benefits, departments and approvals.", DefaultScope = "organization" },
                new AppRole { Id = 3, Name = "Employee", Description = "Uses self-service attendance, leave, profile, expense, resignation and policy workflows.", DefaultScope = "own" });
        }

        if (!db.Permissions.Any())
        {
            db.Permissions.AddRange(
                Permission(1, "attendance.read", "View attendance records."),
                Permission(2, "attendance.create", "Create attendance punches or manual records."),
                Permission(3, "attendance.correct", "Request or manage attendance corrections."),
                Permission(4, "attendance.approve", "Approve attendance correction requests."),
                Permission(5, "leave.read", "View leave balances and requests."),
                Permission(6, "leave.create", "Create leave requests."),
                Permission(7, "leave.approve", "Approve leave requests."),
                Permission(8, "employee.read", "View employee profiles."),
                Permission(9, "employee.create", "Create employees."),
                Permission(10, "employee.update", "Update employee profiles."),
                Permission(11, "benefit.read", "View benefit, mobility and expense category policies."),
                Permission(12, "benefit.manage", "Manage benefit, mobility and expense category policies."),
                Permission(13, "expense.read", "View expense claims."),
                Permission(14, "expense.create", "Create expense claims."),
                Permission(15, "expense.approve", "Approve expense claims."),
                Permission(16, "resignation.read", "View resignation requests."),
                Permission(17, "resignation.create", "Create resignation requests."),
                Permission(18, "resignation.approve", "Approve resignation and offboarding requests."),
                Permission(19, "policy.read", "View policies and documents."),
                Permission(20, "policy.manage", "Manage policies and documents."),
                Permission(21, "role.manage", "Manage system roles."),
                Permission(22, "permission.manage", "Manage permission assignments."),
                Permission(23, "audit.read", "View audit logs."),
                Permission(24, "report.read", "View reports."));
        }

        if (!db.RolePermissions.Any())
        {
            var id = 1;
            GrantRange(db, ref id, 1, "organization", Enumerable.Range(1, 24).ToArray());
            GrantRange(db, ref id, 2, "organization", 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 15, 16, 18, 19, 20, 23, 24);
            GrantRange(db, ref id, 3, "own", 1, 2, 3, 5, 6, 8, 10, 11, 13, 14, 16, 17, 19);
        }
    }

    private static void SeedReferenceData(PeopleOsDbContext db)
    {
        if (!db.Holidays.Any())
        {
            db.Holidays.AddRange(
                new Holiday { Id = 1, Name = "Eid Holiday", Date = new DateOnly(2026, 5, 27), Type = "Public Holiday" },
                new Holiday { Id = 2, Name = "Company Wellness Day", Date = new DateOnly(2026, 6, 7), Type = "Company Holiday" },
                new Holiday { Id = 3, Name = "Independence Day", Date = new DateOnly(2026, 8, 14), Type = "Public Holiday" });
        }

        if (!db.Announcements.Any())
        {
            db.Announcements.AddRange(
                new Announcement { Id = 1, Title = "Policy refresh", Body = "Attendance and leave policy updates are available in Policies.", PublishedOn = new DateOnly(2026, 5, 18), Audience = "All employees" },
                new Announcement { Id = 2, Title = "Probation cycle", Body = "Managers should complete open probation reviews before due dates.", PublishedOn = new DateOnly(2026, 5, 16), Audience = "Managers" },
                new Announcement { Id = 3, Title = "Document cleanup", Body = "Please upload missing employment records from the Profile section.", PublishedOn = new DateOnly(2026, 5, 14), Audience = "Employees" });
        }

        if (!db.QuickActions.Any())
        {
            db.QuickActions.AddRange(
                new QuickAction { Id = 1, Label = "Apply Leave", Target = "leave", DisplayOrder = 1 },
                new QuickAction { Id = 2, Label = "Correct Attendance", Target = "attendance", DisplayOrder = 2 },
                new QuickAction { Id = 3, Label = "Submit Expense", Target = "expense", DisplayOrder = 3 },
                new QuickAction { Id = 4, Label = "Open Policies", Target = "policies", DisplayOrder = 4 });
        }

        if (!db.LifecycleSignals.Any())
        {
            db.LifecycleSignals.AddRange(
                new LifecycleSignal { Id = 1, Label = "Open onboarding tasks", Value = "2", Status = "In progress", DisplayOrder = 1 },
                new LifecycleSignal { Id = 2, Label = "Documents pending", Value = "1", Status = "Needs attention", DisplayOrder = 2 },
                new LifecycleSignal { Id = 3, Label = "Probation reviews due", Value = "1", Status = "Pending", DisplayOrder = 3 });
        }

        if (!db.ActivityFeedItems.Any())
        {
            db.ActivityFeedItems.AddRange(
                new ActivityFeedItem { Id = 1, Message = "Leave request moved to manager approval", ActivityDate = new DateOnly(2026, 5, 18) },
                new ActivityFeedItem { Id = 2, Message = "Probation review opened for Bilal Raza", ActivityDate = new DateOnly(2026, 5, 17) },
                new ActivityFeedItem { Id = 3, Message = "Employment letter document marked ready", ActivityDate = new DateOnly(2026, 5, 16) },
                new ActivityFeedItem { Id = 4, Message = "Attendance sync completed for today", ActivityDate = new DateOnly(2026, 5, 18) });
        }
    }

    private static LifecycleStage Stage(int id, int employeeId, string stage, string owner, string status, int year, int month, int day, string summary) =>
        new() { Id = id, EmployeeId = employeeId, Stage = stage, Owner = owner, Status = status, DueDate = new DateOnly(year, month, day), Summary = summary };

    private static AttendanceRecord Attendance(int id, int employeeId, int year, int month, int day, int? inHour, int? inMinute, int? outHour, int? outMinute, string status, string source) =>
        new() { Id = id, EmployeeId = employeeId, WorkDate = new DateOnly(year, month, day), CheckIn = inHour.HasValue ? new TimeOnly(inHour.Value, inMinute ?? 0) : null, CheckOut = outHour.HasValue ? new TimeOnly(outHour.Value, outMinute ?? 0) : null, Status = status, Source = source };

    private static LeaveBalance Balance(int id, int employeeId, string type, decimal entitlement, decimal balance) =>
        new() { Id = id, EmployeeId = employeeId, LeaveType = type, AnnualEntitlement = entitlement, AvailableBalance = balance };

    private static EmployeeDocument Doc(int id, int employeeId, string name, string category, string status, int year, int month, int day) =>
        new() { Id = id, EmployeeId = employeeId, Name = name, Category = category, Status = status, UpdatedOn = new DateOnly(year, month, day) };

    private static AppPermission Permission(int id, string key, string description) =>
        new() { Id = id, Key = key, Description = description };

    private static void GrantRange(PeopleOsDbContext db, ref int id, int roleId, string scope, params int[] permissionIds)
    {
        foreach (var permissionId in permissionIds)
        {
            db.RolePermissions.Add(new AppRolePermission { Id = id++, RoleId = roleId, PermissionId = permissionId, Scope = scope });
        }
    }
}
