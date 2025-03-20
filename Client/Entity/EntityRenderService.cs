using System.Drawing;
using Client.Graphics;
using Client.Resource;
using Common.DI;
using Common.Entity;
using Common.Mathematics;
using Common.Network;
using Common.Player;
using Common.Resource;
using Common.World;
using ImGuiNET;

namespace Client.Entity;

public class EntityRenderService : IService, IDrawable
{
    private readonly IEntityService _entityService;
    private readonly IPlayerService _playerService;
    private readonly IResourceService _resourceService;
    
    private GameEntity? _playerEntity;
    private readonly Rectangle _textureSource = new(32, 112, 20, 16);
    
    
    public EntityRenderService(IEntityService entityService, IPlayerService playerService, IResourceService resourceService)
    {
        _entityService = entityService;
        _playerService = playerService;
        _resourceService = resourceService;
        
        entityService.EventOnEntitySpawn += OnEntitySpawn;
        entityService.EventOnEntityDespawn += OnEntityDespawn;
        
    }
    
    private void OnEntitySpawn(GameEntity entity)
    {
        if (entity.EntityId == _playerService.PlayerId)
        {
            _playerEntity = entity;
        }
            
    }

    private void OnEntityDespawn(GameEntity entity)
    {
        if (entity.EntityId == _playerService.PlayerId)
            _playerEntity = null;
    }

    public void Draw(Renderer renderer)
    {
        ImGui.Begin("EntityService");
        
        if (_playerEntity == null) return;

        var texture = _resourceService.Get<Aseprite>("Aseprites/Player.aseprite").Texture;
        
        foreach (var entity in _entityService.Entities)
        {
            ImGui.Text($"{entity.EntityId} {entity.OwnerId} ({entity.Position.X}, {entity.Position.Y})");
            
            const int worldSize = IWorldService.WorldSize * ChunkData.ChunkSize;
            
            var position = entity.Position.Modulus(worldSize);
            var entityX = (position.X - _playerEntity.Position.X + worldSize / 2f).Modulus(worldSize) - worldSize / 2f + _playerEntity.Position.X;
            var entityY = (position.Y - _playerEntity.Position.Y + worldSize / 2f).Modulus(worldSize) - worldSize / 2f + _playerEntity.Position.Y;
            
            // Debug draw for showing network positions vs render position (not world wrapping atm)
            // if (entity.EntityType == 0 && entity.QueuedMovements.Count > 1)
            // { // this is a player
            //     var firstMovement = entity.QueuedMovements.ToArray()[0];
            //     _rendererService.DrawTexture(_texture, _textureSource, new RectangleF(firstMovement.Position.X, firstMovement.Position.Y, 1.25f, 1), Color.Green);
            //     
            //     var secondMovement = entity.QueuedMovements.ToArray()[1];
            //     _rendererService.DrawTexture(_texture, _textureSource, new RectangleF(secondMovement.Position.X, secondMovement.Position.Y, 1.25f, 1), Color.Red);
            // }
            
            renderer.DrawTexture(texture, _textureSource, new RectangleF(entityX, entityY, 1.25f, 1), Color.White);
        }
        
        ImGui.End();
    }
}