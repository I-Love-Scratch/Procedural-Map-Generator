using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

using terrainGenerator.Generator;


namespace terrainGenerator {

	/*
		 Class for the state of the map.
	*/
	public class MapState : IMapState {
		public const string CURRENT_VERSION = "v0.3";
		public const int MAX_TILE_BIT_SIZE = 32; // Maximum number of bits a tile can use when saved.


		// Version number of the map.
		public string Version { get; set; } = CURRENT_VERSION;

		// Name of the map.
		public string Name { get; set; } = "ImABigDummyWhoForgotToNameMyMap";

		// Altitude of ocean surface.
		public float OceonLevel { get; set; } = Tile.OCEAN_ALT_CUTOFF;
		
		// Altitude of mountain base.
		public float MountainLevel { get; set; } = Tile.MOUNTAIN_ALT_CUTOFF;

		// Width of grid.
		public int Width { set; get; }
		// Height of grid.
		public int Height { set; get; }
		// The seed used for randomization.
		public int Seed { set; get; } 


		// Number of images the grid is stored in.
		public int GridImgCount { set; get; }


		// Controls what tile properties are saved and how many bits they each get.
		[JsonProperty]
		private Dictionary<TileProperty, int> TileStorageConfig { get; set; }


		/*
			Scaling factor for the map.
			NOTE: MapScalingFactor is not magical, each step generator needs to manually make use of it where applicable.
		*/
		public float MapScalingFactor { get; set; } = 1;

		/*
			The aspects of the map that have been defined.
		*/
		public MapAspects MapAspects { get; set; }

		/*
			The biomes of the map.
		*/
		public IReadOnlyDictionary<int, Biome> Biomes { get; set; }


		/*
			The resources of the map.
		*/
		public IReadOnlyDictionary<int, Resource> Resources { get; set; }

		/*
			Get a biome by id.
		*/
		public IBiome GetBiome(int id) => Biomes[id];




		/*
			The biomes of the map.
		*/
		IReadOnlyDictionary<int, IBiome> IMapState.Biomes
			=> Biomes.ToDictionary(pair => pair.Key, pair => (IBiome) pair.Value);



		// Controls what tile properties are saved and how many bits they each get.
		[JsonIgnore]
		IReadOnlyDictionary<TileProperty, int> IMapState.TileStorageConfig { 
			get => TileStorageConfig; 
			set {
				// Validate the new value.
				var err = IsValidTileStorageConfig(value);
				if(err != null) throw new ArgumentException(nameof(value), err);
				TileStorageConfig = new Dictionary<TileProperty, int>(value);
			} 
		}


		/*
			Checks if a TileStorageConfig is valid.
			Returns null if valid, otherwise a string with the reason.
			Checks: 
			- No property is allocated less than 1 bit.
			- Total size is within a certain amount.
			- The TileProperty.None is not used.
			- If biomes is set, it has enough room for the current number of biomes.
		*/
		public string IsValidTileStorageConfig(IReadOnlyDictionary<TileProperty, int> dict) {
			if(dict == null) return "The dictionary cannot be null";
			if(dict.ContainsKey(TileProperty.None)) return "Cannot store TileProperty.None";

			// Check that if we store biomes, we have enough space.
			if(dict.TryGetValue(TileProperty.Biome, out var biomeNum) && Biomes.Count > (1 << biomeNum) - 1) 
				return $"Cannot store biomes in {biomeNum} bits, that would only hold {(1 << biomeNum) - 1} biomes, map currently has {Biomes.Count}.";

			int sum = 0;
			foreach(var (key, val) in dict) {
				// If a property was allocated too few bits.
				if(val < 1) return $"Key {key} was allocated {val} bits, minimum is 1.";
				if(val > MAX_TILE_BIT_SIZE) return $"Key {key} was allocated {val} bits, max is {MAX_TILE_BIT_SIZE}.";
				sum += val;
			}

			// If we allocated too much space.
			//if(sum > MAX_TILE_BIT_SIZE) return $"You cannot allocate {sum} bits, the cap is {MAX_TILE_BIT_SIZE}.";

			return null;
		}


		/*
			Creates a MapState from an IGeneratorState.
		*/
		public static MapState Create(GeneratorState genState) {
			var res = new MapState {
				Name = genState.Name,
				OceonLevel = genState.OceonLevel,
				MountainLevel = genState.MountainLevel,
				Width = genState.Width,
				Height = genState.Height,
				Seed = genState.Seed,
				Biomes = genState.Biomes,
				Resources = genState.Resources,
				MapScalingFactor = genState.MapScalingFactor,
				MapAspects = genState.MapAspects,
				//TileStorageConfig = genState.TileStorageConfig,
			};
			
			var test = res.IsValidTileStorageConfig(genState.TileStorageConfig);
			if(test == null)
				res.TileStorageConfig = genState.TileStorageConfig;
			else Logger.Log(test);

			return res;
		}
	}

}
