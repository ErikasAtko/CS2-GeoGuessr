namespace CS2_GEO.Dtos;

public record StartMatchRequestDto(string JoinCode);

public record MatchRoundDto(int RoundNumber, int LocationId, string ImageUrl, string MinimapUrl);

public record StartMatchResultDto(int MatchId, int LobbyId, int TotalRounds, MatchRoundDto Round);

public record SubmitGuessRequestDto(double X, double Y);

public record SubmitGuessAckDto(int Score);

public record PlayerRoundResultDto(int PlayerId, string Nickname, double X, double Y, double DistanceUnits, int Score);

public record RoundCompletedDto(
    int RoundNumber,
    double ActualX,
    double ActualY,
    List<PlayerRoundResultDto> Results,
    MatchRoundDto? NextRound);

public record PlayerStandingDto(int PlayerId, string Nickname, int TotalScore);
