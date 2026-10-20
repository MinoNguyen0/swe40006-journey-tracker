using JourneyTracker.Web.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JourneyTracker.UnitTests.Web;

//setup tool for WebAppTests: runs the whole app in memory with an in-memory db instead of mysql, so no docker needed
// each test gets its own factory = its own app + db, so tests don't share data
public class InMemoryWebFactory(string environment = "Development") : WebApplicationFactory<Program>
{
    public const string CommitSha = "abc1234def5678"; // fake sha so we can check the footer + /health

    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environment);
        // app crashes without a connection string, but it's never used since we swap the db below
        builder.UseSetting("ConnectionStrings:Default", "Server=unused;Database=unused;User=unused;Password=unused;");
        builder.UseSetting("Database:MigrateOnStartup", "false"); // migrations only work on real mysql
        builder.UseSetting("COMMIT_SHA", CommitSha);

        // take out the mysql setup from Program.cs and put the in-memory db in instead
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(_dbName));
        });
    }

    // creates the tables + seed journeys, then gives back a client to send requests with
    // AllowAutoRedirect = false so tests can check redirects themselves
    public HttpClient CreateSeededClient()
    {
        using (var scope = Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        }

        return CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }
}
