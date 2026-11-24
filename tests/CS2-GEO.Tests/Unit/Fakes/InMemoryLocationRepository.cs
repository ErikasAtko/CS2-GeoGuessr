using CS2_GEO.Data.Entities;
using CS2_GEO.Data.Repositories;

namespace CS2_GEO.Tests.Unit.Fakes;

/// <summary>
/// Fake: a working, in-memory <see cref="ILocationRepository"/> that keeps the interface's contract
/// (every location has its map loaded, unknown ids are left out, never an error), so services can be
/// tested without a database. "Random" is simply insertion order, which keeps the tests deterministic.
/// </summary>
public class InMemoryLocationRepository : ILocationRepository
{
    private readonly List<Location> _locations = [];

    public Location Add(Map map, double x, double y)
    {
        var location = new Location
        {
            Id = _locations.Count + 1,
            MapId = map.Id,
            Map = map,
            ImageUrl = $"/screenshots/{map.Code}/{_locations.Count + 1}.jpg",
            X = x,
            Y = y,
        };
        _locations.Add(location);
        return location;
    }

    public Task<List<Location>> GetRandomAsync(string? mapCode, int count, CancellationToken cancellationToken) =>
        Task.FromResult(_locations.Where(l => mapCode is null || l.Map.Code == mapCode).Take(count).ToList());

    public Task<Location?> GetByIdAsync(int id, CancellationToken cancellationToken) => Task.FromResult(_locations.FirstOrDefault(l => l.Id == id));

    public Task<Dictionary<int, Location>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken) =>
        Task.FromResult(_locations.Where(l => ids.Contains(l.Id)).ToDictionary(l => l.Id));
}
