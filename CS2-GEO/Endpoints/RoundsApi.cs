using CS2_GEO.Dtos;
using CS2_GEO.Services;

namespace CS2_GEO.Endpoints;

public static class RoundsApi
{
    public static void MapRoundsApi(this WebApplication app)
    {
        app.MapGet("/api/rounds/random", async (string? map, IRoundsService service) =>
        {
            var round = await service.GetRandomRoundAsync(map);
            return round is null ? Results.NotFound("Round not found") : Results.Ok(round);
        });

        app.MapPost("/api/rounds/{locationId:int}/guess", async (int locationId, GameDtos.GuessRequestDto guess, IRoundsService service) =>
        {
            var result = await service.SubmitGuessAsync(locationId, guess);
            return result is null ? Results.NotFound("Round not found") : Results.Ok(result);
        });
    }
}
