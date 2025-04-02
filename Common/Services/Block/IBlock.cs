using Common.Mathematics;
using Common.Services.Registry;

namespace Common.Services.Block;

public interface IBlock : IRegisterable
{
    // TODO: Block data
    // uint Hardness { get; }
    // string PreferredToolType { get; }
    // IItemStack[] GenerateDrops (bool isPreferredTool);
    
    string? TexturePath { get; }
    Vector2i? TextureOffset { get; }
}