using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Generates map biomes using the factors of the tile to pick the biome.
	*/
	[Serializable]
	public class FactorBasedBiomeGenerator : StepGeneratorBase, IBiomeGenerator {
		[SerializeField]
		[Range(0.01f, 0.99f)]
		[Tooltip("Values below this will be considered very low.")]
		public float biomeCutoffA = GenerationUtils.BIOME_CUTOFF_A;

		[SerializeField]
		[Range(0.01f, 0.99f)]
		[Tooltip("Values below this will be considered low.")]
		public float biomeCutoffB = GenerationUtils.BIOME_CUTOFF_B;

		[SerializeField]
		[Range(0.01f, 0.99f)]
		[Tooltip("Values below this will be considered high.")]
		public float biomeCutoffC = GenerationUtils.BIOME_CUTOFF_C;

		[SerializeField]
		[Range(0.01f, 0.99f)]
		[Tooltip("Values below this will be considered very high.")]
		public float biomeCutoffD = GenerationUtils.BIOME_CUTOFF_D;


		private BiomeMap _map = GenerationUtils.BIOME_MATRIX;


		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.Altitude | MapAspects.TemperatureHumidity;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.Biomes;


		public FactorBasedBiomeGenerator(BiomeMap map = null) {
			_map = map ?? GenerationUtils.BIOME_MATRIX;
		}
		public FactorBasedBiomeGenerator() : this(null) { }

		/*
			Get index for altitude, temperature, and humidity, then set biome with biome map.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {
			// Gets the index 
			int getIndex(float val) {
				if(val < biomeCutoffB) return (val < biomeCutoffA) ? 0 : 1;
				if(val > biomeCutoffC) return (val > biomeCutoffD) ? 4 : 3;
				return 2;
			}

			// For each tile
			for (int y = 0; y < state.Height; ++y) for (int x = 0; x < state.Width; ++x) {
				var t = grid[x, y];
				if(t.Biome.BiomeType.CanSpread()) {
					var res = _map[
						getIndex(t.altitude.Lerp(state.OceonLevel, state.MountainLevel)),
						getIndex(t.temperature),
						getIndex(t.humidity)
					];

					t.Set(state.GetDefaultBiome(res.BiomeType));
				}
			}

			return grid;
		}

	}
}
