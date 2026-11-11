using CS2_GEO.Dtos;
using CS2_GEO.Services;

namespace CS2_GEO.Endpoints;

public static class GuestSessionsApi
{
    public static void MapGuestSessionsApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/guest-sessions");

        // POST /api/guest-sessions - creates a guest identity; the returned token must be sent on every later request to act as that player
        group.MapPost("/", async (CreateGuestSessionRequestDto request, IGuestSessionService service) =>
        {
            var result = await service.CreateAsync(request.Nickname);
            return result.Error switch
            {
                CreateGuestSessionError.InvalidNickname => Results.BadRequest("Nickname must be 1-32 characters"),
                _ => Results.Ok(result.Session),
            };
        });
    }
}
