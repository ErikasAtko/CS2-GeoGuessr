namespace CS2_GEO.Data.Entities;

/// <summary>One player's guess for one round of a match. Unique per (round, player) - see <see cref="Data.GameDbContext"/>.</summary>
public class PlayerGuess
{
    public int Id { get; set; }
    public int MatchRoundId { get; set; }
    public MatchRound MatchRound { get; set; } = null!;
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public double X { get; set; }
    public double Y { get; set; }
    public double Distance { get; set; }
    public int Score { get; set; }
    public DateTime SubmittedAt { get; set; }
}
