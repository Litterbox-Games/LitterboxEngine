using Common.Mathematics;

namespace Common.Services.Block.Blocks;

public class TemperateGrassBlock : IBlock
{
    public string Id => "temperate_grass";
    public ushort MappedId { get; set; }
    public string? TexturePath => "Aseprites/BiomePalette.aseprite";
    public Vector2i? TextureOffset => new Vector2i(1, 2);
}