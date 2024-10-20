using System.Numerics;
using System.Runtime.CompilerServices;

namespace Common.Mathematics;

public struct Vector2i: IEquatable<Vector2i>
{
    public int X;
    public int Y;

    public Vector2i(int x, int y)
    {
        X = x;
        Y = y;
    }
    
    public Vector2i(int value)
    {
        X = Y = value;
    }

    public Vector2 ToVector2()
    {
        return new Vector2(X, Y);
    }
    
    public static bool operator == (Vector2i a, Vector2i b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Vector2i a, Vector2i b)
    {
        return !a.Equals(b);
    }
    
    public bool Equals(Vector2i other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector2i other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override string ToString()
    {
        return $"<{X}, {Y}>";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2i operator /(Vector2i value1, int value2)
    {
        return value1 / new Vector2i(value2);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2i operator /(Vector2i value1, Vector2i value2)
    {
        return new Vector2i(value1.X / value2.X, value2.Y / value2.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator +(Vector2 value1, Vector2i value2)
    {
        return new Vector2(value1.X + value2.X, value2.Y + value2.Y);
    }
    //TODO: Add methods as needed.
}