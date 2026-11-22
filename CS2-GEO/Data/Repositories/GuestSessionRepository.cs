using CS2_GEO.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Data.Repositories;

public class GuestSessionRepository(QueryRepository<GuestSession> sessions, QueryRepository<Player> players) : IGuestSessionRepository
{
    public Task<GuestSession?> GetByTokenAsync(string token) =>
        sessions.FindAsync(s => s.Token == token, q => q.Include(s => s.Player));

    // No password behind this yet - typing a taken name reclaims that player outright, active game
    // included. Deliberate for now (a stepping stone to a real account system); case-insensitive so
    // "Bob" and "bob" land on the same player instead of silently forking into two.
    public async Task<GuestSession> CreateAsync(string nickname, string token)
    {
        var existingPlayer = await players.FindAsync(p => EF.Functions.ILike(p.Nickname, nickname));

        var session = new GuestSession
        {
            Player = existingPlayer ?? new Player { Nickname = nickname, CreatedAt = DateTime.UtcNow },
            Token = token,
            CreatedAt = DateTime.UtcNow,
        };

        sessions.Add(session);
        await sessions.SaveChangesAsync();

        return session;
    }
}
