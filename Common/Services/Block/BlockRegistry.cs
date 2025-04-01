using Common.Registry;
using Common.Services.Block.Blocks;

namespace Common.Services.Block;

public class BlockRegistry : IRegistry<IBlock>
{
    public BlockRegistry()
    {
        this.Register(new EmptyBlock());
        this.Register(new GrassBlock());
        this.Register(new SnowBlock());
        this.Register(new StoneBlock());
        this.Register(new WaterBlock());
        this.Register(new DeepWaterBlock());
    }
    
    public IDictionary<ushort, IBlock> ObjectMapping { get; } = new Dictionary<ushort, IBlock>();
    public IDictionary<string, ushort> IdMapping { get; } = new Dictionary<string, ushort>();
}