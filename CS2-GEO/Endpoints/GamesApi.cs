using CS2_GEO.Data;
using CS2_GEO.Dtos;
using CS2_GEO.Game;
using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Endpoints;

public static class GamesApi
{
    public static void MapGamesApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/games");

        // GET /api/games/new?map=de_dust2&rounds=5 - distinct random locations for one game
        group.MapGet("/new", async (string? map, GameDbContext db, int rounds = 5) =>
        {
            if (rounds < 1)
                return Results.BadRequest("A game needs at least one round");

            var query = db.Locations.AsQueryable();

            if (map is not null)
                query = query.Where(l => l.Map.Code == map);

            // Each row is picked at most once, so locations never repeat within a game.
            var gameRounds = await query
                .OrderBy(_ => EF.Functions.Random())
                .Take(rounds)
                .Select(l => new GameDtos.RoundDto(l.Id, l.Map.Code, l.Map.MinimapUrl, l.ImageUrl))
                .ToListAsync();

            return gameRounds.Count == 0
                ? Results.NotFound("No locations found")
                : Results.Ok(new GameDtos.GameDto(map, gameRounds));
        });

        // POST /api/games/summary - re-scores every guess and returns the rounds best to worst
        group.MapPost("/summary", async (GameDtos.GameSummaryRequestDto request, GameDbContext db) =>
        {
            if (request.Guesses.Count == 0)
                return Results.BadRequest("No guesses submitted");

            var locationIds = request.Guesses.Select(g => g.LocationId).ToList();
            if (locationIds.Distinct().Count() != locationIds.Count)
                return Results.BadRequest("Each location can only be guessed once per game");

            // One query for all locations, then O(1) lookups while iterating.
            var locations = await db.Locations
                .Include(l => l.Map)
                .Where(l => locationIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id);

            var rounds = new List<Round>(request.Guesses.Count);
            foreach (var (guess, index) in request.Guesses.Select((g, i) => (g, i)))
            {
                if (!locations.TryGetValue(guess.LocationId, out var location))
                    return Results.NotFound($"Location {guess.LocationId} not found");

                var result = Scoring.CalculateResult(
                    new Point(guess.X, guess.Y), new Point(location.X, location.Y), maxDistance: 0.6);
                rounds.Add(new Round(index + 1, guess.LocationId, result));
            }

            var summaryRounds = rounds
                .Order()
                .Select(r =>
                {
                    var location = locations[r.LocationId];
                    return new GameDtos.RoundSummaryDto(
                        r.RoundNumber,
                        location.ImageUrl,
                        r.Result.Distance * location.Map.SizeUnits,
                        r.Result.Score,
                        r.Result.Tier.ToString());
                })
                .ToList();

            return Results.Ok(new GameDtos.GameSummaryDto(
                TotalScore: rounds.Sum(r => r.Result.Score),
                MaxPossibleScore: rounds.Count * Scoring.MaxScore,
                AverageScore: rounds.Average(r => r.Result.Score),
                Rounds: summaryRounds));
        });
    }
}