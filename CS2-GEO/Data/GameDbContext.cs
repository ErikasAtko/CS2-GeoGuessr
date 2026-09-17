using CS2_GEO.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Data;

public class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options)
{
    public DbSet<Map> Maps => Set<Map>();
    public DbSet<Location> Locations => Set<Location>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Map>()
            .HasIndex(m => m.Code)
            .IsUnique();
    }
}