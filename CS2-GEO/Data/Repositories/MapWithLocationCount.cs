using CS2_GEO.Data.Entities;

namespace CS2_GEO.Data.Repositories;

/// <summary>A map together with how many locations it has, counted by the database instead of loading them all.</summary>
public record MapWithLocationCount(Map Map, int LocationCount);
