namespace CS2_GEO.Game;

public static class PointExtensions
{
    public static double DistanceTo(this Point a, Point b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
