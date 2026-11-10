namespace CS2_GEO.Data.Repositories;

/// <summary>
/// What the services need from stored maps, without saying where they are stored (Dependency Inversion).
/// A missing map is never an error: single lookups return <c>null</c>.
/// </summary>
public interface IMapRepository
{
    /// <summary>Every map with its location count, ordered by display name.</summary>
    Task<List<MapWithLocationCount>> GetAllAsync();

    Task<MapWithLocationCount?> GetByCodeAsync(string code);
}
