using System.Numerics;
using Client.Graphics;
using Client.Graphics.Input;
using Common.DI;
using Common.DI.Attributes;
using Common.Entity;
using Common.Network;

namespace Client.Player;

[TickablePriority(EPriority.High)]
public class PlayerControlService : ITickableService
{
    private readonly INetworkService _networkService;
    private readonly IKeyboardService _keyboardService;
    
    private GameEntity? _playerEntity;
    
    public PlayerControlService(INetworkService networkService, IEntityService entityService, IKeyboardService keyboardService)
    {
        _networkService = networkService;
        _keyboardService = keyboardService;

        entityService.EventOnEntitySpawn += OnEntitySpawn;
        entityService.EventOnEntityDespawn += OnEntityDespawn;
    }
    
    /// <inheritdoc />
    public void Update(float deltaTime)
    {
        if (_playerEntity == null)
            return;

        var playerEntityPosition = _playerEntity.Position;
        const float speed = 150f;
        var direction = Vector2.Zero;
        
        if (_keyboardService.IsKeyDown(Key.W))
            direction.Y -= 1f;
        
        if (_keyboardService.IsKeyDown(Key.A))
            direction.X -= 1f;
        
        if (_keyboardService.IsKeyDown(Key.S))
            direction.Y += 1f;
        
        if (_keyboardService.IsKeyDown(Key.D))
            direction.X += 1f;
        
        if (direction != Vector2.Zero)
        {
            direction = Vector2.Normalize(direction);
            playerEntityPosition += direction * speed * deltaTime;
        }
        
        _playerEntity.Position = playerEntityPosition;
    }
    
    private void OnEntitySpawn(GameEntity entity)
    {
        if (entity.EntityId == _networkService.PlayerId)
            _playerEntity = entity;
    }

    private void OnEntityDespawn(GameEntity entity)
    {
        if (entity.EntityId == _networkService.PlayerId)
            _playerEntity = null;
    }

    /// <inheritdoc />
    public void Draw() { }
}