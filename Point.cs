using System;
using System.Collections.Generic;

public struct Point
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Point))
        {
            return false;
        }

        Point other = (Point)obj;
        return x == other.x && y == other.y;
    }

    public override int GetHashCode()
    {
        return Bb.GetOffset(x, y);
    }

    public override string ToString()
    {
        return String.Format("({0},{1})", x, y);
    }

    public Point Add(Point p)
    {
        return new Point(x + p.x, y + p.y);
    }

    public Point Subtract(Point p)
    {
        return new Point(x - p.x, y - p.y);
    }
}
