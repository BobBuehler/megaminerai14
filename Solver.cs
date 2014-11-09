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
        return p.IsOnBoard() && !Bb.plantLookup.ContainsKey(p) && !poolPoints.Contains(p) && !Bb.spawning.Contains(p);
    }

    public static bool Spawn(int plantType, Point goal, int goalRange)
    {
        if (AI.me.Spores < AI.sporeCosts[plantType]) return false;

        var uprootRange = Bb.GetUprootRange(plantType);
        var starts = Bb.ourMother.Concat(Bb.ourSpawners);
        Func<Point, IEnumerable<Point>> getNeighboors = p =>
        {
            var stepSize = Bb.plantLookup.ContainsKey(p) ? p.GetPlant().Range : uprootRange;
            return Trig.CalcInnerEdgeOfCircle(new Circle(p, stepSize))
                .Concat(poolEdges)
                .Concat(nearEnemies)
                .Where(n => Trig.IsInRange(p, n, stepSize) && IsPassable(n));
        };

        var astar = new Pather.AStar(
            starts,
            p => Trig.IsInRange(p, goal, goalRange),
            (a, b) => 1,
            p => Trig.Distance(p, goal) / uprootRange,
            getNeighboors);
        if (astar.Path.Count() > 1)
        {
            var p = astar.Path.ElementAt(1);
            AI.me.germinate(p.x, p.y, plantType);
            Bb.spawning.Add(p);
            return true;
        }
        return false;
    }

    public static bool Uproot(Point mover, Point goal, int goalRange)
    {
        if (mover.GetPlant().UprootsLeft <= 0)
        {
            return false;
        }
        Bb.readBoard();
        var plant = mover.GetPlant();
        var path = CalcPath(mover.Single(), Bb.GetUprootRange(plant), goal, goalRange);
        if (path.Count() > 1)
        {
            var step = path.ElementAt(1);
            plant.uproot(step.x, step.y);
            return true;
        }
        return false;
    }

    public static IEnumerable<Point> CalcPath(IEnumerable<Point> starts, int stepSize, Point goal, int goalRange)
    {
        Func<Point, IEnumerable<Point>> getNeighboors = p =>
        {
            return Trig.CalcInnerEdgeOfCircle(new Circle(p, stepSize))
                .Concat(poolEdges)
                .Concat(nearEnemies)
                .Where(n => Trig.IsInRange(p, n, stepSize) && IsPassable(n));
        };

        var astar = new Pather.AStar(
            starts,
            p => Trig.IsInRange(p, goal, goalRange),
            (a, b) => 1,
            p => Trig.Distance(p, goal) / stepSize,
            getNeighboors);
        return astar.Path;
    }
}
