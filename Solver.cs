using System;
using System.Linq;
using System.Collections.Generic;

public static class Solver
{
    public static IEnumerable<Point> FindPointsInCirclesNearestTargets(int pointCount, IEnumerable<Circle> starts, IEnumerable<Point> targets, IEnumerable<Circle> avoids)
    {
        if (pointCount <= 0)
        {
            return new Point[] { };
        }

        var startablePoints = starts.SelectMany(c => Trig.CalcPointsInCircle(c));
        var avoidPoints = avoids.SelectMany(c => Trig.CalcPointsInCircle(c));

        var avoidEdgedStarts = avoids.SelectMany(c => Trig.CalcOuterEdgeOfCircle(c))
            .Intersect(startablePoints);

        var potentialStarts = starts.SelectMany(c => Trig.CalcInnerEdgeOfCircle(c))
            .Union(avoidEdgedStarts)
            .Except(avoidPoints);

        return potentialStarts.Where(p => p.IsOnBoard()).Distinct().MinByValue(pointCount, p => targets.Min(t => Trig.Distance(p, t)));
    }
}
