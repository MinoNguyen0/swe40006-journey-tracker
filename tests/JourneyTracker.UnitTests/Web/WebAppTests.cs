using System.Net;
using System.Text.RegularExpressions;

namespace JourneyTracker.UnitTests.Web;

// these start the whole app (Program.cs, pages, html) and send real http requests to it
// each test makes its own factory = its own app + db, so tests don't share data
public class WebAppTests
{
    private static string Today => DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");

    // /health says Healthy and shows the commit sha
    [Fact]
    public async Task Health_ReturnsHealthyWithCommit()
    {
        await using var factory = new InMemoryWebFactory();
        var client = factory.CreateSeededClient();

        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"status\":\"Healthy\"", body);
        Assert.Contains(InMemoryWebFactory.CommitSha, body); // full sha in /health
    }

    // home page loads with the journeys and the sha in the footer
    [Fact]
    public async Task HomePage_ListsJourneysAndShowsShortShaInFooter()
    {
        await using var factory = new InMemoryWebFactory();
        var client = factory.CreateSeededClient();

        var html = await client.GetStringAsync("/");

        Assert.Contains("data-testid=\"journey-list\"", html);
        // just the city names so it works with the old and new UI
        Assert.Contains("Ho Chi Minh City", html);
        Assert.Contains("Sydney", html);
        Assert.Contains(">abc1234<", html); // footer shows the short (7 char) sha
    }

    // progress page loads and shows the stops
    [Fact]
    public async Task ProgressPage_ShowsStopsAndNextStop()
    {
        await using var factory = new InMemoryWebFactory();
        var client = factory.CreateSeededClient();

        var html = await client.GetStringAsync("/Progress/1");

        Assert.Contains("Ninh Binh", html); // one of the hanoi-hcmc stops
        Assert.Contains("data-testid=\"next-stop\"", html);
    }

    // progress page for a journey that doesn't exist gives 404
    [Fact]
    public async Task ProgressPage_UnknownJourney_Returns404()
    {
        await using var factory = new InMemoryWebFactory();
        var client = factory.CreateSeededClient();

        var response = await client.GetAsync("/Progress/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // history page loads and says nothing logged yet
    [Fact]
    public async Task HistoryPage_EmptyAtStart()
    {
        await using var factory = new InMemoryWebFactory();
        var client = factory.CreateSeededClient();

        var html = await client.GetStringAsync("/History");

        Assert.Contains("data-testid=\"history-empty\"", html);
    }

    // submitting the form saves it and it shows on progress + history
    [Fact]
    public async Task LogForm_ValidPost_RedirectsAndShowsUpEverywhere()
    {
        await using var factory = new InMemoryWebFactory();
        var client = factory.CreateSeededClient();

        // log the full 510 km of tokyo-osaka in one go
        var response = await PostActivity(client, journeyId: 2, distance: "510", date: Today);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Progress/2", response.Headers.Location?.OriginalString);

        // then check it shows up on the other pages
        var progress = await client.GetStringAsync("/Progress/2");
        Assert.Contains("data-testid=\"journey-complete\"", progress);

        var history = await client.GetStringAsync("/History");
        Assert.Contains("data-testid=\"history-table\"", history);
        Assert.Contains("Tokyo to Osaka", history);
    }

    // bad input shows the right error message (runs once per InlineData line)
    [Theory]
    [InlineData("-5", "2026-10-01", "Distance must be greater than 0")]
    [InlineData("5", "2030-01-01", "Date can&#x27;t be in the future.")] // &#x27; is how the html writes an apostrophe
    public async Task LogForm_InvalidPost_ShowsError(string distance, string date, string expectedError)
    {
        await using var factory = new InMemoryWebFactory();
        var client = factory.CreateSeededClient();

        var response = await PostActivity(client, journeyId: 1, distance, date);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // 200 = form shown again, not saved
        Assert.Contains(expectedError, html);
    }

    // error page loads
    [Fact]
    public async Task ErrorPage_Renders()
    {
        await using var factory = new InMemoryWebFactory();
        var client = factory.CreateSeededClient();

        var html = await client.GetStringAsync("/Error");

        Assert.Contains("Something went wrong", html);
    }

    //app still starts properly in production mode
    [Fact]
    public async Task Production_StartsAndServesPages()
    {
        // production mode turns on json logs + the error page, make sure it still starts
        await using var factory = new InMemoryWebFactory("Production");
        var client = factory.CreateSeededClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/")).StatusCode);
    }

    // fills in and submits the log form like a browser would
    private static async Task<HttpResponseMessage> PostActivity(HttpClient client, int journeyId, string distance, string date)
    {
        // forms need the hidden anti-forgery token from the page or the post gets rejected
        var form = await client.GetStringAsync($"/Log?journeyId={journeyId}");
        var token = Regex.Match(form, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;

        return await client.PostAsync("/Log", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.JourneyId"] = journeyId.ToString(),
            ["Input.Type"] = "Walk",
            ["Input.DistanceKm"] = distance,
            ["Input.Date"] = date,
            ["__RequestVerificationToken"] = token
        }));
    }
}
