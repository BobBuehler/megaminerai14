using System;
using System.Linq;
using System.Collections.Generic;

public static class Solver
{
    public static Point FindPointInCirclesNearTargets(IEnumerable<Circle> circles, IEnumerable<Point> targets, Func<Point, bool> avoid)
    {
        var circlePoints = circles.SelectMany(c => c.PointsInRange()).ToHashSet();
        return circlePoints.First();
    }

    public static Point FindPointInCircleNearestTarget(IEnumerable<Circle> spawnable, IEnumerable<Point> targets, IEnumerable<Circle> avoids)
    {
        return new Point();
    }
}
