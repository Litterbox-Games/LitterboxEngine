using System.Numerics;
using Arch.Core;
using Client.Graphics;
using Common.Components;
using Common.Core;
using Common.Mathematics;
using Common.Services.Entities;
using Autofac.Features.AttributeFilters;

namespace Client.Services.Players;

[Game]
public class CameraService : IService, IUpdatable
{
    private readonly QueryDescription _target = new QueryDescription().WithAll<CameraFollow, Position>();
    
    private readonly IEntityService _entityService;
    private readonly WindowService _window;
    private readonly RendererService _renderer;
    
    public readonly Camera Camera;
    
    public CameraService(IEntityService entityService, WindowService window, [KeyFilter("Game")] RendererService renderer)
    {
        _entityService = entityService;
        _window = window;
        _renderer = renderer;
        
        Camera = new Camera(Vector2.Zero, window.Size.ToVector2());
    }
    
    /// <inheritdoc />
    public void Update(float deltaTime)
    {
        _entityService.Entities.Query(in _target, ( 
            ref Position position
        ) => {
            var scaleFactor = _window.Width / 20;
            Camera.Size = _window.Size.ToVector2() / scaleFactor;
            Camera.Position = position.Current + Camera.Size / 2;
            Camera.Position *= scaleFactor;
            Camera.Position = Camera.Position.Round();
            Camera.Position /= scaleFactor;

            Camera.Update();    
            
            _renderer.ViewMatrix = Camera.ViewMatrix;
        });
    }
    
    public Vector2 ScreenToWorldPosition(Vector2 position)
    {
        var screenSpace = position / _window.Size.ToVector2() * 2 - Vector2.One;
        var clipSpace = new Vector4(screenSpace, Camera.NearPlane, 1);
        Matrix4x4.Invert(Camera.ViewMatrix, out var inverseViewMatrix);
        var worldSpace = Vector4.Transform(clipSpace, inverseViewMatrix);
        return new Vector2(worldSpace.X, worldSpace.Y);
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}