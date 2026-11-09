namespace CS2_GEO.Dtos;

public record GameDto(string? MapCode, List<RoundDto> Rounds);

public record RoundGuessDto(int LocationId, double X, double Y);

public record GameSummaryRequestDto(List<RoundGuessDto> Guesses);

public record RoundSummaryDto(int RoundNumber, string ImageUrl, double DistanceUnits, int Score, string Tier);

public record GameSummaryDto(int TotalScore, int MaxPossibleScore, double AverageScore, List<RoundSummaryDto> Rounds);
