using CS2_GEO.Data.Entities;

namespace CS2_GEO.Data.Repositories;

/// <summary>What the services need from stored matches, without saying where they are stored (Dependency Inversion).</summary>
public interface IMatchRepository
{
    /// <summary>
    /// The match for an id, with its lobby, rounds (locations + their maps), guesses, and
    /// participants (+ players) all loaded - everything a round-completion check needs in one query.
    /// </summary>
    Task<Match?> GetByIdAsync(int id);

    /// <summary>Creates a match for a lobby's current members, with a fixed round order, and flips the lobby to <see cref="LobbyStatus.InProgress"/>.</summary>
    Task<Match> CreateAsync(Lobby lobby, List<Location> roundLocations);

    Task AddGuessAsync(PlayerGuess guess);

    /// <summary>
    /// A fresh read of every guess submitted for a round, with players loaded. Deliberately a real
    /// query rather than trusting an in-memory navigation collection: EF Core's automatic fixup of an
    /// already-loaded collection after <see cref="AddGuessAsync"/> isn't guaranteed to have applied
    /// by the time the caller checks it, so relying on it made round-completion detection flaky.
    /// </summary>
    Task<List<PlayerGuess>> GetGuessesForRoundAsync(int matchRoundId);

    /// <summary>Persists whatever changes the caller already made to tracked entities (e.g. a participant's running score, match/lobby status).</summary>
    Task SaveChangesAsync();
}
