using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Generates map biomes based only on altitude.
	*/
	[Serializable]
	public class AltitudeBasedBiomeGenerator : StepGeneratorBase, IBiomeGenerator {
		[SerializeField]
		[Range(0.01f, 0.99f)]
		[Tooltip("Values below this will be considered very low.")]
		public float biomeCutoffA = 0.2f;

		[SerializeField]
		[Range(0.01f, 0.99f)]
		[Tooltip("Values below this will be considered low.")]
		public float biomeCutoffB = 0.35f;

		[SerializeField]
		[Range(0.01f, 0.99f)]
		[Tooltip("Values above this will be considered above average.")]
		public float biomeCutoffC = 0.5f;

		[SerializeField]
		[Range(0.01f, 0.99f)]
		[Tooltip("Values above this will be considered high.")]
		public float biomeCutoffD = 0.65f;

		[SerializeField]
		[Range(0.01f, 0.99f)]
		[Tooltip("Values above this will be considered very high.")]
		public float biomeCutoffE = 0.8f;

		public Biome[] _map;

		static readonly private Biome[] defaultMap = new Biome[6]{
			new Biome(BiomeType.Swamp.GetObj()),
			new Biome(BiomeType.Grasslands.GetObj()),
			new Biome(BiomeType.Savanna.GetObj()),
			new Biome(BiomeType.Forest.GetObj()),
			new Biome(BiomeType.Tundra.GetObj()),
			new Biome(BiomeType.Arctic.GetObj()),
		};

		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.Altitude;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.Biomes;


		public AltitudeBasedBiomeGenerator(Biome[] map = null) {
			_map = map ?? defaultMap;
		}
		public AltitudeBasedBiomeGenerator() : this(null) { }

		/*
			Get index based on altitude and set biome with biome map.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {

			// Gets the index 
			int getIndex(float val) {
				if (val < biomeCutoffA) return 0;
				if (val < biomeCutoffB) return 1;
				if (val < biomeCutoffC) return 2;
				if (val < biomeCutoffD) return 3;
				if (val < biomeCutoffE) return 4;
				return 5;
			}

			//for each tile
			for (int y = 0; y < state.Height; ++y) for (int x = 0; x < state.Width; ++x) {
				var t = grid[x, y];
				if(t.Biome.BiomeType.CanSpread()) { //if not already a biome that can't be overwritten
					var index = getIndex(Utils.Lerp(t.altitude, state.OceonLevel, state.MountainLevel));
					var res = _map[index]; //get biome from index
					t.Set(state.GetDefaultBiome(res.BiomeType)); //apply biome
				}
			}
			return grid;
		}
	}
}
