using System.Drawing;
using System.Resources;
using Client.Graphics;
using Client.Graphics.Resources;
using Common.DI;
using Common.Entity;
using MoreLinq;

namespace Client.Entity;

public class EntityRenderService : ITickableService
{
    private IEntityService _entityService;
    private IRendererService _rendererService;
    private Texture? _textureAtlas;

    public EntityRenderService(IEntityService entityService, IRendererService rendererService)
    {
        _entityService = entityService;
        _rendererService = rendererService;
    }
    
    public void Update(float deltaTime) { }

    public void Draw()
    {
        _textureAtlas ??= ResourceManager.Get<Texture>("Objects.png");

        _entityService.Entities.ForEach(x =>
        {
            // TODO: add TextureAtlas class
            _rendererService.DrawTexture(_textureAtlas, _textureAtlas.GetSourceRectangle(5, 0), new Rectangle((int) x.Position.X, (int) x.Position.Y, 50, 50), Color.White);
        });
    }
}