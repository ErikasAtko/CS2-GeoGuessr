using CS2_GEO.Services;

namespace CS2_GEO.Endpoints;

/// <summary>Resolves the caller's player id from the <c>X-Guest-Token</c> header, shared by every endpoint that needs to know who's calling.</summary>
public static class GuestAuth
{
    public static async Task<int?> ResolvePlayerIdAsync(HttpRequest http, IGuestSessionService sessions)
    {
        var token = http.Headers["X-Guest-Token"].FirstOrDefault();
        return token is null ? null : await sessions.ResolvePlayerIdAsync(token);
    }
}
