using Common.Core;

namespace Common.Services.Players;

public interface IPlayerService : IService
{
    ulong PlayerId { get; }
    IEnumerable<NetworkPlayer> Players { get; }
}