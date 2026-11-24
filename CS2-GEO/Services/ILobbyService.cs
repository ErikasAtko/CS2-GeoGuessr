using CS2_GEO.Dtos;

namespace CS2_GEO.Services;

public enum CreateLobbyError
{
    TooFewRounds,
}

public record CreateLobbyResult(LobbyDto? Lobby, CreateLobbyError? Error);

public enum JoinLobbyError
{
    NotFound,
    AlreadyStarted,
}

public record JoinLobbyResult(LobbyDto? Lobby, JoinLobbyError? Error);

public interface ILobbyService
{
    Task<CreateLobbyResult> CreateAsync(int hostPlayerId, string? mapCode, int roundCount, CancellationToken cancellationToken);

    /// <summary>Throws <see cref="Exceptions.LobbyFullException"/> if the lobby is already at capacity.</summary>
    Task<JoinLobbyResult> JoinAsync(string joinCode, int playerId, CancellationToken cancellationToken);

    /// <summary>The caller's active (not yet finished) lobby, or <c>null</c> if they're not in one.</summary>
    Task<LobbyDto?> GetCurrentAsync(int playerId, CancellationToken cancellationToken);
}
