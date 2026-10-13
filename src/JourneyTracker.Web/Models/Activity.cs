namespace JourneyTracker.Web.Models;

public enum ActivityType
{
    Walk,
    Ride
}

public class Activity
{
    public int Id { get; set; }
    public int JourneyId { get; set; }
    public Journey? Journey { get; set; }
    public ActivityType Type { get; set; }
    public decimal DistanceKm { get; set; }
    public DateOnly Date { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
