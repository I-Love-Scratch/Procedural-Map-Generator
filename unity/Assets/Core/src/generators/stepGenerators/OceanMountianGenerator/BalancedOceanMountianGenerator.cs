using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Sets ocean and mountain level based on desired amount of water and mountian.
	*/
	[Serializable]
	public class BalancedOceanMountianGenerator : StepGeneratorBase, IOceanMountianGenerator {
		[SerializeField] [Range(0f, 0.45f)] [Tooltip("How much of the map you want covered in water.")]
		public float desiredWater = 0.3f;

		[SerializeField] [Range(0f, 0.45f)] [Tooltip("How much of the map you want covered in mountains.")]
		public float desiredMountian = 0.15f;

		[SerializeField] [Range(10, 500)] [Tooltip("How precise you want it.")]
		public int precision = 50;

		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.Altitude;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.OceanMountain;

		/*
			Creates a generator for making controlling aproximate map coverage that is oceon and mountian.
			@params desiredWater    : Aproximate % of the map that should be covered in oceon.
			@params desiredMountian : Aproximate % of the map that should be covered in mountian.
			@params precition       : How many groups to divide the altitudes into. (More groups = closer to desired %)
		*/
		public BalancedOceanMountianGenerator(float desiredWater = 0.3f, float desiredMountian = 0.15f, int precision = 50) {
			this.desiredWater = desiredWater;
			this.desiredMountian = desiredMountian;
			this.precision = precision;
		}
		public BalancedOceanMountianGenerator() : this(0.3f, 0.15f, 50) { }


		/*
			Sort altitudes into segments with specified precision, then set the cutoffs based on this, before setting biomes.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {
			var dist = new DistributionCounter(precision);

			// Get statistics for altitudes
			for (var x = 0; x < state.Width; x++) {
				for (var y = 0; y < state.Height; y++) {
					dist.Add(grid[x, y].altitude);
				}
			}
			var d = dist.GetDistribution();
			float waterCutOff = 0, mountianCutOff = 1;

			// Find the water cut-off
			float cum = 0f;
			for (var i = 0; i < d.Length; i++) {
				cum += d[i];
				if(cum >= desiredWater) {
					waterCutOff = dist.GetStepStart(i + 1);
					break;
				}
			}

			// Find the mountian cut-off
			cum = 0f;
			for (var i = d.Length - 1; i > -1; i--) {
				cum += d[i];
				if (cum >= desiredMountian) {
					mountianCutOff = dist.GetStepStart(i);
					break;
				}
			}

			var emptyBiome = state.GetDefaultBiome(BiomeType.Empty);
			// Set the biomes of the tiles.
			for (var x = 0; x < state.Width; x++) {
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

			// Update the map scale.
			state.OceonLevel = waterCutOff;
			state.MountainLevel = mountianCutOff;

			return grid;
		}

	}
}
