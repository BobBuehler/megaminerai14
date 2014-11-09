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

        var avoidEdgedStarts = avoids.SelectMany(c => Trig.CalcOuterEdgeOfCircle(c))
            .Where(p => starts.Any(c => c.IsInRange(p)));

        var potentialStarts = starts.SelectMany(c => Trig.CalcInnerEdgeOfCircle(c))
            .Concat(avoidEdgedStarts)
            .Where(p => p.IsOnBoard())
            .Where(p => !avoids.Any(c => c.IsInRange(p)))
            .Distinct();

        return potentialStarts.MinByValue(pointCount, p => targets.Min(t => Trig.Distance(p, t)));
    }

    private static HashSet<Point> poolPoints;
    private static HashSet<Point> poolEdges;
    private static HashSet<Point> nearEnemies;
    public static void PreCalc()
    {
        poolPoints = Bb.pools.SelectMany(p => Trig.CalcPointsInCircle(new Circle(p, 100))).ToHashSet();
        poolEdges = Bb.pools.SelectMany(p => Trig.CalcOuterEdgeOfCircle(new Circle(p, 100))).Where(p => IsPassable(p)).ToHashSet();
        nearEnemies = Bb.allTheirPlants.Select(p => new Point(p.x - 1, p.y)).Where(p => IsPassable(p)).ToHashSet();

    }

    public static bool IsPassable(Point p)
    {
        return p.IsOnBoard() && !Bb.plantLookup.ContainsKey(p) && !poolPoints.Contains(p);
    }

    public static Point CalcNextStep(Point start, Point goal, int stepSize, int goalRange)
    {
        Func<Point, IEnumerable<Point>> getNeighboors = p =>
        {
            return Trig.CalcInnerEdgeOfCircle(new Circle(p, stepSize))
                .Concat(poolEdges)
                .Concat(nearEnemies)
                .Where(n => Trig.IsInRange(p, n, stepSize) && IsPassable(n));
        };

        var astar = new Pather.AStar(
            start.Single(),
            p => Trig.IsInRange(p, goal, goalRange),
            (a, b) => Trig.IsInRange(a, b, stepSize) ? 1 : 2,
            p => Trig.Distance(p, goal),
            getNeighboors);
        var path = astar.Path;
        if (path.Count() > 1)
        {
            var step = path.ElementAt(1);
            if (IsPassable(step))
            {
                return step;
            }
        }
        return new Point(-1, -1);
    }
}
