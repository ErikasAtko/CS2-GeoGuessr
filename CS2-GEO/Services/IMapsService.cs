using CS2_GEO.Dtos;

namespace CS2_GEO.Services;

public interface IMapsService
{
    Task<List<MapDto>> GetAllAsync();

    Task<MapDto?> GetByCodeAsync(string code);
}
