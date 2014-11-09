using System;
using System.Linq;
using System.Collections.Generic;

public static class Solver
{
    public static IEnumerable<Point> FindPointsInCirclesNearestTargets(int pointCount, IEnumerable<Circle> starts, IEnumerable<Point> targets, IEnumerable<Circle> avoids)
    {
        var startablePoints = starts.SelectMany(c => Trig.PointsInCircle(c));
        var avoidPoints = avoids.SelectMany(c => Trig.PointsInCircle(c));

        var avoidEdgedStarts = avoids.SelectMany(c => Trig.CalcOuterEdgeOfCircle(c)).ToHashSet();
        avoidEdgedStarts.IntersectWith(startablePoints);

        var potentialStarts = starts.SelectMany(c => Trig.CalcInnerEdgeOfCircle(c)).ToHashSet();
        potentialStarts.UnionWith(avoidEdgedStarts);

        potentialStarts.ExceptWith(avoidPoints);

        return potentialStarts.MinByValue(pointCount, p => targets.Min(t => Trig.Distance(p, t)));
    }
}
