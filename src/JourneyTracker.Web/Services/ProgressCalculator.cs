using JourneyTracker.Web.Models;

namespace JourneyTracker.Web.Services;

public record JourneyProgress(
    decimal TotalDistanceKm,
    decimal PercentComplete,
    Stop? LastStopPassed,
    Stop? NextStop,
    decimal? DistanceToNextStopKm,
    bool IsComplete);

public static class ProgressCalculator
{
    public static decimal TotalDistance(IEnumerable<decimal> activityDistancesKm) =>
        activityDistancesKm.Sum();

    public static decimal PercentComplete(decimal travelledKm, decimal journeyKm)
    {
        if (journeyKm <= 0) return 0;
        var percent = travelledKm / journeyKm * 100;
        return Math.Round(Math.Min(percent, 100), 1);
    }

    public static Stop? LastStopPassed(IEnumerable<Stop> stops, decimal travelledKm) =>
        stops.Where(s => s.DistanceFromStartKm <= travelledKm)
             .OrderBy(s => s.DistanceFromStartKm)
             .LastOrDefault();

    public static Stop? NextStop(IEnumerable<Stop> stops, decimal travelledKm) =>
        stops.Where(s => s.DistanceFromStartKm > travelledKm)
             .OrderBy(s => s.DistanceFromStartKm)
             .FirstOrDefault();

    public static JourneyProgress Calculate(Journey journey, IEnumerable<decimal> activityDistancesKm)
    {
        var travelled = TotalDistance(activityDistancesKm);
        var next = NextStop(journey.Stops, travelled);

        return new JourneyProgress(
            TotalDistanceKm: travelled,
            PercentComplete: PercentComplete(travelled, journey.TotalDistanceKm),
            LastStopPassed: LastStopPassed(journey.Stops, travelled),
            NextStop: next,
            DistanceToNextStopKm: next is null ? null : next.DistanceFromStartKm - travelled,
            IsComplete: travelled >= journey.TotalDistanceKm);
    }
}
