using Common.Core;
using Common.Mathematics;

namespace Common.Services.World.Generation;

public interface IWorldGenerator : IService
{
    ChunkData GenerateChunkAtPosition(Vector2i position);
}