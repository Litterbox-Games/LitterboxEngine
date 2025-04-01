namespace Common.Services.Block.Blocks;

public class StoneBlock : IBlock
{
    public string Id => "stoneBlock";
    public uint MappedId { get; set; }
    
    public uint Hardness => 10;
    public string PreferredToolType => "pickaxe";
}