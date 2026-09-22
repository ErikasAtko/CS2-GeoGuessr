namespace CS2_GEO.Game;

/// <summary>A location expressed as normalized (0-1) coordinates on a minimap image.</summary>
public readonly struct Point
{
    public double X { get; }
    public double Y { get; }

    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }
}
