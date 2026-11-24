using CS2_GEO.Data.Repositories;
using CS2_GEO.Dtos;

namespace CS2_GEO.Services;

public class MapsService(IMapRepository maps) : IMapsService
{
    public async Task<List<MapDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var all = await maps.GetAllAsync(cancellationToken);
        return all.Select(m => m.ToMapDto()).ToList();
    }

    public async Task<MapDto?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        var map = await maps.GetByCodeAsync(code, cancellationToken);
        return map?.ToMapDto();
    }
}
