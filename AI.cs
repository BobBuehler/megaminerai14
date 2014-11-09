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

    public static LinkedList<int> toSpawn = new LinkedList<int>(new int[] { Bb.ARALIA, Bb.ARALIA, Bb.ARALIA, Bb.ARALIA});
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

        var toSpawn = new LinkedList<int>(new int[] { Bb.ARALIA, Bb.ARALIA, Bb.ARALIA, Bb.ARALIA });

        if (!Bb.ourSpawners.Any(sp => Trig.IsInRange(sp, Bb.theirMother.First(), plantRanges[SPAWNER] + plantRanges[CHOKER])))
        {
            toSpawn.AddFirst(Bb.SPAWNER);
        }
        if (Bb.ourTitans.Count < 3 && Bb.allTheirPlants.Any(pl => Trig.IsInRange(pl, Bb.ourMother.First(), plantRanges[MOTHER] + plantRanges[ARALIA])))
        {
            toSpawn.AddFirst(Bb.TITAN);
        }
        
        foreach (var plantType in toSpawn)
        {
            Bb.readBoard();
            switch(plantType)
            {
                case SPAWNER:
                    if(Bb.allTheirPlants.Any(ts => Bb.ourSpawners.Any(os => Trig.IsInRange(ts, os, 180))))
                    {
                        Solver.Spawn(plantType, Bb.theirMother.First(), 75 + 40);
                    }
                    else
                        Solver.Spawn(plantType, Bb.theirMother.First(), 75 + 40, false);
                    break;
                case TITAN:
                    Solver.Spawn(plantType, Bb.ourMother.First(), 200);
                    break;
                default:
                    Solver.Spawn(plantType, Bb.theirMother.First(), 50);
                    toSpawn.RemoveFirst();
                    break;

            }
        }


        //Step 3: Move
        //Move plants in groups
        //Move soakers in pools (needing more strength) closer to the allies by the pool so the soaker is in range
        //Chokers should just always move towards the mother (attack while passing by)
        //Check enemy range to move out of range (if desired i.e. soakers to another part of the pool that is outside enemy range)
        //Keep titans out of enemy attack range but in titan debuff range for the enemies

        foreach (var t in Bb.ourTitans)
        {
            Solver.DefendMother(t, Bb.allTheirPlants);
        }
        Bb.readBoard();
        foreach (var p in Bb.allOurPlants)
        {
            Solver.Uproot(p, Bb.theirMother.First(), p.GetPlant().Range);
        }


        //Step 4: Radiate
        //All soakers buff teammates in range
        //All chokers attack (priority nearest our mother)
        //Titans debuff cause yeah

        Bb.readBoard();
        foreach (var ourPlantPoint in Bb.allOurPlants)
        {
            var ourPlant = ourPlantPoint.GetPlant();
            if (ourPlant.RadiatesLeft > 0)
            {
                foreach (var theirPlantPoint in Bb.allTheirPlants.Where(p => p.GetPlant().Rads < p.GetPlant().MaxRads && p.GetPlant().Mutation == MOTHER))
                {
                    if (Trig.IsInRange(ourPlantPoint, theirPlantPoint, ourPlant.Range))
                    {
                        
                        ourPlant.talk("HUEUEUEUEUE");
                        ourPlant.radiate(theirPlantPoint.x, theirPlantPoint.y);
                        Bb.readBoard();
                        break;
                    }
                }

                if(ourPlant.RadiatesLeft > 0)
                {
                    foreach (var theirPlantPoint in Bb.allTheirPlants.Where(p => p.GetPlant().Rads < p.GetPlant().MaxRads))
                    {
                        if (Trig.IsInRange(ourPlantPoint, theirPlantPoint, ourPlant.Range))
                        {
                            if(rand.Next(10) > 8)
                            {
                                ourPlant.talk("CHORTLE");
                            }
                            ourPlant.radiate(theirPlantPoint.x, theirPlantPoint.y);
                            Bb.readBoard();
                            break;
                        }
                    }
                }
            }
        }

        sw.Stop();
        Console.WriteLine("Turn {0} done in {1}s", turnNumber(), sw.Elapsed);

        return true;
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