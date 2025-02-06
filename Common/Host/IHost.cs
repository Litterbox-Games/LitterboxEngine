using Common.DI;

namespace Common.Host;

public interface IHost: IDisposable
{
    Container Container { get; }
    void Update(float deltaTime);
}