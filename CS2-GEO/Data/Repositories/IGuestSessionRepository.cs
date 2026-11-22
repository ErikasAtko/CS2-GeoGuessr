using CS2_GEO.Data.Entities;

namespace CS2_GEO.Data.Repositories;

/// <summary>What the services need from stored guest sessions, without saying where they are stored (Dependency Inversion).</summary>
public interface IGuestSessionRepository
{
    /// <summary>The session for a reconnect token, with its <see cref="GuestSession.Player"/> loaded, or <c>null</c> if the token is unknown.</summary>
    Task<GuestSession?> GetByTokenAsync(string token);

    /// <summary>
    /// Creates a new guest credential for <paramref name="nickname"/>: reuses the existing
    /// <see cref="Player"/> if that name (case-insensitive) is already taken, or creates one otherwise.
    /// </summary>
    Task<GuestSession> CreateAsync(string nickname, string token);
}
