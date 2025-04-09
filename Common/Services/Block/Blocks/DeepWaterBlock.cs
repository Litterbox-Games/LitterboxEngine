using Common.Mathematics;

namespace Common.Services.Block.Blocks;

public class DeepWaterBlock : IBlock
{
    public string Id => "deep_water";
    public ushort MappedId { get; set; }
    public string? TexturePath => "Aseprites/BiomePalette.aseprite";
    public Vector2i? TextureOffset => new Vector2i(1, 3);
}