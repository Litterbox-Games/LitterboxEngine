using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Common.Entity
{
    public abstract class GameEntity
    {
        public ulong EntityId { get; set; }
        public abstract ushort EntityType { get; }
        public ulong OwnerId { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 LastSentPosition { get; set; }
        public IEntityService EntitySystem { get; set; } = null!;

        public Queue<QueuedMovement> QueuedMovements { get; } = new();

        public virtual void DeserializeEntityData(byte[] data) { }

        internal virtual byte[] SerializeEntityData()
        {
            return Array.Empty<byte>();
        }
    }

    public struct QueuedMovement
    {
        public QueuedMovement(Vector2 position, DateTime timeStamp)
        {
            Position = position;
            TimeStamp = timeStamp;
        }
        
        public Vector2 Position;
        public DateTime TimeStamp;
    }
}