using CS2_GEO.Data.Repositories;
using CS2_GEO.Dtos;
using CS2_GEO.Game;

namespace CS2_GEO.Services;

public class GamesService(ILocationRepository locations, IGameScorer scorer, IScoreCalculator calculator) : IGamesService
{
    public async Task<NewGameResult> CreateNewGameAsync(string? map, int rounds)
    {
        if (rounds < 1)
            return new NewGameResult(null, NewGameError.TooFewRounds);

        var picked = await locations.GetRandomAsync(map, rounds);

        return picked.Count == 0
            ? new NewGameResult(null, NewGameError.NoLocationsFound)
            : new NewGameResult(new GameDto(map, picked.Select(l => l.ToRoundDto()).ToList()), null);
    }

    public async Task<SummaryResult> BuildSummaryAsync(GameSummaryRequestDto request)
    {
        if (request.Guesses.Count == 0)
            return new SummaryResult(null, SummaryError.NoGuesses);

        var locationIds = request.Guesses.Select(g => g.LocationId).ToList();
        if (locationIds.Distinct().Count() != locationIds.Count)
            return new SummaryResult(null, SummaryError.DuplicateGuess);

        // One query for all locations, then O(1) lookups while iterating.
        var found = await locations.GetByIdsAsync(locationIds);

        var missingLocationId = locationIds
            .Where(id => !found.ContainsKey(id))
            .Select(id => (int?)id)
            .FirstOrDefault();
        if (missingLocationId is not null)
            return new SummaryResult(null, SummaryError.LocationNotFound, missingLocationId);

        var guesses = request.Guesses
            .Select(g => new RoundGuess(g.LocationId, new Point(g.X, g.Y), found[g.LocationId].ToPoint()))
            .ToList();

        var rounds = scorer.ScoreRounds(guesses);

        var summaryRounds = rounds
            .Select(r =>
            {
                var location = found[r.LocationId];
                return new RoundSummaryDto(
                    r.RoundNumber,
                    location.ImageUrl,
                    location.ToMapUnits(r.Result.Distance),
                    r.Result.Score,
                    r.Result.Tier.ToString());
            })
            .ToList();

        var summary = new GameSummaryDto(
            TotalScore: rounds.Sum(r => r.Result.Score),
            MaxPossibleScore: rounds.Count * calculator.MaxScore,
            AverageScore: rounds.Average(r => r.Result.Score),
            Rounds: summaryRounds);

        return new SummaryResult(summary, null);
    }
}
