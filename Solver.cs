using System;
using System.Linq;
using System.Collections.Generic;

public static class Solver
{
    public static Point FindPointInCirclesNearestTargets(IEnumerable<Circle> spawnable, IEnumerable<Point> targets, IEnumerable<Circle> avoids)
    {
        var spawnablePoints = spawnable.SelectMany(c => Trig.PointsInCircle(c));
        var avoidPoints = avoids.SelectMany(c => Trig.PointsInCircle(c));

        var avoidEdgedSpawns = avoids.SelectMany(c => Trig.CalcOuterEdgeOfCircle(c)).ToHashSet();
        avoidEdgedSpawns.IntersectWith(spawnablePoints);

        var potentialSpawns = spawnable.SelectMany(c => Trig.CalcInnerEdgeOfCircle(c)).ToHashSet();
        potentialSpawns.UnionWith(avoidEdgedSpawns);

        potentialSpawns.ExceptWith(avoidPoints);

        return potentialSpawns.MinByValue(p => targets.Min(t => Trig.Distance(p, t)));
    }
}
