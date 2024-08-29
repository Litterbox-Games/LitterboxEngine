﻿namespace Common.Mathematics;

public static class MathFExtensions
{
    public static float Modulus(this float x, int divisor)
    {
        var output = x % divisor;

        if (x < 0 && output != 0)
        {
            output = divisor + output;
        }

        return output;
    }

    public static int Modulus(this int x, int divisor)
    {
        var output = x % divisor;

        if (x < 0 && output != 0)
        {
            output = divisor + output;
        }

        return output;
    }

    public static int ModulusToInt(this float x, int divisor)
    {
        var output = (int) MathF.Floor(x) % divisor;

        if (x < 0 && output != 0)
        {
            output = divisor + output;
        }

        return output;
    }
    
    public static float Map(this float value, float fromSource, float toSource, float fromTarget, float toTarget)
    {
        return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
    }
}