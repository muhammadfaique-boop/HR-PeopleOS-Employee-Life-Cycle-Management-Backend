namespace PeopleOS.Api.Domain.Entities;

public class LifecycleSignal
{
    public int Id { get; set; }
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string Status { get; set; } = "";
    public int DisplayOrder { get; set; }
}
