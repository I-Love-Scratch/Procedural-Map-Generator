using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Generates map humidity and temperature based on environmental factors.
	*/
	[Serializable]
	public class EnvironmentalTemperatureHumidityGenerator : StepGeneratorBase, ITemperatureHumidityGenerator {

		// The most altitude can affect temperature.
		private const float ALTITUDE_TEMP_MODIFIER = 0.2f;
		// The most humidity can affect temperature.
		private const float HUMIDITY_TEMP_MODIFIER = 0.2f;
		// The lowest humidity for modifying temperature.
		private const float MIN_HUMIDITY_FOR_TEMP_MOD = 0.4f;


		// Minimum number of tiles till a land tile for humidity to spread.
		private const int HUMIDITY_SPREAD_LAND_PROXIMITY_MIN = 10;
		// Minimum amount of humidity spread pr tile for a chunk to be considered a source.
		private const float MIN_AVG_HUMIDITY_SOURCE = 0.30f;

		// The percent of humidity to disperse each round.
		private const float DISPURSE_PERCENT = 0.3f;


		// The highest difference in distance between circle and diamond spread.
		private const float CIRCLE_DIAMOND_MAX_DIST_DIFF = 1.41421356237309504f; // Root of 2.
		/*
			The multiplier for max distance to divide by when calculating spread efficiency. 
			Should never go under 1. (Increase to make distance from source less significant)
		*/
		private const float SPREAD_DISTANCE_MULTIPLIER = 2.2f;
		/*
			Final distance modifier or spread effectiveness.
		*/
		private const float SPREAD_DISTANCE_MOD = CIRCLE_DIAMOND_MAX_DIST_DIFF * SPREAD_DISTANCE_MULTIPLIER;

		const float SUNLIGHT_STRENGTH = 0.6f; // How much temperature sunlight adds.

		// Maps BiomeTypes with how far out their humidity spreads.
		private static readonly IReadOnlyDictionary<BiomeType, float> BiomeTypeHumidities = new Dictionary<BiomeType, float>{
			{ BiomeType.Ocean,    0.7f },
			{ BiomeType.River,    1.3f },
			{ BiomeType.Lake,     0.7f },
			{ BiomeType.Glacier,  0.7f },
		};



		private System.Random rng;

		[SerializeField]
		[Range(1, 250)]
		[Tooltip("How far humidity can spread from a source.")]
		public int humiditySpreadDistance;


		[SerializeField]
		[Range(1, 20)]
		[Tooltip("How tall and wide each chunk should be. (Groups tiles into chunks for some steps to reduce the cost of large spreads)")]
		public int chunkSize;


		[SerializeField]
		[Range(1, 20)]
		[Tooltip("How many iterations of dispersion to do.")]
		public int dispersionSteps;


		[SerializeField]
		[Range(1, 20)]
		[Tooltip("How far to disperse each iteration.")]
		public int dispersionRange;


		[SerializeField]
		[Range(0.05f, 0.75f)]
		[Tooltip("How much of a tile's humidity to disperse each step.")]
		public float dispersionProp;

		[SerializeField]
		[Tooltip("Whether to use wind to transport humidity. (must have a wind aspect)")]
		public bool useWind = false;

		[SerializeField]
		[Range(0.1f, 0.9f)]
		[Tooltip("How much humidity is carried from a tile with wind.")]
		public float windDispersionTake;

		[SerializeField]
		[Range(0.1f, 0.9f)]
		[Tooltip("How much humidity is carried to a tile with wind.")]
		public float windDispersionGive;

		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.Altitude | MapAspects.OceanMountain | (useWind ? MapAspects.Wind : MapAspects.None);

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.TemperatureHumidity;

		public EnvironmentalTemperatureHumidityGenerator(
			int humiditySpreadDistance = 80, int chunkSize = 4, 
			int dispersionSteps = 3, int dispersionRange = 3, float dispersionProp = DISPURSE_PERCENT,
			float windDispersionTake = 0.4f, float windDispersionGive = 0.6f
		) {
			this.humiditySpreadDistance = humiditySpreadDistance;
			this.chunkSize = chunkSize;
			this.dispersionSteps = dispersionSteps;
			this.dispersionRange = dispersionRange;
			this.dispersionProp = dispersionProp;
			this.windDispersionTake = windDispersionTake;
			this.windDispersionGive = windDispersionGive;
		}
		public EnvironmentalTemperatureHumidityGenerator() : this(80, 4, 3, 3, DISPURSE_PERCENT, 0.4f, 0.6f) { }

		/*
			Simulates factors arising from the environment.
			- Humidity spreads from water sources.
			- Wind moves humidity.
			- Sunlight causes heat.
			- Heat is modified by altitude and humidity.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {
			rng = new System.Random(Seed);

			int w = state.Width, h = state.Height;

			int l = chunkSize; // The lenght of a chunk (In number of tiles width and height)
			int chunkSpreadDistance = (int) Math.Ceiling((float) humiditySpreadDistance / l);

			// The width and height of the chunk grid.
			int cw = (int) Math.Ceiling((float) w / l),
			    ch = (int) Math.Ceiling((float) h / l);

			// Grids for humidity and humidity source of each chunk.
			var hSourceGrid = new float[cw, ch];
			var hChunkGrid = new float[cw, ch];


			// Compress the humidity sources into the chunks.
			for(int y = 0; y < h; ++y) {
				for(int x = 0; x < w; ++x) {
					hSourceGrid[x / l, y / l] += BiomeTypeHumidities.GetValueOrDefault(grid[x, y].BiomeType, 0f);
				}
			}


			// 1) Go through grid, apply humidity from each tile.
			hChunkGrid = Spread(hChunkGrid, hSourceGrid);


			// 1.1) Add dispersion to spread the humidity further out?
			//

			// Scale the humidity
			hChunkGrid.Apply(val => (float) Math.Sqrt(val));

			// New grid for the humidity of each tile
			var hGrid = new float[w, h];
			for(int y = 0; y < h; ++y) {
				for(int x = 0; x < w; ++x) {
					hGrid[x, y] = hChunkGrid[x / l, y / l];
				}
			}

			// Disperse the humidity
			hGrid = Disperse(hGrid, dispersionSteps, dispersionProp, dispersionRange);

			// Spread humidity with wind
			if (useWind)
				hGrid = WindDisperse(hGrid, grid, w, h);

			// 1.2) Get humidity cap.
			float maxHumidity = GetHumidityCap(grid, state, hGrid);

			// 2) Normalize the grid. (With capped values)
			hGrid
				.Normalize(0, maxHumidity)
				//.Apply(val => (float) Math.Sqrt(val))
				;


			// 4) Pick temperature.
			var tGrid = GetTemperatureGrid(grid, state, hGrid);


			// 5) Apply the factors to the tiles.
			for (int y = 0; y < state.Height; ++y) for (int x = 0; x < state.Width; ++x) {
				var t = grid[x, y];
				t.Set(tGrid[x, y], hGrid[x, y]);
			}

			return grid;
		}

		/*
			Gets the humidity cap to use for normalization.
			The cap is the middle between the highest, and the average of tiles above average.
		*/
		private float GetHumidityCap(Grid grid, IGeneratorState state, float[,] hGrid) {
			int w = state.Width, h = state.Height;

			// Sum and count for all non-soure tiles.
			int count = 0; float sum = 0; float max = 0; float min = 0.05f;
			for(int y = 0; y < h; ++y) {
				for(int x = 0; x < w; ++x) {
					var hum = hGrid[x, y];

					// If this tile is a source of humidity or is dry, skip.
					if(BiomeTypeHumidities.ContainsKey(grid[x, y].BiomeType) || hum < min)
						continue;

					if(hum > max) max = hum;

					// Count up and add to sum.
					++count; sum += hGrid[x, y];
				}
			}

			// Set minimum to average.
			min = (sum / count);

			// Count again, get avg of above avg.
			count = 0; sum = 0;
			for(int y = 0; y < h; ++y) {
				for(int x = 0; x < w; ++x) {
					var hum = hGrid[x, y];

					// If this tile is a source of humidity or is dry, skip.
					if(BiomeTypeHumidities.ContainsKey(grid[x, y].BiomeType) || hum < min)
						continue;

					// Count up and add to sum.
					++count; sum += hGrid[x, y];
				}
			}

			var avg = (sum / count);

			const float PROP = 0.5f;

			// Max is average non-source * multiplier.
			float maxHumidity = avg * PROP + max * (1 - PROP);//(sum / count) * MAX_HUMIDITY_AVG_MULTIPLIER;

			return maxHumidity;
		}


		/*
			
		*/
		private float[,] WindDisperse(float[,] hGrid, Grid grid, int w, int h) {
			for (var x = 0; x < w - 1; x++) for (var y = 0; y < h; y++) {
					var windStrength = grid[x, y].windMagnitude;
					var humidity = hGrid[x, y];
					hGrid[x, y] -= humidity * windStrength * windDispersionTake;
					hGrid[x + 1, y] += humidity * windStrength * windDispersionGive;
				}
			return hGrid;
		}


		/*
			Gets a temperature grid for the map.
		*/
		private float[,] GetTemperatureGrid(Grid grid, IGeneratorState state, float[,] hGrid) {
			int w = state.Width, h = state.Height;

			var tGrid = Utils.GenerateDiamondSquareMap(w, h, 1f, seed: rng.Next(0, int.MaxValue));

			// Simulate sunlight, disperse, normalize
			tGrid = Disperse(
				SunLightSimulation(grid, state, tGrid, new float[] { 0.001f,0.005f,0.015f,0.007f,0.003f, }), 
				2, 0.2f, 1
			).Normalize();

			// Midpoint between oceon and mountain, mountain
			float altMid = (state.MountainLevel + state.OceonLevel) / 2,
			      altMax = state.MountainLevel;

			// Apply temp modifications based on altitude and humidity.
			for(int y = 0; y < h; ++y) {
				for(int x = 0; x < w; ++x) {
					var t = tGrid[x, y];
			
					// Altitude reduces temperature slightly.
					t *= 1f - grid[x, y].altitude.Lerp(altMid, altMax) * ALTITUDE_TEMP_MODIFIER;
			
					// Humidity pulls temperature closer to the middle.
					t = (
						(t * 2 - 1) 
						* (1f - HUMIDITY_TEMP_MODIFIER * hGrid[x, y].Lerp(MIN_HUMIDITY_FOR_TEMP_MOD, 1.0f))
						+ 1
					) / 2;
			
					tGrid[x, y] = t.Clamp(0f, 1f);
				}
			}

			return tGrid;
			//return tGrid.Normalize();
		}


		/*
			Disperses part of each tile's value to neighboring tiles in a certain range.
		*/
		private float[,] Disperse(float[,] grid, int dispersionSteps, float dispersionProp, int dispersionRange) {
			int w = grid.GetLength(0), h = grid.GetLength(1);

			//  Each turn spread a portion of each tile to its neighbours based on who has the least humidity already.
			for(int i = 0; i < dispersionSteps; ++i) {
				var neo = new float[w, h];
				for(int y = 0; y < h; ++y) {
					for(int x = 0; x < w; ++x) {
						var original = grid[x, y];

						if(original < 0.05f) continue;

						neo[x, y] += original * (1f - dispersionProp);

						var toDisperse = original * dispersionProp;

						(int x, int y)[] neighbors = Utils.DiamondAround(x, y, dispersionRange, w, h)
							.Where(pos => pos.Item1 != x || pos.Item2 != y)
							.ToArray();

						var neighborSum = Math.Max(0.5f, neighbors.Sum(pos => grid[pos.x, pos.y]));

						// Spread to each neighbor based on how much it already had.
						foreach(var (nx, ny) in neighbors) {
							neo[nx, ny] += toDisperse * (1f - (grid[nx, ny] / neighborSum));
						}

					}
				}
				grid = neo;
			}
			return grid;
		}


		/*
			Spreads humidity from all source tiles to all nearby tiles.
		*/
		private float[,] Spread(float[,] hGrid, float[,] hSourceGrid) {
			int w = hGrid.GetLength(0), h = hGrid.GetLength(1);

			int l = chunkSize; // The lenght of a chunk (In number of tiles width and height)
			int chunkSpreadDistance = (int) Math.Ceiling((float) humiditySpreadDistance / l);
			float minSourceChunkSpread = MIN_AVG_HUMIDITY_SOURCE * l * l;
			int maxSourceChunkLandDist = (int) Math.Ceiling((float) HUMIDITY_SPREAD_LAND_PROXIMITY_MIN / l);
			for(int y = 0; y < h; ++y) {
				for(int x = 0; x < w; ++x) {
					var hMod = hSourceGrid[x, y];
					if(hMod == 0f) continue;

					// Check that the chunk has at least one nearby neighbor that is counted as land.
					bool valid = Utils
						.DiamondAround(x, y, maxSourceChunkLandDist, w, h, returnOrigo: true)
						.Any(((int x, int y) pos) => hSourceGrid[pos.x, pos.y] < minSourceChunkSpread);
					if(!valid) continue;

					// Spread humidity out from the source tile. (Give reduced humidity depending on how far away the tile is)
					foreach(var (nx, ny) in Utils.CircleAround((x, y), chunkSpreadDistance, w, h))
						hGrid[nx, ny] += hMod * (1f - (x, y).DistanceToSimple((nx, ny)) / (chunkSpreadDistance * SPREAD_DISTANCE_MOD + 1));
				}
			}

			return hGrid;
		}


		/*
			Simulates sunlight on the map to apply temperature.
		*/
		private float[,] SunLightSimulation(Grid grid, IGeneratorState state, float[,] tGrid, float[] shadowDecays, float totalTemp = SUNLIGHT_STRENGTH) {
			const float MIN_ALTITUDE_DECAY = 0.005f; // How fast the shadow goes down pr tile.

			int w = state.Width, h = state.Height;

			//var tGrid = new float[w,h];

			var sunStrenght = totalTemp / shadowDecays.Length;

			foreach(var shadowDecay in shadowDecays) {
				tGrid = SunLightSimulationStep(grid, state, tGrid, shadowDecay, sunStrenght);
			}

			return tGrid;
		}

		/*
			Simulates sunlight on the map to apply temperature.
		*/
		private float[,] SunLightSimulationStep(Grid grid, IGeneratorState state, float[,] tGrid, float shadowDecay, float sunStrength) {
			int w = state.Width, h = state.Height;

			for(int y = 0; y < h; ++y) {
				var minAlt = 0.0f; 
				for(int x = 0; x < w; ++x) {
					var altitude = grid[x, y].altitude;

					// If this tile is tall enough to get sunlight.
					if(altitude >= minAlt) {
						tGrid[x, y] += sunStrength;
						minAlt = altitude;
					}

					// Move the shadow down a step.
					minAlt -= shadowDecay;

				}
			}

			return tGrid;
		}


	}
}
