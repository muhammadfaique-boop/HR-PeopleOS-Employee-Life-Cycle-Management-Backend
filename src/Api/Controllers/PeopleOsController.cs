using Microsoft.AspNetCore.Mvc;
using PeopleOS.Api.Application.DTOs;
using PeopleOS.Api.Application.Interfaces;

namespace PeopleOS.Api.Api.Controllers;

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
    IApprovalService approvalService,
    INotificationService notificationService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardResponseDto>> Dashboard() =>
        Ok(await dashboardService.GetDashboardAsync());

    [HttpGet("employees")]
    public async Task<ActionResult<List<EmployeeResponseDto>>> Employees() =>
        Ok(await employeeService.GetEmployeesAsync());

    [HttpGet("employees/{id:int}")]
    public async Task<ActionResult<EmployeeProfileResponseDto>> EmployeeProfile(int id)
    {
        var profile = await employeeService.GetProfileAsync(id);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpGet("attendance")]
    public async Task<ActionResult<AttendanceResponseDto>> Attendance(int employeeId = 2) =>
        Ok(await attendanceService.GetAttendanceAsync(employeeId));

    [HttpPost("attendance/corrections")]
    public async Task<ActionResult<AttendanceCorrectionResponseDto>> CreateAttendanceCorrection(CreateAttendanceCorrectionRequestDto request)
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
    public async Task<ActionResult<LeaveResponseDto>> Leave(int employeeId = 2) =>
        Ok(await leaveService.GetLeaveAsync(employeeId));

    [HttpPost("leave/requests")]
    public async Task<ActionResult<LeaveRequestResponseDto>> CreateLeaveRequest(CreateLeaveRequestDto request)
    {
        var leave = await leaveService.CreateRequestAsync(request);
        return leave is null ? NotFound() : Ok(leave);
    }

    [HttpGet("benefits")]
    public async Task<ActionResult<List<BenefitPlanResponseDto>>> Benefits() =>
        Ok(await benefitsService.GetBenefitsAsync());

    [HttpGet("expense")]
    public async Task<ActionResult<List<ExpenseClaimResponseDto>>> ExpenseClaims(int employeeId = 2) =>
        Ok(await expenseService.GetClaimsAsync(employeeId));

    [HttpPost("expense/claims")]
    public async Task<ActionResult<ExpenseClaimResponseDto>> CreateExpenseClaim(CreateExpenseClaimRequestDto request)
    {
        var claim = await expenseService.CreateClaimAsync(request);
        return claim is null ? NotFound() : Ok(claim);
    }

    [HttpGet("resignations")]
    public async Task<ActionResult<List<ResignationResponseDto>>> Resignations(int employeeId = 2) =>
        Ok(await resignationService.GetResignationsAsync(employeeId));

    [HttpPost("resignations")]
    public async Task<ActionResult<ResignationResponseDto>> CreateResignation(CreateResignationRequestDto request)
    {
        var resignation = await resignationService.CreateResignationAsync(request);
        return resignation is null ? NotFound() : Ok(resignation);
    }

    [HttpPatch("employees/{id:int}/profile")]
    public async Task<ActionResult<EmployeeResponseDto>> UpdateProfile(int id, UpdateEmployeeProfileRequestDto request)
    {
        var employee = await employeeService.UpdateProfileAsync(id, request);
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpGet("policies")]
    public async Task<ActionResult<List<PolicyDocumentResponseDto>>> Policies() =>
        Ok(await policyService.GetPoliciesAsync());

    [HttpGet("approvals")]
    public async Task<ActionResult<List<ApprovalTaskResponseDto>>> Approvals() =>
        Ok(await approvalService.GetApprovalsAsync());

    [HttpPost("approvals/{approvalId:int}/decision")]
    public async Task<ActionResult<ApprovalTaskResponseDto>> DecideApproval(int approvalId, ApprovalDecisionRequestDto request)
    {
        if (!request.Decision.Equals("Approved", StringComparison.OrdinalIgnoreCase) &&
            !request.Decision.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Decision must be Approved or Rejected.");
        }

        var approval = await approvalService.DecideAsync(approvalId, request);
        return approval is null ? NotFound() : Ok(approval);
    }

    [HttpGet("notifications")]
    public async Task<ActionResult<List<EmployeeNotificationResponseDto>>> Notifications(int employeeId = 2) =>
        Ok(await notificationService.GetNotificationsAsync(employeeId));

    [HttpPost("notifications/read")]
    public async Task<IActionResult> MarkNotificationsRead(int employeeId = 2)
    {
        await notificationService.MarkAllReadAsync(employeeId);
        return NoContent();
    }

    [HttpDelete("notifications/{notificationId:int}")]
    public async Task<IActionResult> ClearNotification(int notificationId, int employeeId = 2)
    {
        var cleared = await notificationService.ClearAsync(employeeId, notificationId);
        return cleared ? NoContent() : NotFound();
    }
}
