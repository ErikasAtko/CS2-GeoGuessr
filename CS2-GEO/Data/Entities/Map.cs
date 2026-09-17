namespace CS2_GEO.Data.Entities;

public class Map
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string DisplayName { get; set; } = "";

    public string MinimapUrl { get; set; } = "";

    public List<Location> Locations { get; set; } = new();
}