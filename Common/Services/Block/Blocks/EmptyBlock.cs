using Common.Mathematics;

namespace Common.Services.Block.Blocks;

public class EmptyBlock : IBlock
{
    public string Id => "air";
    public ushort MappedId { get; set; }
    public string? TexturePath => null;
    public Vector2i? TextureOffset => null;
}