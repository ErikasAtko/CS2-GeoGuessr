namespace CS2_GEO.Game;

/// <summary>Picks a tier by comparing the score with the thresholds in <see cref="ScoringOptions"/>.</summary>
public class ThresholdScoreTierClassifier(ScoringOptions options) : IScoreTierClassifier
{
    public ScoreTier Classify(int score) => score switch
    {
        _ when score >= options.MaxScore => ScoreTier.Perfect,
        _ when score >= options.GreatThreshold => ScoreTier.Great,
        _ when score >= options.GoodThreshold => ScoreTier.Good,
        > 0 => ScoreTier.Poor,
        _ => ScoreTier.Miss,
    };
}
