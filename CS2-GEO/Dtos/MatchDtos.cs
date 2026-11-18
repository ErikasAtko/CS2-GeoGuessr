namespace CS2_GEO.Dtos;

public record StartMatchRequestDto(string JoinCode);

public record MatchRoundDto(int RoundNumber, int LocationId, string ImageUrl, string MinimapUrl);

public record StartMatchResultDto(int MatchId, int LobbyId, int TotalRounds, MatchRoundDto Round);

/// <summary>A guess for one round. <paramref name="RoundNumber"/> is the round the client is showing, so a guess for a round that already closed is rejected instead of being scored against the next one.</summary>
public record SubmitGuessRequestDto(int RoundNumber, double X, double Y);

public record SubmitGuessAckDto(int Score);

public record PlayerRoundResultDto(int PlayerId, string Nickname, double X, double Y, double DistanceUnits, int Score);

public record RoundCompletedDto(
    int RoundNumber,
    double ActualX,
    double ActualY,
    List<PlayerRoundResultDto> Results,
    MatchRoundDto? NextRound);

public record PlayerStandingDto(int PlayerId, string Nickname, int TotalScore);
