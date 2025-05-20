using Common.Services.Block.Blocks;
using Common.Services.Registry;

namespace Common.Services.Block;

public class BlockRegistry : IRegistry<IBlock>
{
    public void RegisterDefaultBlocks()
    {
        this.Register(new EmptyBlock());
        
        this.Register(new GrassBlock());
        this.Register(new TundraGrassBlock());
        this.Register(new BorealGrassBlock());
        this.Register(new WoodlandGrassBlock());
        this.Register(new SeasonalGrassBlock());
        this.Register(new SavannaGrassBlock());
        this.Register(new TemperateGrassBlock());
        this.Register(new TropicalGrassBlock());
        
        this.Register(new SandBlock());
        
        this.Register(new WaterBlock());
        this.Register(new DeepWaterBlock());
        this.Register(new IceBlock());
    }
    
    public IDictionary<ushort, IBlock> ObjectMapping { get; } = new Dictionary<ushort, IBlock>();
    public IDictionary<string, ushort> IdMapping { get; } = new Dictionary<string, ushort>();
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}