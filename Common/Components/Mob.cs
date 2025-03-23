using System.Numerics;

namespace Common.Components;

public struct Mob()
{
    public Vector2 Direction;
    public DateTime LastChangedDirections = DateTime.Now;
};