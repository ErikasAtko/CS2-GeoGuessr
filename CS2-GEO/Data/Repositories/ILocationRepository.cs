using CS2_GEO.Data.Entities;

namespace CS2_GEO.Data.Repositories;

/// <summary>
/// What the services need from stored locations, without saying where they are stored (Dependency Inversion).
/// Every implementation has to honor this contract, or callers can't swap one for another (Liskov):
/// <list type="bullet">
/// <item>Every returned location has its <see cref="Location.Map"/> loaded.</item>
/// <item>A missing id is never an error: it is simply left out (or <c>null</c> for a single lookup).</item>
/// </list>
/// </summary>
public interface ILocationRepository
{
    /// <summary>
    /// Up to <paramref name="count"/> distinct locations in random order, optionally limited to one map.
    /// Returns fewer when there aren't enough, and an empty list when there are none.
    /// </summary>
    Task<List<Location>> GetRandomAsync(string? mapCode, int count);

    Task<Location?> GetByIdAsync(int id);

    /// <summary>The locations with the given ids, keyed by id. Ids that don't exist are not in the result.</summary>
    Task<Dictionary<int, Location>> GetByIdsAsync(IReadOnlyCollection<int> ids);
}
