using CS2_GEO.Services;

namespace CS2_GEO.Endpoints;

public static class MapsApi
{
    public static void MapMapsApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/maps");

        // GET /api/maps - all maps
        group.MapGet("/", async (IMapsService service) =>
            Results.Ok(await service.GetAllAsync()));

        // GET /api/maps/de_mirage
        group.MapGet("/{code}", async (string code, IMapsService service) =>
        {
            var map = await service.GetByCodeAsync(code);
            return map is null ? Results.NotFound() : Results.Ok(map);
        });
    }
}
