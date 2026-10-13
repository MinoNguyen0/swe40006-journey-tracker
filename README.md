# swe40006-journey-tracker

Group 5 project for SWE40006. It's a simple web app where you log walks/rides and see how far you'd get on a real trip (like Hanoi to HCMC). The main point of the project is the DevOps pipeline, so the app is kept small.

Built with ASP.NET Core Razor Pages (.NET 10), EF Core 9 and MySQL.

## How to run it

You need the .NET 10 SDK and Docker Desktop (open Docker Desktop first).

```bash
docker compose up -d
dotnet run --project src/JourneyTracker.Web
```

Then go to http://localhost:5236. The tables and the 3 journeys get created automatically the first time.

Run the tests with:

```bash
dotnet test
```

To reset the local database: `docker compose down -v`

## Where things are

- `src/JourneyTracker.Web/Pages` - the pages (Index, Progress, Log, History)
- `src/JourneyTracker.Web/Services/ProgressCalculator.cs` - all the progress maths
- `src/JourneyTracker.Web/Models` - Journey, Stop, Activity
- `src/JourneyTracker.Web/Data` - database stuff, seed data and migrations
- `tests/JourneyTracker.UnitTests` - unit tests

## For the pipeline / AWS

Settings the app needs (as environment variables):

- `ConnectionStrings__Default` - MySQL connection string
- `ASPNETCORE_ENVIRONMENT` - `Staging` or `Production`
- `COMMIT_SHA` - shows in the footer and /health (shows `local` if not set)

Other things:

- runs on port 8080 in the container
- `/health` returns 200 if the app can reach the database, 503 if it can't
- the app migrates its own database when it starts
- each request gets logged with how long it took (`Duration`), for CloudWatch
- important elements have `data-testid` attributes for the Playwright tests

## Changing the database

If you change a model or the seed data, make a new migration:

```bash
dotnet tool restore
dotnet ef migrations add YourChangeName --project src/JourneyTracker.Web --output-dir Data/Migrations
```
