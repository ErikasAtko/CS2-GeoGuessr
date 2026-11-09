namespace CS2_GEO.Dtos;

public record RoundDto(int LocationId, string MapCode, string MinimapUrl, string ImageUrl);

public record GuessRequestDto(double X, double Y);

public record GuessResultDto(double ActualX, double ActualY, double DistanceUnits, int Score, string Tier);
