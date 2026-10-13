using System.ComponentModel.DataAnnotations;

namespace JourneyTracker.Web.Validation;

// server is on UTC, so allow +1 day for users ahead of UTC (melbourne, hanoi)
[AttributeUsage(AttributeTargets.Property)]
public class NotInFutureAttribute : ValidationAttribute
{
    public NotInFutureAttribute() : base("Date can't be in the future.") { }

    public override bool IsValid(object? value) =>
        value is not DateOnly date || IsNotInFuture(date, DateOnly.FromDateTime(DateTime.UtcNow));

    public static bool IsNotInFuture(DateOnly date, DateOnly utcToday) =>
        date <= utcToday.AddDays(1);
}
