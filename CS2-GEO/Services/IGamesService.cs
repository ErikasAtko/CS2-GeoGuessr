using CS2_GEO.Dtos;

namespace CS2_GEO.Services;

public enum NewGameError
{
    TooFewRounds,
    NoLocationsFound,
}

public record NewGameResult(GameDtos.GameDto? Game, NewGameError? Error);

public enum SummaryError
{
    NoGuesses,
    DuplicateGuess,
    LocationNotFound,
}

public record SummaryResult(GameDtos.GameSummaryDto? Summary, SummaryError? Error, int? MissingLocationId = null);

public interface IGamesService
{
    Task<NewGameResult> CreateNewGameAsync(string? map, int rounds);

    Task<SummaryResult> BuildSummaryAsync(GameDtos.GameSummaryRequestDto request);
}
