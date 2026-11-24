using CS2_GEO.Dtos;
using CS2_GEO.Services;

namespace CS2_GEO.Endpoints;

public static class GamesApi
{
    public static void MapGamesApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/games");

        // Every handler takes a CancellationToken: minimal APIs bind it to HttpContext.RequestAborted, so
        // database work stops when the client disconnects instead of running for nobody.

        // GET /api/games/new?map=de_dust2&rounds=5 - distinct random locations for one game
        group.MapGet("/new", async (string? map, IGamesService service, CancellationToken cancellationToken, int rounds = 5) =>
        {
            var result = await service.CreateNewGameAsync(map, rounds, cancellationToken);
            return result.Error switch
            {
                NewGameError.TooFewRounds => Results.BadRequest("A game needs at least one round"),
                NewGameError.NoLocationsFound => Results.NotFound("No locations found"),
                _ => Results.Ok(result.Game),
            };
        });

        // POST /api/games/summary - re-scores every guess and returns the rounds best to worst
        group.MapPost("/summary", async (GameSummaryRequestDto request, IGamesService service, CancellationToken cancellationToken) =>
        {
            var result = await service.BuildSummaryAsync(request, cancellationToken);
            return result.Error switch
            {
                SummaryError.NoGuesses => Results.BadRequest("No guesses submitted"),
                SummaryError.DuplicateGuess => Results.BadRequest("Each location can only be guessed once per game"),
                SummaryError.LocationNotFound => Results.NotFound($"Location {result.MissingLocationId} not found"),
                _ => Results.Ok(result.Summary),
            };
        });
    }
}
