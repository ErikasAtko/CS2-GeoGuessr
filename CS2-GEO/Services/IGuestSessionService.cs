using CS2_GEO.Dtos;

namespace CS2_GEO.Services;

public enum CreateGuestSessionError
{
    InvalidNickname,
}

public record CreateGuestSessionResult(GuestSessionDto? Session, CreateGuestSessionError? Error);

public interface IGuestSessionService
{
    Task<CreateGuestSessionResult> CreateAsync(string nickname);

    /// <summary>The player a reconnect token belongs to, or <c>null</c> if the token is unknown.</summary>
    Task<int?> ResolvePlayerIdAsync(string token);
}
