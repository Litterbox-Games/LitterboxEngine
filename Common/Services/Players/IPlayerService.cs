using Common.DI;

namespace Common.Players;

public interface IPlayerService : IService
{
    ulong PlayerId { get; }
    IEnumerable<NetworkPlayer> Players { get; }
}