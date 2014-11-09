using System;
using System.Linq;
using System.Collections.Generic;

public static class Trig
{
    private static double[] edgePoints = {0.0, 0.5};

    private static Func<Point, int> _calcMagnitude = v => (int)Math.Sqrt(v.x * v.x + v.y * v.y);
    public static Func<Point, int> CalcMagnitude = _calcMagnitude.Memoize();

    private static Func<int, IEnumerable<Point>> _calcInnerEdge = r => {
        var rSquare = r * r;
        List<Point> halfQuad = new List<Point>();
        foreach (double e in edgePoints)
        {
            var y = e * r;
            var x = Math.Ceiling(Math.Sqrt(rSquare - y * y));
            halfQuad.Add(new Point((int)x, (int)Math.Ceiling(y)));
        }
        return halfQuad.SelectMany(p => {
            return new Point[] {
                p,
                new Point(p.y, p.x),
                new Point(-p.x, p.y),
                new Point(-p.y, p.x),
                new Point(-p.x, -p.y),
                new Point(-p.y, -p.x),
                new Point(p.x, -p.y),
                new Point(p.y, -p.x),
            };
        }).ToHashSet();
    };
    public static Func<int, IEnumerable<Point>> CalcInnerEdge = _calcInnerEdge.Memoize();

    public static IEnumerable<Point> CalcInnerEdgeOfCircle(Circle c)
    {
        return CalcInnerEdge(c.r).Select(p => p.Add(c.p));
    }

    private static Func<int, IEnumerable<Point>> _calcOuterEdge = r =>
    {
        double rSquare = r * r;
        List<Point> halfQuad = new List<Point>();
        foreach (double e in edgePoints)
        {
            var y = e * r;
            var x = Math.Ceiling(Math.Sqrt(rSquare - y * y));
            halfQuad.Add(new Point((int)x + 1, (int)Math.Ceiling(y)));
        }
        return halfQuad.SelectMany(p =>
        {
            return new Point[] {
                p,
                new Point(p.y, p.x),
                new Point(-p.x, p.y),
                new Point(-p.y, p.x),
                new Point(-p.x, -p.y),
                new Point(-p.y, -p.x),
                new Point(p.x, -p.y),
                new Point(p.y, -p.x),
            };
        }).ToHashSet();
    };
    public static Func<int, IEnumerable<Point>> CalcOuterEdge = _calcOuterEdge.Memoize();

    public static IEnumerable<Point> CalcOuterEdgeOfCircle(Circle c)
    {
        return CalcOuterEdge(c.r).Select(p => p.Add(c.p));
    }

    public static int Distance(int x1, int y1, int x2, int y2)
    {
        return CalcMagnitude(new Point(x1 - x2, y1 - y2));
    }

    public static int Distance(Point p1, Point p2)
    {
        return Distance(p1.x, p1.y, p2.x, p2.y);
    }

    public static bool IsInRange(int x1, int y1, int x2, int y2, int range)
    {
        return Distance(x1, y1, x2, y2) <= range;
    }

    public static bool IsInRange(Point p1, Point p2, int range)
    {
        return IsInRange(p1.x, p1.y, p2.x, p2.y, range);
    }

    public static IEnumerable<Point> PointsInRect(int minX, int maxX, int minY, int maxY)
    {
        // Bound
        /*
        if (minX < 0) minX = 0;
        if (maxX >= Bb.Width) maxX = Bb.Width - 1;
        if (minY < 0) minY = 0;
        if (maxY >= Bb.Height) maxY = Bb.Height - 1;
        */
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                yield return new Point(x, y);
            }
        }
    }

    private static IEnumerable<Point> PointsInCircle(Circle c)
    {
        return PointsInRect(c.p.x - c.r, c.p.x + c.r, c.p.y - c.r, c.p.y + c.r).Where(p => c.IsInRange(p));
    }
    private static Func<Circle, IEnumerable<Point>> _calcPointsInCircle = c => PointsInCircle(c).ToHashSet();
    public static Func<Circle, IEnumerable<Point>> CalcPointsInCircle = _calcPointsInCircle.Memoize();

    public static Tuple<Point, Point> FindClosestPair(IEnumerable<Point> set1, IEnumerable<Point> set2)
    {
        var allPairs = set1.SelectMany(p1 => set2.Select(p2 => Tuple.Create(p1, p2)));
        return allPairs.MinByValue(pp => Trig.Distance(pp.Item1, pp.Item2));
    }

    public static bool IsInRect(Point p, int minX, int maxX, int minY, int maxY)
    {
        return p.x >= minX && p.x <= maxX && p.y >= minY && p.y <= maxY;
    }

    public static bool IsInBounds(Point p)
    {
        return IsInRect(p, 0, Bb.Width - 1, 0, Bb.Height - 1);
    }
}
