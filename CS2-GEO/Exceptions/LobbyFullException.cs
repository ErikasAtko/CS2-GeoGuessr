namespace CS2_GEO.Exceptions;

/// <summary>
/// Thrown when a join would push a lobby over its player cap. Only reachable from inside
/// <see cref="Services.LobbyJoinGate"/>'s race-safe check, so it signals a real race lost, not routine validation.
/// </summary>
public class LobbyFullException(int lobbyId) : Exception($"Lobby {lobbyId} is full")
{
    public int LobbyId { get; } = lobbyId;
}
