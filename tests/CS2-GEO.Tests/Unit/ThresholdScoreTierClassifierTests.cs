using CS2_GEO.Game;

namespace CS2_GEO.Tests.Unit;

public class ThresholdScoreTierClassifierTests
{
    private readonly ThresholdScoreTierClassifier _classifier = new(new ScoringOptions());

    // Each threshold and the value just below it: the tier changes exactly at the threshold.
    [Theory]
    [InlineData(5000, ScoreTier.Perfect)]
    [InlineData(4999, ScoreTier.Great)]
    [InlineData(3500, ScoreTier.Great)]
    [InlineData(3499, ScoreTier.Good)]
    [InlineData(1500, ScoreTier.Good)]
    [InlineData(1499, ScoreTier.Poor)]
    [InlineData(1, ScoreTier.Poor)]
    [InlineData(0, ScoreTier.Miss)]
    public void Classify_ScoreAtOrJustBelowAThreshold_PicksTheRightTier(int score, ScoreTier expected)
    {
        Assert.Equal(expected, _classifier.Classify(score));
    }

    // The thresholds come from the options, not from the classifier, so a difficulty setting can move them.
    [Fact]
    public void Classify_CustomThresholds_AreUsed()
    {
        var strict = new ThresholdScoreTierClassifier(new ScoringOptions(GreatThreshold: 4500, GoodThreshold: 3000));

        Assert.Equal(ScoreTier.Good, strict.Classify(4000));
        Assert.Equal(ScoreTier.Poor, strict.Classify(2000));
    }
}
