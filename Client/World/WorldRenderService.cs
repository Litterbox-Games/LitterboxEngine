using System.Drawing;
using Client.Graphics;
using Client.Resource;
using Common.DI;
using Common.Mathematics;
using Common.Network;
using Common.Resource;
using Common.World;
using MoreLinq;

namespace Client.World;

public class WorldRenderService : ITickableService
{
 
    private readonly IRendererService _rendererService;
    private readonly IWorldService _worldService;
    private readonly INetworkService _networkService;
    
    private readonly Texture _texture;

    
    public WorldRenderService(INetworkService networkService, IResourceService resourceService, IRendererService rendererService, IWorldService worldService)
    {
        _networkService = networkService;
        _rendererService = rendererService;
        _worldService = worldService;
        
        _texture = resourceService.Get<Texture>("BiomePalette.png");
    }
    
    public void Update(float deltaTime)
    {
        
    }

    public void Draw()
    {
        IEnumerable<ChunkData> chunks;

        if (_worldService is ServerWorldService serverWorld)
        {
            chunks = serverWorld.NetworkedChunks.Where(x => x.Observers.Any(y => y.PlayerID == _networkService.PlayerId))
                .Select(x => x.ChunkData);
        }
        else
        {
            chunks = _worldService.Chunks;
        }

        chunks.ForEach(data =>
        {
            for (var x = 0; x < 16; x++)
            {
                for (var y = 0; y < 16; y++)
                {
                    var id = data.GroundArray[ChunkData.GetIndexFromLocalPositionFast(new Vector2i(x, y))];

                    var sourceRectangle = ((EBiomeType)id) switch
                    {
                        EBiomeType.Ice => _texture.GetSourceRectangle(0, 0),
                        EBiomeType.BorealForest => _texture.GetSourceRectangle(2, 0),
                        EBiomeType.Desert => _texture.GetSourceRectangle(4, 0),
                        EBiomeType.Grassland => _texture.GetSourceRectangle(0, 2),
                        EBiomeType.SeasonalForest => _texture.GetSourceRectangle(2, 2),
                        EBiomeType.Tundra => _texture.GetSourceRectangle(4, 2),
                        EBiomeType.Savanna => _texture.GetSourceRectangle(0, 4),
                        EBiomeType.TemperateRainforest => _texture.GetSourceRectangle(2, 4),
                        EBiomeType.TropicalRainforest => _texture.GetSourceRectangle(4, 4),
                        EBiomeType.Woodland => _texture.GetSourceRectangle(0, 6),
                        EBiomeType.DeepOcean => _texture.GetSourceRectangle(2, 6),
                        EBiomeType.Ocean => _texture.GetSourceRectangle(4, 6),
                        _ => _texture.GetSourceRectangle(0, 8)
                    };
                    
                    _rendererService.DrawTexture(_texture, sourceRectangle, new RectangleF((data.Position.X * 16) + x, (data.Position.Y * 16) + y, 1,
                        1), Color.White);
                }
            }
        });
    }
}