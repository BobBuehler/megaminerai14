using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

/// <summary>
/// The class implementing gameplay logic.
/// </summary>
class AI : BaseAI
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
    List<Plant> myPlants = getMyPlants();

    //for every plant we own, move them forward and attack if it finds an enemy
    for (int j = 0; j < myPlants.Count; j++)
    {
      Plant plant = myPlants[j];
      //only try radiating if it's possible
      if(plant.RadiatesLeft > 0)
      {
        //only heal or buff allies and attack enemies
        int targetOwner = 1 - playerID();
        if(plant.Mutation == BUMBLEWEED || plant.Mutation == SOAKER)
        {
          targetOwner = playerID();
        }
        for (int i = 0; i < plants.Length; i++)
        {
          Plant foe = plants[i];


          //don't attack yourself!
          if(foe.Id == plant.Id)
          {
             continue;
          }

          //if it's dead skip it
          if(foe.Rads >= foe.MaxRads)
          {
            continue;
          }

          //don't mess with pools
          if(foe.Mutation == POOL)
          {
            continue;
          }

          //if it's not the right target
          if(foe.Owner != targetOwner)
          {
            continue;
          }

          //if a healer or soaker can't effect the mother weed
          if(targetOwner == playerID() && foe.Mutation == MOTHER)
          {
            continue;
          }

          //if a soaker can't effect other soakers
          if(plant.Mutation == SOAKER && foe.Mutation == SOAKER)
          {
            continue;
          }

          //if we're within range...
          if (distance(plant.X, plant.Y, foe.X, foe.Y) < plant.Range)
          {
            //get 'im!
            plant.radiate(foe.X, foe.Y);
            break;
          }
        }
      }
      //move them straight to the other side. no regrets.
      //move as far as possible, as long as it's not off the map
      int wantedX = plant.X;
      if(plant.Mutation == BUMBLEWEED)
      {
        wantedX += directionOfEnemy * bumbleweedSpeed();
      }
      else
      {
        wantedX += directionOfEnemy * uprootRange();
      }
      if (plant.UprootsLeft > 0 &&
          getPlantAt(wantedX, plant.Y) == null &&
          wantedX >= 0 && wantedX < mapWidth())
      {
        plant.uproot(wantedX, plant.Y);
      }
    }
  
    //make a new plant every turn, because why not?
  
    //first, check if we can actually do that
    if (myPlants.Count >= maxPlants())
    {
      //end turn
      return true;
    }
  
    int spawnX = -1, spawnY = -1;
    double angle = 0;
    for (int i = 0; i < myPlants.Count; i++)
    {
      Plant plant = myPlants[i];
      //remove all plants in our list except for mothers and spawners
      if (!(plant.Mutation == MOTHER || plant.Mutation == SPAWNER))
      {
        myPlants.Remove(myPlants[i]);
      }
    }
  
    //get a random spawner or mother plant
    Plant spawnerPlant = myPlants[rand.Next(myPlants.Count)];
  
    //get a new position centered around that spawner within its range
    //also, keep generating new coordinates until they become valid ones
    //Remember from trig:
    //(random x inside a circle) = centerX + rand(0,1)*radius*cos(angle)
    int spawnCheckLimit = 0;
    while (!withinBounds(spawnX, spawnY) || getPlantAt(spawnX, spawnY) != null)
    {
      angle = rand.NextDouble() * 2 * Math.PI;
      while (spawnX < 0 || spawnX >= mapWidth())
      {
        spawnX = spawnerPlant.X + (int)(rand.NextDouble() * spawnerPlant.Range * Math.Cos(angle));
      }
      while (spawnY < 0 || spawnY >= mapHeight())
      {
        spawnY = spawnerPlant.Y + (int)(rand.NextDouble() * spawnerPlant.Range * Math.Sin(angle));
      }
      spawnCheckLimit++;
  
      //if we try to spawn too many times, just give up and end the turn
      if (spawnCheckLimit > 10)
      {
        return true;
      }
    }
  
    //spawn a random type of plant that isn't a mother or a pool at the coordinates we made
    //of course, make sure we have enough spores to do the job!
    int mutationType = rand.Next(6)+1;
    if (me.Spores >= mutations[mutationType].Spores &&
        withinSpawnerRange(spawnX, spawnY))
    {
      me.germinate(spawnX, spawnY, mutationType);
    }
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
    if (mother.X < mapWidth()/2)
    {
      directionOfEnemy = 1;
    }
    else
    {
      directionOfEnemy = -1;
    }
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
        if (distance(x, y, plant.X, plant.Y) < plant.Range)
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
