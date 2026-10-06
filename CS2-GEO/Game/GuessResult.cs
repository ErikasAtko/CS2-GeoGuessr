namespace CS2_GEO.Game;

public record GuessResult(Point ActualLocation, double Distance, int Score)
{
    public ScoreTier Tier => Scoring.TierFor(Score);
}
