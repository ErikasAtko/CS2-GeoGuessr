using CS2_GEO.Dtos;

namespace CS2_GEO.Services;

public enum StartMatchError
{
    LobbyNotFound,
    NotHost,
    AlreadyStarted,
    NoLocationsFound,
}

public record StartMatchResult(StartMatchResultDto? Match, StartMatchError? Error);

public enum SubmitGuessError
{
    MatchNotFound,
    NotAParticipant,
    MatchFinished,
    AlreadyGuessed,
}

public record SubmitGuessResult(SubmitGuessAckDto? Ack, SubmitGuessError? Error);

/// <summary>
/// The <c>cancellationToken</c> only cancels the reading and waiting before a request changes anything.
/// Once a method starts writing, it finishes the write and the broadcasts that go with it even if the
/// caller gives up, so a closed tab can't leave a match half-started or a round never completed.
/// </summary>
public interface IMatchService
{
    Task<StartMatchResult> StartAsync(string joinCode, int requestingPlayerId, CancellationToken cancellationToken);

    Task<SubmitGuessResult> SubmitGuessAsync(int matchId, int playerId, int roundNumber, double x, double y, CancellationToken cancellationToken);

    /// <summary>The lobby's in-progress match and its current (not yet completed) round, or <c>null</c> if no match is running there. Used to resume a client that only has a guest token.</summary>
    Task<StartMatchResultDto?> GetCurrentStateAsync(int lobbyId, CancellationToken cancellationToken);

    /// <summary>
    /// Called when a player's last live connection to a lobby drops. Closes the current round if
    /// everyone still online has guessed, so one closed tab can't stall the match for everyone.
    /// Takes no cancellation token: it runs after a disconnect, when the connection's own token has
    /// already fired, and stopping halfway would leave the round stuck.
    /// </summary>
    Task HandlePlayerLeftAsync(int lobbyId);
}
