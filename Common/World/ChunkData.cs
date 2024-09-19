using System.Runtime.CompilerServices;
using Common.Mathematics;

namespace Common.World;

public class ChunkData
{
    public const int ChunkSize = 16;

    public Vector2i Position { get; }
    public bool IsDirty { get; internal set; }

    public ushort[] ObjectArray = new ushort[256];
    public ushort[] GroundArray = new ushort[256];

    public EMoistureType[] MoistureArray = new EMoistureType[256];
    public EBiomeType[] BiomeArray = new EBiomeType[256];
    public EHeatType[] HeatArray = new EHeatType[256];

    public ChunkData(Vector2i position)
    {
        Position = position;
    }

    public ushort GetBlockAtLocalPosition(Vector2i position, EBlockType layer)
    {
        var index = GetIndexFromLocalPosition(position);

        return layer switch
        {
            EBlockType.Ground => GroundArray[index],
            EBlockType.Object => ObjectArray[index],
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer,
                "Attempting to set a block at an invalid layer.")
        };
    }

    public void SetBlockAtLocalPosition(ushort id, Vector2i position, EBlockType layer)
    {
        var index = GetIndexFromLocalPosition(position);

        switch (layer)
        {
            case EBlockType.Ground:
                GroundArray[index] = id;
                break;
            case EBlockType.Object:
                ObjectArray[index] = id;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(layer), layer,
                    "Attempting to set a block at an invalid layer.");
        }

        IsDirty = true;
    }

    internal Vector2i GetLocalPositionFromWorldPosition(Vector2i position)
    {
        if (position.X / 16 != Position.X || position.Y / 16 != Position.Y)
        {
            throw new ArgumentOutOfRangeException(nameof(position), position,
                "Request position does not exist within the bounds of this chunk.");
        }

        return new Vector2i(position.X % 16, position.X % 16);
    }

    internal int GetIndexFromWorldPosition(Vector2i position)
    {
        return GetIndexFromLocalPosition(GetLocalPositionFromWorldPosition(position));
    }

    public static int GetIndexFromLocalPosition(Vector2i position)
    {
        if (position.X is >= 16 or < 0 || position.Y is >= 16 or < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position), position,
                "Position was out of range for setting a block. Must be between 0 (inclusive) and 15 (inclusive).");
        }

        return GetIndexFromLocalPositionFast(position);
    }

    // Need for rendering loop where we're constrained between 0 and 15 already
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIndexFromLocalPositionFast(Vector2i position)
    {
        return position.X + (position.Y * 16);
    }
}