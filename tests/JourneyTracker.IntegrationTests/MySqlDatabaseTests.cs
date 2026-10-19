
using JourneyTracker.Web.Data;
using JourneyTracker.Web.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JourneyTracker.IntegrationTests;

public class MySqlDatabaseTests
{
    [Fact]
    public async Task Database_CanConnect_AndLoadSeededJourneys()
    {
        // Get the isolated test database connection
        var connectionString =
            Environment.GetEnvironmentVariable(
                "JOURNEYTRACKER_TEST_CONNECTION");

        Assert.False(
            string.IsNullOrWhiteSpace(connectionString),
            "Test database connection string is missing.");

        // Configure the real MySQL provider
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString))
            .Options;

        await using var db = new AppDbContext(options);

        // Verify that MySQL accepts connections
        Assert.True(await db.Database.CanConnectAsync());

        // Create the schema and seed data if necessary
        await db.Database.EnsureCreatedAsync();

        // Verify that predefined journeys are available
        var journeys = await db.Journeys
            .AsNoTracking()
            .ToListAsync();

        Assert.NotEmpty(journeys);
    }

    
[Fact]
public async Task Activity_CanBeSavedAndRetrieved_FromMySql()
{
    var connectionString =
        Environment.GetEnvironmentVariable(
            "JOURNEYTRACKER_TEST_CONNECTION");

    Assert.False(string.IsNullOrWhiteSpace(connectionString));

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString))
        .Options;

    await using var db = new AppDbContext(options);

    await db.Database.EnsureCreatedAsync();

    // Use a transaction so test data can be rolled back.
    await using var transaction =
        await db.Database.BeginTransactionAsync();

    // Retrieve an existing journey.
    var journey = await db.Journeys
        .AsNoTracking()
        .FirstAsync();

    var activity = new Activity
    {
        JourneyId = journey.Id,
        Type = Enum.GetValues<ActivityType>()[0],
        DistanceKm = 4.25m,
        Date = DateOnly.FromDateTime(DateTime.UtcNow),
        CreatedAtUtc = DateTime.UtcNow
    };

    // Save activity to MySQL.
    db.Activities.Add(activity);
    await db.SaveChangesAsync();

    // Clear tracked objects so the query reads persisted data.
    db.ChangeTracker.Clear();

    // Retrieve the newly saved record.
    var saved = await db.Activities
        .AsNoTracking()
        .SingleAsync(a => a.Id == activity.Id);

    // Verify saved values.
    Assert.Equal(journey.Id, saved.JourneyId);
    Assert.Equal(4.25m, saved.DistanceKm);
    Assert.Equal(activity.Type, saved.Type);
    Assert.Equal(activity.Date, saved.Date);

    // Remove test changes.
    await transaction.RollbackAsync();
}

}