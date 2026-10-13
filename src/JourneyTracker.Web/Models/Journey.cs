namespace JourneyTracker.Web.Models;

public class Journey
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal TotalDistanceKm { get; set; }

    // includes start and end city
    public List<Stop> Stops { get; set; } = [];
    public List<Activity> Activities { get; set; } = [];
}
