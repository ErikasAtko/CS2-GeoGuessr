using CS2_GEO.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Data.Repositories;

public class LobbyRepository(GameDbContext db, QueryRepository<Lobby> lobbies) : ILobbyRepository
{
    public Task<Lobby?> GetByJoinCodeAsync(string joinCode, CancellationToken cancellationToken) =>
        lobbies.FindAsync(
            l => l.JoinCode == joinCode,
            cancellationToken,
            q => q.Include(l => l.Members).ThenInclude(m => m.Player));

    public Task<int?> GetIdByJoinCodeAsync(string joinCode, CancellationToken cancellationToken) =>
        db.Lobbies.Where(l => l.JoinCode == joinCode).Select(l => (int?)l.Id).FirstOrDefaultAsync(cancellationToken);

    public Task<Lobby?> GetActiveForPlayerAsync(int playerId, CancellationToken cancellationToken) =>
        db.Lobbies
            .Where(l => l.Status != LobbyStatus.Finished && l.Members.Any(m => m.PlayerId == playerId))
            .Include(l => l.Members).ThenInclude(m => m.Player)
            .OrderByDescending(l => l.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Lobby> CreateAsync(int hostPlayerId, string joinCode, string? mapCode, int roundCount, CancellationToken cancellationToken)
    {
        var lobby = new Lobby
        {
            JoinCode = joinCode,
            HostPlayerId = hostPlayerId,
            MapCode = mapCode,
            RoundCount = roundCount,
            CreatedAt = DateTime.UtcNow,
        };
        lobby.Members.Add(new LobbyMembership { PlayerId = hostPlayerId, IsReady = true, JoinedAt = DateTime.UtcNow });

        lobbies.Add(lobby);
        await lobbies.SaveChangesAsync(cancellationToken);

        // Reload so Members[].Player is populated for DTO mapping.
        return (await GetByJoinCodeAsync(joinCode, cancellationToken))!;
    }

    public async Task AddMemberAsync(int lobbyId, int playerId, CancellationToken cancellationToken)
    {
        db.LobbyMemberships.Add(new LobbyMembership { LobbyId = lobbyId, PlayerId = playerId, JoinedAt = DateTime.UtcNow });
        await db.SaveChangesAsync(cancellationToken);
    }
}
