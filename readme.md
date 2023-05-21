# Procedural Map Generator


## What is this
This is a program that can procedurally generate terrains in unity.
It has been designed to be flexible and easy to extend.
This is the result of a bachelor project that we decided to publish as open source.
It also comes with a simple UI tool to try out different generators in real time. However this is only a simple tool for trying things out.
The intended way to use this is to integrate it into another project that can use this to generate maps.


## Installation
1) Download the code.
2) Open the "unity" folder as a project in unity.
3) Go to Assets>Scenes and open "DisplayScene"

From here, you can change what map to load and how maps are displayed in the Grid object and you can customize your own generator with RuntimeGenerator and run it using the big button that says "Generate", tho you might need to expand the screen to see it.


## How to use

An example of a generator parameter is as follows:
```cs
var par = new GenParams {
  Width = 257,
  Height = 257,
  Seed = 50_999,
  TileStorageConfig = new Dictionary<TileProperty, int> {
    { TileProperty.Biome, 8 },
    { TileProperty.Altitude, 8 },
    { TileProperty.Temperature, 8 },
    { TileProperty.Humidity, 8 },
    { TileProperty.WindMagnitude, 8 },
    { TileProperty.WindDirection, 8 },
  },
};
```

It can be used to create a ChainMapGenerator like this:
```cs
var gen = new ChainMapGenerator(new List<IStepGenerator>{
  new PDSAltitudeGenerator(2, 9),
  new AltitudeAverageSmoothener(3, 3, false),
  new BalancedOceanMountianGenerator(0.2f, 0.05f, 50),
  new RiverLakeGenerator(4, 2),
  new FlowingWindGenerator(),
  new DiamondSquareTemperatureHumidityGenerator(),
  new FactorBasedBiomeGenerator(),
  new CellularAutomataSmoothener(2, 2),
  new FloodFillResourceGenerator(),
}, par);
```

Generators can be used like this:
```cs
// Generate a map.
var map = gen.Generate();

// Create various images depicting different aspects of the map.
var pic1 = map.ToPrettyImage();
var pic2 = map.ToAltitudeMap();
var pic3 = map.ToHumidityMap();
var pic4 = map.ToTemperatureMap();
var pic5 = map.ToFactorMap();
var pic6 = map.ToResourceMap();
var pic7 = map.ToWindMap();

// Parsing a map.
var mapParser = new GameMapParser();
mapParser.Encode(map); // Store in file.
var map2 = mapParser.Decode(name); // Retrieve map from file by name.
```

You can also create a generator that iterates on an existing map:
```cs
var gen = new ChainMapGenerator(new List<IStepGenerator>{
  new AltitudeAverageSmoothener(3, 3, false),
  new CellularAutomataSmoothener(2, 2),
}, par, map); 
// By passing a map, it will be used as the basis.
// Here we iterate on an existing map by smoothening it a little.
var map2 = gen.Generate();
```

If none of the algorithms provided in this library do exactly what you need, you can create your own step generator that you can design. A good starting point would be to copy an existing one and edit it to fill your needs. Just remember to set the required and added map aspects so the program understands properly what your step generator needs and what it adds.


## File structure
- Unity
  - Assets
    - Code - Scripts for using the main library in unity.
    - Core/src - The source code for the main library.
      - Core       - The core data classes.
      - generators - The generators used to create maps.
      - Parser     - The code related to converting maps and generators into files and back.
      - Test       - The code used for testing and benchmarking.
      - Utilities  - Utility and extension classes.
  - Data
    - Maps   - Maps are stored here by default.
    - Prints - Images displaying the map are stored here by default.
    - Test   - Other various data files are stored here, such as benchmarks.

## Some benchmarks
```
=========================================================================================================================================
Sizes                             |           129 |           257 |           513 |          1025 |          2049 |          4097 | Notes
-----------------------------------------------------------------------------------------------------------------------------------------
Altitude_Perlin                   | 00:00:00.0404 | 00:00:00.1579 | 00:00:00.6308 | 00:00:02.5724 | 00:00:10.2935 | 00:00:41.2164 | 
Altitude_DS                       | 00:00:00.0014 | 00:00:00.0059 | 00:00:00.0219 | 00:00:00.0988 | 00:00:00.4064 | 00:00:01.7963 | 
Altitude_PDS                      | 00:00:00.0414 | 00:00:00.1617 | 00:00:00.6451 | 00:00:02.5819 | 00:00:10.3698 | 00:00:41.5147 | 
AltSmooth_ThermalErosion          | 00:00:00.0109 | 00:00:00.0349 | 00:00:00.1134 | 00:00:00.4236 | 00:00:01.7354 | 00:00:06.9923 | 
AltSmooth_Average                 | 00:00:00.0239 | 00:00:00.1004 | 00:00:00.3724 | 00:00:01.5063 | 00:00:06.3431 | 00:00:25.5408 | 
AltSmooth_Varied                  | 00:00:00.0374 | 00:00:00.1208 | 00:00:00.5046 | 00:00:02.0802 | 00:00:07.1933 | 00:00:30.7917 | 
OceanMountain_Raw                 | 00:00:00.0044 | 00:00:00.0177 | 00:00:00.0594 | 00:00:00.1801 | 00:00:00.5907 | 00:00:02.2922 | 
OceanMountain_Balanced            | 00:00:00.0055 | 00:00:00.0167 | 00:00:00.0598 | 00:00:00.2344 | 00:00:00.9807 | 00:00:03.8557 | 
RiverLake_Closest                 | 00:00:00.0034 | 00:00:00.0029 | 00:00:00.0109 | 00:00:00.0308 | 00:00:00.0283 | 00:00:00.3723 | 
RiverLake                         | 00:00:00.0054 | 00:00:00.0187 | 00:00:00.0224 | 00:00:00.0428 | 00:00:00.0260 | 00:00:00.0204 | 
Wind_Flowing                      | 00:00:00.0235 | 00:00:00.0841 | 00:00:00.3221 | 00:00:01.2856 | 00:00:05.1084 | 00:00:20.7990 | 
Wind_LocalTiles                   | 00:00:00.0458 | 00:00:00.1857 | 00:00:00.7363 | 00:00:02.9863 | 00:00:11.7883 | 00:00:47.6435 | 
TemperatureHumidity_Perlin        | 00:00:00.0784 | 00:00:00.3138 | 00:00:01.2538 | 00:00:05.0217 | 00:00:20.0758 | 00:01:20.1058 | 
TemperatureHumidity_DS            | 00:00:00.0034 | 00:00:00.0119 | 00:00:00.0566 | 00:00:00.2498 | 00:00:01.0422 | 00:00:04.7275 | 
TemperatureHumidity_Environmental | 00:00:00.6163 | 00:00:02.3647 | 00:00:06.1541 | 00:00:21.2583 | 00:01:27.7561 | 00:03:13.0464 | Range: 160, Chunks: 2
TemperatureHumidity_Environmental | 00:00:00.2316 | 00:00:00.8824 | 00:00:03.1257 | 00:00:11.3351 | 00:00:44.7974 | 00:01:59.7566 | Range: 160, Chunks: 4
TemperatureHumidity_Environmental | 00:00:00.2034 | 00:00:00.7604 | 00:00:02.8613 | 00:00:10.5237 | 00:00:41.6887 | 00:01:53.6192 | Range: 160, Chunks: 6
Biome_Expanding                   | 00:00:02.2652 | 00:00:08.4919 | 00:00:34.4579 | 00:02:17.3858 | 00:09:08.0226 | 00:37:22.5081 | 
Biome_FactorBased                 | 00:00:00.0114 | 00:00:00.0319 | 00:00:00.1844 | 00:00:01.4159 | 00:00:17.9109 | 00:05:20.2790 | 
Biome_AltitudeBased               | 00:00:00.0074 | 00:00:00.0229 | 00:00:00.1583 | 00:00:01.0643 | 00:00:12.8843 | 00:03:42.5472 | 
Biome_OneDNoise                   | 00:00:00.0498 | 00:00:00.2003 | 00:00:00.7396 | 00:00:03.3404 | 00:00:19.4218 | 00:03:29.6349 | 
Smoothening_CellularAutomata      | 00:00:00.2262 | 00:00:00.8338 | 00:00:03.3889 | 00:00:13.7728 | 00:00:54.1154 | 00:03:38.6501 | 
Resource_FloodFill                | 00:00:00.0029 | 00:00:00.0000 | 00:00:00.0000 | 00:00:00.0039 | 00:00:00.0040 | 00:00:00.0059 | 
Resource_Primitive                | 00:00:00.0014 | 00:00:00.0000 | 00:00:00.0000 | 00:00:00.0000 | 00:00:00.0009 | 00:00:00.0000 | 
-----------------------------------------------------------------------------------------------------------------------------------------
Total                             | 00:00:03.9422 | 00:00:14.8225 | 00:00:55.8806 | 00:03:39.3960 | 00:15:02.5848 | 01:05:47.7172 
```
