namespace CS2_GEO.Data.Entities;

public class Location
{
    public int Id { get; set; }
    public int MapId { get; set; }
    public Map Map { get; set; } = null!;
    public string ImageUrl { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
}