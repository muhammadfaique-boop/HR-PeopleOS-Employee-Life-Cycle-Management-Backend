namespace PeopleOS.Api.Domain.Entities;

public class Holiday
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateOnly Date { get; set; }
    public string Type { get; set; } = "";
}
