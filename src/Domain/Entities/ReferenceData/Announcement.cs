namespace PeopleOS.Api.Domain.Entities;

public class Announcement
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public DateOnly PublishedOn { get; set; }
    public string Audience { get; set; } = "";
}
