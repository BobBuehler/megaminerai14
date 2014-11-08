using System;
using System.Collections.Generic;

public struct Point
{
    public short x;
    public short y;

    public Point(short x, short y)
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
        return 0; // TODO
    }

    public override string ToString()
    {
        return String.Format("{0},{1}", x, y);
    }
}
