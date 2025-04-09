using Client.Services.Network;
using Common.Mathematics;
using Common.Services.Block;
using Common.Services.Events;
using Common.Services.Network;
using Common.Services.Players;
using Common.Services.World;
using Common.Services.World.Events;

namespace Client.Services.World;

public class ClientWorldService : IWorldService
{
    private readonly EventService _eventService;
    private readonly List<ChunkData> _chunks = [];

    public IEnumerable<ChunkData> Chunks => _chunks;

    public ClientWorldService(EventService eventService, BlockRegistry blockRegistry)
    {
        _eventService = eventService;
        _eventService.Handle<ChunkDataEvent>(OnChunkDataMessage);
        
        // TODO: This should be initialized somewhere else, maybe a resource loading stage of program startup?
        blockRegistry.RegisterDefaultBlocks();
    }

    private readonly HashSet<Vector2i> _chunksToRequestLoad = [];
    private readonly HashSet<Vector2i> _chunksToRequestUnload = [];

    public void Update(float deltaTime)
    {
        if (_chunksToRequestLoad.Count != 0)
        {
            var chunkRequestMessage = new ChunkRequestEvent()
            {
                RequestType = EChunkRequest.Load,
                Chunks = _chunksToRequestLoad.ToArray()
            };

            _eventService.Emit(chunkRequestMessage);

            _chunksToRequestLoad.Clear();
        }

        if (_chunksToRequestUnload.Count != 0)
        {
            var chunkRequestMessage = new ChunkRequestEvent()
            {
                RequestType = EChunkRequest.Unload,
                Chunks = _chunksToRequestUnload.ToArray()
            };

            _eventService.Emit(chunkRequestMessage);

            _chunksToRequestUnload.Clear();
        }
    }

    public void RequestChunk(Vector2i position)
    {
        var chunkData = _chunks.FirstOrDefault(x => x.Position == position);

        if (chunkData != null)
            return;
        
        _chunksToRequestLoad.Add(position);
    }

    public void RequestUnloadChunk(Vector2i position)
    {
        var chunkData = _chunks.FirstOrDefault(x => x.Position == position);

        if (chunkData == null)
            return;

        _chunksToRequestUnload.Add(position);

        _chunks.Remove(chunkData);
    }

    private void OnChunkDataMessage(ChunkDataEvent e)
    {
        var chunkData = _chunks.FirstOrDefault(x => x.Position == e!.Position);

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
}