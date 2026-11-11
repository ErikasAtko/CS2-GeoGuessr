using System.Collections.Concurrent;

namespace CS2_GEO.Hubs;

/// <summary>
/// Tracks which lobby each live SignalR connection belongs to. Purely in-memory and ephemeral on
/// purpose: this answers "who's actually online right now", which the database has no business
/// answering. Registered as a singleton so it outlives any one connection's scoped services.
/// </summary>
public class ConnectionRegistry
{
    private readonly ConcurrentDictionary<string, int> connectionToLobby = new();

    public void Add(string connectionId, int lobbyId) => connectionToLobby[connectionId] = lobbyId;

    public int? Remove(string connectionId) =>
        connectionToLobby.TryRemove(connectionId, out var lobbyId) ? lobbyId : null;
}
