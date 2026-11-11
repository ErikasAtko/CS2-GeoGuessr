using CS2_GEO.Data.Entities;

namespace CS2_GEO.Data.Repositories;

/// <summary>What the services need from stored lobbies, without saying where they are stored (Dependency Inversion).</summary>
public interface ILobbyRepository
{
    /// <summary>The lobby for a join code, with <see cref="Lobby.Members"/> and their players loaded, or <c>null</c> if unknown.</summary>
    Task<Lobby?> GetByJoinCodeAsync(string joinCode);

    /// <summary>Creates a lobby with the given player as host and already-joined member.</summary>
    Task<Lobby> CreateAsync(int hostPlayerId, string joinCode, string? mapCode, int roundCount);

    Task AddMemberAsync(int lobbyId, int playerId);
}
