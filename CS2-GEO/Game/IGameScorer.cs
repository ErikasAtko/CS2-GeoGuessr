namespace CS2_GEO.Game;

/// <summary>Turns a whole game's guesses into scored rounds.</summary>
public interface IGameScorer
{
    /// <summary>
    /// Scores every guess, numbering rounds in the order they were played (starting at 1),
    /// and returns them ranked best to worst.
    /// </summary>
    List<Round> ScoreRounds(IReadOnlyList<RoundGuess> guesses);
}
