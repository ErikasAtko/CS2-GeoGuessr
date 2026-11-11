using CS2_GEO.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Data.Repositories;

public class GuestSessionRepository(GameDbContext db) : IGuestSessionRepository
{
    private readonly QueryRepository<GuestSession> _sessions = new(db);

    public Task<GuestSession?> GetByTokenAsync(string token) =>
        _sessions.FindAsync(s => s.Token == token, q => q.Include(s => s.Player));

    public async Task<GuestSession> CreateAsync(string nickname, string token)
    {
        var session = new GuestSession
        {
            Player = new Player { Nickname = nickname, CreatedAt = DateTime.UtcNow },
            Token = token,
            CreatedAt = DateTime.UtcNow,
        };

        _sessions.Add(session);
        await _sessions.SaveChangesAsync();

        return session;
    }
}
