using System;
using System.Linq;
using System.Collections.Generic;

public static class Extensions
{
    public static Circle ToCircle(this Point p, int r)
    {
        return new Circle(p, r);
    }

    public static IEnumerable<Circle> ToCircles(this IEnumerable<Point> points, int r)
    {
        return points.Select(p => p.ToCircle(r));
    }

    public static bool IsInRange(this Circle c, Point p)
    {
        return Trig.IsInRange(c.p, p, c.r);
    }

    public static Func<Point, bool> RangeChecker(this Circle c)
    {
        return p => c.IsInRange(p);
    }

    public static IEnumerable<Point> PointsInRange(this Circle c)
    {
        return Trig.PointsInCircle(c);
    }
}
