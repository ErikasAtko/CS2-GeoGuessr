using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Data;

/// <summary>Brings the database up to date at startup, so Program.cs only wires the app together.</summary>
public static class DatabaseInitializer
{
    /// <summary>Applies pending EF migrations, then fills an empty database from the seed file.</summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();

        // No request exists yet, so the only reason to stop is the app shutting down mid-startup.
        var stopping = app.Lifetime.ApplicationStopping;

        await db.Database.MigrateAsync(stopping);

        var seedPath = Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "seed.json");
        await DatabaseSeeder.SeedAsync(db, seedPath, stopping);
    }
}
