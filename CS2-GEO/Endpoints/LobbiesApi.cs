using CS2_GEO.Dtos;
using CS2_GEO.Exceptions;
using CS2_GEO.Services;

namespace CS2_GEO.Endpoints;

public static class LobbiesApi
{
    public static void MapLobbiesApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/lobbies");

        // POST /api/lobbies - creates a lobby hosted by the caller, returning a join code to share with friends
        group.MapPost("/", async (CreateLobbyRequestDto request, HttpRequest http, ILobbyService lobbyService, IGuestSessionService sessions) =>
        {
            var playerId = await GuestAuth.ResolvePlayerIdAsync(http, sessions);
            if (playerId is null)
                return Results.Unauthorized();

            var result = await lobbyService.CreateAsync(playerId.Value, request.MapCode, request.RoundCount);
            return result.Error switch
            {
                CreateLobbyError.TooFewRounds => Results.BadRequest("A lobby needs at least one round"),
                _ => Results.Ok(result.Lobby),
            };
        });

        // POST /api/lobbies/{joinCode}/join - adds the caller to a waiting lobby, or 409s if it just filled up
        group.MapPost("/{joinCode}/join", async (string joinCode, HttpRequest http, ILobbyService lobbyService, IGuestSessionService sessions) =>
        {
            var playerId = await GuestAuth.ResolvePlayerIdAsync(http, sessions);
            if (playerId is null)
                return Results.Unauthorized();

            try
            {
                var result = await lobbyService.JoinAsync(joinCode, playerId.Value);
                return result.Error switch
                {
                    JoinLobbyError.NotFound => Results.NotFound("Lobby not found"),
                    JoinLobbyError.AlreadyStarted => Results.BadRequest("Lobby already started"),
                    _ => Results.Ok(result.Lobby),
                };
            }
            catch (LobbyFullException ex)
            {
                return Results.Conflict(ex.Message);
            }
        });
    }
}
