namespace JourneyTracker.Web.Models;

public class Stop
{
    public int Id { get; set; }
    public int JourneyId { get; set; }
    public required string Name { get; set; }
    public decimal DistanceFromStartKm { get; set; }
}
