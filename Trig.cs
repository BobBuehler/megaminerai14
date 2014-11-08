using System;
using System.Linq;
using System.Collections.Generic;

public static class Trig
{
    private static Dictionary<Point, int> _magnitudeMap = new Dictionary<Point, int>();
    public static int CalcMagnitude(Point v)
    {
        int magnitude;
        if (!_magnitudeMap.TryGetValue(v, out magnitude))
        {
            magnitude = (int)Math.Sqrt(v.x * v.x + v.y * v.y);
            _magnitudeMap[v] = magnitude;
        }
        return magnitude;
    }

    public static bool IsInRange(int x1, int y1, int x2, int y2, int range)
    {
        return CalcMagnitude(new Point(x1 - x2, y1 - y2)) < range;
    }

    public static bool IsInRange(Point p1, Point p2, int range)
    {
        return CalcMagnitude(new Point(p1.x - p2.x, p1.y - p2.y)) < range;
    }

    public static IEnumerable<Point> PointsInRect(int minX, int maxX, int minY, int maxY)
    {
        // Bound
        if (minX < 0) minX = 0;
        if (maxX >= Bb.Width) maxX = Bb.Width - 1;
        if (minY < 0) minY = 0;
        if (maxY >= Bb.Height) maxY = Bb.Height - 1;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                yield return new Point(x, y);
            }
        }
    }

    public static IEnumerable<Point> PointsInCircle(Circle c)
    {
        return PointsInRect(c.p.x - c.r, c.p.x + c.r, c.p.y - c.r, c.p.y + c.r).Where(p => c.IsInRange(p));
    }
}
