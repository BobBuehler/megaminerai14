using System;
using System.Linq;
using System.Collections.Generic;

public static class Extensions
{
    public static Circle ToCircle(this Point p, int r)
    {
        return new Circle(p, r);
    }

    public static IEnumerable<Circle> ToCircles(this IEnumerable<Point> points, int r)
    {
        return points.Select(p => p.ToCircle(r));
    }

    public static bool IsInRange(this Circle c, Point p)
    {
        return Trig.IsInRange(c.p, p, c.r);
    }

    public static Func<Point, bool> RangeChecker(this Circle c)
    {
        return p => c.IsInRange(p);
    }

    public static IEnumerable<Point> PointsInRange(this Circle c)
    {
        return Trig.PointsInCircle(c);
    }

    public static K MinByValue<K, V>(this IEnumerable<K> source, Func<K, V> predicate)
    {
        K min = source.First();
        V minValue = predicate(min);
        var comparer = Comparer<V>.Default;
        foreach (var k in source)
        {
            var v = predicate(k);
            if (comparer.Compare(minValue, v) > 0)
            {
                min = k;
                minValue = v;
            }
        }
        return min;
    }

    public static IEnumerable<K> MinByValue<K, V>(this IEnumerable<K> source, int count, Func<K, V> predicate)
    {
        var mins = new LinkedList<Tuple<K, V>>();

        var comparer = Comparer<V>.Default;
        foreach (var k in source)
        {
            var v = predicate(k);
            if (mins.Count < count || comparer.Compare(mins.Last.Value.Item2, v) > 0)
            {
                var node = mins.First;
                var t = Tuple.Create(k, v);
                while (node != null && comparer.Compare(node.Value.Item2, v) < 0) node = node.Next;
                if (node != null)
                {
                    mins.AddBefore(node, t);
                }
                else
                {
                    mins.AddLast(t);
                }
                if (mins.Count > count)
                {
                    mins.RemoveLast();
                }
            }
        }
        return mins.Select(m => m.Item1);
    }

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
    {
        return new HashSet<T>(source);
    }

    public static bool IsOnBoard(this Point point)
    {
        return point.x >= 0 && point.x < Bb.Width && point.y >= 0 && point.y < Bb.Height;
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach(var t in source)
            action(t);
    }

    public static Func<T, TResult> Memoize<T, TResult>(this Func<T, TResult> func)
    {
        var results = new Dictionary<T, TResult>();
        return t =>
        {
            TResult r;
            if (!results.TryGetValue(t, out r))
            {
                r = func(t);
                results[t] = r;
            }
            return r;
        };
    }

    public static Plant GetPlant(this Point p)
    {
        return Bb.plantLookup[p];
    }

    public static Circle ToCircle(this Plant pl)
    {
        return new Circle(pl.X, pl.Y, pl.Range);
    }

    public static IEnumerable<T> Single<T>(this T t)
    {
        return new T[] { t };
    }
}
