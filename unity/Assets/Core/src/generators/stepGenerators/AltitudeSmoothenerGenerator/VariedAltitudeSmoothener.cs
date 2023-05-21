using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Smoothens the altitude of tiles on the map using a generated map to determine how much to smoothen each tile.
	*/
	[Serializable]
	public class VariedAltitudeSmoothener : StepGeneratorBase, IAltitudeSmoothenerGenerator {
		// Number of iterations on the map.
		[SerializeField]
		[Range(1, 20)]
		[Tooltip("How many times you want to go over the map and smoothen.")]
		public int Iterations = 5;

		// Maximum range to average to smoothen the tile.
		[SerializeField]
		[Range(1, 20)]
		[Tooltip("How far away from the tile we could look to average the altitude.")]
		public int MaxRange = 5;

		// Minimum value required for a tile to get smoothened.
		[SerializeField]
		[Range(0f, 0.75f)]
		[Tooltip("How much of the map to exclude from smoothening.")]
		public float MinValueToSmoothen = 0.1f;

		// Minimum value required for a tile to get smoothened.
		[SerializeField]
		[Range(0f, 2f)]
		[Tooltip("How chaotic to make the smoothening map.")]
		public float Randomness = 1.0f;


		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.Altitude;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.None;

		public VariedAltitudeSmoothener() { }
		public VariedAltitudeSmoothener(int iterations, int range, float minValue, float randomness = 1.0f) {
			Iterations = iterations;
			MaxRange = range;
			MinValueToSmoothen = minValue;
			Randomness = randomness;
		}

		/*
			Smoothen out the altitude of tiles with diamond square map.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {
			int w = state.Width, h = state.Height;

			// Generate smoothening map.
			var smootheningGrid = Utils.GenerateDiamondSquareMap(w, h, Randomness, Seed);

			// Set all values under minimum to 0.
			smootheningGrid.Normalize(MinValueToSmoothen, 1f);

			// Extract altitudes.
			var inGrid = new float[w,h];
			var outGrid = new float[w,h];

			for(var x = 0; x < w; x++) {
				for(var y = 0; y < h; y++) {
					inGrid[x, y] = grid[x, y].altitude;
				}
			}

			// Iterate.
			float valDecrement = 1 / (Iterations * 1.5f);
			for(var s = 0; s < Iterations; s++) {

				for(var x = 0; x < state.Width; x++) {
					for(var y = 0; y < state.Height; y++) {

						var val = smootheningGrid[x,y];

						// Skip if under the limit.
						if(val < 0.001f) {
							outGrid[x, y] = inGrid[x, y];
							continue;
						}

						int range = (int) Mathf.Max(MaxRange * val, 1);

						// Average altitude of area.
						float sum = 0;
						float count = 0;
						foreach(var (px, py) in (x, y).DiamondAround(range, (w, h))) {
							++count;
							sum += inGrid[px, py];
						}

						if(count == 0) Logger.Log($"[{s} : ({x}, {y})] range: {range}, val: {val:N4}, count is 0!!!");

						// Include self proportional to smoothening value.
						float selfWeight = 2 * (1f - val);
						sum += inGrid[x, y] * selfWeight;
						count += selfWeight;

						outGrid[x, y] = sum / count;

						// Reduce the smoothening value for next run.
						smootheningGrid[x, y] -= valDecrement;
					}
				}

				// apply new grid by swapping places
				var tmp = inGrid;
				inGrid = outGrid;
				outGrid = tmp;
			}

			// Normalize the grid
			inGrid = inGrid.Normalize();

			// Replace the altitudes.
			for(var x = 0; x < w; x++) {
				for(var y = 0; y < h; y++) {
					grid[x, y].altitude = inGrid[x, y];
				}
			}

			return grid;
		}

	}
}
