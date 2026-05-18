namespace PeopleOS.Api.Domain.Entities;

public class AppRole
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string DefaultScope { get; set; } = "";
}
