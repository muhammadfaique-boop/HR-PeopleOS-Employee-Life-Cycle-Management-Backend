namespace PeopleOS.Api.Domain.Entities;

public class PolicyDocument
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string Version { get; set; } = "";
    public DateOnly PublishedOn { get; set; }
}
