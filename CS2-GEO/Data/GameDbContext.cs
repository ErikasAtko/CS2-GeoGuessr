using CS2_GEO.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Data;

public class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options)
{
    public DbSet<Map> Maps => Set<Map>();
    public DbSet<Location> Locations => Set<Location>();

    public DbSet<Player> Players => Set<Player>();
    public DbSet<GuestSession> GuestSessions => Set<GuestSession>();
    public DbSet<Lobby> Lobbies => Set<Lobby>();
    public DbSet<LobbyMembership> LobbyMemberships => Set<LobbyMembership>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchRound> MatchRounds => Set<MatchRound>();
    public DbSet<MatchParticipant> MatchParticipants => Set<MatchParticipant>();
    public DbSet<PlayerGuess> PlayerGuesses => Set<PlayerGuess>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Map>()
            .HasIndex(m => m.Code)
            .IsUnique();

        modelBuilder.Entity<GuestSession>()
            .HasIndex(s => s.Token)
            .IsUnique();

        modelBuilder.Entity<Lobby>()
            .HasIndex(l => l.JoinCode)
            .IsUnique();

        // A player can only be in a lobby once, so concurrent duplicate joins collide on this index instead of creating two rows.
        modelBuilder.Entity<LobbyMembership>()
            .HasIndex(m => new { m.LobbyId, m.PlayerId })
            .IsUnique();

        // A player can only participate in a match once.
        modelBuilder.Entity<MatchParticipant>()
            .HasIndex(p => new { p.MatchId, p.PlayerId })
            .IsUnique();

        // A player can only guess a given round once, so two concurrent submits collide here rather than both succeeding.
        modelBuilder.Entity<PlayerGuess>()
            .HasIndex(g => new { g.MatchRoundId, g.PlayerId })
            .IsUnique();
    }
}
