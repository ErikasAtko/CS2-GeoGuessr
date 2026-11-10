using CS2_GEO.Dtos;

namespace CS2_GEO.Services;

public interface IRoundsService
{
    Task<RoundDto?> GetRandomRoundAsync(string? map);

    Task<GuessResultDto?> SubmitGuessAsync(int locationId, GuessRequestDto guess);
}
