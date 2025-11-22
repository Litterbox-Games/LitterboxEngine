using Common.Core;
using Common.Mathematics;
using Common.Services.Block;
using Common.Services.Events;
using Common.Services.World;
using Common.Services.World.Events;

namespace Client.Services.World;

[Game(EMode.Client)]
[As<IWorldService>]
public class ClientWorldService : IWorldService
{
    private readonly List<ChunkData> _chunks = [];
    private readonly EventService _eventService;
    
    public IEnumerable<ChunkData> Chunks => _chunks;

    public ClientWorldService(EventService eventService, BlockRegistry blockRegistry)
    {
        _eventService = eventService;
        
        // TODO: This should be initialized somewhere else, maybe a resource loading stage of program startup?
        blockRegistry.RegisterDefaultBlocks();
        
        _eventService.Handle<ChunkDataEvent>(OnChunkDataMessage);
        _eventService.Handle<ChunkRequestEvent>(OnChunkRequest);
    }
    private void OnChunkRequest(ChunkRequestEvent e)
    {
        if (e.RequestType == EChunkRequest.Load) 
            return;

        // Unload chunks
        foreach (var position in e.Chunks)
        {
            // TODO: store chunks in a dictionary to prevent having to search like this
            var chunkData = _chunks.FirstOrDefault(x => x.Position == position);
            
            if (chunkData == null) 
                continue;
            
            _chunks.Remove(chunkData);
        }
    }

    private void OnChunkDataMessage(ChunkDataEvent e)
    {
        var chunkData = _chunks.FirstOrDefault(x => x.Position == e.Position);

        if (chunkData == null)
        {
            chunkData = new ChunkData(e.Position);
            _chunks.Add(chunkData);
        }

        chunkData.GroundArray = e.GroundLayer!;
        chunkData.ObjectArray = e.ObjectLayer!;

        chunkData.BiomeArray = e.BiomeMap!.Cast<EBiomeType>().ToArray();
        chunkData.HeatArray = e.HeatMap!.Cast<EHeatType>().ToArray();
        chunkData.MoistureArray = e.MoistureMap!.Cast<EMoistureType>().ToArray();
    }
    
    public ChunkData? GetChunkData(Vector2i position)
    {
        return Chunks.FirstOrDefault(x => x.Position == position);
    }
    
    public void Dispose()
    {
        _eventService.Unhandle<ChunkDataEvent>(OnChunkDataMessage);
        _eventService.Unhandle<ChunkRequestEvent>(OnChunkRequest);
        
        GC.SuppressFinalize(this);
    }
}