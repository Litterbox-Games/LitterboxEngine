using Common.DI;
using Common.Mathematics;

namespace Common.World.Generation;

public interface IWorldGenerator : IService
{
    ChunkData GenerateChunkAtPosition(Vector2i position);
}