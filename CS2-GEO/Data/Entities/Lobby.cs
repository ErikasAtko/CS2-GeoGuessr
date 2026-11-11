namespace CS2_GEO.Data.Entities;

public enum LobbyStatus
{
    Waiting,
    InProgress,
    Finished,
}

public class Lobby
{
    public int Id { get; set; }
    public string JoinCode { get; set; } = "";
    public int HostPlayerId { get; set; }
    public Player HostPlayer { get; set; } = null!;
    public LobbyStatus Status { get; set; } = LobbyStatus.Waiting;
    public string? MapCode { get; set; }
    public int RoundCount { get; set; } = 5;
    public int MaxPlayers { get; set; } = 8;
    public DateTime CreatedAt { get; set; }

    public List<LobbyMembership> Members { get; set; } = new();
}

/// <summary>One player's membership in a lobby: when they joined and whether they're ready to start.</summary>
public class LobbyMembership
{
    public int Id { get; set; }
    public int LobbyId { get; set; }
    public Lobby Lobby { get; set; } = null!;
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public bool IsReady { get; set; }
    public DateTime JoinedAt { get; set; }
}
