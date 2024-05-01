using Common.DI;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Player;

public interface IPlayerService : IService
{
    IEnumerable<Player> Players { get; }
}