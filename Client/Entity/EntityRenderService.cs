using System.Drawing;
using Client.Graphics;
using Client.Resource;
using Common.DI;
using Common.Entity;
using Common.Resource;
using MoreLinq;

namespace Client.Entity;

public class EntityRenderService : ITickableService
{
    private readonly IEntityService _entityService;
    private readonly IRendererService _rendererService;
    private readonly IResourceService _resourceService;
    private Texture? _textureAtlas;

    public EntityRenderService(IEntityService entityService, IRendererService rendererService, IResourceService resourceService)
    {
        _entityService = entityService;
        _rendererService = rendererService;
        _resourceService = resourceService;
    }
    
    public void Update(float deltaTime) { }

    public void Draw()
    {
        _textureAtlas ??= _resourceService.Get<Texture>("Objects.png");

        _entityService.Entities.ForEach(x =>
        {
            _rendererService.DrawTexture(_textureAtlas, _textureAtlas.GetSourceRectangle(5, 0), new RectangleF(x.Position.X, x.Position.Y, 1, 1), Color.White);
        });
    }
}