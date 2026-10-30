using CS2_GEO.Dtos;

namespace CS2_GEO.Services;

public interface IRoundsService
{
    Task<GameDtos.RoundDto?> GetRandomRoundAsync(string? map);

    Task<GameDtos.GuessResultDto?> SubmitGuessAsync(int locationId, GameDtos.GuessRequestDto guess);
}
