using System.Numerics;
using Client.Graphics;
using Client.Graphics.Input;
using Common.DI;
using Common.DI.Attributes;
using Common.Entity;
using Common.Mathematics;
using Common.Network;
using Common.World;
using ImGuiNET;
using Silk.NET.Input;

namespace Client.Player;

[TickablePriority(EPriority.High)]
public class PlayerControlService : ITickableService
{
    private readonly INetworkService _networkService;
    private readonly IWorldService _worldService;
    private readonly InputService _inputService;
    private readonly CameraService _cameraService;
    private readonly IEntityService _entityService;
    
    private GameEntity? _playerEntity;
    private Vector2i _chunkPosition;
    
    public PlayerControlService(INetworkService networkService, IEntityService entityService, IWorldService worldService, InputService inputService, CameraService cameraService)
    {
        _networkService = networkService;
        _worldService = worldService;
        _inputService = inputService;
        _cameraService = cameraService;
        _entityService = entityService;

        entityService.EventOnEntitySpawn += OnEntitySpawn;
        entityService.EventOnEntityDespawn += OnEntityDespawn;
    }
    
    /// <inheritdoc />
    public void Update(float deltaTime)
    {
        var newPosition = UpdatePosition(deltaTime);

        if (newPosition.HasValue)
        {
            UpdateChunks(newPosition.Value);
        }

    }

    private Vector2? UpdatePosition(float deltaTime)
    {
        if (_playerEntity == null)
            return null;

        var playerEntityPosition = _playerEntity.Position;
        const float speed = 15f; // TODO: assign speeds to entities rather than hard coding here
        var direction = Vector2.Zero;
        
        if (_inputService.IsKeyDown(Key.W))
            direction.Y -= 1f;
        
        if (_inputService.IsKeyDown(Key.A))
            direction.X -= 1f;
        
        if (_inputService.IsKeyDown(Key.S))
            direction.Y += 1f;
        
        if (_inputService.IsKeyDown(Key.D))
            direction.X += 1f;

        if (direction == Vector2.Zero)
            return null;
        
        direction = Vector2.Normalize(direction);
        playerEntityPosition += direction * speed * deltaTime;
        _playerEntity.Position = _cameraService.Target = playerEntityPosition;

        return playerEntityPosition;
    }

    private void UpdateChunks(Vector2 playerEntityPosition)
    {
        const int chunkRadius = 2;
        
        var chunkX = (int)(playerEntityPosition.X / ChunkData.ChunkSize).Modulus(IWorldService.WorldSize);
        var chunkY = (int)(playerEntityPosition.Y / ChunkData.ChunkSize).Modulus(IWorldService.WorldSize);
    
        _chunkPosition = new Vector2i(chunkX, chunkY);
    
        // Load and unload chunks based on square distance
        for (var dx = -chunkRadius - 1; dx <= chunkRadius + 1; dx++)
        {
            for (var dy = -chunkRadius - 1; dy <= chunkRadius + 1; dy++)
            {
                var chunk = new Vector2i(
                    (_chunkPosition.X + dx).Modulus(IWorldService.WorldSize),
                    (_chunkPosition.Y + dy).Modulus(IWorldService.WorldSize)
                );
            
                var squareDistance = dx * dx + dy * dy;
            
                switch (squareDistance)
                {
                    case <= chunkRadius * chunkRadius:
                        _worldService.RequestChunk(chunk);
                        break;
                    case <= (chunkRadius + 1) * (chunkRadius + 1):
                        _worldService.RequestUnloadChunk(chunk);
                        break;
                }
            }
        }
    }
    
    private void OnEntitySpawn(GameEntity entity)
    {
        if (entity.EntityId == _networkService.PlayerId)
        {
            _playerEntity = entity;
            UpdateChunks(entity.Position);
        }
            
    }

    private void OnEntityDespawn(GameEntity entity)
    {
        if (entity.EntityId == _networkService.PlayerId)
            _playerEntity = null;
    }

    /// <inheritdoc />
    public void Draw()
    {
        ImGui.Begin("Debug");

        if (_playerEntity != null)
        {
            ImGui.Text($"Player Position: ({_playerEntity.Position.X}, {_playerEntity.Position.Y})");  
            ImGui.Text($"Chunk Position: ({_chunkPosition.X}, {_chunkPosition.Y})");
            ImGui.Text($"Entity Count: {_entityService.Entities.Count()}");
        }
        
        ImGui.End();
    }
}