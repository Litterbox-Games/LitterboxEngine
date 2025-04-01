using Common.Services.Events;

namespace Common.Services.Network.Events;

// TODO: split this into a ServerStartEvent and ClientStartEvent?
public struct StartEvent: IEvent;