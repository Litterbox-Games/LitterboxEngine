using Common.Registry;

namespace Common.Services.Block;

public class BlockRegistry : IRegistry<IBlock>
{
    public IDictionary<uint, IBlock> ObjectMapping { get; } = new Dictionary<uint, IBlock>();
    public IDictionary<string, uint> IdMapping { get; } = new Dictionary<string, uint>();
}