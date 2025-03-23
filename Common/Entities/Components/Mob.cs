using System.Numerics;

namespace Common.Entities.Components;

public struct Mob()
{
    public Vector2 Direction;
    public DateTime LastChangedDirections = DateTime.Now;
};