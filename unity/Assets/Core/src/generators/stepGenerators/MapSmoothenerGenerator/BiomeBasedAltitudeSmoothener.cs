using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Smoothens the altitude of biomes, with different parameters depending on the biome type.
	*/
	[Serializable]
	public class BiomeBasedAltitudeSmoothener : StepGeneratorBase, IMapSmoothenerGenerator {

		// A struct of parameters representing a certain number of smoothening iterations.
		[Serializable]
		public struct Par {
			public int iterations;
			public int range;
			public bool includeSelf;

			public Par(int iterations, int range, bool includeSelf) {
				this.iterations = iterations;
				this.range = range;
				this.includeSelf = includeSelf;
			}
		}

		private static readonly Par DEFAULT_PAR = new Par(0, 0, false);

		// A dictinary mapping biome types with parameter structs
		public Dictionary<BiomeType, Par> BiomePars = new Dictionary<BiomeType, Par>{
			{ BiomeType.Desert,		new Par(2, 2, false) },
			{ BiomeType.Grasslands,	new Par(1, 1, false) },
		};

		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.Biomes | MapAspects.Altitude;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.Altitude;

		public BiomeBasedAltitudeSmoothener() { }

		/*
			For each iteration, goes through every tile and smoothens based on par values.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {
			int w = state.Width, h = state.Height;

			// Altitude grids to iterate over.
			var inGrid = new float[w, h];
			var outGrid = new float[w, h];

			// Copy grid altitudes into inGrid
			for (var x = 0; x < w; x++) {
				for (var y = 0; y < h; y++) {
					inGrid[x, y] = grid[x, y].altitude;
				}
			}

			//get highest iteration in dictionary
			var maxIterations = 0;
			foreach (var (b, p) in BiomePars) {
				if (p.iterations > maxIterations)
					maxIterations = p.iterations;
			}

			// Run through and smoothen the altitudes.
			for (var i = 0; i < maxIterations; i++) {
				for (var x = 0; x < state.Width; x++) for (var y = 0; y < state.Height; y++) {

					//get biome parameters
					var b = grid[x, y].BiomeType;
					var bPar = BiomePars.GetValueOrDefault(b, DEFAULT_PAR);

					if (i < bPar.iterations) { //if this biome has this many iterations

						// Average altitude of area.
						float sum = 0;
						int count = 0;
						foreach (var (px, py) in (x, y).SqareAround(bPar.range, (w, h), bPar.includeSelf)) {
							++count;
							sum += inGrid[px, py];
						}
						outGrid[x, y] = sum / count; //apply average
					} else {
						outGrid[x, y] = inGrid[x, y]; //otherwise copy old value
					}
				}

				// apply new grid by swapping places
				var tmp = inGrid;
				inGrid = outGrid;
				outGrid = tmp;
			}


			// Normalize the grid
			inGrid = inGrid.Normalize();

			//clamp for mountain/ocean level
			for (var x = 0; x < state.Width; x++) for (var y = 0; y < state.Height; y++) {
				//if mountain, make sure altitude is not too low
				if (grid[x, y].BiomeType == BiomeType.Mountain) {
					if (inGrid[x, y] < state.MountainLevel)
						inGrid[x, y] = state.MountainLevel;
				//if ocean, make sure altitude is not too high
				} else if (grid[x, y].BiomeType == BiomeType.Ocean) {
					if (inGrid[x, y] > state.OceonLevel)
						inGrid[x, y] = state.OceonLevel;
				//if neither, make sure altitude is between ocean and mountain level
				} else {
					if (inGrid[x, y] > state.MountainLevel)
						inGrid[x, y] = state.MountainLevel;
					else if (inGrid[x, y] < state.OceonLevel)
						inGrid[x, y] = state.OceonLevel;
				}
			}

			// Apply the resulting altitudes.
			for (var x = 0; x < w; x++) {
				for (var y = 0; y < h; y++) {
					grid[x, y].altitude = inGrid[x, y];
				}
			}

			return grid;
		}

	}
}
