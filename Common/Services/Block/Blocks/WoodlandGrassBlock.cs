using Common.Mathematics;

namespace Common.Services.Block.Blocks;

public class WoodlandGrassBlock : IBlock
{
    public string Id => "woodland_grass";
    public ushort MappedId { get; set; }
    public string? TexturePath => "Aseprites/BiomePalette.aseprite";
    public Vector2i? TextureOffset => new Vector2i(0, 3);
}