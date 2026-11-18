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

    /// <summary>The guess was for a round that already closed, e.g. it finished without this player while they were disconnected.</summary>
    RoundClosed,
}

public record SubmitGuessResult(SubmitGuessAckDto? Ack, SubmitGuessError? Error);

public interface IMatchService
{
    Task<StartMatchResult> StartAsync(string joinCode, int requestingPlayerId);

    Task<SubmitGuessResult> SubmitGuessAsync(int matchId, int playerId, int roundNumber, double x, double y);

    /// <summary>
    /// Called when a player's last live connection to a lobby drops. Closes the current round if
    /// everyone still online has guessed, so one closed tab can't stall the match for everyone.
    /// </summary>
    Task HandlePlayerLeftAsync(int lobbyId);
}
