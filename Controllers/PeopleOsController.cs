using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeopleOS.Api.Data;
using PeopleOS.Api.Models;

namespace PeopleOS.Api.Controllers;

[ApiController]
[Route("api/peopleos")]
public class PeopleOsController(PeopleOsDbContext db) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> Dashboard()
    {
        var employees = await db.Employees.OrderBy(x => x.FullName).ToListAsync();
        var activeEmployee = employees.First(x => x.Email == "muhammad.faique@peopleos.dev");
        var pendingTasks = await db.ApprovalTasks.Where(x => x.Status != "Approved").ToListAsync();

        return Ok(new
        {
            metrics = new[]
            {
                new { label = "Active employees", value = employees.Count(x => x.LifecycleStatus == "Active").ToString(), accent = "teal" },
                new { label = "On probation", value = employees.Count(x => x.LifecycleStatus == "Probation").ToString(), accent = "amber" },
                new { label = "Pending approvals", value = pendingTasks.Count.ToString(), accent = "violet" },
                new { label = "Profile completion", value = $"{activeEmployee.ProfileCompletion}%", accent = "rose" }
            },
            activeEmployee,
            employees,
            lifecycle = await db.LifecycleStages.OrderBy(x => x.DueDate).ToListAsync(),
            approvals = pendingTasks.OrderBy(x => x.DueDate),
            recentActivity = new[]
            {
                "Leave request moved to manager approval",
                "Probation review opened for Bilal Raza",
                "Employment letter document marked ready",
                "Attendance sync completed for today"
            }
        });
    }

    [HttpGet("employees")]
    public async Task<ActionResult<List<Employee>>> Employees() =>
        await db.Employees.OrderBy(x => x.Department).ThenBy(x => x.FullName).ToListAsync();

    [HttpGet("employees/{id:int}")]
    public async Task<ActionResult<object>> EmployeeProfile(int id)
    {
        var employee = await db.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            employee,
            documents = await db.Documents.Where(x => x.EmployeeId == id).ToListAsync(),
            lifecycle = await db.LifecycleStages.Where(x => x.EmployeeId == id).ToListAsync(),
            leaveBalances = await db.LeaveBalances.Where(x => x.EmployeeId == id).ToListAsync()
        });
    }

    [HttpGet("attendance")]
    public async Task<ActionResult<object>> Attendance(int employeeId = 2)
    {
        return Ok(new
        {
            records = await db.AttendanceRecords.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.WorkDate).ToListAsync(),
            corrections = await db.AttendanceCorrections.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.WorkDate).ToListAsync()
        });
    }

    [HttpGet("leave")]
    public async Task<ActionResult<object>> Leave(int employeeId = 2)
    {
        return Ok(new
        {
            balances = await db.LeaveBalances.Where(x => x.EmployeeId == employeeId).ToListAsync(),
            requests = await db.LeaveRequests.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.FromDate).ToListAsync()
        });
    }

    [HttpGet("benefits")]
    public async Task<ActionResult<List<BenefitPlan>>> Benefits() =>
        await db.BenefitPlans.OrderBy(x => x.Category).ToListAsync();

    [HttpGet("policies")]
    public async Task<ActionResult<List<PolicyDocument>>> Policies() =>
        await db.Policies.OrderByDescending(x => x.PublishedOn).ToListAsync();

    [HttpGet("approvals")]
    public async Task<ActionResult<List<ApprovalTask>>> Approvals() =>
        await db.ApprovalTasks.OrderBy(x => x.DueDate).ToListAsync();
}
