namespace PeopleOS.Api.Domain.Entities;

public class BenefitPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Coverage { get; set; } = "";
    public string Status { get; set; } = "";
    public string Description { get; set; } = "";
}
