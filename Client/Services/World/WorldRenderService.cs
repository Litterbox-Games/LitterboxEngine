using System.Drawing;
using Arch.Core;
using Arch.Core.Extensions;
using Client.Graphics;
using Client.Services.Resource;
using Common.Components;
using Common.Core;
using Common.Core.Attributes;
using Common.Mathematics;
using Common.Services.Block;
using Common.Services.Entities.Events;
using Common.Services.Events;
using Common.Services.Players;
using Common.Services.Resource;
using Common.Services.World;
using ImGuiNET;

namespace Client.Services.World;

[UpdatablePriority(EPriority.High)]
public class WorldRenderService : IService, IDrawable
{
    private readonly IWorldService _worldService;
    private readonly IPlayerService _playerService;
    private readonly IResourceService _resourceService;
    private readonly BlockRegistry _blockRegistry;
    
    private Entity? _playerEntity;

    private Dictionary<ushort, (Texture, Vector2i)> _textureCache = new();
    
    public WorldRenderService(IPlayerService playerService, IResourceService resourceService, IWorldService worldService, EventService eventService, BlockRegistry blockRegistry)
    {
        _playerService = playerService;
        _worldService = worldService;
        _resourceService = resourceService;
        _blockRegistry = blockRegistry;

        eventService.Handle<EntityCreatedEvent>(OnEntityCreated);
        eventService.Handle<EntityDestroyedEvent>(OnEntityDestroyed);
    }
    
    private void OnEntityCreated(EntityCreatedEvent e)
    {
        if (!e.Entity.Has<Networked, Player>()) return;
        var networked = e.Entity.Get<Networked>();
        if (networked.OwnerId == _playerService.PlayerId) _playerEntity = e.Entity;
    }
    
    private void OnEntityDestroyed(EntityDestroyedEvent e)
    {
        if (!e.Entity.Has<Networked, Player>()) return;
        var networked = e.Entity.Get<Networked>();
        if (networked.OwnerId == _playerService.PlayerId) _playerEntity = null;       
    }

    public void Draw(RendererService renderer)
    {
        if (_playerEntity == null) return;

        IEnumerable<ChunkData> chunks;

        if (_worldService is ServerWorldService serverWorld)
        {
            chunks = serverWorld.NetworkedChunks.Where(x => x.Observers.Any(y => y.PlayerId == _playerService.PlayerId))
                .Select(x => x.ChunkData);
        }
        else
        {
            chunks = _worldService.Chunks;
        }

        var playerPosition = _playerEntity.Value.Get<Position>();
        var playerChunkX = (int)Math.Floor(playerPosition.Current.X / ChunkData.ChunkSize);
        var playerChunkY = (int)Math.Floor(playerPosition.Current.Y / ChunkData.ChunkSize);

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
                    // Ground Layer
                    var groundId = chunk.GroundArray[ChunkData.GetIndexFromLocalPositionFast(new Vector2i(x, y))];

                    if (groundId == 0)
                        continue;
                    
                    if (!_textureCache.TryGetValue(groundId, out var texMap))
                    {
                        var block = _blockRegistry.ObjectMapping[groundId];
                        var path = block.TexturePath;
                        
                        if (path == null)
                            continue;

                        texMap = (_resourceService.Get<Aseprite>(path).Texture, block.TextureOffset!.Value);
                        _textureCache[groundId] = texMap;
                    }

                    var (texture, offset) = texMap;
                    
                    renderer.DrawTexture(texture, texture.GetSourceRectangle(offset.X, offset.Y) , new RectangleF(chunkX * 16 + x, chunkY * 16 + y, 1,
                        1), Color.White);
                    /*
                    // Object Layer
                    var objectId = chunk.ObjectArray[ChunkData.GetIndexFromLocalPositionFast(new Vector2i(x, y))];

                    if (objectId != 0)
                    {
                        renderer.DrawTexture(texture, texture.GetSourceRectangle(0, 4), new RectangleF(chunkX * 16 + x, chunkY * 16 + y, 1,
                            1), Color.White);
                    }
                    */
                }
            }
        }
        
        ImGui.End();
    }
}