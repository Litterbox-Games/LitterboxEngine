using System.Resources;
using Client.Resource;
using Common.DI;
using Common.Mathematics;
using Common.Resource;

namespace Client.World;

public class WorldRenderService : ITickableService
{
    // TODO: Use JSON files to load blocks/textures.
    private readonly Vector2i _grassTexCoord = new(9, 25);
    private readonly Vector2i _deadGrassTexCoord = new(9, 28);
    private readonly Vector2i _dirtTexCoord = new(6, 6);
    private readonly Vector2i _stoneTexCoord = new(6, 11);
    private readonly Vector2i _waterTexCoord = new(6, 21);

    private readonly Texture _tileMap;
    
    public WorldRenderService(IResourceService resourceService)
    {
        _tileMap = resourceService.Get<Texture>("Terrain.png");
    }
    
    public void Update(float deltaTime)
    {
        
    }

    public void Draw()
    {
        
    }
}