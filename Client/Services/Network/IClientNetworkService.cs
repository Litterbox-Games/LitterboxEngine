using Common.Services.Network;

namespace Client.Services.Network;

public interface IClientNetworkService: INetworkService
{
    public event Action? EventOnConnect;
    public event Action? EventOnDisconnect;
    
    public void Connect(string ip, ushort port);
    public void Disconnect();
    public void SendToServer(INetworkMessage message);
}