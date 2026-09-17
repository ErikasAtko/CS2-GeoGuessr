using CS2_GEO.Data;
using CS2_GEO.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Endpoints;

public static class MapsApi
{
    public static void MapMapsApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/maps");

        // GET /api/maps - all maps
        group.MapGet("/", async (GameDbContext db) =>
            await db.Maps
                .OrderBy(m => m.DisplayName)
                .Select(m => new GameDtos.MapDto(m.Id, m.Code, m.DisplayName, m.MinimapUrl, m.Locations.Count))
                .ToListAsync());

        // GET /api/maps/de_mirage
        group.MapGet("/{code}", async (string code, GameDbContext db) =>
        {
            var map = await db.Maps
                .Where(m => m.Code == code)
                .Select(m => new GameDtos.MapDto(m.Id, m.Code, m.DisplayName, m.MinimapUrl, m.Locations.Count))
                .FirstOrDefaultAsync();

            return map is null ? Results.NotFound() : Results.Ok(map);
        });
    }
}