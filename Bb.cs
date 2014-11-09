using System;
using System.Collections.Generic;

public static class Bb
{
    public const int
               MOTHER = 0,
               SPAWNER = 1,
               CHOKER = 2,
               SOAKER = 3,
               BUMBLEWEED = 4,
               ARALIA = 5,
               TITAN = 6,
               POOL = 7;

    //Plants
    public static int myPlayer;
    public static AI ourAI;
    public static int Width = 2048;
    public static int Height = 1024;
    public static HashSet<Point> ourSpawners;
    public static HashSet<Point> ourChokers;
    public static HashSet<Point> ourSoakers;
    public static HashSet<Point> ourTitans;
    public static HashSet<Point> ourBumbleweeds;
    public static HashSet<Point> ourAralias;
    public static HashSet<Point> ourMother;

    public static HashSet<Point> theirSpawners;
    public static HashSet<Point> theirChokers;
    public static HashSet<Point> theirSoakers;
    public static HashSet<Point> theirTitans;
    public static HashSet<Point> theirBumbleweeds;
    public static HashSet<Point> theirAralias;
    public static HashSet<Point> theirMother;

    public static HashSet<Point> pools;
    public static HashSet<Point> allOurPlants;
    public static HashSet<Point> allTheirPlants;
    public static Dictionary<Point, Plant> plantLookup;
    
    public static void init(AI ai)
    {
        ourAI = ai;
    }
    public static void readBoard()
    {
        myPlayer = ourAI.playerID();
        ourSpawners = new HashSet<Point>();
        ourChokers = new HashSet<Point>();
        ourSoakers = new HashSet<Point>();
        ourTitans = new HashSet<Point>();
        ourBumbleweeds = new HashSet<Point>();
        ourAralias = new HashSet<Point>();
        ourMother = new HashSet<Point>();
        theirSpawners = new HashSet<Point>();
        theirChokers = new HashSet<Point>();
        theirSoakers = new HashSet<Point>();
        theirTitans = new HashSet<Point>();
        theirBumbleweeds = new HashSet<Point>();
        theirAralias = new HashSet<Point>();
        theirMother = new HashSet<Point>();
        pools = new HashSet<Point>();
        allOurPlants = new HashSet<Point>();
        allTheirPlants = new HashSet<Point>();
        plantLookup = new Dictionary<Point, Plant>();

        foreach(var plant in BaseAI.plants)
        {
            Point p = new Point(plant.X, plant.Y);
            int type = plant.Mutation;
            plantLookup[p] = plant;

            switch(type)
            {
                case MOTHER:
                    if (plant.Owner == myPlayer)
                    {
                        ourMother.Add(p);
                        allOurPlants.Add(p);
                    }
                    else 
                    {
                        theirMother.Add(p);
                        allTheirPlants.Add(p);
                    }
                    break;
                case SPAWNER:
                    if (plant.Owner == myPlayer) 
                    {
                        ourSpawners.Add(p);
                        allOurPlants.Add(p);
                    } 
                    else 
                    {
                        theirSpawners.Add(p);
                        allTheirPlants.Add(p);
                    }
                    break;
                case CHOKER:
                    if (plant.Owner == myPlayer) 
                    {
                        ourChokers.Add(p);
                        allOurPlants.Add(p);
                    } 
                    else 
                    {
                        theirChokers.Add(p);
                        allTheirPlants.Add(p);
                    }
                    break;
                case SOAKER:
                    if (plant.Owner == myPlayer)
                    {
                        ourSoakers.Add(p);
                        allOurPlants.Add(p);
                    }
                    else
                    {
                        theirSoakers.Add(p);
                        allTheirPlants.Add(p);
                    }
                    break;
                case BUMBLEWEED:
                    if (plant.Owner == myPlayer)
                    {
                        ourBumbleweeds.Add(p);
                        allOurPlants.Add(p);
                    }
                    else
                    {
                        theirBumbleweeds.Add(p);
                        allTheirPlants.Add(p);
                    }
                    break;
                case ARALIA:
                    if (plant.Owner == myPlayer)
                    {
                        ourAralias.Add(p);
                        allOurPlants.Add(p);
                    }
                    else
                    {
                        theirAralias.Add(p);
                        allTheirPlants.Add(p);
                    }
                    break;
                case TITAN:
                    if (plant.Owner == myPlayer)
                    {
                        ourTitans.Add(p);
                        allOurPlants.Add(p);
                    }
                    else
                    {
                        theirTitans.Add(p);
                        allTheirPlants.Add(p);
                    }
                    break;
                case POOL:
                    pools.Add(p);
                    break;
            }
        }
    }

    public static int GetOffset(int x, int y)
    {
        return y * Bb.Width + x;
    }

    public static int GetUprootRange(Plant p)
    {
        return p.Mutation == BUMBLEWEED ? 75 : 50;
    }
}
