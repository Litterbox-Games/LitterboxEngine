using Common.Archetypes;

namespace Common.Block;

public interface IBlockEntity : IBlock
{
    IArchetype EntityArchetype { get; }
}