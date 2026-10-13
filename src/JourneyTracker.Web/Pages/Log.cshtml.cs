using System.ComponentModel.DataAnnotations;
using JourneyTracker.Web.Data;
using JourneyTracker.Web.Models;
using JourneyTracker.Web.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace JourneyTracker.Web.Pages;

public class LogModel(AppDbContext db, ILogger<LogModel> logger) : PageModel
{
    public class ActivityInput
    {
        [Display(Name = "Journey")]
        public int JourneyId { get; set; }

        public ActivityType Type { get; set; }

        [Display(Name = "Distance (km)")]
        [Range(0.01, 1000, ErrorMessage = "Distance must be greater than 0 and at most 1000 km.")]
        public decimal DistanceKm { get; set; }

        [DataType(DataType.Date)]
        [NotInFuture]
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    }

    [BindProperty]
    public ActivityInput Input { get; set; } = new();

    public SelectList JourneyOptions { get; private set; } = null!;

    public async Task OnGetAsync(int? journeyId)
    {
        if (journeyId is not null) Input.JourneyId = journeyId.Value;
        await LoadJourneyOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await db.Journeys.AnyAsync(j => j.Id == Input.JourneyId))
        {
            ModelState.AddModelError("Input.JourneyId", "Choose a journey.");
        }

        if (!ModelState.IsValid)
        {
            await LoadJourneyOptionsAsync();
            return Page();
        }

        db.Activities.Add(new Activity
        {
            JourneyId = Input.JourneyId,
            Type = Input.Type,
            DistanceKm = Input.DistanceKm,
            Date = Input.Date,
            CreatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        logger.LogInformation("Logged {Type} of {DistanceKm} km on journey {JourneyId}",
            Input.Type, Input.DistanceKm, Input.JourneyId);

        return RedirectToPage("/Progress", new { id = Input.JourneyId });
    }

    private async Task LoadJourneyOptionsAsync()
    {
        var journeys = await db.Journeys.AsNoTracking().OrderBy(j => j.Id).ToListAsync();
        JourneyOptions = new SelectList(journeys, nameof(Journey.Id), nameof(Journey.Name));
    }
}
