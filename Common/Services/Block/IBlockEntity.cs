using Common.Archetypes;

namespace Common.Services.Block;

public interface IBlockEntity : IBlock
{
    IArchetype EntityArchetype { get; }
}