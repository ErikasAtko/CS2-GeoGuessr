namespace CS2_GEO.Game;

/// <summary>
/// The numbers behind scoring, kept out of the calculator so a stricter or more forgiving
/// difficulty is a different set of options, not an edit to the scoring code (Open/Closed).
/// All distances are in normalized minimap units.
/// </summary>
/// <param name="MaxScore">Score for a guess inside <paramref name="PerfectRadius"/>.</param>
/// <param name="Decay">How fast the score falls off with distance; smaller is stricter.</param>
/// <param name="PerfectRadius">Guesses this close to the actual location get <paramref name="MaxScore"/>.</param>
/// <param name="MaxDistance">Guesses at or beyond this distance score zero.</param>
/// <param name="GreatThreshold">Lowest score that still counts as <see cref="ScoreTier.Great"/>.</param>
/// <param name="GoodThreshold">Lowest score that still counts as <see cref="ScoreTier.Good"/>.</param>
public record ScoringOptions(
    int MaxScore = 5000,
    double Decay = 0.15,
    double PerfectRadius = 0.01,
    double MaxDistance = 0.6,
    int GreatThreshold = 3500,
    int GoodThreshold = 1500);
