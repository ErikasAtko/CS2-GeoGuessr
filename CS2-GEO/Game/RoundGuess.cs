namespace CS2_GEO.Game;

/// <summary>One round's guess paired with where the screenshot was actually taken.</summary>
public record RoundGuess(int LocationId, Point Guess, Point Actual);
