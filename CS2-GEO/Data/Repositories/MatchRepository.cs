using CS2_GEO.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Data.Repositories;

public class MatchRepository(GameDbContext db) : IMatchRepository
{
    private readonly QueryRepository<Match> _matches = new(db);

    public Task<Match?> GetByIdAsync(int id) =>
        _matches.FindAsync(
            m => m.Id == id,
            q => q
                .Include(m => m.Lobby)
                .Include(m => m.Rounds).ThenInclude(r => r.Location).ThenInclude(l => l.Map)
                .Include(m => m.Rounds).ThenInclude(r => r.Guesses)
                .Include(m => m.Participants).ThenInclude(p => p.Player));

    public async Task<Match> CreateAsync(Lobby lobby, List<Location> roundLocations)
    {
        var match = new Match
        {
            LobbyId = lobby.Id,
            Status = MatchStatus.InProgress,
            StartedAt = DateTime.UtcNow,
        };

        for (var i = 0; i < roundLocations.Count; i++)
            match.Rounds.Add(new MatchRound { RoundNumber = i + 1, LocationId = roundLocations[i].Id });

        foreach (var member in lobby.Members)
            match.Participants.Add(new MatchParticipant { PlayerId = member.PlayerId });

        lobby.Status = LobbyStatus.InProgress;

        _matches.Add(match);
        await _matches.SaveChangesAsync();

        // Reload so Location/Map/Player navs are populated for DTO mapping.
        return (await GetByIdAsync(match.Id))!;
    }

    public Task AddGuessAsync(PlayerGuess guess)
    {
        db.PlayerGuesses.Add(guess);
        return db.SaveChangesAsync();
    }

    public Task<List<PlayerGuess>> GetGuessesForRoundAsync(int matchRoundId) =>
        db.PlayerGuesses
            .Include(g => g.Player)
            .Where(g => g.MatchRoundId == matchRoundId)
            .ToListAsync();

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
