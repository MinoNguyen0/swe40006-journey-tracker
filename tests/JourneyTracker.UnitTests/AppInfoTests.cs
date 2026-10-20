using JourneyTracker.Web;
using Microsoft.Extensions.Configuration;

namespace JourneyTracker.UnitTests;

// AppInfo gives the commit sha for the footer and /health
public class AppInfoTests
{
    // pretend COMMIT_SHA is set to this value
    private static AppInfo FromSha(string? sha) =>
        new(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["COMMIT_SHA"] = sha })
            .Build());

    // full sha is kept, short sha is the first 7 chars
    [Fact]
    public void UsesCommitShaAndShortensIt()
    {
        var info = FromSha("abc1234def5678");

        Assert.Equal("abc1234def5678", info.CommitSha);
        Assert.Equal("abc1234", info.ShortSha);
    }

    //no sha set = shows "local" (runs once per InlineData line)
    [Theory]
    [InlineData(null)] // not set, like running locally
    [InlineData("")]
    public void FallsBackToLocal(string? sha)
    {
        var info = FromSha(sha);

        Assert.Equal("local", info.CommitSha);
        Assert.Equal("local", info.ShortSha);
    }
}
