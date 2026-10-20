
using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Xunit;

namespace JourneyTracker.E2ETests;

public class ActivityLoggingTests : PageTest
{
    [Fact]
    public async Task LoggingWalk_SavesActivityInHistory()
    {
        var baseUrl = Environment.GetEnvironmentVariable(
            "JOURNEYTRACKER_BASE_URL"
        ) ?? "http://localhost:8080";

        // Generate a distinctive distance for this test run.
        // A new value helps avoid matching an older activity.
        var distanceKm = (Random.Shared.Next(1000, 9000) / 100m)
            .ToString("0.00", CultureInfo.InvariantCulture);

        // 1. Open the activity logging form.
        await Page.GotoAsync(
            $"{baseUrl.TrimEnd('/')}/Log?journeyId=3"
        );

        await Expect(Page.GetByRole(
            AriaRole.Heading,
            new() { Name = "Log an activity" }
        )).ToBeVisibleAsync();

        // 2. Select the journey and activity type.
        await Page.Locator("select").Nth(0)
            .SelectOptionAsync(new SelectOptionValue
            {
                Label = "Melbourne to Sydney"
            });

        await Page.Locator("select").Nth(1)
            .SelectOptionAsync(new SelectOptionValue
            {
                Label = "Walk"
            });

        // 3. Enter distance and date.
        await Page.GetByLabel("Distance (km)")
            .FillAsync(distanceKm);

        await Page.Locator("input[type='date']")
            .FillAsync(DateTime.Today.ToString(
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture
            ));

        // 4. Submit the activity.
        await Page.GetByRole(
            AriaRole.Button,
            new() { Name = "Save" }
        ).ClickAsync();

        // 5. Open History to verify the saved activity.
        await Page.GetByRole(
            AriaRole.Link,
            new() { Name = "History" }
        ).ClickAsync();

        // 6. Confirm the saved distance appears in history.
        // The UI removes trailing zeros (e.g. 45.60 becomes 45.6).
        var expectedDistance = decimal.Parse(
            distanceKm,
            CultureInfo.InvariantCulture
        ).ToString("0.##", CultureInfo.InvariantCulture);

        await Expect(Page.GetByTestId("history-table"))
            .ToContainTextAsync($"{expectedDistance} km");
    }
}
