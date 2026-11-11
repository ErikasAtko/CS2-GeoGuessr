using CS2_GEO.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Data.Repositories;

public class LobbyRepository(GameDbContext db) : ILobbyRepository
{
    private readonly QueryRepository<Lobby> lobbies = new(db);

    public Task<Lobby?> GetByJoinCodeAsync(string joinCode) =>
        lobbies.FindAsync(
            l => l.JoinCode == joinCode,
            q => q.Include(l => l.Members).ThenInclude(m => m.Player));

    public async Task<Lobby> CreateAsync(int hostPlayerId, string joinCode, string? mapCode, int roundCount)
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
        await lobbies.SaveChangesAsync();

        // Reload so Members[].Player is populated for DTO mapping.
        return (await GetByJoinCodeAsync(joinCode))!;
    }

    public async Task AddMemberAsync(int lobbyId, int playerId)
    {
        db.LobbyMemberships.Add(new LobbyMembership { LobbyId = lobbyId, PlayerId = playerId, JoinedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
    }
}
