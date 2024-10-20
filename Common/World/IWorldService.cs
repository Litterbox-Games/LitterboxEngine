using Common.DI;
using Common.Mathematics;

namespace Common.World;

public interface IWorldService : ITickableService
{
    public const int WorldSize = 16;
    
    IEnumerable<ChunkData> Chunks { get; }
    public ChunkData? GetChunkData(Vector2i position);
    
    void RequestChunk(Vector2i position);
    void RequestUnloadChunk(Vector2i position);
    
}