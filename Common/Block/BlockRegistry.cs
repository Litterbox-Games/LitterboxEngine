using Common.Registry;

namespace Common.Block;

public class BlockRegistry : IRegistry<IBlock>
{
    public IDictionary<uint, IBlock> ObjectMapping => new Dictionary<uint, IBlock>();
    public IDictionary<string, uint> IdMapping => new Dictionary<string, uint>();
}