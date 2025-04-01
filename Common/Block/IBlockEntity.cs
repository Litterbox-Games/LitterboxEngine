using Common.Archetypes;

namespace Common.Block;

public interface IBlockEntity : IBlock
{
    IArchetype EntityArchetype { get; }
}

/* OR

public interface IBlockEntity<T> : IBlock where T : IArchetype
{
    T EntityArchetype { get; }
}

*/