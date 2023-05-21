using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Sets ocean and mountain level based on specified altitude cutoffs.
	*/
	[Serializable]
	public class RawOceanMountianGenerator : StepGeneratorBase, IOceanMountianGenerator {
		public const float DEFAULT_WATER_CUTOFF = 0.4f;
		public const float DEFAULT_MOUNTAIN_CUTOFF = 0.8f;

		// Minimum altitude range reserved for normal land.
		public const float MIN_LAND_ALTITUDE_RANGE = 0.1f;

		[SerializeField]
		[Range(0f, 0.6f)]
		[Tooltip("How high up the water should go.")]
		public float WaterCutOff = DEFAULT_WATER_CUTOFF;

		[SerializeField]
		[Range(0.4f, 1.0f)]
		[Tooltip("How far down the mountains should go.")]
		public float MountianCutOff = DEFAULT_MOUNTAIN_CUTOFF;


		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.Altitude;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.OceanMountain;

		public RawOceanMountianGenerator(float waterCutOff, float mountianCutOff) {
			if(waterCutOff < 0) 
				throw new ArgumentOutOfRangeException(nameof(waterCutOff), $"Cannot have water surface bellow 0.0.");
			if(mountianCutOff > 1) 
				throw new ArgumentOutOfRangeException(nameof(mountianCutOff), $"Cannot have mountain base above 1.0.");
			if(mountianCutOff - waterCutOff < MIN_LAND_ALTITUDE_RANGE) 
				throw new ArgumentException($"There has to be at least {MIN_LAND_ALTITUDE_RANGE} of the height left for regular land.");
			WaterCutOff = waterCutOff;
			MountianCutOff = mountianCutOff;
		}
		public RawOceanMountianGenerator() : this(DEFAULT_WATER_CUTOFF, DEFAULT_MOUNTAIN_CUTOFF) { }

		/*
			Sets the cutoffs to desired altitudes, then sets biomes.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {
			
			float waterCutOff = WaterCutOff, 
			      mountianCutOff = MountianCutOff;

			var emptyBiome = state.GetDefaultBiome(BiomeType.Empty);
			// Set the biomes of the tiles.
			for(var x = 0; x < state.Width; x++) {
				for (var y = 0; y < state.Height; y++) {
					var t = grid[x, y];
					if(t.altitude < waterCutOff)                                                  t.Biome = state.GetDefaultBiome(BiomeType.Ocean);
					else if(t.altitude < waterCutOff + GenerationUtils.OCEON_SAND_PADDING)        t.Biome = state.GetDefaultBiome(BiomeType.Sand);
					else if(t.altitude > mountianCutOff)                                          t.Biome = state.GetDefaultBiome(BiomeType.Mountain);
					else if(t.altitude > mountianCutOff - GenerationUtils.MOUNTIAN_STONE_PADDING) t.Biome = state.GetDefaultBiome(BiomeType.Stone);
					else { // If this tile is in the normal range: Don't allow it to be any of the above biome types.
						switch(t.BiomeType) {
							case BiomeType.Ocean:
							case BiomeType.Sand:
							case BiomeType.Stone:
							case BiomeType.Mountain:
								t.Biome = emptyBiome;
								break;
						}
					}
				}
			}

			return grid;
		}

	}
}
