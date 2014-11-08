using System;

public struct Circle
{
    public Point p;
    public int r;

    public Circle(Point p, int r)
    {
        this.p = p;
        this.r = r;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Circle))
        {
            return false;
        }

        Circle other = (Circle)obj;
        return p.Equals(other.p) && r == other.r;
    }

    public override int GetHashCode()
    {
        return p.GetHashCode() ^ r.GetHashCode();
    }

    public override string ToString()
    {
        return String.Format("(({0},{1})>{2})", p, r);
    }
}
