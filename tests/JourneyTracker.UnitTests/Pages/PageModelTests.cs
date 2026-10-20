using JourneyTracker.Web.Data;
using JourneyTracker.Web.Models;
using JourneyTracker.Web.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace JourneyTracker.UnitTests.Pages;

// these call each page's OnGet/OnPost directly (no browser, no http)
// every test gets its own fresh in-memory db from TestDb.Create() so they don't affect each other

// home page list of journeys
public class IndexPageTests
{
    // home page shows all 3 journeys, in order, at 0%
    [Fact]
    public async Task OnGet_ListsSeededJourneysInOrder()
    {
        using var db = TestDb.Create();
        var page = new IndexModel(db);

        await page.OnGetAsync();

        // same order as the seed data ids
        Assert.Equal(
            ["Hanoi to Ho Chi Minh City", "Tokyo to Osaka", "Melbourne to Sydney"],
            page.Journeys.Select(j => j.Name));
        // nothing logged yet so all 0%
        Assert.All(page.Journeys, j => Assert.Equal(0m, j.PercentComplete));
    }

    // home page % goes up when you log km
    [Fact]
    public async Task OnGet_PercentComesFromLoggedActivities()
    {
        using var db = TestDb.Create();
        // 500 + 350 = 850 of 1700 km = 50%
        TestDb.AddActivity(db, journeyId: 1, km: 500);
        TestDb.AddActivity(db, journeyId: 1, km: 350);
        var page = new IndexModel(db);

        await page.OnGetAsync();

        Assert.Equal(50m, page.Journeys.Single(j => j.Id == 1).PercentComplete);
        Assert.Equal(0m, page.Journeys.Single(j => j.Id == 2).PercentComplete); // other journeys untouched
    }
}

// progress page  /Progress/{id}
public class ProgressPageTests
{
    // test: a journey that doesn't exist gives a 404 page
    [Fact]
    public async Task OnGet_UnknownJourney_ReturnsNotFound()
    {
        using var db = TestDb.Create();
        var page = new ProgressModel(db);

        var result = await page.OnGetAsync(999); // no journey with id 999

        Assert.IsType<NotFoundResult>(result);
    }

    //  progress page works out km, last stop and next stop
    [Fact]
    public async Task OnGet_CalculatesProgressFromActivities()
    {
        using var db = TestDb.Create();
        // the example from the brief: 665 km in = reached hue, 95 km to da nang
        TestDb.AddActivity(db, journeyId: 1, km: 400);
        TestDb.AddActivity(db, journeyId: 1, km: 265);
        var page = new ProgressModel(db);

        var result = await page.OnGetAsync(1);

        Assert.IsType<PageResult>(result);
        Assert.Equal(665m, page.Progress.TotalDistanceKm);
        Assert.Equal("Hue", page.Progress.LastStopPassed?.Name);
        Assert.Equal("Da Nang", page.Progress.NextStop?.Name);
        Assert.Equal(95m, page.Progress.DistanceToNextStopKm);
    }

    // stops show in the right order along the route
    [Fact]
    public async Task OnGet_LoadsStopsInRouteOrder()
    {
        using var db = TestDb.Create();
        var page = new ProgressModel(db);

        await page.OnGetAsync(2);

        // stops have to come back sorted by distance, not random db order
        Assert.Equal(
            ["Tokyo", "Yokohama", "Shizuoka", "Nagoya", "Kyoto", "Osaka"],
            page.Journey.Stops.Select(s => s.Name));
    }

    // test: km from other journeys don't get mixed in
    [Fact]
    public async Task OnGet_OnlyCountsActivitiesForThatJourney()
    {
        using var db = TestDb.Create();
        // km logged on tokyo-osaka shouldn't count towards hanoi-hcmc
        TestDb.AddActivity(db, journeyId: 2, km: 100);
        var page = new ProgressModel(db);

        await page.OnGetAsync(1);

        Assert.Equal(0m, page.Progress.TotalDistanceKm);
        Assert.Equal("Hanoi", page.Progress.LastStopPassed?.Name);
    }

    // test: journey says complete once you hit the full distance
    [Fact]
    public async Task OnGet_FullDistance_MarksComplete()
    {
        using var db = TestDb.Create();
        TestDb.AddActivity(db, journeyId: 2, km: 510); // tokyo-osaka is 510 km total
        var page = new ProgressModel(db);

        await page.OnGetAsync(2);

        Assert.True(page.Progress.IsComplete);
        Assert.Equal(100m, page.Progress.PercentComplete);
    }
}

// log page  the form for adding a walk/ride
public class LogPageTests
{
    // makes a log page we can test
    // the log page needs a db, a logger and somewhere to keep form errors
    private static LogModel NewPage(AppDbContext db) =>
        TestDb.WithPageContext(new LogModel(db, NullLogger<LogModel>.Instance));

    // test: coming from a progress page, that journey is already picked in the dropdown
    [Fact]
    public async Task OnGet_PreselectsJourneyFromQuery()
    {
        using var db = TestDb.Create();
        var page = NewPage(db);

        await page.OnGetAsync(journeyId: 3); // correctly picking melb to sydney

        Assert.Equal(3, page.Input.JourneyId); // journey 3 is picked
        Assert.Equal(3, page.JourneyOptions.Count()); // all 3 journeys are in the dropdown
    }

    // opening /Log normally, nothing is picked but the dropdown still has all journeys
    [Fact]
    public async Task OnGet_WithoutJourney_StillLoadsOptions()
    {
        using var db = TestDb.Create();
        var page = NewPage(db);

        await page.OnGetAsync(journeyId: null); // like opening /Log from the nav bar

        Assert.Equal(0, page.Input.JourneyId); // nothing picked
        Assert.Equal(3, page.JourneyOptions.Count()); // dropdown still has all 3
    }

    // filling the form in properly saves it and goes to the progress page
    [Fact]
    public async Task OnPost_ValidInput_SavesActivityAndRedirectsToProgress()
    {
        using var db = TestDb.Create();
        var page = NewPage(db);
        //user filled in the form 12.5 km ride on tokyo-osaka
        page.Input = new LogModel.ActivityInput
        {
            JourneyId = 2,
            Type = ActivityType.Ride,
            DistanceKm = 12.5m,
            Date = new DateOnly(2026, 10, 10)
        };

        var result = await page.OnPostAsync(); 

        // send the user to that journey's progress page
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Progress", redirect.PageName);
        Assert.Equal(2, redirect.RouteValues?["id"]);

        // and it's in the db
        var saved = await db.Activities.SingleAsync();
        Assert.Equal(2, saved.JourneyId);
        Assert.Equal(ActivityType.Ride, saved.Type);
        Assert.Equal(12.5m, saved.DistanceKm);
        Assert.Equal(new DateOnly(2026, 10, 10), saved.Date);
    }

    // picking a journey that doesn't exist shows an error and saves nothing
    [Fact]
    public async Task OnPost_UnknownJourney_ShowsErrorAndDoesNotSave()
    {
        using var db = TestDb.Create();
        var page = NewPage(db);
        page.Input = new LogModel.ActivityInput { JourneyId = 999, DistanceKm = 5 };

        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.True(page.ModelState.ContainsKey("Input.JourneyId")); // error shows under the journey dropdown
        Assert.Empty(db.Activities);
        Assert.NotNull(page.JourneyOptions); // dropdown reloaded so the form still works
    }

    // test bad input (like -5 km) doesn't get saved
    [Fact]
    public async Task OnPost_InvalidModelState_DoesNotSave()
    {
        using var db = TestDb.Create();
        var page = NewPage(db);
        page.Input = new LogModel.ActivityInput { JourneyId = 1, DistanceKm = -5 };
        // calling OnPost directly skips validation, so fake the error the [Range] rule would add
        page.ModelState.AddModelError("Input.DistanceKm", "Distance must be greater than 0");

        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Empty(db.Activities);
    }
}

// history page all logged activities
public class HistoryPageTests
{
    // history is empty before anything is logged
    [Fact]
    public async Task OnGet_NoActivities_IsEmpty()
    {
        using var db = TestDb.Create();
        var page = new HistoryModel(db);

        await page.OnGetAsync();

        Assert.Empty(page.Activities);
    }

    // history shows newest first, with the journey name
    [Fact]
    public async Task OnGet_ShowsNewestFirstWithJourneyName()
    {
        using var db = TestDb.Create();
        // added out of order on purpose
        TestDb.AddActivity(db, journeyId: 1, km: 3, date: new DateOnly(2026, 10, 1));
        TestDb.AddActivity(db, journeyId: 3, km: 7, date: new DateOnly(2026, 10, 5));
        TestDb.AddActivity(db, journeyId: 2, km: 5, date: new DateOnly(2026, 10, 3));
        var page = new HistoryModel(db);

        await page.OnGetAsync();

        Assert.Equal([7m, 5m, 3m], page.Activities.Select(a => a.DistanceKm)); // 5th, 3rd, 1st oct
        Assert.Equal("Melbourne to Sydney", page.Activities[0].Journey?.Name);
    }
}

// error page  shown if something crashes in staging/prod
public class ErrorPageTests
{
    // error page shows a request id (helps find the error in the logs)
    [Fact]
    public void OnGet_SetsRequestId()
    {
        var page = TestDb.WithPageContext(new ErrorModel());
        page.PageContext.HttpContext.TraceIdentifier = "trace-123";

        page.OnGet();

        Assert.True(page.ShowRequestId);
        Assert.False(string.IsNullOrEmpty(page.RequestId));
    }
}
