using PeopleOS.Api.Models;

namespace PeopleOS.Api.Data;

public static class PeopleOsSeed
{
    public static void Seed(PeopleOsDbContext db)
    {
        if (db.Users.Any())
        {
            return;
        }

        db.Employees.AddRange(
            new Employee
            {
                Id = 1,
                EmployeeCode = "EMP-1001",
                FullName = "Ayesha Khan",
                Email = "ayesha.khan@peopleos.dev",
                Department = "People Operations",
                Position = "HR Operations Lead",
                Manager = "Sara Ahmed",
                LifecycleStatus = "Active",
                JoiningDate = new DateOnly(2023, 3, 6),
                ProfileCompletion = 96,
                WorkLocation = "Lahore"
            },
            new Employee
            {
                Id = 2,
                EmployeeCode = "EMP-1042",
                FullName = "Muhammad Faique",
                Email = "muhammad.faique@peopleos.dev",
                Department = "Engineering",
                Position = "Senior Software Engineer",
                Manager = "Ayesha Khan",
                LifecycleStatus = "Active",
                JoiningDate = new DateOnly(2026, 1, 16),
                ProfileCompletion = 88,
                WorkLocation = "Lahore"
            },
            new Employee
            {
                Id = 3,
                EmployeeCode = "EMP-1077",
                FullName = "Bilal Raza",
                Email = "bilal.raza@peopleos.dev",
                Department = "Engineering",
                Position = "Frontend Engineer",
                Manager = "Muhammad Faique",
                LifecycleStatus = "Probation",
                JoiningDate = new DateOnly(2026, 4, 1),
                ProfileCompletion = 72,
                WorkLocation = "Karachi"
            });

        db.Users.AddRange(
            new AppUser { Id = 1, Email = "admin@peopleos.dev", Password = "Admin@123", Role = "Admin", EmployeeId = 1 },
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

        db.AttendanceCorrections.AddRange(
            new AttendanceCorrection
            {
                Id = 1,
                EmployeeId = 2,
                WorkDate = new DateOnly(2026, 5, 17),
                RequestedChange = "Update check-in to 09:15",
                Reason = "Device queue synced late",
                Status = "Pending manager review",
                Approver = "Ayesha Khan"
            });

        db.LeaveBalances.AddRange(
            Balance(1, 2, "Casual Leave", 12, 7),
            Balance(2, 2, "Sick Leave", 10, 9),
            Balance(3, 2, "Annual Leave", 18, 14),
            Balance(4, 2, "Work from Home", 24, 20));

        db.LeaveRequests.AddRange(
            new LeaveRequest
            {
                Id = 1,
                EmployeeId = 2,
                LeaveType = "Casual Leave",
                FromDate = new DateOnly(2026, 5, 14),
                ToDate = new DateOnly(2026, 5, 14),
                TotalDays = 1,
                Reason = "Family medical support",
                ContactDuringLeave = "Available on mobile",
                Status = "Approval Required"
            },
            new LeaveRequest
            {
                Id = 2,
                EmployeeId = 2,
                LeaveType = "Work from Home",
                FromDate = new DateOnly(2026, 5, 21),
                ToDate = new DateOnly(2026, 5, 21),
                TotalDays = 1,
                Reason = "Deep work day",
                ContactDuringLeave = "Teams and phone",
                Status = "Approved"
            });

        db.BenefitPlans.AddRange(
            new BenefitPlan { Id = 1, Name = "Health Insurance", Category = "Medical", Coverage = "Employee, spouse and children", Status = "Active" },
            new BenefitPlan { Id = 2, Name = "Vehicle Benefit", Category = "Mobility", Coverage = "Role-based allowance categories", Status = "Policy managed" },
            new BenefitPlan { Id = 3, Name = "Expense Categories", Category = "Reimbursement", Coverage = "Business and OPD categories", Status = "Active" });

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
            new ApprovalTask { Id = 1, Type = "Leave", Subject = "Casual Leave - Muhammad Faique", Requester = "Muhammad Faique", ApproverRole = "Manager", Status = "Pending", DueDate = new DateOnly(2026, 5, 19) },
            new ApprovalTask { Id = 2, Type = "Probation", Subject = "Probation review - Bilal Raza", Requester = "People Operations", ApproverRole = "Manager", Status = "Pending", DueDate = new DateOnly(2026, 6, 28) },
            new ApprovalTask { Id = 3, Type = "Document", Subject = "Employment letter verification", Requester = "Muhammad Faique", ApproverRole = "HR", Status = "In review", DueDate = new DateOnly(2026, 5, 22) });

        db.SaveChanges();
    }

    private static LifecycleStage Stage(int id, int employeeId, string stage, string owner, string status, int year, int month, int day, string summary) =>
        new() { Id = id, EmployeeId = employeeId, Stage = stage, Owner = owner, Status = status, DueDate = new DateOnly(year, month, day), Summary = summary };

    private static AttendanceRecord Attendance(int id, int employeeId, int year, int month, int day, int? inHour, int? inMinute, int? outHour, int? outMinute, string status, string source) =>
        new()
        {
            Id = id,
            EmployeeId = employeeId,
            WorkDate = new DateOnly(year, month, day),
            CheckIn = inHour.HasValue ? new TimeOnly(inHour.Value, inMinute ?? 0) : null,
            CheckOut = outHour.HasValue ? new TimeOnly(outHour.Value, outMinute ?? 0) : null,
            Status = status,
            Source = source
        };

    private static LeaveBalance Balance(int id, int employeeId, string type, decimal entitlement, decimal balance) =>
        new() { Id = id, EmployeeId = employeeId, LeaveType = type, AnnualEntitlement = entitlement, AvailableBalance = balance };

    private static EmployeeDocument Doc(int id, int employeeId, string name, string category, string status, int year, int month, int day) =>
        new() { Id = id, EmployeeId = employeeId, Name = name, Category = category, Status = status, UpdatedOn = new DateOnly(year, month, day) };
}
