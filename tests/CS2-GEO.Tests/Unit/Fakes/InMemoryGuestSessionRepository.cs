using CS2_GEO.Data.Entities;
using CS2_GEO.Data.Repositories;

namespace CS2_GEO.Tests.Unit.Fakes;

/// <summary>Fake: keeps guest sessions in a list and assigns player ids the way the database would.</summary>
public class InMemoryGuestSessionRepository : IGuestSessionRepository
{
    private readonly List<GuestSession> _sessions = [];

    public Task<GuestSession?> GetByTokenAsync(string token, CancellationToken cancellationToken) =>
        Task.FromResult(_sessions.FirstOrDefault(s => s.Token == token));

    public Task<GuestSession> CreateAsync(string nickname, string token, CancellationToken cancellationToken)
    {
        var playerId = _sessions.Count + 1;
        var session = new GuestSession
        {
            Id = playerId,
            PlayerId = playerId,
            Player = new Player { Id = playerId, Nickname = nickname, CreatedAt = DateTime.UtcNow },
            Token = token,
            CreatedAt = DateTime.UtcNow,
        };
        _sessions.Add(session);
        return Task.FromResult(session);
    }
}
