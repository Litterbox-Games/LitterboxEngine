namespace Common.Entities.Components;

public struct Networked
{
    public required ulong OwnerId;
    public required ushort EntityType;
    public required ulong NetworkId;
}