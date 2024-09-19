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

    //TODO: Add methods as needed.
}