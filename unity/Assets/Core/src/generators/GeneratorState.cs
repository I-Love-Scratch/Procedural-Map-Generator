using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace terrainGenerator.Generator {

	/*
		Represents the overall state of the map being generated.
	*/
	public interface IGeneratorState {
		// The name of the map.
		string Name { get; }

		// The width of grid.
		int Width { get; }

		// The height of grid.
		int Height { get; }

		// The initial seed.
		int Seed { get; } 


		/*
			Scaling factor for the map.
			NOTE: MapScalingFactor is not magical, each step generator needs to manually make use of it where applicable.
		*/
		float MapScalingFactor { get; }

		// Sea level of the map.
		float OceonLevel { get; set; }
		// Mountain base of the map.
		float MountainLevel { get; set; }

		// If true, the biome cap will be raised whenever it is exceeded, automatically changing how much capacity to reserve.
		bool DynamicBiomeCap { get; }

		// The biomes found on the map.
		IReadOnlyDictionary<int, IBiome> Biomes { get; }

		// The maximum number of biomes on the map.
		int BiomeCap { get; }

		// The aspects of the map that have been generated so far.
		MapAspects MapAspects { get; }

		/*
			Add a new biome of the given type to the dictionary.
		*/
		IBiome AddBiome(BiomeType type);

		/*
			Get the first instance of a given biome type.
			If no biome of the given type was found, add one.
		*/
		IBiome GetDefaultBiome(BiomeType type);

		/*
			Get the biome of a given id.
		*/
		IBiome GetBiome(int id);
		/*
		 *	Add a resource blob instance to the map
		 */
		IResource AddResource(ResourceType type);
		/*
		 *	Get resource from id
		 */
		IResource GetResource(int id);


		/*
			Informes the state that the following aspects have been applied to the map.
		*/
		void ApplyAspects(MapAspects aspects);
	}

	/*
		Contains information about the state of the generator.
	*/
	public class GeneratorState : IGeneratorState {
		public const float MIN_SCALING_FACTOR = 0.25f;
		public const float SCALING_FACTOR_STANDARD_UNIT = 512 * 512;

		// Max cap for number of biomes.
		public const int MAX_BIOME_CAP = (1 << 20) - 1;

		// Default tile storage configurations.
		public static Dictionary<TileProperty, int> DEFAULT_TILE_STORAGE_CONFIG 
			=> GenParams.DEFAULT_TILE_STORAGE_CONFIG;

		// The name of the map.
		public string Name { get; set; } = "ImABigDummyWhoForgotToNameMyMap";

		//  The width of grid.
		public int Width { set; get; } 

		// The height of grid.
		public int Height { set; get; } 

		// The initial seed.
		public int Seed { set; get; }


		/*
			Scaling factor for the map.
			NOTE: MapScalingFactor is not magical, each step generator needs to manually make use of it where applicable.
		*/
		public float MapScalingFactor { get; set; } = 1;

		// Sea level of the map.
		public float OceonLevel { get; set; } = Tile.OCEAN_ALT_CUTOFF;
		// Mountain base of the map.
		public float MountainLevel { get; set; } = Tile.MOUNTAIN_ALT_CUTOFF;

		// If true, the biome cap will be raised whenever it is exceeded, automatically changing how much capacity to reserve.
		public bool DynamicBiomeCap { get; set; }


		// The biomes found on the map.
		public Dictionary<int, Biome> Biomes { get; set; } = new Dictionary<int, Biome> {
			{ 0, Biome.DEFAULT },
		};

		// The maximum number of biomes on the map.
		public int BiomeCap { get; private set; }

		// The aspects of the map that have been generated so far.
		public MapAspects MapAspects { get; private set; }
		

		// Controls what tile properties are saved and how many bits they each get.
		public Dictionary<TileProperty, int> TileStorageConfig { get; private set; }


		// The biomes found on the map.
		IReadOnlyDictionary<int, IBiome> IGeneratorState.Biomes 
			=> (IReadOnlyDictionary<int, IBiome>) Biomes.ToDictionary(pair => pair.Key, pair => (IBiome) pair.Value);

		/*
			All resource blobs present on the map and their ids
		*/
		public Dictionary<int, Resource> Resources = new Dictionary<int, Resource>();



		/*
			Create a GeneratorState from GenParams.
		*/
		public static GeneratorState Create(GenParams par) {

			var res = new GeneratorState {
				Name = par.Name,
				Width = par.Width,
				Height = par.Height,
				Seed = par.Seed,
				TileStorageConfig = par.TileStorageConfig ?? DEFAULT_TILE_STORAGE_CONFIG,
				DynamicBiomeCap = par.DynamicBiomeCap,
			};

			// If seed is -1, replace it with a random number.
			if(res.Seed == -1) res.Seed = new Random().Next(0, int.MaxValue);

			if(res.TileStorageConfig.TryGetValue(TileProperty.Biome, out var num)) {
				if(num < GridParser_ImageArray.ENCODED_BIOME_BITS) {
					num = res.TileStorageConfig[TileProperty.Biome] = GridParser_ImageArray.ENCODED_BIOME_BITS;
				}
			} else {
				num = GridParser_ImageArray.ENCODED_BIOME_BITS;
			}
			res.BiomeCap = (1 << num) - 1;

			// Get scaling factor.
			var scaler = par.MapScalingFactor;

			// If scaling factor isn't set: calculate it.
			if(scaler < 0f) scaler = (par.Width * par.Height) / SCALING_FACTOR_STANDARD_UNIT;

			// Set it.
			res.MapScalingFactor = Math.Max(MIN_SCALING_FACTOR, scaler);

			return res;
		}

		/*
			Create a GeneratorState from GenParams and the state of a basis map.
		*/
		public static GeneratorState Create(GenParams par, MapState state) {

			var res = new GeneratorState {
				Name = par.Name ?? state.Name,
				Width = state.Width,
				Height = state.Height,
				Seed = par.Seed,
				TileStorageConfig = par.TileStorageConfig ?? DEFAULT_TILE_STORAGE_CONFIG,

				OceonLevel = state.OceonLevel,
				MountainLevel = state.MountainLevel,

				Biomes = new Dictionary<int, Biome>(state.Biomes),
				MapAspects = state.MapAspects,
				DynamicBiomeCap = par.DynamicBiomeCap,
			};

			// If seed is -1, replace it with a random number.
			if(res.Seed == -1) res.Seed = new Random().Next(0, int.MaxValue);

			// Get the biome cap and propery for how many bits to allocate to biomes.
			var numMin = (int) MathF.Max(GridParser_ImageArray.ENCODED_BIOME_BITS, MathF.Ceiling(MathF.Log(res.Biomes.Count + 1, 2)));
			if(res.TileStorageConfig.TryGetValue(TileProperty.Biome, out var num)) {
				if(num < numMin) {
					num = res.TileStorageConfig[TileProperty.Biome] = numMin;
				}
			} else {
				num = numMin;
			}
			res.BiomeCap = (1 << num) - 1;

			// Get scaling factor.
			var scaler = par.MapScalingFactor;

			// If scaling factor isn't set: calculate it.
			if(scaler < 0f) scaler = (par.Width * par.Height) / SCALING_FACTOR_STANDARD_UNIT;

			// Set it.
			res.MapScalingFactor = Math.Max(MIN_SCALING_FACTOR, scaler);

			return res;
		}


		/*
			Add a new biome of the given type to the dictionary.
		*/
		public IBiome AddBiome(BiomeType type) {
			// Check that we have room for another biome.
			if(Biomes.Count >= BiomeCap) {
				if(!DynamicBiomeCap) 
					throw new InvalidOperationException($"The biome list is already full. ({Biomes.Count}/{BiomeCap})");

				if(BiomeCap >= MAX_BIOME_CAP)
					new InvalidOperationException($"The biome cap has been reached. ({BiomeCap}/{MAX_BIOME_CAP})");
					
				BiomeCap = BiomeCap * 2 + 1;
				if(TileStorageConfig.TryGetValue(TileProperty.Biome, out var num)) 
					TileStorageConfig[TileProperty.Biome] = num + 1;
			}

			// Make new biome
			//var biome = new Biome(type.GetObj()) {
			//	Id = Biomes.Count,
			//};
			//var typeObj = type.GetObj();
			//var biome = type switch {
			//	BiomeType.Lake => new LakeBiome(typeObj),
			//	_ => new Biome(typeObj),
			//};
			var biome = Biome.Create(type);
			biome.Id = Biomes.Count;

			// Add to dict
			Biomes.Add(biome.Id, biome);

			// Return
			return biome;
		}

		/*
			Get the first instance of a given biome type.
			If no biome of the given type was found, add one.
		*/
		public IBiome GetDefaultBiome(BiomeType type) {
			return Biomes.Values.FirstOrDefault(b => b.BiomeType == type) ?? AddBiome(type);
		}

		/*
			Get the biome of a given id.
		*/
		public IBiome GetBiome(int id) {
			return Biomes[id];
		}

		/*
			Add a resource blob instance to the map
		*/
		public IResource AddResource(ResourceType type) {
			var id = Resources.Count;
			var res = new Resource(id, type);
			Resources.Add(id, res);
			return res;
		}

		/*
			Get resource from id
		*/
		public IResource GetResource(int id)
			=> Resources[id];


		/*
			Informes the state that the following aspects have been applied to the map.
		*/
		public void ApplyAspects(MapAspects aspects)
			=> MapAspects |= aspects;
	}
}