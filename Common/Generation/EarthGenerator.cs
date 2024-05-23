using Common.Mathematics;
using Common.World;

namespace Common.Generation;

public class EarthGenerator : IWorldGenerator
{
    public readonly int Seed = 555;

    public ChunkData GenerateChunkAtPosition(Vector2i position)
    {
        throw new NotImplementedException();
    }
}