using System;
using System.Linq;
using System.Runtime.InteropServices;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            var passable = new bool[][] {
                new bool[]{ true, true, false, true },
                new bool[]{ true, true, false, true },
                new bool[]{ true, true, true, true }};
            var starts = new Point[] {new Point(0, 0)};
            var goal  = new Point(3, 0);
            var search = new Pather.AStar(
                starts,
                g => g.Equals(goal),
                (a, b) => 1,
                p => 0,
                p => Pather.CalcNeighboors(p, n => Trig.IsInRect(n, 0, 3, 0, 2) && passable[n.y][n.x]));
            search.Path.ForEach(p => Console.WriteLine(p));

            System.Console.WriteLine("Please enter a hostname");
            return;
        }

        IntPtr connection = Client.createConnection();

        AI ai = new AI(connection);
        if (Client.serverConnect(connection, args[0], "19000") == 0)
        {
            System.Console.WriteLine("Unable to connect to server");
            return;
        }

        if (Client.serverLogin(connection, ai.username(), ai.password()) == 0)
            return;

        if (args.Length < 2)
            Client.createGame(connection);
        else
            Client.joinGame(connection, Int32.Parse(args[1]), "player");

        while (Client.networkLoop(connection) != 0)
        {
            if (ai.startTurn())
                Client.endTurn(connection);
            else
                Client.getStatus(connection);
        }

        Client.networkLoop(connection); //Grab end game state
        Client.networkLoop(connection); //Grab log
        ai.end();
    }
}
