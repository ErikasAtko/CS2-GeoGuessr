using CS2_GEO.Dtos;

namespace CS2_GEO.Services;

public interface IMapsService
{
    Task<List<GameDtos.MapDto>> GetAllAsync();

    Task<GameDtos.MapDto?> GetByCodeAsync(string code);
}
