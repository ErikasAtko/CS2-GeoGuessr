namespace CS2_GEO.Game;

/// <summary>Names how good a score is. Kept apart from the formula, so either can change on its own.</summary>
public interface IScoreTierClassifier
{
    ScoreTier Classify(int score);
}
