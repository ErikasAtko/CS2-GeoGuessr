using CS2_GEO.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Data.Repositories;

public class LocationRepository(GameDbContext db) : ILocationRepository
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
        db.Locations
            .Include(l => l.Map)
            .FirstOrDefaultAsync(l => l.Id == id);

    public Task<Dictionary<int, Location>> GetByIdsAsync(IReadOnlyCollection<int> ids) =>
        db.Locations
            .Include(l => l.Map)
            .Where(l => ids.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id);
}
