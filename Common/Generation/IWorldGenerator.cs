using Common.DI;
using Common.Mathematics;
using Common.World;

namespace Common.Generation;

public interface IWorldGenerator : IService
{
    ChunkData GenerateChunkAtPosition(Vector2i position);
}