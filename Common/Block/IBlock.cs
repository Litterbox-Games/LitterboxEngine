using Common.Registry;

namespace Common.Block;

public interface IBlock : IRegisterable
{
    // TODO: Block data
    
    uint Hardness { get; }
    
    // Maybe tools could have a string[] property defining what type of tool it is.
    string PreferredToolType { get; }
    
    // IItemStack[] GenerateDrops (bool isPreferredTool);
}