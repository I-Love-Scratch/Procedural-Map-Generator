using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.IO;

using terrainGenerator.Generator;
using terrainGenerator.Test;
using UnityEngine;

namespace terrainGenerator
{
	/*
		Main program
	*/
	public partial class Program {
		/*
			Starting point of the program
		*/
		public static void Main(string[] args) {

			var seed = 69_420;
			Demo(seed);

			Logger.Log("done");
			Console.ReadLine(); 
		}

		/*
			Demo function
		*/
		static void Demo(int seed) {

			//make map parameters
			var par = new GenParams {

				Name = "DemoMap",
				Width = 257,
				Height = 257,
				Seed = seed,

				TileStorageConfig = new Dictionary<TileProperty, int> {
					{ TileProperty.Biome,           8 },
					{ TileProperty.Altitude,        8 },
					{ TileProperty.Temperature,     8 },
					{ TileProperty.Humidity,        8 },
					{ TileProperty.WindMagnitude,   8 },
					{ TileProperty.WindDirection,   8 },
				},
			};

			//make map generator
			var gen = new ChainMapGenerator(new List<IStepGenerator>{
					new PDSAltitudeGenerator(2, 9),
					new BalancedOceanMountianGenerator(0.2f, 0.05f, 50),
					new RiverLakeGenerator(4, 2),
					new FlowingWindGenerator(),
					new DiamondSquareTemperatureHumidityGenerator(),
					new FactorBasedBiomeGenerator(),
					new CellularAutomataSmoothener(2, 2),
					new FloodFillResourceGenerator(),
				}, par);

			//generate map
			var map = gen.Generate();

			//save to file
			SaveMap(map);
		}







		/*
			Generate a number of maps for testing.
		*/
		static void TestMaps(int seed, int testCount) {

			//make grid
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

			// Test with randomized seeds.
			for(int i = 0; i < testCount; ++i) {
				//par.Seed = rnd.Next(0, 99_999);
				par.Name = $"TestMap_{i}";
				var size = ((par.Width - 1) << 1) + 1;
				par.Width = par.Height = size;

				//var gen = new GeneratorBuilder(par) {
				//	AltitudeGenerator = new PDSAltitudeGenerator(2, 9),
				//	AltitudeSmoothenerGenerator = new AltitudeAverageSmoothener(3, 3, false),
				//	RiverLakeGenerator = new RiverLakeGenerator(4, 2),
				//	OceanMountianGenerator = new BalancedOceanMountianGenerator(0.2f, 0.05f, 50),
				//	WindGenerator = new FlowingWindGenerator(),
				//	TemperatureHumidityGenerator = new DiamondSquareTemperatureHumidityGenerator(),
				//	BiomeGenerator = new FactorBasedBiomeGenerator(),
				//	MapSmoothenerGenerator = new CellularAutomataSmoothener(2, 2),
				//	ResourceGenerator = new FloodFillResourceGenerator(),
				//}.Build();

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


				var map = gen.Generate();


				SaveMap(map.ToPrettyImage(), $"TestMap_{i}");
				SaveMap(map.ToAltitudeMap(), $"TestMap_Altitude_{i}");
				SaveMap(map.ToHumidityMap(), $"TestMap_Humidity_{i}");
				SaveMap(map.ToTemperatureMap(), $"TestMap_Temperature_{i}");
				SaveMap(map.ToFactorMap(), $"TestMap_Factor_{i}");
				SaveMap(map.ToResourceMap(), $"TestMap_Resource_{i}");
				SaveMap(map.ToWindMap(), $"TestMap_Wind_{i}");
				SaveMap(map);
			}
		}


		/*
			Tests out saving and loading generators.
		*/
		static void TestGeneratorSaving() {
			
			var par = new GenParams {
			Name = "Test1",
				Width = 513, // 129, 513, 1025
				Height = 513,
				Seed = 9999,//rnd.Next(0, 99_999),
			}; 

			var gen = new GeneratorBuilder(par) {
				AltitudeGenerator = new PDSAltitudeGenerator(2, 9),
				AltitudeSmoothenerGenerator = new AltitudeAverageSmoothener(
					new List<AltitudeAverageSmoothener.Par> {
						new AltitudeAverageSmoothener.Par(2, 5, true),
						new AltitudeAverageSmoothener.Par(3, 2, true),
						new AltitudeAverageSmoothener.Par(3, 1, false),
					}),
				OceanMountianGenerator = new BalancedOceanMountianGenerator(0.2f, 0.05f, 50),
				TemperatureHumidityGenerator = new EnvironmentalTemperatureHumidityGenerator(160, 6, dispersionSteps: 2, dispersionRange: 4, dispersionProp: 0.15f),
				BiomeGenerator = new FactorBasedBiomeGenerator(GenerationUtils.BIOME_MATRIX_SIMPLIFIED_2),
			};

			var name = _mapGeneratorParser.EncodeFile(gen);
			var gen2 = _mapGeneratorParser.DecodeFile(name);
			var name2 = _mapGeneratorParser.EncodeFile(gen2, "Test2");
		}

		/*
			Runs some benchmarking tests.
		*/
		static void TestBenchmark(int seed, int iterations = 3, string filename = "benchmark.txt") {
			var par = new GenParams {
				Width = 257, // 33, 65, 129, 513, 1025
				Height = 257,
				Seed = 99_999,//rnd.Next(0, 99_999),
				TileStorageConfig = new Dictionary<TileProperty, int> {
					{ TileProperty.Biome, 6 },
					{ TileProperty.Altitude, 8 },
					{ TileProperty.Temperature, 6 },
					{ TileProperty.Humidity, 6 },
					{ TileProperty.WindMagnitude, 6 },
					{ TileProperty.WindDirection, 6 },
				},
			};


			var steps = new List<StepGenTest>{
				new StepGenTest(new PerlinAltitudeGenerator(), "Altitude_Perlin"),
				new StepGenTest(new DiamondSquareAltitudeGenerator(), "Altitude_DS"),
				new StepGenTest(new PDSAltitudeGenerator(2, 9), "Altitude_PDS"),

				new StepGenTest(new ThermalErosionSmoothener(1, 0.01f), "AltSmooth_ThermalErosion"),
				new StepGenTest(new AltitudeAverageSmoothener(2, 2, true), "AltSmooth_Average"),
				new StepGenTest(new VariedAltitudeSmoothener(), "AltSmooth_Varied"),

				new StepGenTest(new RawOceanMountianGenerator(), "OceanMountain_Raw"),
				new StepGenTest(new BalancedOceanMountianGenerator(0.2f, 0.05f, 50), "OceanMountain_Balanced"),

				new StepGenTest(new ClosestOceanRiverGenerator(), "RiverLake_Closest"),
				new StepGenTest(new RiverLakeGenerator(), "RiverLake"),

				new StepGenTest(new FlowingWindGenerator(), "Wind_Flowing"),
				new StepGenTest(new LocalTilesWindGenerator(), "Wind_LocalTiles"),

				new StepGenTest(new PerlinTemperatureHumidityGenerator(), "TemperatureHumidity_Perlin"),
				new StepGenTest(new DiamondSquareTemperatureHumidityGenerator(), "TemperatureHumidity_DS"),
				new StepGenTest(new EnvironmentalTemperatureHumidityGenerator(160, 2), "TemperatureHumidity_Environmental", "Range: 160, Chunks: 2"),
				new StepGenTest(new EnvironmentalTemperatureHumidityGenerator(160, 4), "TemperatureHumidity_Environmental", "Range: 160, Chunks: 4"),
				new StepGenTest(new EnvironmentalTemperatureHumidityGenerator(160, 6), "TemperatureHumidity_Environmental", "Range: 160, Chunks: 6"),

				new StepGenTest(new ExpandingBiomeGenerator(),     "Biome_Expanding"),
				new StepGenTest(new FactorBasedBiomeGenerator(),   "Biome_FactorBased"),
				new StepGenTest(new AltitudeBasedBiomeGenerator(), "Biome_AltitudeBased"),
				new StepGenTest(new OneDNoiseBiomeGenerator(),     "Biome_OneDNoise"),

				new StepGenTest(new CellularAutomataSmoothener(2, 2), "Smoothening_CellularAutomata"),

				new StepGenTest(new FloodFillResourceGenerator(), "Resource_FloodFill"),
			};


			var sizes = new List<int> { 129, 257, 513 };//, 513, 1025, 2049, 4097, 8193


			string res = new GenTest().Benchmark(steps, sizes, par, seed, iterations);

			// Save the results.
			using(StreamWriter sw = File.CreateText($"{TEST_ROOT}/{filename}")) {
				sw.WriteLine(res);
			}
		}


	}

	public partial class Program {
		// Rood directory for prints
		public const string PRINT_ROOT = "Data/Prints";
		public const string TEST_ROOT = "Data/Test";

		// Parser for map generators.
		static readonly MapGeneratorParser _mapGeneratorParser = new MapGeneratorParser();
		// Parser for maps.
		static readonly GameMapParser _mapParser = new GameMapParser();


		/*
			Saves a picture of the map
		*/
		static void SaveMap(Texture2D pic, string name) {
			var data = ImageConversion.EncodeToPNG(pic);
			var filename = $"{PRINT_ROOT}/{name}.png";
			File.WriteAllBytes(filename, data);
			Logger.Log($"[SaveMap()] Saved: {filename}");
		}

		/*
			Save a map.
		*/
		static void SaveMap(GameMap map) {
			_mapParser.Encode(map);
			Logger.Log($"[SaveMap()] Saved: maps/{map.State.Name}");
		}

		/*
			Load a map.
		*/
		static GameMap LoadMap(string name) {
			var map = _mapParser.Decode(name);
			Logger.Log($"[LoadMap()] Loaded: maps/{name}");
			return map;
		}
	}

}
