using CS2_GEO.Data;
using CS2_GEO.Dtos;
using CS2_GEO.Game;
using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Services;

public class GamesService(GameDbContext db) : IGamesService
{
    public async Task<NewGameResult> CreateNewGameAsync(string? map, int rounds)
    {
        if (rounds < 1)
            return new NewGameResult(null, NewGameError.TooFewRounds);

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
            ? new NewGameResult(null, NewGameError.NoLocationsFound)
            : new NewGameResult(new GameDtos.GameDto(map, gameRounds), null);
    }

    public async Task<SummaryResult> BuildSummaryAsync(GameDtos.GameSummaryRequestDto request)
    {
        if (request.Guesses.Count == 0)
            return new SummaryResult(null, SummaryError.NoGuesses);

        var locationIds = request.Guesses.Select(g => g.LocationId).ToList();
        if (locationIds.Distinct().Count() != locationIds.Count)
            return new SummaryResult(null, SummaryError.DuplicateGuess);

        // One query for all locations, then O(1) lookups while iterating.
        var locations = await db.Locations
            .Include(l => l.Map)
            .Where(l => locationIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id);

        var (rounds, missingLocationId) = GameSummary.ScoreRounds(request.Guesses, locations);
        if (rounds is null)
            return new SummaryResult(null, SummaryError.LocationNotFound, missingLocationId);

        var summaryRounds = rounds
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

        var summary = new GameDtos.GameSummaryDto(
            TotalScore: rounds.Sum(r => r.Result.Score),
            MaxPossibleScore: rounds.Count * Scoring.MaxScore,
            AverageScore: rounds.Average(r => r.Result.Score),
            Rounds: summaryRounds);

        return new SummaryResult(summary, null);
    }
}
