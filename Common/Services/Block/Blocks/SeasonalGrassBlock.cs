using Common.Mathematics;

namespace Common.Services.Block.Blocks;

public class SeasonalGrassBlock : IBlock
{
    public string Id => "seasonal_grass";
    public ushort MappedId { get; set; }
    public string? TexturePath => "Aseprites/BiomePalette.aseprite";
    public Vector2i? TextureOffset => new Vector2i(1, 1);
}