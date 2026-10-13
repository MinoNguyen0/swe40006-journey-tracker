using JourneyTracker.Web.Models;
using JourneyTracker.Web.Services;

namespace JourneyTracker.UnitTests;

public class ProgressCalculatorTests
{
    private static Journey HanoiToHcmc() => new()
    {
        Id = 1,
        Name = "Hanoi to Ho Chi Minh City",
        TotalDistanceKm = 1700,
        Stops =
        [
            new Stop { Name = "Hanoi", DistanceFromStartKm = 0 },
            new Stop { Name = "Hue", DistanceFromStartKm = 665 },
            new Stop { Name = "Da Nang", DistanceFromStartKm = 760 },
            new Stop { Name = "Ho Chi Minh City", DistanceFromStartKm = 1700 }
        ]
    };

    [Fact]
    public void TotalDistance_SumsAllActivities()
    {
        Assert.Equal(17.5m, ProgressCalculator.TotalDistance([5m, 10m, 2.5m]));
    }

    [Fact]
    public void TotalDistance_IsZeroWithNoActivities()
    {
        Assert.Equal(0m, ProgressCalculator.TotalDistance([]));
    }

    [Theory]
    [InlineData(0, 1700, 0)]
    [InlineData(850, 1700, 50)]
    [InlineData(665, 1700, 39.1)]
    [InlineData(2000, 1700, 100)] // max 100%
    [InlineData(10, 0, 0)]        // divide by 0
    public void PercentComplete_IsRoundedAndCapped(decimal travelled, decimal total, decimal expected)
    {
        Assert.Equal(expected, ProgressCalculator.PercentComplete(travelled, total));
    }

    [Theory]
    [InlineData(0, "Hanoi")]
    [InlineData(664.9, "Hanoi")]
    [InlineData(665, "Hue")]   // exactly at the stop
    [InlineData(700, "Hue")]
    [InlineData(1800, "Ho Chi Minh City")]
    public void LastStopPassed_IsFurthestStopReached(decimal travelled, string expected)
    {
        Assert.Equal(expected, ProgressCalculator.LastStopPassed(HanoiToHcmc().Stops, travelled)?.Name);
    }

    [Theory]
    [InlineData(0, "Hue")]
    [InlineData(665, "Da Nang")]
    [InlineData(1699, "Ho Chi Minh City")]
    public void NextStop_IsFirstStopNotYetReached(decimal travelled, string expected)
    {
        Assert.Equal(expected, ProgressCalculator.NextStop(HanoiToHcmc().Stops, travelled)?.Name);
    }

    [Fact]
    public void NextStop_IsNullWhenJourneyComplete()
    {
        Assert.Null(ProgressCalculator.NextStop(HanoiToHcmc().Stops, 1700));
    }

    [Fact]
    public void Calculate_MatchesBriefExample_ReachedHue95KmToDaNang()
    {
        var progress = ProgressCalculator.Calculate(HanoiToHcmc(), [400m, 265m]);

        Assert.Equal(665m, progress.TotalDistanceKm);
        Assert.Equal("Hue", progress.LastStopPassed?.Name);
        Assert.Equal("Da Nang", progress.NextStop?.Name);
        Assert.Equal(95m, progress.DistanceToNextStopKm);
        Assert.False(progress.IsComplete);
    }

    [Fact]
    public void Calculate_MarksJourneyCompleteAtFullDistance()
    {
        var progress = ProgressCalculator.Calculate(HanoiToHcmc(), [1700m]);

        Assert.True(progress.IsComplete);
        Assert.Equal(100m, progress.PercentComplete);
        Assert.Null(progress.NextStop);
        Assert.Null(progress.DistanceToNextStopKm);
    }
}
