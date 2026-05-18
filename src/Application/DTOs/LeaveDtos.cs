namespace PeopleOS.Api.Application.DTOs;

public record LeaveBalanceResponseDto(int Id, int EmployeeId, string LeaveType, decimal AnnualEntitlement, decimal AvailableBalance);
public record LeaveRequestResponseDto(int Id, int EmployeeId, string LeaveType, DateOnly FromDate, DateOnly ToDate, decimal TotalDays, string Reason, string ContactDuringLeave, string AttachmentFileName, string AttachmentDataUrl, string Status);
public record LeaveResponseDto(IReadOnlyList<LeaveBalanceResponseDto> Balances, IReadOnlyList<LeaveRequestResponseDto> Requests);
public record CreateLeaveRequestDto(int EmployeeId, string LeaveType, DateOnly FromDate, DateOnly ToDate, string Reason, string ContactDuringLeave, string? AttachmentFileName, string? AttachmentDataUrl);
