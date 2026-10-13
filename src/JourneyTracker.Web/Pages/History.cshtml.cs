using JourneyTracker.Web.Data;
using JourneyTracker.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace JourneyTracker.Web.Pages;

public class HistoryModel(AppDbContext db) : PageModel
{
    public List<Activity> Activities { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Activities = await db.Activities
            .AsNoTracking()
            .Include(a => a.Journey)
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.CreatedAtUtc)
            .ToListAsync();
    }
}
