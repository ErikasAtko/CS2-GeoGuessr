using CS2_GEO.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Data.Repositories;

public class MatchRepository(
    GameDbContext db,
    QueryRepository<Match> matches,
    QueryRepository<PlayerGuess> guesses) : IMatchRepository
{
    public Task<Match?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        matches.FindAsync(m => m.Id == id, IncludeEverything, cancellationToken);

    public Task<Match?> GetInProgressByLobbyIdAsync(int lobbyId, CancellationToken cancellationToken) =>
        matches.FindAsync(m => m.LobbyId == lobbyId && m.Status == MatchStatus.InProgress, IncludeEverything, cancellationToken);

    public Task<int?> GetLobbyIdAsync(int matchId, CancellationToken cancellationToken) =>
        db.Matches.Where(m => m.Id == matchId).Select(m => (int?)m.LobbyId).FirstOrDefaultAsync(cancellationToken);

    public async Task<Match> CreateAsync(Lobby lobby, List<Location> roundLocations, CancellationToken cancellationToken)
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

        matches.Add(match);
        await matches.SaveChangesAsync(cancellationToken);

        // Reload so Location/Map/Player navs are populated for DTO mapping.
        return (await GetByIdAsync(match.Id, cancellationToken))!;
    }

    // Saving through the shared DbContext also persists whatever the caller changed on tracked
    // entities (the participant's running score), in the same transaction as the guess.
    public Task AddGuessAsync(PlayerGuess guess, CancellationToken cancellationToken)
    {
        guesses.Add(guess);
        return guesses.SaveChangesAsync(cancellationToken);
    }

    public Task<List<PlayerGuess>> GetGuessesForRoundAsync(int matchRoundId, CancellationToken cancellationToken) =>
        db.PlayerGuesses.Where(g => g.MatchRoundId == matchRoundId).ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);

    // Split query: rounds x guesses x participants in one JOIN multiplies rows for no reason.
    private static IQueryable<Match> IncludeEverything(IQueryable<Match> query) =>
        query
            .Include(m => m.Lobby)
            .Include(m => m.Rounds).ThenInclude(r => r.Location).ThenInclude(l => l.Map)
            .Include(m => m.Rounds).ThenInclude(r => r.Guesses)
            .Include(m => m.Participants).ThenInclude(p => p.Player)
            .AsSplitQuery();
}
