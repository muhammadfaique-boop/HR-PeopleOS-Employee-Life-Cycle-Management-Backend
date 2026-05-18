namespace PeopleOS.Api.Domain.Entities;

public class ActivityFeedItem
{
    public int Id { get; set; }
    public string Message { get; set; } = "";
    public DateOnly ActivityDate { get; set; }
}
