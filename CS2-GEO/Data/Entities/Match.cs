namespace CS2_GEO.Data.Entities;

public enum MatchStatus
{
    InProgress,
    Finished,
}

public class Match
{
    public int Id { get; set; }
    public int LobbyId { get; set; }
    public Lobby Lobby { get; set; } = null!;
    public MatchStatus Status { get; set; } = MatchStatus.InProgress;
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public List<MatchRound> Rounds { get; set; } = new();
    public List<MatchParticipant> Participants { get; set; } = new();
}

/// <summary>One location in a match's fixed round order, the same for every participant.</summary>
public class MatchRound
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public Match Match { get; set; } = null!;
    public int RoundNumber { get; set; }
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;

    /// <summary>
    /// When the round's results went out. Stored rather than derived from the guess count because a round
    /// can close without a guess from a player who disconnected, and it must not count as open after that.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    public List<PlayerGuess> Guesses { get; set; } = new();
}

/// <summary>A player's participation in one match, with their running total score.</summary>
public class MatchParticipant
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public Match Match { get; set; } = null!;
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public int TotalScore { get; set; }
}
