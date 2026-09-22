using CS2_GEO.Data;
using CS2_GEO.Dtos;
using CS2_GEO.Game;
using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Endpoints;

public static class RoundsApi
{
    public static void MapRoundsApi(this WebApplication app)
    {
        app.MapGet("/api/rounds/random", async (string? map, GameDbContext db) =>
        {
            var query = db.Locations.AsQueryable();

            if (map is not null)
                query = query.Where(l => l.Map.Code == map);

            var round = await query
                .OrderBy(_ => EF.Functions.Random())
                .Select(l => new GameDtos.RoundDto(l.Id, l.Map.Code, l.Map.MinimapUrl, l.ImageUrl))
                .FirstOrDefaultAsync();

            return round is null
                ? Results.NotFound("Round not found")
                : Results.Ok(round);
        });

        app.MapPost("/api/rounds/{locationId:int}/guess", async (int locationId, GameDtos.GuessRequestDto guess, GameDbContext db) =>
        {
            var location = await db.Locations.Include(l => l.Map).FirstOrDefaultAsync(l => l.Id == locationId);
            if (location is null)
                return Results.NotFound("Round not found");

            var result = Scoring.CalculateResult(new Point(guess.X, guess.Y), new Point(location.X, location.Y));
            var distanceUnits = result.Distance * location.Map.SizeUnits;

            return Results.Ok(new GameDtos.GuessResultDto(
                result.ActualLocation.X, result.ActualLocation.Y, distanceUnits, result.Score));
        });
    }
}