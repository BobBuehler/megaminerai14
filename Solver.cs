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

    private static HashSet<Point> poolEdges;
    private static HashSet<Point> nearEnemies;
    private static HashSet<Point> nearAllies;
    private static HashSet<Point> impassable;
    public static void PreCalc()
    {
        impassable = Bb.pools.SelectMany(p => Trig.CalcPointsInCircle(new Circle(p, 100))).Concat(Bb.plantLookup.Keys).ToHashSet();

        poolEdges = Bb.pools.SelectMany(p => Trig.CalcOuterEdgeOfCircle(new Circle(p, 100))).Where(p => IsPassable(p)).ToHashSet();
        nearEnemies = Bb.allTheirPlants.Select(p => new Point(p.x - 1, p.y)).Where(p => IsPassable(p)).ToHashSet();
        nearAllies = Bb.allOurPlants.Select(p => new Point(p.x - 1, p.y)).Where(p => IsPassable(p)).ToHashSet();

    }

    public static bool IsPassable(Point p)
    {
        return p.IsOnBoard() && !impassable.Contains(p);
    }

    public static void MoveToward(Point mover, Point target, int range)
    {
        var plant = mover.GetPlant();
        var speed = Bb.GetUprootRange(plant);
        Func<Point, IEnumerable<Point>> getNeighboors = p => {
            return Trig.CalcInnerEdgeOfCircle(new Circle(p, speed))
                .Where(p => IsPassable(p));
        };

        var astar = new Pather.AStar(
            mover.Single(),
            p => Trig.IsInRange(p, target, range),
            (a, b) => Trig.IsInRange(a, b, speed) ? 1 : 2,
            p => Trig.Distance(p, target),
            getNeighboors);
        var path = astar.Path;
        if (path.Any())
        {
            var step = path.ElementAt(1);
            if (IsPassable(step))
            {
                mover.GetPlant().uproot(step.x, step.y);
            }
        }
    }
}
