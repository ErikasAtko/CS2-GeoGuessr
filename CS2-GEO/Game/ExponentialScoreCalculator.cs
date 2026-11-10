namespace CS2_GEO.Game;

/// <summary>
/// Exponential falloff: full score inside the perfect radius, then MaxScore * e^(-(distance - perfectRadius) / decay)
/// so the curve continues smoothly from the radius edge, and zero at or beyond the max distance.
/// </summary>
public class ExponentialScoreCalculator(ScoringOptions options, IScoreTierClassifier tierClassifier) : IScoreCalculator
{
    public int MaxScore => options.MaxScore;

    public GuessResult Calculate(Point guess, Point actual)
    {
        var distance = guess.DistanceTo(actual);

        var score = distance switch
        {
            _ when distance <= options.PerfectRadius => options.MaxScore,
            _ when distance >= options.MaxDistance => 0,
            _ => (int)Math.Round(options.MaxScore * Math.Exp(-(distance - options.PerfectRadius) / options.Decay)),
        };

        return new GuessResult(actual, distance, score, tierClassifier.Classify(score));
    }
}
