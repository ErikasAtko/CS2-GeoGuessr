using CS2_GEO.Game;
using NSubstitute;

namespace CS2_GEO.Tests.Unit;

// GameScorer's own job is numbering rounds and ranking them; how a guess is scored is
// IScoreCalculator's job. A stub calculator returns whatever score and distance each test
// needs, so these tests don't depend on the scoring formula and don't break when it changes.
public class GameScorerTests
{
    private readonly IScoreCalculator _calculator = Substitute.For<IScoreCalculator>();
    private readonly GameScorer _scorer;

    public GameScorerTests()
    {
        _scorer = new GameScorer(_calculator);
    }

    // Each location's actual point is (locationId, 0), so the stub can tell the rounds apart.
    private RoundGuess Guess(int locationId, int score, double distance)
    {
        var actual = new Point(locationId, 0);
        _calculator.Calculate(Arg.Any<Point>(), actual)
            .Returns(new GuessResult(actual, distance, score, ScoreTier.Good));
        return new RoundGuess(locationId, new Point(0, 0), actual);
    }

    [Fact]
    public void ScoreRounds_ReturnsBestFirst_KeepingTheOrderTheyWerePlayedIn()
    {
        var guesses = new[]
        {
            Guess(locationId: 10, score: 100, distance: 0.5),
            Guess(locationId: 20, score: 3000, distance: 0.1),
            Guess(locationId: 30, score: 900, distance: 0.3),
        };

        var rounds = _scorer.ScoreRounds(guesses);

        Assert.Equal([20, 30, 10], rounds.Select(r => r.LocationId));
        Assert.Equal([2, 3, 1], rounds.Select(r => r.RoundNumber));
    }

    [Fact]
    public void ScoreRounds_EqualScores_CloserGuessRanksFirst()
    {
        var guesses = new[]
        {
            Guess(locationId: 1, score: 5000, distance: 0.008),
            Guess(locationId: 2, score: 5000, distance: 0.002),
        };

        var rounds = _scorer.ScoreRounds(guesses);

        Assert.Equal([2, 1], rounds.Select(r => r.LocationId));
    }
}
