namespace CS2_GEO.Data.Entities;

/// <summary>A guest player's reconnect credential: presenting <see cref="Token"/> again resolves back to the same <see cref="Player"/>.</summary>
public class GuestSession
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public string Token { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
