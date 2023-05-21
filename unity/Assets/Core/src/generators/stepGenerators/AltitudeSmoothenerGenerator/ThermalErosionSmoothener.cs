using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Simulates thermal erosion on heightmap.
		For each tile, if neighbor is lower and steep enough, transfers some of it's altitude to neighbor.
	*/
	[Serializable]
	public class ThermalErosionSmoothener : StepGeneratorBase, IAltitudeSmoothenerGenerator {

		// The fraction of the altitude difference between tiles that gets transferred on erosion.
		public const float ALT_TRANSFER_RATIO = 0.2f;

		// A struct of parameters representing a certain number of smoothening iterations.
		[Serializable]
		public struct Par {
			public int iterations;
			public float altDiffThreshold;

			public Par(int iterations, float altDiffThreshold) {
				this.iterations = iterations;
				this.altDiffThreshold = altDiffThreshold;
			}
		}

		// A list of parameter structs
		[SerializeField]
		public List<Par> Parameters = new List<Par> { new Par(1, 0.01f) };

		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.Altitude;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.None;

		public ThermalErosionSmoothener() { }

		public ThermalErosionSmoothener(List<Par> parameters) {
			Parameters = parameters;
		}
		public ThermalErosionSmoothener(int iterations = 1, float altDiffThreshold = 0.01f) 
			: this(new List<Par> { new Par(iterations, altDiffThreshold) }) { }

		/*
			Erosion. Gets applied if altitude difference between neighboring tiles is big enough.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {
			int w = state.Width, h = state.Height;

			// Altitude grids to iterate over.
			var inGrid = new float[w,h];
			var outGrid = new float[w,h];

			// Copy grid altitudes into inGrid and outGrid
			for(var x = 0; x < w; x++) {
				for(var y = 0; y < h; y++) {
					inGrid[x, y] = outGrid[x, y] = grid[x, y].altitude;
				}
			}

			// Run through and erode.
			foreach(var par in Parameters) {
				var iterations = par.iterations;
				var altDiffThreshold = par.altDiffThreshold;

				for (var s = 0; s < iterations; s++) {
					 //for each tile
					for(var x = 0; x < state.Width; x++) for(var y = 0; y < state.Height; y++) {

						//for each neighbor
						foreach(var (nx, ny) in (x, y).SqareAround(1, (w, h))) {

							var altDiff = inGrid[x, y] - inGrid[nx, ny]; //get altitude diff

							//if steep enough
							if (altDiff > altDiffThreshold) {
								var transfer = altDiff * ALT_TRANSFER_RATIO; //transfer some altitude
								outGrid[x, y] -= transfer;
								outGrid[nx, ny] += transfer;
							}
						}
					}

					// apply new grid by swapping places
					var tmp = inGrid;
					inGrid = outGrid;
					outGrid = tmp;
				}
			}
		
			// Normalize the grid
			inGrid = inGrid.Normalize();
			
			// Apply the resulting altitudes.
			for(var x = 0; x < w; x++) {
				for(var y = 0; y < h; y++) {
					grid[x, y].altitude = inGrid[x, y];
				}
			}

			return grid;
		}
	}
}
