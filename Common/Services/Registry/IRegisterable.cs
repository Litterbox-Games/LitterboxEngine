namespace Common.Registry;

public interface IRegisterable
{
    string Id { get; }
    ushort MappedId { get; internal set; }
}