
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Xunit;

namespace JourneyTracker.E2ETests;

public class ProgressHistoryTests : PageTest
{
    [Fact]
    public async Task ViewingProgressAndHistory_ShowsJourneyDetails()
    {
        var baseUrl = Environment.GetEnvironmentVariable(
            "JOURNEYTRACKER_BASE_URL"
        ) ?? "http://localhost:8080";

        // 1. Open the Melbourne to Sydney progress page.
        await Page.GotoAsync(
            $"{baseUrl.TrimEnd('/')}/Progress/3"
        );

        // 2. Verify the correct journey.
        await Expect(Page.GetByRole(
            AriaRole.Heading,
            new() { Name = "Melbourne to Sydney" }
        )).ToBeVisibleAsync();

        // 3. Verify the progress information.
        await Expect(
            Page.GetByText("Distance travelled")
        ).ToBeVisibleAsync();

        await Expect(
            Page.GetByText("Last stop passed")
        ).ToBeVisibleAsync();

        await Expect(
            Page.GetByText("Next stop", new() { Exact = true })
        ).ToBeVisibleAsync();

        // 4. Verify the total distance and dynamic progress.
        var progressText = await Page.Locator("body")
            .InnerTextAsync();

        var distancePattern = new Regex(
            @"\b\d+(?:\.\d+)?\s+of\s+880\s+km\b"
        );

        Assert.Matches(distancePattern, progressText);

        // 5. Verify expected route stops.
        await Expect(Page.Locator(
            "span").Filter(new() {
                HasTextRegex = new Regex(@"^Seymour$") }
            )).ToBeVisibleAsync();

        await Expect(Page.Locator(
            "span").Filter(new() {
                HasTextRegex = new Regex(@"^Wangaratta$") }
            )).ToBeVisibleAsync();

        // 6. Navigate to Activity History.
        await Page.GetByRole(
            AriaRole.Link,
            new() { Name = "History" }
        ).ClickAsync();

        // 7. Verify History page navigation.
        await Expect(Page).ToHaveURLAsync(
            new Regex(@"/History/?$", RegexOptions.IgnoreCase)
        );

        await Expect(Page.GetByRole(
            AriaRole.Heading,
            new() { Name = "Activity history" }
        )).ToBeVisibleAsync();
        // 8. Verify History handles both valid states: an empty database or existing activities.
        var emptyState = Page.GetByText(
            "No activities logged yet."
        );

        if (await emptyState.IsVisibleAsync())
        {
            // Fresh database: verify the empty state.
            await Expect(emptyState).ToBeVisibleAsync();

            await Expect(Page.GetByRole(
                AriaRole.Link,
                new() { Name = "Log your first one." }
            )).ToBeVisibleAsync();
        }
        else
        {
            // Populated database: verify table columns.
            foreach (var column in new[]
            {
                "Date", "Type", "Distance", "Journey"
            })
            {
                await Expect(Page.GetByRole(
                    AriaRole.Columnheader,
                    new() { Name = column }
                )).ToBeVisibleAsync();
            }
        }
    }
}