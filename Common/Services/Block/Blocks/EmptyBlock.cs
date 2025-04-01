namespace Common.Services.Block.Blocks;

public class EmptyBlock : IBlock
{
    public string Id => "air";
    public ushort MappedId { get; set; }
}