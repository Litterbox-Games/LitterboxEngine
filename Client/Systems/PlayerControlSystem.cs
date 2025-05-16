using System.Numerics;
using Arch.Core;
using Client.Graphics;
using Common.Components;
using Common.Core;
using Common.Core.Attributes;
using Common.Mathematics;
using Common.Services.Entities;
using Common.Services.Events;
using Common.Services.Players;
using Common.Services.World;
using Common.Services.World.Events;
using ImGuiNET;
using Silk.NET.Input;

namespace Client.Systems;

[UpdatablePriority(EPriority.High)]
public class PlayerControlSystem : ISystem, IInputable, IUpdatable, IDrawable
{
    private readonly QueryDescription _playerControlled = new QueryDescription().WithAll<Position, Velocity, PlayerControls>();
    
    private readonly IWorldService _worldService;
    private readonly CameraSystem _cameraSystem;
    private readonly IEntityService _entityService;
    private readonly IPlayerService _playerService;
    private readonly EventService _eventService;
    
    private Vector2i _chunkPosition;

    private readonly Queue<float> _fpsRecordings = new();
    
    public PlayerControlSystem(IEntityService entityService, IWorldService worldService, CameraSystem cameraSystem, IPlayerService playerService, EventService eventService)
    {
        _worldService = worldService;
        _cameraSystem = cameraSystem;
        _entityService = entityService;
        _playerService = playerService;
        _eventService = eventService;
    }

    public void Input(InputService input)
    {
        _entityService.Entities.Query(in _playerControlled, ( 
            ref Velocity velocity
        ) => {
            const float speed = 15f; // TODO: assign speeds to entities rather than hard coding here
            velocity = Vector2.Zero;
        
            if (input.IsKeyDown(Key.W))
                velocity.Y -= 1f;
        
            if (input.IsKeyDown(Key.A))
                velocity.X -= 1f;
        
            if (input.IsKeyDown(Key.S))
                velocity.Y += 1f;
        
            if (input.IsKeyDown(Key.D))
                velocity.X += 1f;

            if (velocity == Vector2.Zero) return;
        
            velocity = Vector2.Normalize(velocity) * speed;
        });

        // if (input.IsMouseDown(MouseButton.Right))
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
    public void Update(float deltaTime)
    {
        UpdatePosition(deltaTime);
        UpdateChunks();
        
        _fpsRecordings.Enqueue(MathF.Round(1f / deltaTime));
        if (_fpsRecordings.Count > 60) _fpsRecordings.Dequeue();
    }
    
    private void UpdatePosition(float deltaTime)
    {
        _entityService.Entities.Query(in _playerControlled, ( 
            ref Position position,
            ref Velocity velocity
        ) => {
            position.Current += velocity.ToVector2() * deltaTime;
        });
    }
    
    private void UpdateChunks()
    {
        // TODO: turn into user setting - can expose through ImGui first
        const int chunkRadius = 2;
        
        List<Vector2i> chunksToRequestLoad = [];
        List<Vector2i> chunksToRequestUnload = [];
        
        _entityService.Entities.Query(in _playerControlled, ( 
            ref Position position
        ) =>
        {
            // TODO: maybe need to prevent from doing this every frame?
            _chunkPosition = (position.Current / ChunkData.ChunkSize).Modulus(IWorldService.WorldSize).ToVector2i();
            
            // Load and unload chunks based on square distance
            for (var dx = -chunkRadius - 1; dx <= chunkRadius + 1; dx++)
            {
                for (var dy = -chunkRadius - 1; dy <= chunkRadius + 1; dy++)
                {
                    var chunkPosition = new Vector2i(
                        (_chunkPosition.X + dx).Modulus(IWorldService.WorldSize),
                        (_chunkPosition.Y + dy).Modulus(IWorldService.WorldSize)
                    );
            
                    // TODO: store Chunks in a dictionary to prevent costly look ups
                    var chunk = _worldService.Chunks.FirstOrDefault(x => x.Position == chunkPosition);
                    
                    var squareDistance = dx * dx + dy * dy;
                    
                    switch (squareDistance)
                    {
                        case <= chunkRadius * chunkRadius:
                        {
                            if (chunk == null)
                                chunksToRequestLoad.Add(chunkPosition);
                            break;
                        }
                        case <= (chunkRadius + 1) * (chunkRadius + 1):
                        {
                            if (chunk != null)
                                chunksToRequestUnload.Add(chunkPosition);
                            break;
                        }
                            
                    }
                }
            }
        });
        
        if (chunksToRequestLoad.Count != 0)
        {
            var chunkRequestMessage = new ChunkRequestEvent
            {
                RequestType = EChunkRequest.Load,
                Chunks = chunksToRequestLoad.ToArray()
            };

            _eventService.Emit(chunkRequestMessage);
        }

        if (chunksToRequestUnload.Count != 0)
        {
            var chunkRequestMessage = new ChunkRequestEvent
            {
                RequestType = EChunkRequest.Unload,
                Chunks = chunksToRequestUnload.ToArray()
            };

            _eventService.Emit(chunkRequestMessage);
        }
    }
    
    private float _fpsAverage = 0;
    private float _timeSinceAverage = 0;
    
    /// <inheritdoc />
    public void Draw(float deltaTime, RendererService renderer)
    {
        ImGui.Begin("Debug");

        if (_timeSinceAverage >= 1f)
        {
            _timeSinceAverage -= 1f;

            _fpsAverage = _fpsRecordings.ToArray().Average();
        }

        _entityService.Entities.Query(in _playerControlled, ( 
            ref Position position
        ) => {
            if (_fpsRecordings.Count > 0)
            {
                ImGui.PlotLines($"FPS: {_fpsAverage:#.##}", ref _fpsRecordings.ToArray()[0], _fpsRecordings.Count, 0, "", 0, 60, new Vector2(450, 150));    
            }
            ImGui.Text($"Player: {_playerService.PlayerId}");
            ImGui.Text($"Player Position: ({position.Current.X}, {position.Current.Y})");  
            ImGui.Text($"Chunk Position: ({_chunkPosition.X}, {_chunkPosition.Y})");
            ImGui.Text($"Entity Count: {_entityService.Entities.CountEntities(new QueryDescription())}");
        });
        
        ImGui.End();
        
        _timeSinceAverage += deltaTime;
    }
}