using CS2_GEO.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CS2_GEO.Tests.Integration;

public class DatabaseTests(Cs2GeoApiFactory factory)
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    // Fails when an entity changed but nobody ran `dotnet ef migrations add`. Startup would then
    // refuse to migrate (PendingModelChangesWarning) and the app wouldn't start at all; this
    // test names the actual cause instead.
    [Fact]
    public void Model_HasNoChangesWithoutAMigration()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();

        Assert.False(db.Database.HasPendingModelChanges());
    }

    // The app seeds on every start. A restart must keep the data as it is, not add the maps again.
    [Fact]
    public async Task Seed_DatabaseAlreadyHasMaps_AddsNothing()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        var mapsBefore = await db.Maps.CountAsync(Cancellation);
        var locationsBefore = await db.Locations.CountAsync(Cancellation);

        await DatabaseSeeder.SeedAsync(db, Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "seed.json"));

        Assert.True(mapsBefore > 0, "The test database should have been seeded at startup");
        Assert.Equal(mapsBefore, await db.Maps.CountAsync(Cancellation));
        Assert.Equal(locationsBefore, await db.Locations.CountAsync(Cancellation));
    }
}
