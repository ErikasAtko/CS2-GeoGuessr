using System.Collections.Concurrent;

namespace CS2_GEO.Hubs;

/// <summary>
/// Tracks which lobby and player each live SignalR connection belongs to. Purely in-memory and ephemeral on
/// purpose: this answers "who's actually online right now", which the database has no business
/// answering. Registered as a singleton so it outlives any one connection's scoped services.
/// </summary>
public class ConnectionRegistry
{
    private readonly ConcurrentDictionary<string, (int LobbyId, int PlayerId)> _connections = new();

    public void Add(string connectionId, int lobbyId, int playerId) => _connections[connectionId] = (lobbyId, playerId);

    public (int LobbyId, int PlayerId)? Remove(string connectionId) =>
        _connections.TryRemove(connectionId, out var entry) ? entry : null;

    /// <summary>
    /// Players in a lobby with at least one live connection. Counted per player, not per connection:
    /// after an automatic reconnect the new connection often registers before the server notices the
    /// old one is gone, and the player must not count as offline in between.
    /// </summary>
    public HashSet<int> OnlinePlayers(int lobbyId) =>
        _connections.Values.Where(c => c.LobbyId == lobbyId).Select(c => c.PlayerId).ToHashSet();
}
