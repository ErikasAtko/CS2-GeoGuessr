using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Data.Repositories;

public class MapRepository(GameDbContext db) : IMapRepository
{
    public Task<List<MapWithLocationCount>> GetAllAsync(CancellationToken cancellationToken) =>
        db.Maps
            .OrderBy(m => m.DisplayName)
            .Select(m => new MapWithLocationCount(m, m.Locations.Count))
            .ToListAsync(cancellationToken);

    public Task<MapWithLocationCount?> GetByCodeAsync(string code, CancellationToken cancellationToken) =>
        db.Maps
            .Where(m => m.Code == code)
            .Select(m => new MapWithLocationCount(m, m.Locations.Count))
            .FirstOrDefaultAsync(cancellationToken);
}
