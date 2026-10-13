namespace JourneyTracker.Web;

public class AppInfo(IConfiguration configuration)
{
    public string CommitSha { get; } = configuration["COMMIT_SHA"] is { Length: > 0 } sha ? sha : "local";

    public string ShortSha => CommitSha.Length > 7 ? CommitSha[..7] : CommitSha;
}
