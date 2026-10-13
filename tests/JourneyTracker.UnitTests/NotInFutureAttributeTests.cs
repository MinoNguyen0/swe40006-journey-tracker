using JourneyTracker.Web.Validation;

namespace JourneyTracker.UnitTests;

public class NotInFutureAttributeTests
{
    private static readonly DateOnly UtcToday = new(2026, 10, 13);

    [Theory]
    [InlineData(2026, 10, 1, true)]
    [InlineData(2026, 10, 13, true)]
    [InlineData(2026, 10, 14, true)]  // +1 day allowed
    [InlineData(2026, 10, 15, false)]
    public void IsNotInFuture_AllowsUpToOneDayAheadOfUtc(int y, int m, int d, bool expected)
    {
        Assert.Equal(expected, NotInFutureAttribute.IsNotInFuture(new DateOnly(y, m, d), UtcToday));
    }

    [Fact]
    public void IsValid_RejectsFarFutureDate()
    {
        var attribute = new NotInFutureAttribute();
        Assert.False(attribute.IsValid(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30)));
        Assert.True(attribute.IsValid(DateOnly.FromDateTime(DateTime.UtcNow)));
    }
}
