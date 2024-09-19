using System.Numerics;

namespace Common.Entity;

public class MobEntity : GameEntity
{
    public override ushort EntityType => 1;

    public Vector2 SpawnPosition;
}