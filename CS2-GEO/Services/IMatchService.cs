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

public interface IMatchService
{
    Task<StartMatchResult> StartAsync(string joinCode, int requestingPlayerId);

    Task<SubmitGuessResult> SubmitGuessAsync(int matchId, int playerId, double x, double y);
}
