using System;
using System.Collections.Generic;

public static class Solver
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
}
