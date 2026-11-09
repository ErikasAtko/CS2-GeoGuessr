namespace CS2_GEO.Game;

public class GameScorer(IScoreCalculator calculator) : IGameScorer
{
    public List<Round> ScoreRounds(IReadOnlyList<RoundGuess> guesses)
    {
        var rounds = new List<Round>(guesses.Count);

        for (var i = 0; i < guesses.Count; i++)
        {
            var guess = guesses[i];
            var result = calculator.Calculate(guess.Guess, guess.Actual);
            rounds.Add(new Round(i + 1, guess.LocationId, result));
        }

        rounds.Sort();
        return rounds;
    }
}
