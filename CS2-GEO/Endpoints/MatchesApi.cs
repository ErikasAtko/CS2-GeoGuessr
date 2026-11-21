using CS2_GEO.Dtos;
using CS2_GEO.Services;

namespace CS2_GEO.Endpoints;

public static class MatchesApi
{
    public static void MapMatchesApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/matches");

        // POST /api/matches/start - the lobby host starts the match: picks the shared rounds and broadcasts round 1 to everyone
        group.MapPost("/start", async (StartMatchRequestDto request, HttpRequest http, IMatchService matchService, IGuestSessionService sessions) =>
        {
            var playerId = await GuestAuth.ResolvePlayerIdAsync(http, sessions);
            if (playerId is null)
                return Results.Unauthorized();

            var result = await matchService.StartAsync(request.JoinCode, playerId.Value);
            return result.Error switch
            {
                StartMatchError.LobbyNotFound => Results.NotFound("Lobby not found"),
                StartMatchError.NotHost => Forbidden("Only the host can start the match"),
                StartMatchError.AlreadyStarted => Results.BadRequest("Match already started"),
                StartMatchError.NoLocationsFound => Results.NotFound("No locations found for this map"),
                _ => Results.Ok(result.Match),
            };
        });

        // POST /api/matches/{matchId}/guess - scores the caller's guess for whichever round is still open; once everyone in
        // the match has guessed, the result (and the next round, or final standings) goes out over the hub, not in this response
        group.MapPost("/{matchId:int}/guess", async (int matchId, SubmitGuessRequestDto request, HttpRequest http, IMatchService matchService, IGuestSessionService sessions) =>
        {
            var playerId = await GuestAuth.ResolvePlayerIdAsync(http, sessions);
            if (playerId is null)
                return Results.Unauthorized();

            var result = await matchService.SubmitGuessAsync(matchId, playerId.Value, request.RoundNumber, request.X, request.Y);
            return result.Error switch
            {
                SubmitGuessError.MatchNotFound => Results.NotFound("Match not found"),
                SubmitGuessError.NotAParticipant => Forbidden("You are not playing in this match"),
                SubmitGuessError.MatchFinished => Results.BadRequest("Match already finished"),
                SubmitGuessError.AlreadyGuessed => Results.BadRequest("Already guessed this round"),
                _ => Results.Ok(result.Ack),
            };
        });
    }

    // Not Results.Forbid(): that hands off to the authentication middleware, which this app doesn't
    // register (guests are identified by a header token instead), so it throws and returns a 500.
    private static IResult Forbidden(string message) => Results.Json(message, statusCode: StatusCodes.Status403Forbidden);
}
