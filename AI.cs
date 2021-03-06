using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;

/// <summary>
/// The class implementing gameplay logic.
/// </summary>
public class AI : BaseAI
{
    public static Player me;
    public const int
               MOTHER = 0,
               SPAWNER = 1,
               CHOKER = 2,
               SOAKER = 3,
               BUMBLEWEED = 4,
               ARALIA = 5,
               TITAN = 6,
               POOL = 7;

    public static Dictionary<int, int> sporeCosts = new Dictionary<int, int>(){
        {SPAWNER, 5},
        {CHOKER, 10},
        {SOAKER, 25},
        {BUMBLEWEED, 10},
        {ARALIA, 60},
        {TITAN, 24}
    };

    public static Dictionary<int, int> plantRanges = new Dictionary<int, int>(){
        {MOTHER, 150},
        {SPAWNER, 75},
        {CHOKER, 40},
        {SOAKER, 20},
        {BUMBLEWEED, 30},
        {ARALIA, 60},
        {TITAN, 70}
    };

    public static int[] uprootRanges = new int[] {
        0,
        0,
        50,
        50,
        75,
        50,
        50,
        0
    };

    public override string username()
    {
        return "Needs Review";
    }

    public override string password()
    {
        return "password";
    }

    public static Random rand = new Random();
    public static LinkedList<int> toSpawn = new LinkedList<int>(new int[] {});
    private static Point leadSpawner = new Point(-1, -1);

    public void DoSpawn()
    {
        // Build defensive if
        //   They are on our half of the screen
        var ourMother = Bb.ourMother.First();
        var midX = Bb.Width / 2;
        if (Bb.allTheirPlants.Any(p => me.Id == 0 ? p.x < midX : p.x > midX))
        {
            // Get 2 defensive Titans
            if (Bb.theirChokers.Count > Bb.theirAralias.Count && Bb.ourTitans.Count < 2)
            {
                Solver.Spawn(TITAN, ourMother, 50);
            }
            // Get 4 defensive Aralia
            if (Bb.ourAralias.Where(a => Trig.Distance(a, ourMother) <= 200).Count() < 4)
            {
                Solver.Spawn(ARALIA, ourMother, 100);
            }
        }

        // Build offensive if they are killing our lead or their mother is in range, but only when clumpable
        var canClump = me.Spores > (AI.sporeCosts[ARALIA] * 6);
        var inRange = Bb.ourSpawners.Any(s => Trig.IsInRange(s, Bb.theirMother.First(), 200));
        var leadKilled = leadSpawner.x != -1 && !Bb.plantLookup.ContainsKey(leadSpawner);
        if (canClump && (inRange || leadKilled))
        {
            while (me.Spores > sporeCosts[ARALIA])
            {
                Solver.Spawn(ARALIA, Bb.theirMother.First(), 200);
            }
        }

        leadSpawner = Solver.Spawn(SPAWNER, Bb.theirMother.First(), 200, false);

        if (me.Spores > 450)
        {
            Solver.Spawn(ARALIA, ourMother, 100);
        }
    }

    public void DoMove()
    {
        Bb.readBoard();
        foreach (var d in Bb.ourTitans.Concat(Bb.ourAralias.Where(a => Trig.IsInRange(a, Bb.ourMother.First(), 200))))
        {
            Solver.DefendMother(d, Bb.theirAralias.Concat(Bb.theirChokers)
                .Where(p => p.GetPlant().Rads < p.GetPlant().MaxRads)
                .Where(p => Trig.IsInRange(p, Bb.ourMother.First(), 200)));
        }
        Bb.readBoard();
        foreach (var p in Bb.allOurPlants.Where(p => !Trig.IsInRange(p, Bb.ourMother.First(), 200)))
        {
            Solver.Uproot(p, Bb.theirMother.First(), p.GetPlant().Range);
        }
    }

    public void DoRadiate()
    {
        Bb.readBoard();
        var theirMother = Bb.theirMother.First();
        var heavyDefense = Bb.theirTitans.Concat(Bb.theirAralias).Count(p => Trig.IsInRange(p, theirMother, 200)) > 4;
        foreach (var plant in Bb.allOurPlants.Select(p => p.GetPlant()).Where(p => p.RadiatesLeft > 0))
        {
            var targetsInRange = Bb.allTheirPlants.Where(t => Trig.IsInRange(plant.ToPoint(), t, plant.Range))
                .Select(t => t.GetPlant()).Where(t => t.Rads < t.MaxRads).ToArray();

            var motherInRange = plant.ToRangeCircle().IsInRange(theirMother);
            if (motherInRange && heavyDefense)
            {
                plant.radiate(theirMother.x, theirMother.y);
                plant.talk("Your mother.");
            }
            else if (targetsInRange.Any(t => t.Mutation == TITAN))
            {
                var titan = targetsInRange.First(t => t.Mutation == TITAN);
                plant.radiate(titan.X, titan.Y);
            }
            else if (motherInRange)
            {
                plant.radiate(theirMother.x, theirMother.y);
            }
            else if (targetsInRange.Any())
            {
                var t = targetsInRange.First();
                plant.radiate(t.X, t.Y);
                plant.talk("pew pew I guess");
            }
            Bb.readBoard();
        }
    }

    /// <summary>
    /// This function is called once, before your first turn.
    /// </summary>
    public override void init()
    {
        //set up me field
        me = players[playerID()];
        //set up mother field

        Bb.init(this);

        Trig.CalcPointsInCircle(new Circle(0, 0, 300));
    }

    /// <summary>
    /// This function is called each time it is your turn.
    /// </summary>
    /// <returns>True to end your turn. False to ask the server for updated information.</returns>
    public override bool run()
    {
        var sw = new Stopwatch();
        sw.Start();

        //Step 1: Initialization
        Bb.newTurn();
        Bb.readBoard();
        Solver.PreCalc();

        DoSpawn();

        DoMove();

        DoRadiate();

        //Step 4: Radiate
        //All soakers buff teammates in range
        //All chokers attack (priority nearest our mother)
        //Titans debuff cause yeah
        /*
        Bb.readBoard();
        foreach (var ourPlantPoint in Bb.allOurPlants)
        {
            var ourPlant = ourPlantPoint.GetPlant();
            if (ourPlant.RadiatesLeft > 0)
            {
                foreach (var theirPlantPoint in Bb.allTheirPlants.Where(p => p.GetPlant().Rads < p.GetPlant().MaxRads && p.GetPlant().Mutation == TITAN && Trig.IsInRange(p, Bb.theirMother.First(), plantRanges[TITAN])))
                {
                    if (Trig.IsInRange(ourPlantPoint, theirPlantPoint, ourPlant.Range))
                    {
                        ourPlant.radiate(theirPlantPoint.x, theirPlantPoint.y);
                        Bb.readBoard();
                        break;
                    }
                }

                if (ourPlant.RadiatesLeft > 0)
                {
                    foreach (var theirPlantPoint in Bb.allTheirPlants.Where(p => p.GetPlant().Rads < p.GetPlant().MaxRads && p.GetPlant().Mutation == MOTHER))
                    {
                        if (Trig.IsInRange(ourPlantPoint, theirPlantPoint, ourPlant.Range))
                        {
                            ourPlant.radiate(theirPlantPoint.x, theirPlantPoint.y);
                            Bb.readBoard();
                            break;
                        }
                    }

                    if (ourPlant.RadiatesLeft > 0 && ourPlant.Mutation == ARALIA)
                    {
                        foreach (var theirPlantPoint in Bb.allTheirPlants.Where(p => p.GetPlant().Rads < p.GetPlant().MaxRads))
                        {
                            if (Trig.IsInRange(ourPlantPoint, theirPlantPoint, ourPlant.Range) && theirPlantPoint.GetPlant().Mutation != SPAWNER)
                            {
                                ourPlant.radiate(theirPlantPoint.x, theirPlantPoint.y);
                                Bb.readBoard();
                                break;
                            }
                        }
                    }

                    if (ourPlant.RadiatesLeft > 0 && ourPlant.Mutation != ARALIA)
                    {
                        foreach (var theirPlantPoint in Bb.allTheirPlants.Where(p => p.GetPlant().Rads < p.GetPlant().MaxRads))
                        {
                            if (Trig.IsInRange(ourPlantPoint, theirPlantPoint, ourPlant.Range))
                            {
                                ourPlant.radiate(theirPlantPoint.x, theirPlantPoint.y);
                                Bb.readBoard();
                                break;
                            }
                        }
                    }
                }
            }
        }
         * */

        sw.Stop();
        Console.WriteLine("[{0}] {1} x {2} - {3}s",
            turnNumber(),
            Bb.ourMother.First().GetPlant().Rads,
            Bb.theirMother.First().GetPlant().Rads,
            sw.Elapsed);

        return true;
    }

    /// <summary>
    /// This function is called once, after your last turn.
    /// </summary>
    public override void end() { }

    //returns the distance between two points
    int distance(int x1, int y1, int x2, int y2)
    {
        return (int)(Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2)));
    }
    //Helper function to get all of the plants owned
    List<Plant> getMyPlants()
    {
        List<Plant> myPlants = new List<Plant>();
        for (int i = 0; i < plants.Length; i++)
        {
            if (plants[i].Owner == playerID())
            {
                myPlants.Add(plants[i]);
            }
        }
        return myPlants;
    }

    //Helper function to get a Plant at a point
    //Returns null if no plant found
    Plant getPlantAt(int x, int y)
    {
        //if it's out of bounds, we don't need to check anything
        if (!withinBounds(x, y))
        {
            return null;
        }

        //for every plant, if a plant is at the position we want, return it
        for (int i = 0; i < plants.Length; i++)
        {
            if (plants[i].X == x && plants[i].Y == y)
            {
                return plants[i];
            }
        }
        return null;
    }

    //Helper function for bounds checking
    bool withinBounds(int x, int y)
    {
        if (x < 0 || x >= mapWidth() || y < 0 || y >= mapHeight())
        {
            return false;
        }

        return true;
    }

    //Helper function to check if we're within range of a Spawner or Mother
    bool withinSpawnerRange(int x, int y)
    {
        //No need to check if we're not within the bounds of the map
        if (!withinBounds(x, y))
        {
            return false;
        }

        //for every plant
        for (int i = 0; i < plants.Length; i++)
        {
            Plant plant = plants[i];

            //check for ownership and correct mutation
            if (plant.Owner == me.Id &&
                (plant.Mutation == SPAWNER || plant.Mutation == MOTHER))
            {
                //if we're within range, we're good
                if (Trig.IsInRange(x, y, plant.X, plant.Y, plant.Range))
                {
                    return true;
                }
            }
        }

        //if we found none, nope
        return false;
    }

    public AI(IntPtr c)
        : base(c) { }
}