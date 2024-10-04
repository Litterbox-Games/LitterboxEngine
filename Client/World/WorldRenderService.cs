using System.Drawing;
using Client.Graphics;
using Client.Resource;
using Common.DI;
using Common.Entity;
using Common.Mathematics;
using Common.Network;
using Common.Resource;
using Common.World;
using ImGuiNET;

namespace Client.World;

public class WorldRenderService : ITickableService
{
 
    private readonly RendererService _rendererService;
    private readonly IWorldService _worldService;
    private readonly INetworkService _networkService;
    
    private GameEntity? _playerEntity;
    private readonly Texture _texture;

    
    public WorldRenderService(INetworkService networkService, IResourceService resourceService, RendererService rendererService, IWorldService worldService, IEntityService entityService)
    {
        _networkService = networkService;
        _rendererService = rendererService;
        _worldService = worldService;

        entityService.EventOnEntitySpawn += OnEntitySpawn;
        entityService.EventOnEntityDespawn += OnEntityDespawn;
        
        _texture = resourceService.Get<Texture>("BiomePalette.png");
    }
    
    private void OnEntitySpawn(GameEntity entity)
    {
        if (entity.EntityId == _networkService.PlayerId)
        {
            _playerEntity = entity;
        }
            
    }

    private void OnEntityDespawn(GameEntity entity)
    {
        if (entity.EntityId == _networkService.PlayerId)
            _playerEntity = null;
    }
    
    public void Update(float deltaTime)
    {
        
    }

    public void Draw()
    {
        if (_playerEntity == null) return;

        IEnumerable<ChunkData> chunks;

        if (_worldService is ServerWorldService serverWorld)
        {
            chunks = serverWorld.NetworkedChunks.Where(x => x.Observers.Any(y => y.PlayerID == _networkService.PlayerId))
                .Select(x => x.ChunkData);
        }
        else
        {
            chunks = _worldService.Chunks;
        }

        var playerChunkX = (int)Math.Floor(_playerEntity.Position.X / ChunkData.ChunkSize);
        var playerChunkY = (int)Math.Floor(_playerEntity.Position.Y / ChunkData.ChunkSize);

        ImGui.Begin("WorldRenderService");
        
        ImGui.Text($"{playerChunkX}, {playerChunkY}");
        
        
        
        foreach (var chunk in chunks)
        {
            var chunkX = (chunk.Position.X - playerChunkX + IWorldService.WorldSize / 2).Modulus(IWorldService.WorldSize) - IWorldService.WorldSize / 2 + playerChunkX;
            var chunkY = (chunk.Position.Y - playerChunkY + IWorldService.WorldSize / 2).Modulus(IWorldService.WorldSize) - IWorldService.WorldSize / 2 + playerChunkY;
            
            ImGui.Text($"({chunk.Position.X}, {chunk.Position.Y})\t\t({chunkX}, {chunkY})");            
            
            for (var x = 0; x < 16; x++)
            {
                for (var y = 0; y < 16; y++)
                {
                    var id = chunk.GroundArray[ChunkData.GetIndexFromLocalPositionFast(new Vector2i(x, y))];

                    var sourceRectangle = ((EBiomeType)id) switch
                    {
                        EBiomeType.Ice => _texture.GetSourceRectangle(0, 0),
                        EBiomeType.BorealForest => _texture.GetSourceRectangle(2, 0),
                        EBiomeType.Desert => _texture.GetSourceRectangle(4, 0),
                        EBiomeType.Grassland => _texture.GetSourceRectangle(0, 2),
                        EBiomeType.SeasonalForest => _texture.GetSourceRectangle(2, 2),
                        EBiomeType.Tundra => _texture.GetSourceRectangle(4, 2),
                        EBiomeType.Savanna => _texture.GetSourceRectangle(0, 4),
                        EBiomeType.TemperateRainforest => _texture.GetSourceRectangle(2, 4),
                        EBiomeType.TropicalRainforest => _texture.GetSourceRectangle(4, 4),
                        EBiomeType.Woodland => _texture.GetSourceRectangle(0, 6),
                        EBiomeType.DeepOcean => _texture.GetSourceRectangle(2, 6),
                        EBiomeType.Ocean => _texture.GetSourceRectangle(4, 6),
                        _ => _texture.GetSourceRectangle(0, 8)
                    };
                    
                    _rendererService.DrawTexture(_texture, sourceRectangle, new RectangleF(chunkX * 16 + x, chunkY * 16 + y, 1,
                        1), Color.White);
                }
            }
        }
        
        ImGui.End();
    }
}