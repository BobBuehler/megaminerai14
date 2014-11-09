using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
public class Pather
{
    public static IEnumerable<Point> CalcNeighboors(Point p, Func<Point, bool> isPassable)
    {
        var neighboors = new Point[]{
            new Point(p.x - 1, p.y - 1),
            new Point(p.x - 1, p.y),
            new Point(p.x - 1, p.y + 1),
            new Point(p.x, p.y - 1),
            new Point(p.x, p.y + 1),
            new Point(p.x + 1, p.y - 1),
            new Point(p.x + 1, p.y),
            new Point(p.x + 1, p.y + 1),
        };
        return neighboors.Where(isPassable);
    }

    public class AStar
    {
        public LinkedList<Point> Path;
        public HashSet<Point> Open;
        public HashSet<Point> Closed;
        public Dictionary<Point, int> GScore;
        public Dictionary<Point, int> FScore;
        public Dictionary<Point, Point> From;

        public AStar(IEnumerable<Point> starts, Func<Point, bool> isGoal, Func<Point, Point, int> costCalc, Func<Point, int> hCalc, Func<Point, IEnumerable<Point>> neighboorCalc)
        {
            Open = new HashSet<Point>(starts);
            Closed = new HashSet<Point>();

            GScore = starts.ToDictionary(s => s, s => 0);
            FScore = starts.ToDictionary(s => s, s => GScore[s] + hCalc(s));

            From = new Dictionary<Point, Point>();

            while (Open.Any())
            {
                var current = Open.MinByValue(p => FScore[p]);
                if (GScore[current] > int.MaxValue) return;

                if (isGoal(current))
                {
                    Path = new LinkedList<Point>();
                    Path.AddFirst(current);
                    while (From.ContainsKey(current))
                    {
                        current = From[current];
                        Path.AddFirst(current);
                    }
                    return;
                }

                Open.Remove(current);
                Closed.Add(current);

                foreach (var n in neighboorCalc(current).Where(n => !Closed.Contains(n)))
                {
                    var g = GScore[current] + costCalc(current, n);
                    if (!Open.Contains(n) || g < GScore[n])
                    {
                        GScore[n] = g;
                        FScore[n] = g + hCalc(n);
                        From[n] = current;
                        Open.Add(n);
                    }
                }
            }
        }
    }
}