namespace PeopleOS.Api.Domain.Entities;

public class ExpenseClaim
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string ClaimType { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public string Description { get; set; } = "";
    public string ReceiptFileName { get; set; } = "";
    public string ReceiptDataUrl { get; set; } = "";
    public string Status { get; set; } = "";
    public string LineManager { get; set; } = "";
}
