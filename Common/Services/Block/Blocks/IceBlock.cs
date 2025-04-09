using Common.Mathematics;

namespace Common.Services.Block.Blocks;

public class IceBlock : IBlock
{
    public string Id => "ice";
    public ushort MappedId { get; set; }
    public string? TexturePath => "Aseprites/BiomePalette.aseprite";
    public Vector2i? TextureOffset => new Vector2i(0, 0);
}