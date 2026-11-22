using CS2_GEO.Data.Entities;

namespace CS2_GEO.Data.Repositories;

/// <summary>What the services need from stored lobbies, without saying where they are stored (Dependency Inversion).</summary>
public interface ILobbyRepository
{
    /// <summary>The lobby for a join code, with <see cref="Lobby.Members"/> and their players loaded, or <c>null</c> if unknown.</summary>
    Task<Lobby?> GetByJoinCodeAsync(string joinCode);

    /// <summary>
    /// Just the id for a join code, without loading or tracking the lobby. Callers use it to take the
    /// lobby's lock before reading its state: once EF tracks an entity, querying it again inside the
    /// lock returns the cached values instead of what another request just wrote.
    /// </summary>
    Task<int?> GetIdByJoinCodeAsync(string joinCode);

    /// <summary>
    /// The player's most recent lobby that hasn't finished (Waiting or InProgress), with
    /// <see cref="Lobby.Members"/> and their players loaded, or <c>null</c> if they're not in one.
    /// </summary>
    Task<Lobby?> GetActiveForPlayerAsync(int playerId);

    /// <summary>Creates a lobby with the given player as host and already-joined member.</summary>
    Task<Lobby> CreateAsync(int hostPlayerId, string joinCode, string? mapCode, int roundCount);

    Task AddMemberAsync(int lobbyId, int playerId);
}
