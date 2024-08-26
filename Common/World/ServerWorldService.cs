using Common.DI;
using Common.Generation;
using Common.Host;
using Common.Mathematics;
using Common.Network;
using Common.Player;
using Common.World.Messages;

namespace Common.World;

public class ServerWorldService : IWorldService
{
    public readonly List<NetworkedChunk> NetworkedChunks = new();
    public IEnumerable<ChunkData> Chunks => NetworkedChunks.Select(x => x.ChunkData);

    private readonly IHost _host;
    private readonly ServerNetworkService _networkService;
    private readonly IWorldGenerator _generation;

    public ServerWorldService(IHost host, INetworkService networkService)
    {
        _host = host;
        _networkService = (ServerNetworkService)networkService;
        _generation = host.Resolve<IWorldGenerator>("earth");
        
        _networkService.EventOnPlayerDisconnect += OnPlayerDisconnect;
        _networkService.RegisterMessageHandle<ChunkRequestMessage>(OnChunkRequest);
    }
    
    public void RequestChunk(Vector2i position)
    {
        if (_host.GameMode == EGameMode.Dedicated)
        {
            throw new InvalidOperationException(
                "Invalid use of method. This may only be called when the server acts as a host.");
        }

        if (position.X is >= IWorldService.WorldSize or < 0 || position.Y is >= IWorldService.WorldSize or < 0)
        {
            throw new InvalidOperationException("INVALID CHUNK REQUESTED AT POSITION = " + position);
        }

        var player = _networkService.Players.First(x => x.PlayerID == _networkService.PlayerId);

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
        if (_host.GameMode == EGameMode.Dedicated)
        {
            throw new InvalidOperationException(
                "Invalid use of method. This may only be called when the server acts as a host.");
        }

        var chunk = NetworkedChunks.FirstOrDefault(x => x.ChunkData.Position == position);

        chunk?.Observers.Remove(_networkService.Players.FirstOrDefault(x => x.PlayerID == _networkService.PlayerId)!);
    }

    private void OnChunkRequest(INetworkMessage message, NetworkPlayer? player)
    {
        var chunkRequest = message as ChunkRequestMessage;

        var serverPlayer = (ServerPlayer) player!;

        foreach (var pos in chunkRequest!.Chunks!)
        {
            var chunk = GetChunk(pos);

            if (chunkRequest.RequestType == EChunkRequest.Load)
            {
                if (chunk == null)
                {
                    chunk = new NetworkedChunk(_generation.GenerateChunkAtPosition(pos));

                    NetworkedChunks.Add(chunk);
                }

                if (!chunk.Observers.Contains(serverPlayer))
                {
                    chunk.Observers.Add(serverPlayer);
                }

                var dataMessage = new ChunkDataMessage
                {
                    Position = pos,
                    GroundLayer = chunk.ChunkData.GroundArray,
                    ObjectLayer = chunk.ChunkData.ObjectArray,
                    BiomeMap = chunk.ChunkData.BiomeArray.Cast<byte>().ToArray(),
                    HeatMap = chunk.ChunkData.HeatArray.Cast<byte>().ToArray(),
                    MoistureMap = chunk.ChunkData.MoistureArray.Cast<byte>().ToArray(),
                };

                _networkService.SendToPlayer(dataMessage, serverPlayer);
            }
            else
            {
                chunk?.Observers.Remove(serverPlayer);
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
            };

            x.Observers.ForEach(p => _networkService.SendToPlayer(dataMessage, p));
        });

        chunksToUnload.ForEach(x => NetworkedChunks.Remove(x));
    }

    private void OnPlayerDisconnect(ServerPlayer player)
    {
        var removedChunk = new List<NetworkedChunk>();

        NetworkedChunks.ForEach(x =>
        {
            if (x.Observers.Contains(player))
            {
                removedChunk.Add(x);
            }
        });

        removedChunk.ForEach(x =>
        {
            x.Observers.Remove(player);
            if (x.Observers.Count == 0)
                NetworkedChunks.Remove(x);
        });
    }

    private NetworkedChunk? GetChunk(Vector2i position)
    {
        return NetworkedChunks.FirstOrDefault(x => x.ChunkData.Position == position);
    }
    
    public void Draw() { }
}

public sealed class NetworkedChunk
{
    public readonly ChunkData ChunkData;
    public readonly List<ServerPlayer> Observers;

    public NetworkedChunk(ChunkData data)
    {
        ChunkData = data;
        Observers = new List<ServerPlayer>();
    }
}