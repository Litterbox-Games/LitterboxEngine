using System.Numerics;

namespace Common.Entities.Components;

public struct Position(Vector2 current)
{
    public DateTime LastUpdate = DateTime.Now;
    public Vector2 Current = current;
    public Vector2 LastSent = current;
    public readonly Queue<QueuedMovement> Queued = new();
}

public record struct QueuedMovement(Vector2 Position, DateTime TimeStamp);