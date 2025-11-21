using Common.Core;
using Common.Core.Attributes;
using Common.Mathematics;
using Common.Services.Block;
using Common.Services.Events;
using Common.Services.Players;
using Common.Services.Players.Events;
using Common.Services.World.Events;
using Common.Services.World.Generation;

namespace Common.Services.World;

// Dedicated
// Localhost
// SinglePlayer

// Cant do Server attribute here because it wouldnt end up in SinglePlayer, unless server means Host and Client means Client rather than Server meaning Network Server
// We can solve that with a [Network] attribute layer

// [Host][Network][As<INetworkService>]
// ServerNetworkService : INetworkService 

// [Client][Game][As<IWorldService>]
// ClientWorldService : IWorldService

[Game(EMode.Host)]
[As<IWorldService>]
public class ServerWorldService : IWorldService, IUpdatable
{
    public IEnumerable<ChunkData> Chunks => _networkedChunks
        .Where(x => x.Observers.Any(y => y.PlayerId == _playerService.PlayerId))
        .Select(x => x.ChunkData);

    private readonly List<NetworkedChunk> _networkedChunks = [];
    
    private readonly IPlayerService _playerService;
    private readonly EventService _eventService;
    private readonly IWorldGenerator _generation;

    public ServerWorldService(IPlayerService playerService, EventService eventService, BlockRegistry blockRegistry, IWorldGenerator generation)
    {
        _playerService = playerService;
        _eventService = eventService;
        _generation = generation;
        
        // TODO: This should be initialized somewhere else, maybe a resource loading stage of program startup?
        blockRegistry.RegisterDefaultBlocks();
        
        _eventService.Handle<PlayerDisconnectEvent>(OnPlayerDisconnect);
        _eventService.Handle<ChunkRequestEvent>(OnChunkRequest);
        _eventService.Handle<BlockUpdateEvent>(OnBlockUpdate);
    }

    private void OnBlockUpdate(BlockUpdateEvent e)
    { 
        var chunk = GetChunk(e.Chunk);

        if (chunk == null)
        {
            // TODO: this should be a warning
            throw new InvalidOperationException("Player tried to update a block in an unloaded chunk");
        }
        
        chunk.ChunkData.SetBlockAtLocalPosition(e.Id, e.Position, e.BlockType);
    }

    private void OnChunkRequest(ChunkRequestEvent e)
    {
        // TODO: a better way of grabbing the current player? Maybe we should add IPlayerService.Player?
        var player = e.Sender ?? _playerService.Players.FirstOrDefault(player => player.PlayerId == _playerService.PlayerId);

        if (player == null) return;
        
        foreach (var position in e.Chunks)
        {
            var chunk = GetChunk(position);

            if (e.RequestType == EChunkRequest.Load)
            {
                if (chunk == null)
                {
                    chunk = new NetworkedChunk(_generation.GenerateChunkAtPosition(position));

                    _networkedChunks.Add(chunk);
                }

                if (!chunk.Observers.Contains(player))
                {
                    chunk.Observers.Add(player);
                }
                
                _eventService.Emit(new ChunkDataEvent
                {
                    Position = position,
                    GroundLayer = chunk.ChunkData.GroundArray,
                    ObjectLayer = chunk.ChunkData.ObjectArray,
                    BiomeMap = chunk.ChunkData.BiomeArray.Cast<byte>().ToArray(),
                    HeatMap = chunk.ChunkData.HeatArray.Cast<byte>().ToArray(),
                    MoistureMap = chunk.ChunkData.MoistureArray.Cast<byte>().ToArray(),
                    Receivers = serverPlayer => serverPlayer == player
                });
            }
            else
            {
                chunk?.Observers.Remove(player);
            }
        }
    }
    
    public void Update(float deltaTime)
    {
        var chunksToUnload = new List<NetworkedChunk>();

        _networkedChunks.ForEach(x =>
        {
            if (x.Observers.Count == 0)
            {
                chunksToUnload.Add(x);
                return;
            }

            if (!x.ChunkData.IsDirty)
            {
                return;
            }

            x.ChunkData.IsDirty = false;
        
            var dataMessage = new ChunkDataEvent
            {
                Position = x.ChunkData.Position,
                GroundLayer = x.ChunkData.GroundArray,
                ObjectLayer = x.ChunkData.ObjectArray,
                
                // These values shouldn't change, so we may not need to send them in the future.
                BiomeMap = x.ChunkData.BiomeArray.Cast<byte>().ToArray(),
                HeatMap = x.ChunkData.HeatArray.Cast<byte>().ToArray(),
                MoistureMap = x.ChunkData.MoistureArray.Cast<byte>().ToArray(),
                // TODO: can we make this less disgusting??
                Receivers = x.Observers.Contains
            };

            _eventService.Emit(dataMessage);
        });

        chunksToUnload.ForEach(x => _networkedChunks.Remove(x));
    }

    private void OnPlayerDisconnect(PlayerDisconnectEvent e)
    {
        var removedChunk = new List<NetworkedChunk>();

        _networkedChunks.ForEach(x =>
        {
            if (x.Observers.Contains(e.Sender!))
            {
                removedChunk.Add(x);
            }
        });

        removedChunk.ForEach(x =>
        {
            x.Observers.Remove(e.Sender!);
            if (x.Observers.Count == 0)
                _networkedChunks.Remove(x);
        });
    }

    private NetworkedChunk? GetChunk(Vector2i position)
    {
        return _networkedChunks.FirstOrDefault(x => x.ChunkData.Position == position);
    }
    
    public ChunkData? GetChunkData(Vector2i position)
    {
        return Chunks.FirstOrDefault(x => x.Position == position);
    }
    
    public void Dispose()
    {
        _eventService.Unhandle<PlayerDisconnectEvent>(OnPlayerDisconnect);
        _eventService.Unhandle<ChunkRequestEvent>(OnChunkRequest);
        _eventService.Unhandle<BlockUpdateEvent>(OnBlockUpdate);
        
        GC.SuppressFinalize(this);
    }
}

public sealed class NetworkedChunk(ChunkData data)
{
    public readonly ChunkData ChunkData = data;
    public readonly List<NetworkPlayer> Observers = [];
}