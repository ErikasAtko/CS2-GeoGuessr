namespace CS2_GEO.Game;

public static class Scoring
{
    public const int MaxScore = 5000;

    /// <summary>Linear falloff: full score at zero distance, zero score at or beyond maxDistance.</summary>
    public static GuessResult CalculateResult(Point guess, Point actual, double maxDistance = 1.0)
    {
        var distance = guess.DistanceTo(actual);
        var ratio = Math.Clamp(1 - distance / maxDistance, 0, 1);
        var score = (int)Math.Round(MaxScore * ratio);
        return new GuessResult(actual, distance, score);
    }
}
