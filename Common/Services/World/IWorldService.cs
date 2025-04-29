using Common.Core;
using Common.Mathematics;

namespace Common.Services.World;

public interface IWorldService : IService
{
    public const int WorldSize = 16;
    
    IEnumerable<ChunkData> Chunks { get; }
    public ChunkData? GetChunkData(Vector2i position);
}