﻿using System.Numerics;

namespace Common.Mathematics;

public static class VectorExtensions
{
    public static Vector2 Modulus(this Vector2 x, int divisor)
    {
        return new Vector2(x.X.Modulus(divisor), x.Y.Modulus(divisor));
    }

    public static Vector2 Floor(this Vector2 x)
    {
        return new Vector2(MathF.Floor(x.X), MathF.Floor(x.Y));
    }

    public static Vector2i FloorToVector2i(this Vector2 x)
    {
        return new Vector2i((int) MathF.Floor(x.X), (int) MathF.Floor(x.Y));
    }
}