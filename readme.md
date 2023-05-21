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

