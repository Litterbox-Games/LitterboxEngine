using System.Numerics;

namespace Common.Components;

public struct Velocity(float x, float y)
{
    public float X = x;
    public float Y = y;

    public Vector2 ToVector2() => new(X, Y);
    
    public static implicit operator Vector2(Velocity velocity) => new(velocity.X, velocity.Y);
    public static implicit operator Velocity(Vector2 velocity) => new(velocity.X, velocity.Y);
}