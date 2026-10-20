using CS2_GEO.Data.Entities;
using CS2_GEO.Dtos;

namespace CS2_GEO.Game;

/// <summary>Turns a game's submitted guesses into scored rounds, ranked best to worst.</summary>
public static class GameSummary
{
    /// <summary>
    /// Scores every guess against the location it refers to.
    /// If a guess points at a location that isn't in <paramref name="locations"/>, returns that location's id instead of a result.
    /// </summary>
    public static (List<Round>? Rounds, int? MissingLocationId) ScoreRounds(
        List<GameDtos.RoundGuessDto> guesses,
        Dictionary<int, Location> locations)
    {
        var rounds = new List<Round>();

        for (var i = 0; i < guesses.Count; i++)
        {
            var guess = guesses[i];
            var roundNumber = i + 1;

            if (!locations.TryGetValue(guess.LocationId, out var location))
                return (null, guess.LocationId);

            var guessPoint = new Point(guess.X, guess.Y);
            var actualPoint = new Point(location.X, location.Y);
            var result = Scoring.CalculateResult(guessPoint, actualPoint);

            rounds.Add(new Round(roundNumber, guess.LocationId, result));
        }

        rounds.Sort();
        return (rounds, null);
    }
}
