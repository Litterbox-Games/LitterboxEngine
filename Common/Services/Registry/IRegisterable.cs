namespace Common.Registry;

public interface IRegisterable
{
    string Id { get; }
    uint MappedId { get; internal set; }
}