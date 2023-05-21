using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Smoothens the altitude of a map by taking an average of neighbors.
		For each tile, it searches the neighbors, then takes their average as the new altitude.
	*/
	[Serializable]
	public class AltitudeAverageSmoothener : StepGeneratorBase, IAltitudeSmoothenerGenerator {

		// The minimum % stronger than the original biome required to replace it.
		public const float MIN_BIOME_DOMINANCE = 1.2f;

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

		// A list of parameter structs
		[SerializeField]
		public List<Par> Parameters = new List<Par> { new Par(1, 1, false) };

		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.Altitude;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.None;

		public AltitudeAverageSmoothener() { }

		public AltitudeAverageSmoothener(List<Par> parameters) {
			Parameters = parameters;
		}
		public AltitudeAverageSmoothener(int iterations = 1, int range = 1, bool includeSelf = false) 
			: this(new List<Par> { new Par(iterations, range, includeSelf) }) { }

		/*
			Smoothen out the altitude of tiles by taking averages.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {
			int w = state.Width, h = state.Height;

			// Altitude grids to iterate over.
			var inGrid = new float[w,h];
			var outGrid = new float[w,h];

			// Copy grid altitudes into inGrid
			for(var x = 0; x < w; x++) {
				for(var y = 0; y < h; y++) {
					inGrid[x, y] = grid[x, y].altitude;
				}
			}


			// Run through and smoothen the altitudes.
			foreach(var par in Parameters) {
				var iterations = par.iterations;
				var range = par.range;
				var includeSelf = par.includeSelf;
				for (var s = 0; s < iterations; s++) { //for each iteration

					for(var x = 0; x < state.Width; x++) {
						for(var y = 0; y < state.Height; y++) {

							// Average altitude of area.
							float sum = 0;
							int count = 0;
							foreach(var (px, py) in (x, y).SqareAround(range, (w, h), includeSelf)) {
								++count;
								sum += inGrid[px, py];
							}

							outGrid[x, y] = sum / count;
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
