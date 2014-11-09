using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;

/// <summary>
/// The class implementing gameplay logic.
/// </summary>
public class AI : BaseAI
{
    static Player me;
    static Plant mother;
    static int directionOfEnemy;
    public static int
               MOTHER = 0,
               SPAWNER = 1,
               CHOKER = 2,
               SOAKER = 3,
               BUMBLEWEED = 4,
               ARALIA = 5,
               TITAN = 6,
               POOL = 7;
    static Random rand = new Random();


    public override string username()
    {
        return "Needs Review";
    }

    public override string password()
    {
        return "password";
    }

    /// <summary>
    /// This function is called each time it is your turn.
    /// </summary>
    /// <returns>True to end your turn. False to ask the server for updated information.</returns>
    public override bool run()
    {
        //Step 1: Initialization
        Bb.readBoard();
        int mySpores = me.Spores;
        HashSet<Point> endMoveLocations = new HashSet<Point>();
        HashSet<Point> attackLocations = new HashSet<Point>();

        //Step 2: Spawn Stuff
            //What to spawn:
                //Where to spawn:
                    //Early Game:
                        //Spawn spawners as close to the enemy mother as possible but not in a pool
                        //Spawn soakers as close to pools as they can (to the pools closest to the spawners and closest to the enemy mother first)
                    //General Battle:
                        //Spawn chokers => Where: As close to the fight as possible || How Many: More than the enemy (6 ALWAYS 6)
                        //Reflexive spawning of aralias. So if they have one, we spawn one (too advanced for Bob)
                        //Spawn soakers as close to pools as they can (to the pools closest to the spawners and closest to the enemy mother first)
                    //Late Game:
                        //We are losing:
                            //Spawn Titans around the perimeter
                            //Spawn hecka chokers and aralias because of higher spore rate
                        //We are winning:
                            //KILL THEM
        //Check pool locations to see if and where the ally plants are and place soakers in the pool but in range of the allies
        
        var spawnCount = mySpores / 10; // Choker cost
        var spawnableCircles = Bb.ourMother.Concat(Bb.ourSpawners).Select(m => new Circle(m, Bb.plantLookup[m].Range));
        var targets = Bb.theirMother;
        var avoidCircles = Bb.pools.Select(p => p.ToCircle(Bb.plantLookup[p].Range));
        avoidCircles.Concat(plants.Select(pl => new Circle(new Point(pl.X, pl.Y), 0)));
        var germinateLocations = Solver.FindPointsInCirclesNearestTargets(spawnCount, spawnableCircles, targets, avoidCircles);
        germinateLocations.ForEach(p => me.germinate(p.x, p.y, CHOKER));

        Bb.readBoard();

        Console.WriteLine("Germinated: ");
        germinateLocations.ForEach(p => Console.WriteLine("  " + p));

        //Step 3: Move
            //Move plants in groups
            //Move soakers in pools (needing more strength) closer to the allies by the pool so the soaker is in range
            //Chokers should just always move towards the mother (attack while passing by)
            //Check enemy range to move out of range (if desired i.e. soakers to another part of the pool that is outside enemy range)
            //Keep titans out of enemy attack range but in titan debuff range for the enemies

        foreach (var chokerPoint in Bb.ourChokers)
        {
            var endMovePoint = Solver.FindPointsInCirclesNearestTargets(1, chokerPoint.GetPlant().ToCircle().Single(), Bb.theirMother, new Circle[]{});
            if (endMovePoint.Any())
            {
                var p = endMovePoint.First();
                chokerPoint.GetPlant().uproot(p.x, p.y);
            }
            Bb.readBoard();
        }

        //Step 4: Radiate
            //All soakers buff teammates in range
            //All chokers attack (priority nearest our mother)
            //Titans debuff cause yeah

        foreach (var ourPlantPoint in Bb.allOurPlants)
        {
            var ourPlant = ourPlantPoint.GetPlant();
            while (ourPlant.RadiatesLeft > 0)
            {
                foreach (var theirPlantPoint in Bb.allTheirPlants)
                {
                    if (Trig.IsInRange(ourPlantPoint, theirPlantPoint, ourPlant.Range))
                    {
                        ourPlant.radiate(theirPlantPoint.x, theirPlantPoint.y);
                        Bb.readBoard();
                        continue;
                    }
                }
                break;
            }
        }

        Console.WriteLine("DONE");

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
        mother = getMyPlants()[0];

        //set up directionOfEnemy field
        //if our mother is on the left side of the map, the enemy must be on the right side
        //and vice versa of course
        if (mother.X < mapWidth() / 2)
        {
            directionOfEnemy = 1;
        }
        else
        {
            directionOfEnemy = -1;
        }

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