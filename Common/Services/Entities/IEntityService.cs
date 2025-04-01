using Arch.Core;
using Common.Core;

namespace Common.Services.Entities;

public interface IEntityService : IService
{
    // TODO: just make this a service on it's on? Or inject into systems
    public Arch.Core.World Entities { get; }
}