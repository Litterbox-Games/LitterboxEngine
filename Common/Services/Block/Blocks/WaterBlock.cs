using Common.Mathematics;

namespace Common.Services.Block.Blocks;

public class WaterBlock : IBlock
{
    public string Id => "water";
    public ushort MappedId { get; set; }
    public string? TexturePath => "Aseprites/BiomePalette.aseprite";
    public Vector2i? TextureOffset => new Vector2i(2, 3);
}