#pragma warning disable CS1591 //TODO: Missing XML comment for publicly visible type or member
namespace LitterboxEngine.Common.World;

public enum EChunkRequest : byte
{
    Load = 0,
    Unload = 1
}

public enum EBlockType : byte
{
    Ground = 0,
    Object = 1
}

public enum EBiomeType : byte
{
    Desert = 1,
    Savanna = 2,
    TropicalRainforest = 3,
    Grassland = 4,
    Woodland = 5,
    SeasonalForest = 6,
    TemperateRainforest = 7,
    BorealForest = 8,
    Tundra = 9,
    Ice = 10,
    DeepOcean = 11,
    Ocean = 12
}

public enum EHeatType : byte
{
    Coldest,
    Colder,
    Cold,
    Warm,
    Warmer,
    Warmest
}

public enum EHeightType : byte
{
    DeepWater,
    ShallowWater,
    Sand,
    Grass,Forest,
    Rock,
    Snow
}

public enum EMoistureType : byte
{
    Wettest,
    Wetter,
    Wet,
    Dry,
    Dryer,
    Dryest
}