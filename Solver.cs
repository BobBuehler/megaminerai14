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
    public static void PreCalc()
    {
        poolPoints = Bb.pools.SelectMany(p => Trig.CalcPointsInCircle(new Circle(p, 100))).ToHashSet();

    }

    public static bool IsPassable(Point p, bool avoidPools = true)
    {
        return p.IsOnBoard() && !Bb.plantLookup.ContainsKey(p) && !Bb.spawning.Contains(p) && !(avoidPools && poolPoints.Contains(p));
    }

    public static Point Spawn(int plantType, Point goal, int goalRange, bool avoidPools = true)
    {
        if (AI.me.Spores < AI.sporeCosts[plantType]) return new Point(-1, -1);

        var uprootRange = Bb.GetUprootRange(plantType);
        var starts = Bb.ourMother.Concat(Bb.ourSpawners);
        var goalEdge = Trig.CalcInnerEdgeOfCircle(new Circle(goal, goalRange));
        var additionalNeighboors = goalEdge.Where(n => IsPassable(n)).ToHashSet();

        Func<Point, IEnumerable<Point>> getNeighboors = p =>
        {
            var stepSize = Bb.plantLookup.ContainsKey(p) ? p.GetPlant().Range : uprootRange;
            return Trig.CalcInnerEdgeOfCircle(new Circle(p, stepSize))
                .Concat(additionalNeighboors)
                .Where(n => IsPassable(n, avoidPools) && Trig.IsInRange(p, n, stepSize));
        };

        var astar = new Pather.AStar(
            starts,
            p => Trig.IsInRange(p, goal, goalRange) && IsPassable(p, avoidPools),
            (a, b) => 1,
            p => (int)Math.Ceiling(Trig.Distance(p, goal) / (double)uprootRange),
            getNeighboors);
        if (astar.Path.Count() > 1)
        {
            var p = astar.Path.ElementAt(1);
            AI.me.germinate(p.x, p.y, plantType);
            Bb.spawning.Add(p);
            return p;
        }
        return new Point(-1, -1);
    }

    public static bool Uproot(Point mover, Point goal, int goalRange, bool avoidPools = true)
    {
        if (mover.GetPlant().UprootsLeft <= 0)
        {
            return false;
        }
        Bb.readBoard();
        var plant = mover.GetPlant();
        var astar = Search(mover.Single(), Bb.GetUprootRange(plant), goal, goalRange);
        if (astar.Path.Count() > 1)
        {
            var step = astar.Path.ElementAt(1);
            plant.uproot(step.x, step.y);
            return true;
        }
        return false;
    }

    public static Pather.AStar Search(IEnumerable<Point> starts, int stepSize, Point goal, int goalRange, bool avoidPools = true)
    {
        var goalEdge = Trig.CalcInnerEdgeOfCircle(new Circle(goal, goalRange));
        var additionalNeighboors = goalEdge.Where(n => IsPassable(n)).ToHashSet();

        Func<Point, IEnumerable<Point>> getNeighboors = p =>
        {
            return Trig.CalcInnerEdgeOfCircle(new Circle(p, stepSize))
                .Concat(additionalNeighboors)
                .Where(n => IsPassable(n, avoidPools) && Trig.IsInRange(p, n, stepSize));
        };

        var astar = new Pather.AStar(
            starts,
            p => Trig.IsInRange(p, goal, goalRange),
            (a, b) => 1,
            p => (int)Math.Ceiling(Trig.Distance(p, goal) / (double)stepSize),
            getNeighboors);
        return astar;
    }

    public static void DefendMother(Point defender, IEnumerable<Point> targets)
    {
        Bb.readBoard();
        if (!targets.Any() || !Bb.plantLookup.ContainsKey(defender))
        {
            return;
        }
        var mother = Bb.ourMother.First();
        var plant = defender.GetPlant();
        var nearestTarget = targets.MinByValue(t => Trig.Distance(defender, t));
        var nearestDistance = Trig.Distance(defender, nearestTarget);
        switch (plant.Mutation)
        {
            case AI.ARALIA:
            case AI.CHOKER:
                if (plant.RadiatesLeft > 0)
                {
                    if (plant.UprootsLeft > 0 && nearestDistance > plant.Range && nearestDistance <= plant.Range + Bb.GetUprootRange(plant.Mutation))
                    {
                        var astar = Search(defender.Single(), Bb.GetUprootRange(plant.Mutation), nearestTarget, plant.Range);
                        if (astar.Path.Count() == 2)
                        {
                            var step = astar.Path.ElementAt(1);
                            plant.uproot(step.x, step.y);
                        }
                    }
                    if (Trig.Distance(plant.ToPoint(), nearestTarget) <= plant.Range)
                    {
                        plant.radiate(nearestTarget.x, nearestTarget.y);
                    }
                }
                if (plant.UprootsLeft > 0 && Trig.Distance(mother, defender) > 75)
                {
                    Uproot(defender, mother, 75);
                }
                break;
            case AI.TITAN:
                if (plant.UprootsLeft > 0 && Trig.Distance(mother, defender) > 75)
                {
                    Uproot(defender, mother, 75);
                }
                if (plant.UprootsLeft > 0)
                {
                    var dest = Trig.CalcPointsInCircle(plant.ToUprootCircle()).Where(p => IsPassable(p)).MaxByValue(d => targets.Count(t => Trig.Distance(d, t) <= plant.Range));
                    if (!dest.Equals(defender))
                    {
                        plant.uproot(dest.x, dest.y);
                    }
                }
                break;
        }
    }
}
