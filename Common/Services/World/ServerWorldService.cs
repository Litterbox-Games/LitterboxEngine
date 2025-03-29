using Common.Core;
using Common.Host;
using Common.Mathematics;
using Common.Services.Events;
using Common.Services.Logging;
using Common.Services.Network;
using Common.Services.Players;
using Common.Services.Players.Messages;
using Common.Services.World.Generation;
using Common.Services.World.Messages;

namespace Common.Services.World;

public class ServerWorldService : IWorldService
{
    public readonly List<NetworkedChunk> NetworkedChunks = [];
    public IEnumerable<ChunkData> Chunks => NetworkedChunks.Select(x => x.ChunkData);

    private readonly IContainer _container;
    private readonly ServerNetworkService _networkService;
    private readonly IPlayerService _playerService;
    private readonly ILoggingService _logger;
    private readonly EventService _eventService;
    private readonly IWorldGenerator _generation;

    public ServerWorldService(IContainer container, ServerNetworkService networkService, IPlayerService playerService, ILoggingService logger, EventService eventService)
    {
        _container = container;
        _networkService = networkService;
        _playerService = playerService;
        _logger = logger;
        _eventService = eventService;
        _generation = container.Resolve<IWorldGenerator>("earth");
        
        _eventService.Handle<PlayerDisconnectMessage>(OnPlayerDisconnect);
        _eventService.Handle<ChunkRequestMessage>(OnChunkRequest);
        _eventService.Handle<BlockUpdateMessage>(OnBlockUpdate);
    }

    private void OnBlockUpdate(BlockUpdateMessage blockUpdate)
    { 
        var chunk = GetChunk(blockUpdate.Chunk);

        if (chunk == null)
        {
            // TODO: this should be a warning
            throw new InvalidOperationException("Player tried to update a block in an unloaded chunk");
        }
        
        chunk.ChunkData.SetBlockAtLocalPosition(blockUpdate.Id, blockUpdate.Position, blockUpdate.BlockType);
    }

    public void RequestChunk(Vector2i position)
    {
        if (_container.GameMode == EGameMode.Dedicated)
        {
            throw new InvalidOperationException("Invalid use of method. This may only be called when the server acts as a host.");
        }

        if (position.X is >= IWorldService.WorldSize or < 0 || position.Y is >= IWorldService.WorldSize or < 0)
        {
            _logger.Warning($"Invalid chunk request. Chunk position: {position}");
            return;
        }

        var player = _networkService.Players.First(x => x.PlayerId == _playerService.PlayerId);
        
        var chunk = GetChunk(position);

        if (chunk == null)
        {
            chunk = new NetworkedChunk(_generation.GenerateChunkAtPosition(position));

            NetworkedChunks.Add(chunk);
        }

        chunk.Observers.Add(player);
    }

    public void RequestUnloadChunk(Vector2i position)
    {
        if (_container.GameMode == EGameMode.Dedicated)
        {
            throw new InvalidOperationException(
                "Invalid use of method. This may only be called when the server acts as a host.");
        }

        var chunk = NetworkedChunks.FirstOrDefault(x => x.ChunkData.Position == position);

        chunk?.Observers.Remove(_networkService.Players.FirstOrDefault(x => x.PlayerId == _playerService.PlayerId)!);
    }

    private void OnChunkRequest(ChunkRequestMessage message)
    {
        if (message.Sender == null) return;

        foreach (var pos in message.Chunks!)
        {
            var chunk = GetChunk(pos);

            if (message.RequestType == EChunkRequest.Load)
            {
                if (chunk == null)
                {
                    chunk = new NetworkedChunk(_generation.GenerateChunkAtPosition(pos));

                    NetworkedChunks.Add(chunk);
                }

                if (!chunk.Observers.Contains(message.Sender))
                {
                    chunk.Observers.Add(message.Sender);
                }

                var dataMessage = new ChunkDataMessage
                {
                    Position = pos,
                    GroundLayer = chunk.ChunkData.GroundArray,
                    ObjectLayer = chunk.ChunkData.ObjectArray,
                    BiomeMap = chunk.ChunkData.BiomeArray.Cast<byte>().ToArray(),
                    HeatMap = chunk.ChunkData.HeatArray.Cast<byte>().ToArray(),
                    MoistureMap = chunk.ChunkData.MoistureArray.Cast<byte>().ToArray()
                };

                _eventService.Emit(dataMessage);
            }
            else
            {
                chunk?.Observers.Remove(message.Sender);
            }
        }
    }
    
    public void Update(float deltaTime)
    {
        var chunksToUnload = new List<NetworkedChunk>();

        NetworkedChunks.ForEach(x =>
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
        
            var dataMessage = new ChunkDataMessage
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

        chunksToUnload.ForEach(x => NetworkedChunks.Remove(x));
    }

    private void OnPlayerDisconnect(PlayerDisconnectMessage e)
    {
        var removedChunk = new List<NetworkedChunk>();

        NetworkedChunks.ForEach(x =>
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
                NetworkedChunks.Remove(x);
        });
    }

    private NetworkedChunk? GetChunk(Vector2i position)
    {
        return NetworkedChunks.FirstOrDefault(x => x.ChunkData.Position == position);
    }
    
    public ChunkData? GetChunkData(Vector2i position)
    {
        return Chunks.FirstOrDefault(x => x.Position == position);
    }
}

public sealed class NetworkedChunk(ChunkData data)
{
    public readonly ChunkData ChunkData = data;
    public readonly List<ServerPlayer> Observers = [];
}