using System.Numerics;
using Arch.Core;
using Client.Components;
using Client.Graphics;
using Client.Graphics.Input;
using Common.Components;
using Common.Core;
using Common.Core.Attributes;
using Common.Mathematics;
using Common.Services.Entities;
using Common.Services.Players;
using Common.Services.World;
using ImGuiNET;
using Silk.NET.Input;

namespace Client.Systems;

[UpdatablePriority(EPriority.High)]
public class PlayerControlSystem : ISystem, IUpdatable, IDrawable
{
    private readonly QueryDescription _playerControlled = new QueryDescription().WithAll<Position, PlayerControls>();
    
    private readonly IWorldService _worldService;
    private readonly InputService _inputService;
    private readonly CameraService _cameraService;
    private readonly IEntityService _entityService;
    private readonly IPlayerService _playerService;
    
    private Vector2i _chunkPosition;

    private readonly Queue<float> _fpsRecordings = new();
    
    public PlayerControlSystem(IEntityService entityService, IWorldService worldService, InputService inputService, CameraService cameraService, IPlayerService playerService)
    {
        _worldService = worldService;
        _inputService = inputService;
        _cameraService = cameraService;
        _entityService = entityService;
        _playerService = playerService;
        
        _inputService.EventOnMouseClick += OnMouseClick;
    }
    
    /// <inheritdoc />
    public void Update(float deltaTime)
    {
        _entityService.Entities.Query(in _playerControlled, ( 
            ref Position position
        ) => {
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

            if (direction == Vector2.Zero) return;
        
            direction = Vector2.Normalize(direction);
            position.Current += direction * speed * deltaTime;
            _cameraService.Target = position.Current;

            UpdateChunks(position.Current);
        });
        
        _fpsRecordings.Enqueue(MathF.Round(1f / deltaTime));
        if (_fpsRecordings.Count > 60) _fpsRecordings.Dequeue();
    }

    private void UpdateChunks(Vector2 playerEntityPosition)
    {
        const int chunkRadius = 2;
        
        _chunkPosition = (playerEntityPosition / ChunkData.ChunkSize).Modulus(IWorldService.WorldSize).ToVector2i();

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

    private void OnMouseClick(MouseButton button, Vector2 position)
    {
        // if (button == MouseButton.Left)
        // {
        //     var worldPosition = _cameraService.ScreenToWorldPosition(position); 
        // 
        //     var message = new BlockUpdateMessage
        //     {
        //       Chunk = (worldPosition / ChunkData.ChunkSize).Modulus(IWorldService.WorldSize).ToVector2i(), 
        //       Position = worldPosition.Modulus(IWorldService.WorldSize).ToVector2i(),
        //       BlockType = EBlockType.Object,
        //       Id = 1 // TODO: need a real block to put here (reserve 0 for Air or Nothing)
        //     };
        // 
        //     var chunk = _worldService.GetChunkData(message.Chunk);
        // 
        //     if (chunk == null)
        //     {
        //         return;
        //     }
        //     
        //     // Were going to predict that the server will listen to our request
        //     chunk.SetBlockAtLocalPosition(message.Id, message.Position, message.BlockType);
        //     
        //     _networkService.SendToServer(message);
        // }
    }
    
    /// <inheritdoc />
    public void Draw(Renderer renderer)
    {
        ImGui.Begin("Debug");

        _entityService.Entities.Query(in _playerControlled, ( 
            ref Position position
        ) => {
            ImGui.PlotLines("FPS", ref _fpsRecordings.ToArray()[0], _fpsRecordings.Count, 0, "", 0, 60, new Vector2(450, 150));
            ImGui.Text($"Player: {_playerService.PlayerId}");
            ImGui.Text($"Player Position: ({position.Current.X}, {position.Current.Y})");  
            ImGui.Text($"Chunk Position: ({_chunkPosition.X}, {_chunkPosition.Y})");
            ImGui.Text($"Entity Count: {_entityService.Entities.CountEntities(new QueryDescription())}");
        });
        
        ImGui.End();
    }
}