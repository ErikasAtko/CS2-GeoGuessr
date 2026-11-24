using CS2_GEO.Data.Repositories;
using CS2_GEO.Dtos;
using CS2_GEO.Game;

namespace CS2_GEO.Services;

public class RoundsService(ILocationRepository locations, IScoreCalculator calculator) : IRoundsService
{
    public async Task<RoundDto?> GetRandomRoundAsync(string? map, CancellationToken cancellationToken)
    {
        var picked = await locations.GetRandomAsync(map, count: 1, cancellationToken);
        return picked.FirstOrDefault()?.ToRoundDto();
    }

    public async Task<GuessResultDto?> SubmitGuessAsync(int locationId, GuessRequestDto guess, CancellationToken cancellationToken)
    {
        var location = await locations.GetByIdAsync(locationId, cancellationToken);
        if (location is null)
            return null;

        var result = calculator.Calculate(new Point(guess.X, guess.Y), location.ToPoint());

        return new GuessResultDto(
            result.ActualLocation.X,
            result.ActualLocation.Y,
            location.ToMapUnits(result.Distance),
            result.Score,
            result.Tier.ToString());
    }
}
