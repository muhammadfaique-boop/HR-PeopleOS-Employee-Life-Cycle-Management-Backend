using Microsoft.AspNetCore.Mvc;
using PeopleOS.Api.Models;
using PeopleOS.Api.Models.Dtos;
using PeopleOS.Api.Services;

namespace PeopleOS.Api.Controllers;

[ApiController]
[Route("api/peopleos")]
public class PeopleOsController(
    IDashboardService dashboardService,
    IEmployeeService employeeService,
    IAttendanceService attendanceService,
    ILeaveService leaveService,
    IBenefitsService benefitsService,
    IExpenseService expenseService,
    IResignationService resignationService,
    IPolicyService policyService,
    IApprovalService approvalService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> Dashboard() =>
        Ok(await dashboardService.GetDashboardAsync());

    [HttpGet("employees")]
    public async Task<ActionResult<List<Employee>>> Employees() =>
        Ok(await employeeService.GetEmployeesAsync());

    [HttpGet("employees/{id:int}")]
    public async Task<ActionResult<EmployeeProfileDto>> EmployeeProfile(int id)
    {
        var profile = await employeeService.GetProfileAsync(id);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpGet("attendance")]
    public async Task<ActionResult<AttendanceDto>> Attendance(int employeeId = 2) =>
        Ok(await attendanceService.GetAttendanceAsync(employeeId));

    [HttpPost("attendance/corrections")]
    public async Task<ActionResult<AttendanceCorrection>> CreateAttendanceCorrection(AttendanceCorrectionRequest request)
    {
        var correction = await attendanceService.CreateCorrectionAsync(request);
        return correction is null ? NotFound() : Ok(correction);
    }

    [HttpGet("attendance/download/{format}")]
    public async Task<IActionResult> DownloadAttendance(string format, int employeeId = 2)
    {
        var download = await attendanceService.DownloadAsync(format, employeeId);
        return File(download.Content, download.ContentType, download.FileName);
    }

    [HttpGet("leave")]
    public async Task<ActionResult<LeaveDto>> Leave(int employeeId = 2) =>
        Ok(await leaveService.GetLeaveAsync(employeeId));

    [HttpPost("leave/requests")]
    public async Task<ActionResult<LeaveRequest>> CreateLeaveRequest(LeaveRequestDto request)
    {
        var leave = await leaveService.CreateRequestAsync(request);
        return leave is null ? NotFound() : Ok(leave);
    }

    [HttpGet("benefits")]
    public async Task<ActionResult<List<BenefitPlan>>> Benefits() =>
        Ok(await benefitsService.GetBenefitsAsync());

    [HttpGet("expense")]
    public async Task<ActionResult<List<ExpenseClaim>>> ExpenseClaims(int employeeId = 2) =>
        Ok(await expenseService.GetClaimsAsync(employeeId));

    [HttpPost("expense/claims")]
    public async Task<ActionResult<ExpenseClaim>> CreateExpenseClaim(ExpenseClaimDto request)
    {
        var claim = await expenseService.CreateClaimAsync(request);
        return claim is null ? NotFound() : Ok(claim);
    }

    [HttpGet("resignations")]
    public async Task<ActionResult<List<ResignationRequest>>> Resignations(int employeeId = 2) =>
        Ok(await resignationService.GetResignationsAsync(employeeId));

    [HttpPost("resignations")]
    public async Task<ActionResult<ResignationRequest>> CreateResignation(ResignationDto request)
    {
        var resignation = await resignationService.CreateResignationAsync(request);
        return resignation is null ? NotFound() : Ok(resignation);
    }

    [HttpPatch("employees/{id:int}/profile")]
    public async Task<ActionResult<Employee>> UpdateProfile(int id, ProfileUpdateDto request)
    {
        var employee = await employeeService.UpdateProfileAsync(id, request);
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpGet("policies")]
    public async Task<ActionResult<List<PolicyDocument>>> Policies() =>
        Ok(await policyService.GetPoliciesAsync());

    [HttpGet("approvals")]
    public async Task<ActionResult<List<ApprovalTask>>> Approvals() =>
        Ok(await approvalService.GetApprovalsAsync());
}
