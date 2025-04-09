namespace Common.Services.Registry;

public interface IRegisterable
{
    string Id { get; }
    ushort MappedId { get; internal set; }
}