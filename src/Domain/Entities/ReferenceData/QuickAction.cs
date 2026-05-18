namespace PeopleOS.Api.Domain.Entities;

public class QuickAction
{
    public int Id { get; set; }
    public string Label { get; set; } = "";
    public string Target { get; set; } = "";
    public int DisplayOrder { get; set; }
}
