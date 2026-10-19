using System.Text.Json;
using CS2_GEO.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(GameDbContext db, string path)
    {
        if (await db.Maps.AnyAsync())
            return;

        await using var stream = File.OpenRead(path);

        var seed = await JsonSerializer.DeserializeAsync<SeedData>(
            stream, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        if (seed?.Maps is null || seed.Maps.Count == 0)
            throw new InvalidDataException("Seed file contains no maps");

        db.Maps.AddRange(seed.Maps);
        await db.SaveChangesAsync();
    }

    private sealed record SeedData(List<Map> Maps);
}