using System;
using System.Linq;
using System.Runtime.InteropServices;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            
            var c = new Circle(1, 1, 1);
            Console.WriteLine("IN");
            var ps = Trig.CalcPointsInCircle(c);
            ps.ForEach(p => Console.WriteLine(p));
            Console.WriteLine("OUTER EDGE");
            ps = Trig.CalcOuterEdgeOfCircle(c);
            ps.ForEach(p => Console.WriteLine(p));
            Console.WriteLine("INNER EDGE");
            ps = Trig.CalcInnerEdgeOfCircle(c);
            ps.ForEach(p => Console.WriteLine(p));
            
            /*
            var ps = Solver.FindPointsInCirclesNearestTargets(
                2,
                new Circle(1, 1, 5).Single(),
                new Point(20, 1).Single(),
                new Circle(5, 1, 5).Single());
            ps.ForEach(p => Console.WriteLine(p));
             */
            
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
