using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {
	/*
		Generates windmap using flowing wind from left to right, which is affected by altitude.
	*/
	[Serializable]
	public class FlowingWindGenerator : StepGeneratorBase, IWindGenerator {

		//wind vector struct, used for adding wind vectors together and storing the wind of a tile
		public struct WindVector {
			public float xComp; //[0, 1] with left-to-right wind
			public float yComp; //[-1, 1]
			public float magnitude; //the strength of the winds
			public float scale; //used to add vectors together correctly

			public float angle { //[0, 2 * PI]
				get => (float)Math.Atan2(yComp, xComp);
				set {
					xComp = (float)Math.Cos(value);
					yComp = (float)Math.Sin(value);
				}
			}

			public WindVector(float xComp = 0.0f, float yComp = 0.0f,
				float magnitude = 0.0f, float scale = 0.0f) {
				this.xComp = xComp;
				this.yComp = yComp;
				this.magnitude = magnitude;
				this.scale = scale;
			}

			public override string ToString() {
				return $"direction: ({xComp:F3}, {yComp:F3}) magnitude: {magnitude:F3} scale: {scale:F3}";
			}
		}


		[SerializeField]
		[Range(2, 20)]
		[Tooltip("How sensitive you want the wind direction to be to altitude changes.")]
		public float altitudeSensitivity;

		//the altitude difference required for max altitude factors
		private float maxWindAltDiff => 1 / altitudeSensitivity;


		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.Altitude;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.Wind;
		

		public FlowingWindGenerator(float altitudeSensitivity = 10) {
			this.altitudeSensitivity = altitudeSensitivity;
		}
		public FlowingWindGenerator() : this(10) { }

		

		/*
			Makes wind grid based on altitudes and saves to grid
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {
			int w = state.Width, h = state.Height;

			//make wind grid
			var windGrid = getWindGrid(grid, state);

			//apply wind to the tiles
			for (int x = 0; x < w; ++x) for (int y = 0; y < h; ++y) {
				var t = grid[x, y];
				var wind = windGrid[x, y];
				t.Set(wind.xComp, wind.yComp, wind.angle, wind.magnitude);
			}

			return grid;
		}

		/*
			Makes wind grid. Wind flows from left to right, and is affected by altitude and wind direction
		*/
		WindVector[,] getWindGrid(Grid grid, IGeneratorState state) {
			int w = state.Width, h = state.Height;

			//create empty wind grid
			var windGrid = new WindVector[w, h];

			//set initial wind vectors
			for (var x = 0; x < w; x++) for (var y = 0; y < h; y++) {
				if (x == 0) {
					windGrid[x, y] = new WindVector(1.0f, 0.0f, 1.0f, 1.0f); //first column has mag 1
				} else
					windGrid[x, y] = new WindVector(1.0f, 0.0f, 0.0f, 0.0f); //rest has mag 0
			}

			// Column by column, tile by tile, wind spreads to the 3 tiles to the right of source tile.
			// The factors determine the ratio of wind that spreads to each of the 3 tiles.
			//	  / X
			//	X - X
			//	  \	X
			for (var x = 0; x < w - 1; x++) for (var y = 0; y < h; y++) {
				var t = grid[x, y];
				var v = windGrid[x, y];

				//
				// Get altitude factors, lower altitude than source tile means greater factor
				//

				var altFactors = new float[3];
				for (var i = 0; i < 3; i++) {
					//check out of bounds
					if ((y - 1 + i) == -1 || (y - 1 + i) == h) { 
						altFactors[i] = 0;
						continue;
					}

					//altDiff is new alt minus old alt
					var altDiff = getAltForWind(grid[x + 1, y - 1 + i], state) - getAltForWind(t, state);
					//factor is between 0 and 1
					altFactors[i] = Math.Max(0.0000001f, 1 - Utils.Lerp(altDiff, -maxWindAltDiff, maxWindAltDiff));
				}

				//
				// Get direction factors, upwards-tilting angle means more up etc.
				//

				var dirFactors = new float[3] { 0.001f, 0.001f, 0.001f }; //start at a minimum value

				float up_and_down_addition = v.xComp; //added to up and down factors, ensures more dispersion

				dirFactors[1] += v.xComp; //the tile that's straight ahead, based on xComp of wind

				//the diagonals are based on yComp of wind
				if (v.yComp < 0) {
					dirFactors[0] += (-v.yComp) + up_and_down_addition;
					dirFactors[2] += up_and_down_addition;
				} else {
					dirFactors[2] += v.yComp + up_and_down_addition;
					dirFactors[0] += up_and_down_addition;
				}

				//
				// Combine factors
				//

				//make sum of 3 factors = 1
				var sum = altFactors.Sum();
				for (var i = 0; i < 3; i++) {
					altFactors[i] /= sum;
				}
				sum = dirFactors.Sum();
				for (var i = 0; i < 3; i++) {
					dirFactors[i] /= sum;
				}

				//sumFactors, combination of alt and dir
				var sumFactors = altFactors;
				for (var i = 0; i < 3; i++) {
					sumFactors[i] *= dirFactors[i];
				}
				sum = sumFactors.Sum();
				for (var i = 0; i < 3; i++) {
					sumFactors[i] /= sum;
				}

				//
				// Apply changes to 3 right tiles
				//

				for (var i = 0; i < 3; i++) {
					//new wind vector, set mag
					var from = new WindVector(0, 0, v.magnitude * sumFactors[i], v.magnitude * sumFactors[i]);
					//set angle
					switch (i) {
						case 0: from.angle = -(Mathf.PI / 2) + (Mathf.PI / 20); break;
						case 1: from.angle = 0; break;
						case 2: from.angle = (Mathf.PI / 2) - (Mathf.PI / 20); break;
					}

					//check out of bounds
					if ((y - 1 + i) == -1 || (y - 1 + i) == h) continue;
					//add vector to destination tile
					windGrid[x + 1, y - 1 + i] = sumWindVectors(from, windGrid[x + 1, y - 1 + i]);
				}
			}

			//
			// Normalize magnitudes
			//

			//find lowest and highest
			var lowestMag = float.MaxValue;
			var highestMag = float.MinValue;
			for (var x = 0; x < w; x++) for (var y = 0; y < h; y++) {
				if (windGrid[x, y].magnitude < lowestMag) lowestMag = windGrid[x, y].magnitude;
				if (windGrid[x, y].magnitude > highestMag) highestMag = windGrid[x, y].magnitude;
			}
			//normalize
			for (var x = 0; x < w; x++) for (var y = 0; y < h; y++) {
				windGrid[x, y].magnitude = Utils.Lerp(windGrid[x, y].magnitude, lowestMag, highestMag);
			}

			return windGrid;
		}

		// Get the altitude the wind flows along. For oceans and lakes, this is the surface alt.
		private float getAltForWind(Tile t, IGeneratorState state) {
			if (t.BiomeType == BiomeType.Ocean)
				return state.OceonLevel;
			if (t.BiomeType == BiomeType.Lake)
				return ((ILakeBiome)t.Biome).SurfaceAltitude;
			else
				return t.altitude;
		}

		// Add two wind vectors, xComp yComp and scale is standard vector addition, magnitude is just a+b
		WindVector sumWindVectors(WindVector a, WindVector b) {

			//get direction
			var newX = (a.xComp * a.scale) + (b.xComp * b.scale);
			var newY = (a.yComp * a.scale) + (b.yComp * b.scale);

			//scale so x^2 + y^2 = 1
			var scale = Mathf.Sqrt((newX * newX) + (newY * newY));
			if (scale == 0) { //if no direction, go forward
				newX = 1;
				newY = 0;
			} else {
				newX /= scale;
				newY /= scale;
			}

			//get magnitude
			var newMag = a.magnitude + b.magnitude;

			return new WindVector(newX, newY, newMag, scale);
		}
	}
}
