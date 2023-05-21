using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Smoothens a map using Celular Automata rules.
	*/
	[Serializable]
	public class CellularAutomataSmoothener : StepGeneratorBase, IMapSmoothenerGenerator {

		// The minimum % stronger than the original biome required to replace it.
		public const float MIN_BIOME_DOMINANCE = 1.2f;

		[SerializeField]
		[Range(1, 10)]
		[Tooltip("How many iterations to do on the map.")]
		public int Iterations;

		[SerializeField]
		[Range(1, 20)]
		[Tooltip("How far to look each iteration.")]
		public int Range;


		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.Biomes | MapAspects.Altitude;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.Altitude;


		public CellularAutomataSmoothener(int iterations = 1, int range = 2) {
			Iterations = iterations;
			Range = range;
		}
		public CellularAutomataSmoothener() : this(1, 2) { }

		/*
			Smoothens out the biomes by replacing rouge tiles with neighboring tiles.

			If 3 or more of the 4 neighbors are of a different type, then replace this tile with one of them.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {

			bool canReplace(BiomeType type) => !(type == BiomeType.River);

			Grid newGrid = new Grid(state.Width, state.Height);

			for(var s = 0; s < Iterations; s++) {

				for(var x = 0; x < state.Width; x++) for (var y = 0; y < state.Height; y++) {
					 
					var t = grid[x, y];

					if(!canReplace(t.BiomeType)) {
						newGrid[x, y] = t;
						continue;
					}


					var neighborPos = (x,y).DiamondAround(Range, (state.Width, state.Height))
						.Where(pos => pos != (x,y)) // Exclude self.
						.ToList();

					// Get all neighboring tiles of different type
					var neighbors = neighborPos
						.Select( pos => ( grid[pos], pos ) )
						.Where(pair => canReplace(pair.Item1.BiomeType))
						.ToList();

					if(neighbors.Count == 0) {
						newGrid[x, y] = t;
						continue;
					}

					// Weighted sum for each BiomeType.
					var weights = new Dictionary<BiomeType, float>();
					foreach(var (n, pos) in neighbors) {
						var bType = n.BiomeType;
						weights[bType] = weights.GetValueOrDefault(bType, 0f) + (1f - pos.DistanceToSimple((x,y)) / (Range + 3f));
					}

					// Weight of the tile's BiomeType.
					var originWeight = weights.GetValueOrDefault(t.BiomeType, 0f);

					// Weight of the strongest BiomeType.
					var strongest = weights.OrderByDescending(val => val.Value)
						.First();

					// If origin is much weaker than strongest, then swap.
					if(strongest.Value > originWeight * MIN_BIOME_DOMINANCE) {
						// Find nearest neighbor of the new type.
						var nt = neighbors
							.Where( val => val.Item1.BiomeType == strongest.Key)
							.OrderBy( val => val.Item2.DistanceToSimple((x,y)) )
							.First()
							.Item1;

						//change altitude if biome change involves mountain/ocean/lake
						if (t.BiomeType == BiomeType.Mountain || nt.BiomeType == BiomeType.Mountain)
							t.altitude = state.MountainLevel;
						else if (t.BiomeType == BiomeType.Ocean || nt.BiomeType == BiomeType.Ocean)
							t.altitude = state.OceonLevel;
						else if (t.BiomeType == BiomeType.Lake)
							t.altitude = ((ILakeBiome)t.Biome).SurfaceAltitude;
						else if (nt.BiomeType == BiomeType.Lake)
							t.altitude = ((ILakeBiome)nt.Biome).SurfaceAltitude;

						t.Biome = nt.Biome; //change biome
					}
					newGrid[x, y] = t;
					
				}

				//apply new grid by swapping places
				var tmp = grid;
				grid = newGrid;
				newGrid = tmp;
			}

			return grid;
		}

	}
}
