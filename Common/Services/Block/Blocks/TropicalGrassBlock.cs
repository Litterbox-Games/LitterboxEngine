using Common.Mathematics;

namespace Common.Services.Block.Blocks;

public class TropicalGrassBlock : IBlock
{
    public string Id => "topical_grass";
    public ushort MappedId { get; set; }
    public string? TexturePath => "Aseprites/BiomePalette.aseprite";
    public Vector2i? TextureOffset => new Vector2i(2, 2);
}