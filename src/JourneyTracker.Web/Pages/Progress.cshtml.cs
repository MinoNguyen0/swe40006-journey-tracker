using JourneyTracker.Web.Data;
using JourneyTracker.Web.Models;
using JourneyTracker.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JourneyTracker.Web.Pages;

public class ProgressModel(AppDbContext db) : PageModel
{
    public Journey Journey { get; private set; } = null!;
    public JourneyProgress Progress { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var journey = await db.Journeys
            .AsNoTracking()
            .Include(j => j.Stops.OrderBy(s => s.DistanceFromStartKm))
            .FirstOrDefaultAsync(j => j.Id == id);

        if (journey is null) return NotFound();

        var distances = await db.Activities
            .Where(a => a.JourneyId == id)
            .Select(a => a.DistanceKm)
            .ToListAsync();

        Journey = journey;
        Progress = ProgressCalculator.Calculate(journey, distances);
        return Page();
    }
}
