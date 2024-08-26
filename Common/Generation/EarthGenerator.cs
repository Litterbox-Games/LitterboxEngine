using Common.Mathematics;
using Common.World;
using MoreLinq;

namespace Common.Generation;

public class EarthGenerator : IWorldGenerator
{
    private const int Seed = 132;

    private const float Frequency = 1.25f;
    private const int Octaves = 5;
    private const int Scale = 2;
    private const int HeatOctaves = 4;
    private const float HeatFrequency = 3.0f;
    private const int MoistureOctaves = 4;
    private const float MoistureFrequency = 3.0f;

    private readonly FastNoise _heightNoise;
    private readonly FastNoise _heatNoise;
    private readonly FastNoise _moistureNoise;

    public EarthGenerator()
    {
        _heightNoise = GenerateHeightNoise();
        _heatNoise = GenerateHeatNoise();
        _moistureNoise = GenerateMoistureNoise();
    }
        
    public ChunkData GenerateChunkAtPosition(Vector2i position)
    {
        var heightData = GetData(_heightNoise, position);
        var heatData = GetData(_heatNoise, position);
        heatData = AdjustHeatData(heatData, position);
        var moistureData = GetData(_moistureNoise, position);

        var heightTypes = GetHeightTypes(heightData);
        var heatTypes = GetHeatTypes(heatData);
        var moistureTypes = GetMoistureTypes(moistureData, heightData, heightTypes);
        var biomeTypes = GetBiomeTypes(heightTypes, moistureTypes, heatTypes);

        var result = new ChunkData(position)
        {
            BiomeArray = biomeTypes.Flatten().Cast<EBiomeType>().ToArray(),
            HeatArray = heatTypes.Flatten().Cast<EHeatType>().ToArray(),
            MoistureArray = moistureTypes.Flatten().Cast<EMoistureType>().ToArray()
        };

        // set tiles based on above data

        for (var x = 0; x < ChunkData.ChunkSize; x++)
        {
            for (var y = 0; y < ChunkData.ChunkSize; y++)
            {
                /*ushort value = biomeTypes[x, y] switch
                {
                    EBiomeType.Ice => _worldService.BlockMapping["ice"],
                    EBiomeType.BorealForest => _worldService.BlockMapping["boreal_forest"],
                    EBiomeType.Desert => _worldService.BlockMapping["desert"],
                    EBiomeType.Grassland => _worldService.BlockMapping["grassland"],
                    EBiomeType.SeasonalForest => _worldService.BlockMapping["seasonal_rainforest"],
                    EBiomeType.Tundra => _worldService.BlockMapping["tundra"],
                    EBiomeType.Savanna => _worldService.BlockMapping["savanna"],
                    EBiomeType.TemperateRainforest => _worldService.BlockMapping["temperate_rainforest"],
                    EBiomeType.TropicalRainforest => _worldService.BlockMapping["tropical_rainforest"],
                    EBiomeType.Woodland => _worldService.BlockMapping["woodland"],
                    EBiomeType.DeepOcean => _worldService.BlockMapping["deep_ocean"],
                    EBiomeType.Ocean => _worldService.BlockMapping["ocean"],
                    _ => 0
                };*/
                
                ushort value = biomeTypes[x, y] switch
                {
                    EBiomeType.Ice => 1,
                    EBiomeType.BorealForest => 2,
                    EBiomeType.Desert => 3,
                    EBiomeType.Grassland => 4,
                    EBiomeType.SeasonalForest => 5,
                    EBiomeType.Tundra => 6,
                    EBiomeType.Savanna => 7,
                    EBiomeType.TemperateRainforest => 8,
                    EBiomeType.TropicalRainforest => 9,
                    EBiomeType.Woodland => 10,
                    EBiomeType.DeepOcean => 11,
                    EBiomeType.Ocean => 12,
                    _ => 0
                };

                result.GroundArray[ChunkData.GetIndexFromLocalPosition(new Vector2i(x, y))] = value;
            }
        }

        return result;
    }
        
    private const float ColdestValue = 0.05f;
    private const float ColderValue = 0.18f;
    private const float ColdValue = 0.3f;
    private const float WarmValue = 0.4f;
    private const float WarmerValue = 0.5f;
        
    private EHeatType[,] GetHeatTypes(float[,] heatData)
    {
        var heatTypes = new EHeatType[ChunkData.ChunkSize, ChunkData.ChunkSize];

        for (var x = 0; x < ChunkData.ChunkSize; x++)
        {
            for (var y = 0; y < ChunkData.ChunkSize; y++)
            {
                var heatValue = heatData[x, y];

                heatTypes[x, y] = heatValue switch
                {
                    < ColdestValue => EHeatType.Coldest,
                    < ColderValue => EHeatType.Colder,
                    < ColdValue => EHeatType.Cold,
                    < WarmValue => EHeatType.Warm,
                    < WarmerValue => EHeatType.Warmer,
                    _ => EHeatType.Warmest
                };
            }
        }

        return heatTypes;
    }
        
    private const float DeepWater = 0.45f;
    private const float ShallowWater = 0.5f;
    private const float Sand = 0.525f;
    private const float Grass = 0.55f;
    private const float Forest = 0.6f;
    private const float Rock = 0.65f;
    private const float Snow = 0.7f;

    private EHeightType[,] GetHeightTypes(float[,] heightData)
    {
        var heightTypes = new EHeightType[ChunkData.ChunkSize, ChunkData.ChunkSize];

        for (var x = 0; x < ChunkData.ChunkSize; x++)
        {
            for (var y = 0; y < ChunkData.ChunkSize; y++)
            {
                var heightValue = heightData[x, y];

                heightTypes[x, y] = heightValue switch
                {
                    < DeepWater => EHeightType.DeepWater,
                    < ShallowWater => EHeightType.ShallowWater,
                    < Sand => EHeightType.Sand,
                    < Grass => EHeightType.Grass,
                    < Forest => EHeightType.Forest,
                    < Rock => EHeightType.Rock,
                    _ => EHeightType.Snow
                };
            }
        }

        return heightTypes;
    }
        
    private const float DryerValue = 0.35f;
    private const float DryValue = 0.5f;
    private const float WetValue = 0.55f;
    private const float WetterValue = 0.6f;
    private const float WettestValue = 0.8f;

    private EMoistureType[,] GetMoistureTypes(float[,] moistureData, float[,] heightData, EHeightType[,] heightTypes)
    {
        var moistureTypes = new EMoistureType[ChunkData.ChunkSize, ChunkData.ChunkSize];

        for (var x = 0; x < ChunkData.ChunkSize; x++)
        {
            for (var y = 0; y < ChunkData.ChunkSize; y++)
            {
                //Moisture Map Analyze  
                var moistureValue = moistureData[x, y];
                //moistureValue = (moistureValue - MoistureData.Min) / (MoistureData.Max - MoistureData.Min);

                switch (heightTypes[x,y])
                {
                    //adjust moisture value based on proximity to oceans/lakes
                    case EHeightType.DeepWater:
                        moistureValue += 8f * heightData[x, y];
                        break;
                    case EHeightType.ShallowWater:
                        moistureValue += 3f * heightData[x, y];
                        break;
                    case EHeightType.Sand:
                        moistureValue += 0.25f * heightData[x,y];
                        break;
                }

                moistureTypes[x, y] = moistureValue switch
                {
                    //set moisture type
                    < DryerValue => EMoistureType.Driest,
                    < DryValue => EMoistureType.Dryer,
                    < WetValue => EMoistureType.Dry,
                    < WetterValue => EMoistureType.Wet,
                    < WettestValue => EMoistureType.Wetter,
                    _ => EMoistureType.Wettest
                };
            }
        }

        return moistureTypes;
    }

    private readonly EBiomeType[,] _biomeTable = new EBiomeType[6, 6] {   
        //COLDEST         //COLDER           //COLD                   //HOT                           //HOTTER                        //HOTTEST
        { EBiomeType.Ice, EBiomeType.Tundra, EBiomeType.Grassland,    EBiomeType.Desert,              EBiomeType.Desert,              EBiomeType.Desert },              //DRYEST
        { EBiomeType.Ice, EBiomeType.Tundra, EBiomeType.Grassland,    EBiomeType.Desert,              EBiomeType.Desert,              EBiomeType.Desert },              //DRYER
        { EBiomeType.Ice, EBiomeType.Tundra, EBiomeType.Woodland,     EBiomeType.Woodland,            EBiomeType.Savanna,             EBiomeType.Savanna },             //DRY
        { EBiomeType.Ice, EBiomeType.Tundra, EBiomeType.BorealForest, EBiomeType.Woodland,            EBiomeType.Savanna,             EBiomeType.Savanna },             //WET
        { EBiomeType.Ice, EBiomeType.Tundra, EBiomeType.BorealForest, EBiomeType.SeasonalForest,      EBiomeType.TropicalRainforest,  EBiomeType.TropicalRainforest },  //WETTER
        { EBiomeType.Ice, EBiomeType.Tundra, EBiomeType.BorealForest, EBiomeType.TemperateRainforest, EBiomeType.TropicalRainforest,  EBiomeType.TropicalRainforest }   //WETTEST
    };

    private EBiomeType BiomeTableLookUp(EMoistureType moistureType, EHeatType heatType)
    {
        return _biomeTable[(int)moistureType, (int)heatType];
    }
        
    private EBiomeType[,] GetBiomeTypes(EHeightType[,] heightTypes, EMoistureType[,] moistureTypes, EHeatType[,] heatTypes)
    {
        var biomeTypes = new EBiomeType[ChunkData.ChunkSize, ChunkData.ChunkSize];
        for (var x = 0; x < ChunkData.ChunkSize; x++)
        {
            for (var y = 0; y < ChunkData.ChunkSize; y++)
            {
                biomeTypes[x, y] = heightTypes[x, y] switch
                {
                    EHeightType.DeepWater => EBiomeType.DeepOcean,
                    EHeightType.ShallowWater => EBiomeType.Ocean,
                    _ => BiomeTableLookUp(moistureTypes[x, y], heatTypes[x, y])
                };
            }
        }

        return biomeTypes;
    }
        
        
    private float[,] GetData(FastNoise heightMap, Vector2i chunkPosition)
    {
        var data = new float[ChunkData.ChunkSize, ChunkData.ChunkSize];

        for (var x = 0; x < ChunkData.ChunkSize; x++)
        {
            for (var y = 0; y < ChunkData.ChunkSize; y++)
            {
                // Noise range
                float x1 = 0, x2 = 2;
                float y1 = 0, y2 = 2;
                var dx = x2 - x1;
                var dy = y2 - y1;

                // Sample noise at smaller intervals
                var s = (chunkPosition.X * ChunkData.ChunkSize + x) / ((float)IWorldService.WorldSize * ChunkData.ChunkSize);
                var t = (chunkPosition.Y * ChunkData.ChunkSize + y) / ((float)IWorldService.WorldSize * ChunkData.ChunkSize);

                // Calculate our 4D coordinates
                var nx = x1 + (float)Math.Cos(s * 2 * Math.PI) * dx / (float)(2 * Math.PI);
                var ny = y1 + (float)Math.Cos(t * 2 * Math.PI) * dy / (float)(2 * Math.PI);
                var nz = x1 + (float)Math.Sin(s * 2 * Math.PI) * dx / (float)(2 * Math.PI);
                var nw = y1 + (float)Math.Sin(t * 2 * Math.PI) * dy / (float)(2 * Math.PI);

                var heightValue = heightMap.GetSimplexFractal(nx * Scale, ny * Scale, nz * Scale, nw * Scale);
                heightValue = heightValue.Map(-1, 1, 0, 1);

                data[x, y] = heightValue;
            }
        }

        return data;
    }

    private float[,] AdjustHeatData(float[,] heatData, Vector2i chunkPosition)
    {
        for (var x = 0; x < ChunkData.ChunkSize; x++)
        {
            for (var y = 0; y < ChunkData.ChunkSize; y++)
            {
                heatData[x, y] *= SampleHeatWave((chunkPosition.Y * ChunkData.ChunkSize + y) / ((float)IWorldService.WorldSize * ChunkData.ChunkSize));
            }
        }

        return heatData;
    }

    private float SampleHeatWave(float y)
    {
        return ((-1.0f * MathF.Cos(4.0f * MathF.PI * y) + 1.0f) / 2.0f);
    }
        
    private FastNoise GenerateHeightNoise()
    {
        var heightMap = new FastNoise();
        heightMap.SetFractalType(FastNoise.FractalType.FBM);
        heightMap.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
        heightMap.SetInterp(FastNoise.Interp.Quintic);
        heightMap.SetFrequency(Frequency);
        heightMap.SetFractalOctaves(Octaves);
        heightMap.SetSeed(Seed);  //add once we want to do stuff with seeds

        return heightMap;
    }
    private FastNoise GenerateHeatNoise()
    {
        var heatMap = new FastNoise();
        heatMap.SetFractalType(FastNoise.FractalType.FBM);
        heatMap.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
        heatMap.SetInterp(FastNoise.Interp.Quintic);
        heatMap.SetFrequency(HeatFrequency);
        heatMap.SetFractalOctaves(HeatOctaves);
        heatMap.SetSeed(Seed);  //add once we want to do stuff with seeds

        return heatMap;
    }
    private FastNoise GenerateMoistureNoise()
    {
        var moistureMap = new FastNoise();
        moistureMap.SetFractalType(FastNoise.FractalType.FBM);
        moistureMap.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
        moistureMap.SetInterp(FastNoise.Interp.Quintic);
        moistureMap.SetFrequency(MoistureFrequency);
        moistureMap.SetFractalOctaves(MoistureOctaves);
        moistureMap.SetSeed(Seed);

        return moistureMap;
    }
}