using CS2_GEO.Game;

namespace CS2_GEO.Tests.Unit;

// The scoring curve with the default options (the ones Program.cs registers): 5000 inside a
// 0.01 radius, exponential falloff with decay 0.15 after it, 0 from a distance of 0.6.
public class ExponentialScoreCalculatorTests
{
    private static readonly ScoringOptions _options = new();
    private readonly ExponentialScoreCalculator _calculator = new(_options, new ThresholdScoreTierClassifier(_options));

    private static readonly Point _actual = new(0.5, 0.5);

    private static Point Away(double distance) => new(_actual.X + distance, _actual.Y);

    [Fact]
    public void Calculate_GuessInsidePerfectRadius_ScoresMaxAndPerfect()
    {
        var result = _calculator.Calculate(Away(0.005), _actual);

        Assert.Equal(5000, result.Score);
        Assert.Equal(ScoreTier.Perfect, result.Tier);
    }

    // 5000 * e^(-(0.16 - 0.01) / 0.15) = 5000 / e = 1839. Pins the curve, so a changed decay or a
    // formula that forgets to subtract the perfect radius shows up here.
    [Fact]
    public void Calculate_KnownDistance_FollowsTheExponentialCurve()
    {
        var result = _calculator.Calculate(Away(0.16), _actual);

        Assert.Equal(1839, result.Score);
        Assert.Equal(ScoreTier.Good, result.Tier);
    }

    // The curve starts at the radius edge, so stepping just outside it costs almost nothing.
    [Fact]
    public void Calculate_JustOutsidePerfectRadius_HasNoSuddenDrop()
    {
        var justInside = _calculator.Calculate(Away(0.0099), _actual);
        var justOutside = _calculator.Calculate(Away(0.0101), _actual);

        Assert.Equal(5000, justInside.Score);
        Assert.InRange(justOutside.Score, 4990, 5000);
    }

    [Theory]
    [InlineData(0.6)]
    [InlineData(0.85)]
    public void Calculate_AtOrBeyondMaxDistance_ScoresZeroAndMiss(double distance)
    {
        var result = _calculator.Calculate(Away(distance), _actual);

        Assert.Equal(0, result.Score);
        Assert.Equal(ScoreTier.Miss, result.Tier);
    }

    [Fact]
    public void Calculate_FartherGuess_NeverScoresMore()
    {
        var scores = Enumerable.Range(0, 71)
            .Select(i => _calculator.Calculate(Away(i / 100.0), _actual).Score)
            .ToList();

        Assert.All(scores.Zip(scores.Skip(1)), pair => Assert.True(pair.First >= pair.Second,
            $"A farther guess scored {pair.Second}, more than the closer one's {pair.First}"));
    }

    [Fact]
    public void Calculate_ReportsTheDistanceAndTheActualLocation()
    {
        var result = _calculator.Calculate(new Point(0.2, 0.1), new Point(0.5, 0.5));

        Assert.Equal(0.5, result.Distance, precision: 10); // a 3-4-5 triangle
        Assert.Equal(new Point(0.5, 0.5), result.ActualLocation);
    }
}
