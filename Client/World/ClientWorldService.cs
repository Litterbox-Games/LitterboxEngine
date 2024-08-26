using Client.Network;
using Common.Mathematics;
using Common.Network;
using Common.Player;
using Common.World;
using Common.World.Messages;

namespace Client.World;

public class ClientWorldService : IWorldService
{
    private readonly ClientNetworkService _network;
    private readonly List<ChunkData> _chunks = new();

    public IEnumerable<ChunkData> Chunks => _chunks;

    public ClientWorldService(INetworkService network)
    {
        _network = (ClientNetworkService) network;
        _network.RegisterMessageHandle<ChunkDataMessage>(OnChunkDataMessage);

        _network.EventOnConnect += OnConnect;
    }

    private readonly HashSet<Vector2i> _chunksToRequestLoad = new();
    private readonly HashSet<Vector2i> _chunksToRequestUnload = new();

    public void Update(float deltaTime)
    {
        if (_chunksToRequestLoad.Count != 0)
        {
            var chunkRequestMessage = new ChunkRequestMessage()
            {
                RequestType = EChunkRequest.Load,
                Chunks = _chunksToRequestLoad.ToArray()
            };

            _network.SendToServer(chunkRequestMessage);

            _chunksToRequestLoad.Clear();
        }

        if (_chunksToRequestUnload.Count != 0)
        {
            var chunkRequestMessage = new ChunkRequestMessage()
            {
                RequestType = EChunkRequest.Unload,
                Chunks = _chunksToRequestUnload.ToArray()
            };

            _network.SendToServer(chunkRequestMessage);

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

    private void OnChunkDataMessage(INetworkMessage message, NetworkPlayer? player)
    {
        var dataMessage = message as ChunkDataMessage;

        var chunkData = _chunks.FirstOrDefault(x => x.Position == dataMessage!.Position);

        if (chunkData == null)
        {
            chunkData = new ChunkData(dataMessage!.Position);
            _chunks.Add(chunkData);
        }

        chunkData.GroundArray = dataMessage!.GroundLayer!;
        chunkData.ObjectArray = dataMessage!.ObjectLayer!;

        chunkData.BiomeArray = dataMessage!.BiomeMap!.Cast<EBiomeType>().ToArray();
        chunkData.HeatArray = dataMessage!.HeatMap!.Cast<EHeatType>().ToArray();
        chunkData.MoistureArray = dataMessage!.MoistureMap!.Cast<EMoistureType>().ToArray();
    }

    // TODO: Update on entity system implementation
    private void OnConnect()
    {
        
    }

    public void Draw() { }
}