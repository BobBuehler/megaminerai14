using System;
using System.Collections.Generic;

public static class Bb
{
    public static int Width = 2048;
    public static int Height = 1024;

    public static int GetOffset(int x, int y)
    {
        return y * Bb.Width + x;
    }
}
