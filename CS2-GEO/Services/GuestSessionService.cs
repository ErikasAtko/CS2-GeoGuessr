using System.Security.Cryptography;
using CS2_GEO.Data.Repositories;
using CS2_GEO.Dtos;

namespace CS2_GEO.Services;

public class GuestSessionService(IGuestSessionRepository sessions) : IGuestSessionService
{
    public async Task<CreateGuestSessionResult> CreateAsync(string nickname)
    {
        // System.Text.Json doesn't enforce non-nullable reference types at runtime, so a missing
        // field in the request body still binds to null despite the parameter's compile-time type.
        nickname = nickname?.Trim() ?? "";
        if (nickname.Length is < 1 or > 32)
            return new CreateGuestSessionResult(null, CreateGuestSessionError.InvalidNickname);

        var token = GenerateToken();
        var session = await sessions.CreateAsync(nickname, token);

        return new CreateGuestSessionResult(new GuestSessionDto(session.PlayerId, session.Player.Nickname, token), null);
    }

    public async Task<int?> ResolvePlayerIdAsync(string token)
    {
        var session = await sessions.GetByTokenAsync(token);
        return session?.PlayerId;
    }

    // 256 bits of randomness is a reconnect credential, not a password - it only needs to be unguessable, not hashed.
    private static string GenerateToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
}
