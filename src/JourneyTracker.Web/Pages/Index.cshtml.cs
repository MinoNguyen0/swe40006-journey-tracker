using JourneyTracker.Web.Data;
using JourneyTracker.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JourneyTracker.Web.Pages;

public class IndexModel(AppDbContext db) : PageModel
{
    public record JourneySummary(int Id, string Name, decimal TotalDistanceKm, decimal TravelledKm, decimal PercentComplete);

    public List<JourneySummary> Journeys { get; private set; } = [];
    public decimal TotalLoggedKm { get; private set; }
    public int ActivityCount { get; private set; }

    public async Task OnGetAsync()
    {
        var journeys = await db.Journeys
            .AsNoTracking()
            .OrderBy(j => j.Id)
            .Select(j => new { j.Id, j.Name, j.TotalDistanceKm, Travelled = j.Activities.Sum(a => a.DistanceKm) })
            .ToListAsync();

        Journeys = journeys
            .Select(j => new JourneySummary(j.Id, j.Name, j.TotalDistanceKm, j.Travelled,
                ProgressCalculator.PercentComplete(j.Travelled, j.TotalDistanceKm)))
            .ToList();

        TotalLoggedKm = Journeys.Sum(j => j.TravelledKm);
        ActivityCount = await db.Activities.CountAsync();
    }
}
