using CS2_GEO.Data.Entities;
using CS2_GEO.Data.Repositories;
using CS2_GEO.Dtos;
using CS2_GEO.Game;

namespace CS2_GEO.Services;

/// <summary>The one place that turns stored data into domain values and response shapes.</summary>
public static class DtoMappings
{
    public static Point ToPoint(this Location location) => new(location.X, location.Y);

    /// <summary>Converts a normalized minimap distance into Hammer units for this location's map.</summary>
    public static double ToMapUnits(this Location location, double normalizedDistance) =>
        normalizedDistance * location.Map.SizeUnits;

    public static RoundDto ToRoundDto(this Location location) =>
        new(location.Id, location.Map.Code, location.Map.MinimapUrl, location.ImageUrl);

    public static MapDto ToMapDto(this MapWithLocationCount entry) =>
        new(entry.Map.Id, entry.Map.Code, entry.Map.DisplayName, entry.Map.MinimapUrl, entry.LocationCount);
}
