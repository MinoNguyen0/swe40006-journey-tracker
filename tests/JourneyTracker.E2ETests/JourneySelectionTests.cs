
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Xunit;

namespace JourneyTracker.E2ETests;

public class JourneySelectionTests : PageTest
{
    [Fact]
    public async Task SelectingJourney_ShowsCorrectProgressPage()
    {
        // Allow GitHub Actions to supply a different URL.
        var baseUrl = Environment.GetEnvironmentVariable(
            "JOURNEYTRACKER_BASE_URL"
        ) ?? "http://localhost:8080";

        // 1. Open the Journey Tracker homepage.
        await Page.GotoAsync(baseUrl);

        await Expect(Page.GetByRole(
            AriaRole.Heading,
            new() { Name = "Choose a journey" }
        )).ToBeVisibleAsync();

        // 2. Select Melbourne to Sydney.
        var journeyCard = Page.Locator(".card")
            .Filter(new() { HasText = "Melbourne to Sydney" });

        await journeyCard.GetByRole(
            AriaRole.Link,
            new() { Name = "View progress" }
        ).ClickAsync();

        // 3. Verify navigation to the correct journey.
        await Expect(Page).ToHaveURLAsync(
            new Regex(@"/Progress/3/?$")
        );

        // 4. Verify the correct progress page is displayed.
        await Expect(Page.GetByRole(
            AriaRole.Heading,
            new() { Name = "Melbourne to Sydney" }
        )).ToBeVisibleAsync();

        // 5. Verify essential progress information.
        await Expect(
            Page.GetByText("Distance travelled")
        ).ToBeVisibleAsync();

        await Expect(
            Page.GetByText("Next stop")
        ).ToBeVisibleAsync();

        await Expect(
            Page.Locator("body")
        ).ToContainTextAsync("880 km");
    }
}