using JourneyTracker.Web.Data;
using JourneyTracker.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace JourneyTracker.UnitTests.Pages;
// Helper code for the PageModelTests.cs tests. 
//creates a new in-memory db per test, with the 3 seeded journeys, and lets you log activities quickly
//also sets up a fake request context so you can use ModelState in the page tests.
public static class TestDb
{
    // new empty db per test, with the 3 seeded journeys
    // random name = separate db each time
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    // quick way to log an activity in a test
    public static void AddActivity(AppDbContext db, int journeyId, decimal km, DateOnly? date = null)
    {
        db.Activities.Add(new Activity
        {
            JourneyId = journeyId,
            Type = ActivityType.Walk,
            DistanceKm = km,
            Date = date ?? new DateOnly(2026, 10, 1),
            CreatedAtUtc = DateTime.UtcNow
        });
        db.SaveChanges();
        db.ChangeTracker.Clear(); // forget what we added so the page has to load it from the db
    }

    // pages need this fake request context before you can use ModelState
    public static T WithPageContext<T>(T page) where T : PageModel
    {
        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new PageActionDescriptor(), modelState);
        page.PageContext = new PageContext(actionContext)
        {
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), modelState)
        };
        return page;
    }
}
