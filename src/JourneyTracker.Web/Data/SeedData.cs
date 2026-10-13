using JourneyTracker.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace JourneyTracker.Web.Data;

// approx road distances. need a new migration after changing this
public static class SeedData
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Journey>().HasData(
            new Journey { Id = 1, Name = "Hanoi to Ho Chi Minh City", TotalDistanceKm = 1700 },
            new Journey { Id = 2, Name = "Tokyo to Osaka", TotalDistanceKm = 510 },
            new Journey { Id = 3, Name = "Melbourne to Sydney", TotalDistanceKm = 880 });

        modelBuilder.Entity<Stop>().HasData(
            new Stop { Id = 1, JourneyId = 1, Name = "Hanoi", DistanceFromStartKm = 0 },
            new Stop { Id = 2, JourneyId = 1, Name = "Ninh Binh", DistanceFromStartKm = 95 },
            new Stop { Id = 3, JourneyId = 1, Name = "Vinh", DistanceFromStartKm = 295 },
            new Stop { Id = 4, JourneyId = 1, Name = "Hue", DistanceFromStartKm = 665 },
            new Stop { Id = 5, JourneyId = 1, Name = "Da Nang", DistanceFromStartKm = 760 },
            new Stop { Id = 6, JourneyId = 1, Name = "Quy Nhon", DistanceFromStartKm = 1065 },
            new Stop { Id = 7, JourneyId = 1, Name = "Nha Trang", DistanceFromStartKm = 1280 },
            new Stop { Id = 8, JourneyId = 1, Name = "Ho Chi Minh City", DistanceFromStartKm = 1700 },

            new Stop { Id = 9, JourneyId = 2, Name = "Tokyo", DistanceFromStartKm = 0 },
            new Stop { Id = 10, JourneyId = 2, Name = "Yokohama", DistanceFromStartKm = 30 },
            new Stop { Id = 11, JourneyId = 2, Name = "Shizuoka", DistanceFromStartKm = 180 },
            new Stop { Id = 12, JourneyId = 2, Name = "Nagoya", DistanceFromStartKm = 350 },
            new Stop { Id = 13, JourneyId = 2, Name = "Kyoto", DistanceFromStartKm = 470 },
            new Stop { Id = 14, JourneyId = 2, Name = "Osaka", DistanceFromStartKm = 510 },

            new Stop { Id = 15, JourneyId = 3, Name = "Melbourne", DistanceFromStartKm = 0 },
            new Stop { Id = 16, JourneyId = 3, Name = "Seymour", DistanceFromStartKm = 100 },
            new Stop { Id = 17, JourneyId = 3, Name = "Wangaratta", DistanceFromStartKm = 235 },
            new Stop { Id = 18, JourneyId = 3, Name = "Albury", DistanceFromStartKm = 325 },
            new Stop { Id = 19, JourneyId = 3, Name = "Gundagai", DistanceFromStartKm = 500 },
            new Stop { Id = 20, JourneyId = 3, Name = "Goulburn", DistanceFromStartKm = 690 },
            new Stop { Id = 21, JourneyId = 3, Name = "Sydney", DistanceFromStartKm = 880 });
    }
}
