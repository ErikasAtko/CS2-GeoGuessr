using CS2_GEO.Services;

namespace CS2_GEO.Endpoints;

public static class MapsApi
{
    public static void MapMapsApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/maps");

        // GET /api/maps - all maps
        group.MapGet("/", async (IMapsService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetAllAsync(cancellationToken)));

        // GET /api/maps/de_mirage
        group.MapGet("/{code}", async (string code, IMapsService service, CancellationToken cancellationToken) =>
        {
            var map = await service.GetByCodeAsync(code, cancellationToken);
            return map is null ? Results.NotFound() : Results.Ok(map);
        });
    }
}
