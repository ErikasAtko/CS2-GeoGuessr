namespace CS2_GEO.Game;

/// <summary>
/// Turns one guess into a score. Services depend on this abstraction, so a different scoring
/// formula is a new implementation registered in Program.cs, not an edit to its callers.
/// </summary>
public interface IScoreCalculator
{
    /// <summary>The best score a single round can give.</summary>
    int MaxScore { get; }

    GuessResult Calculate(Point guess, Point actual);
}
