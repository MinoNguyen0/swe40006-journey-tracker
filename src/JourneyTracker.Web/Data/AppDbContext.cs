using JourneyTracker.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace JourneyTracker.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Journey> Journeys => Set<Journey>();
    public DbSet<Stop> Stops => Set<Stop>();
    public DbSet<Activity> Activities => Set<Activity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Journey>(e =>
        {
            e.Property(j => j.Name).HasMaxLength(100);
            e.Property(j => j.TotalDistanceKm).HasPrecision(8, 2);
        });

        modelBuilder.Entity<Stop>(e =>
        {
            e.Property(s => s.Name).HasMaxLength(100);
            e.Property(s => s.DistanceFromStartKm).HasPrecision(8, 2);
        });

        modelBuilder.Entity<Activity>(e =>
        {
            e.Property(a => a.DistanceKm).HasPrecision(8, 2);
            e.Property(a => a.Type).HasConversion<string>().HasMaxLength(10);
        });

        SeedData.Apply(modelBuilder);
    }
}
