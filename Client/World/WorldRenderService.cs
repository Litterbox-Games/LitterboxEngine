using System.Drawing;
using Client.Graphics;
using Client.Resource;
using Common.DI;
using Common.DI.Attributes;
using Common.Entity;
using Common.Mathematics;
using Common.Network;
using Common.Resource;
using Common.World;
using ImGuiNET;

namespace Client.World;


[TickablePriority(EPriority.High)]
public class WorldRenderService : ITickableService
{
 
    private readonly RendererService _rendererService;
    private readonly IWorldService _worldService;
    private readonly INetworkService _networkService;
    private readonly IResourceService _resourceService;
    
    private GameEntity? _playerEntity;

    
    public WorldRenderService(INetworkService networkService, IResourceService resourceService, RendererService rendererService, IWorldService worldService, IEntityService entityService)
    {
        _networkService = networkService;
        _rendererService = rendererService;
        _worldService = worldService;
        _resourceService = resourceService;

        entityService.EventOnEntitySpawn += OnEntitySpawn;
        entityService.EventOnEntityDespawn += OnEntityDespawn;
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

            var texture = _resourceService.Get<Aseprite>("Aseprites/BiomePalette.aseprite").Texture;
            
            for (var x = 0; x < 16; x++)
            {
                for (var y = 0; y < 16; y++)
                {
                    // Ground Layer
                    var groundId = chunk.GroundArray[ChunkData.GetIndexFromLocalPositionFast(new Vector2i(x, y))];

                    var sourceRectangle = ((EBiomeType)groundId) switch
                    {
                        EBiomeType.Ice => texture.GetSourceRectangle(0, 0),
                        EBiomeType.BorealForest => texture.GetSourceRectangle(1, 0),
                        EBiomeType.Desert => texture.GetSourceRectangle(2, 0),
                        EBiomeType.Grassland => texture.GetSourceRectangle(0, 1),
                        EBiomeType.SeasonalForest => texture.GetSourceRectangle(1, 1),
                        EBiomeType.Tundra => texture.GetSourceRectangle(2, 1),
                        EBiomeType.Savanna => texture.GetSourceRectangle(0, 2),
                        EBiomeType.TemperateRainforest => texture.GetSourceRectangle(1, 2),
                        EBiomeType.TropicalRainforest => texture.GetSourceRectangle(2, 2),
                        EBiomeType.Woodland => texture.GetSourceRectangle(0, 3),
                        EBiomeType.DeepOcean => texture.GetSourceRectangle(1, 3),
                        EBiomeType.Ocean => texture.GetSourceRectangle(2, 3),
                        _ => texture.GetSourceRectangle(0, 4)
                    };
                    
                    _rendererService.DrawTexture(texture, sourceRectangle, new RectangleF(chunkX * 16 + x, chunkY * 16 + y, 1,
                        1), Color.White);
                    
                    // Object Layer
                    var objectId = chunk.ObjectArray[ChunkData.GetIndexFromLocalPositionFast(new Vector2i(x, y))];

                    if (objectId != 0)
                    {
                        _rendererService.DrawTexture(texture, texture.GetSourceRectangle(0, 4), new RectangleF(chunkX * 16 + x, chunkY * 16 + y, 1,
                            1), Color.White);
                    }
                }
            }
        }
        
        ImGui.End();
    }
}