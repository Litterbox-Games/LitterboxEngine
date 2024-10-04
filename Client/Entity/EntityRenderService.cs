﻿using System.Drawing;
using Client.Graphics;
using Client.Resource;
using Common.DI;
using Common.Entity;
using Common.Mathematics;
using Common.Network;
using Common.Resource;
using Common.World;
using ImGuiNET;
using MoreLinq;

namespace Client.Entity;

public class EntityRenderService : ITickableService
{
    private readonly IEntityService _entityService;
    private readonly RendererService _rendererService;
    private readonly IResourceService _resourceService;
    private readonly INetworkService _networkService;
    
    private GameEntity? _playerEntity;
    private Texture? _texture;
    
    
    public EntityRenderService(IEntityService entityService, RendererService rendererService, INetworkService networkService, IResourceService resourceService)
    {
        _entityService = entityService;
        _rendererService = rendererService;
        _resourceService = resourceService;
        _networkService = networkService;
        
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
    
    public void Update(float deltaTime) { }

    public void Draw()
    {
        if (_playerEntity == null) return;
        
        _texture ??= _resourceService.Get<Texture>("Items.png");

        foreach (var entity in _entityService.Entities)
        {
            const int worldSize = IWorldService.WorldSize * ChunkData.ChunkSize;
            
            var position = entity.Position.Modulus(worldSize);
            var entityX = (position.X - _playerEntity.Position.X + worldSize / 2f).Modulus(worldSize) - worldSize / 2f + _playerEntity.Position.X;
            var entityY = (position.Y - _playerEntity.Position.Y + worldSize / 2f).Modulus(worldSize) - worldSize / 2f + _playerEntity.Position.Y;

            _rendererService.DrawTexture(_texture, new Rectangle(48, 137, 22, 15), new RectangleF(entityX, entityY, 22f / 15f, 1), Color.White);
        }
    }
}