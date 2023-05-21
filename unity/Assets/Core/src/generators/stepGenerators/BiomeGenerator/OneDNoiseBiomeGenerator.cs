using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Generates map biomes using 1D noise, not factors.
	*/
	[Serializable]
	public class OneDNoiseBiomeGenerator : StepGeneratorBase, IBiomeGenerator {
		[SerializeField]
		[Range(0.01f, 0.99f)]
		[Tooltip("Values below this will be considered low.")]
		public float biomeCutoffA = 0.4f;

		[SerializeField]
		[Range(0.01f, 0.99f)]
		[Tooltip("Values above this will be considered high.")]
		public float biomeCutoffB = 0.6f;

		private Biome[] _map;

		private Biome[] defaultMap = new Biome[3]{
			new Biome(BiomeType.Forest.GetObj()),
			new Biome(BiomeType.Grasslands.GetObj()),
			new Biome(BiomeType.Desert.GetObj()),
		};

		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.None;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.Biomes;


		public OneDNoiseBiomeGenerator(Biome[] map = null) {
			_map = map ?? defaultMap;
		}
		public OneDNoiseBiomeGenerator() : this(null) { }


		/*
			Generate perlin noise, get index from altitude, then set biomes based on biome map.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {
			var rng = new System.Random(Seed);

			// Gets the index 
			int getIndex(float val) {
				if (val < biomeCutoffA) return 0;
				if (val < biomeCutoffB) return 1;
				return 2;
			}

			//generate perlin noise
			var PerlinGen = PerlinGenerator.GenerateNoiseMap(grid.Width, grid.Height, 150.0f,
				rng.Next(0, int.MaxValue), 7, 2.0f, 0.6f, new System.Numerics.Vector2(1, 1));

			for (int y = 0; y < state.Height; ++y) for (int x = 0; x < state.Width; ++x) {
				var t = grid[x, y];
				if(t.Biome.BiomeType.CanSpread()) { //if not already a biome that can't be overwritten
					var noise = PerlinGen[x, y]; //get noise with perlin
					var res = _map[getIndex(noise)]; //get biome from noise value
					t.Set(state.GetDefaultBiome(res.BiomeType)); //apply biome
				}
			}
			return grid;
		}
	}
}
