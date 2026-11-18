using CS2_GEO.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Data.Repositories;

public class LocationRepository(GameDbContext db, QueryRepository<Location> locations) : ILocationRepository
{
    public Task<List<Location>> GetRandomAsync(string? mapCode, int count)
    {
        var query = db.Locations.Include(l => l.Map).AsQueryable();

        if (mapCode is not null)
            query = query.Where(l => l.Map.Code == mapCode);

        // Each row is picked at most once, so locations never repeat in one result.
        return query
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .ToListAsync();
    }

    public Task<Location?> GetByIdAsync(int id) =>
        locations.FindAsync(l => l.Id == id, IncludeMap);

    public Task<Dictionary<int, Location>> GetByIdsAsync(IReadOnlyCollection<int> ids) =>
        locations.ToDictionaryAsync(l => ids.Contains(l.Id), l => l.Id, IncludeMap);

    // ILocationRepository promises every returned location has its Map loaded.
    private static IQueryable<Location> IncludeMap(IQueryable<Location> query) => query.Include(l => l.Map);
}
