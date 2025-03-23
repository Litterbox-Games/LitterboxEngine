using Common.Core;
using Common.Mathematics;

namespace Common.Services.World;

public interface IWorldService : IService, IUpdatable
{
    public const int WorldSize = 16;
    
    IEnumerable<ChunkData> Chunks { get; }
    public ChunkData? GetChunkData(Vector2i position);
    
    void RequestChunk(Vector2i position);
    void RequestUnloadChunk(Vector2i position);
    
}