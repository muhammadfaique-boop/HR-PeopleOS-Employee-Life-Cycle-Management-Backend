namespace PeopleOS.Api.Application.DTOs;

public record ExpenseClaimResponseDto(int Id, int EmployeeId, string ClaimType, string Category, decimal Amount, DateOnly ExpenseDate, string Description, string ReceiptFileName, string ReceiptDataUrl, string Status, string LineManager);
public record CreateExpenseClaimRequestDto(int EmployeeId, string ClaimType, string Category, decimal Amount, DateOnly ExpenseDate, string Description, string? ReceiptFileName, string? ReceiptDataUrl);
