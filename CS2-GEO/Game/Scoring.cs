namespace CS2_GEO.Game;

public static class Scoring
{
    public const int MaxScore = 5000;

    /// <summary>
    /// Exponential falloff: full score inside perfectRadius, then MaxScore * e^(-distance / decay),
    /// and zero at or beyond maxDistance. All distances are in normalized minimap units.
    /// </summary>
    public static GuessResult CalculateResult(
        Point guess,
        Point actual,
        double decay = 0.15,
        double perfectRadius = 0.01,
        double maxDistance = 0.5)
    {
        var distance = guess.DistanceTo(actual);

        var score = distance switch
        {
            _ when distance <= perfectRadius => MaxScore,
            _ when distance >= maxDistance => 0,
            _ => (int)Math.Round(MaxScore * Math.Exp(-distance / decay)),
        };

        return new GuessResult(actual, distance, score);
    }

    public static ScoreTier TierFor(int score) => score switch
    {
        MaxScore => ScoreTier.Perfect,
        >= 3500 => ScoreTier.Great,
        >= 1500 => ScoreTier.Good,
        > 0 => ScoreTier.Poor,
        _ => ScoreTier.Miss,
    };
}