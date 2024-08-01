﻿using System.Numerics;
using Common.Entity;
using Common.Host;
using Common.Network;

namespace Client.Host;

/// <summary>
///     The host used for single player or for local hosting.
/// </summary>
public class HostOrSinglePlayerHost : AbstractHost
{
    /// <inheritdoc />
    public HostOrSinglePlayerHost(bool singlePlayer) : base(singlePlayer ? EGameMode.SinglePlayer : EGameMode.Host)
    {
        RegisterServices();

        if (GameMode != EGameMode.Host)
            return;
        
        var networkService = (ServerNetworkService)Resolve<INetworkService>();
        networkService.Listen(7777);
        
        Resolve<MobControllerService>().SpawnMobEntity(new Vector2(0, 5));
    }
}