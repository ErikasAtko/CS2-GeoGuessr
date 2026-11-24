using CS2_GEO.Dtos;

namespace CS2_GEO.Services;

public interface IMapsService
{
    Task<List<MapDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<MapDto?> GetByCodeAsync(string code, CancellationToken cancellationToken);
}
