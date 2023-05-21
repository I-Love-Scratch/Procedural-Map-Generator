using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace terrainGenerator {

	/*
		A generator that creates perlin values in 2D space.
	*/
	public class PerlinGenerator {
		const int SAMPLE_SIZE = 16; // The width and height of the sample area. (Used to estimate min/max noise)
		const float COEF_GRID_SIZE_MAX = 1000000; // The size of grid where the coefficient reaches 100%


		// The width of the generated area.
		public int Width { get; }

		// The height of the generated area.
		public int Height { get; }

		// The seed used for randomization.
		public int Seed { get; }

		// The scale. (This acts similar to a zoom, lower scale makes it look like a zoomed in version)
		public float Scale { get; }

		// How many different noise values to put together. (Used to add smaller variation between each value)
		public int Octaves { get; }

		// How much frequency increases each octave. (Range [1, INF])
		// Higher frequency means more variation.
		public float Lacunarity { get; }

		// How much amplitude reduces each octave. (Range: (0, 1])
		// Lower persistence causes subsequent octaves to have less impact overall.
		public float Persistance { get; }

		// The offset of all noises.
		// Changing the offset makes it look like the same map at a different point.
		public Vector2 Offset { get; }


		// The minimum noise value. (Used to convert the noise to range [0, 1])
		public float MinNoiseHeight { get; private set; }

		// The maximum noise value. (Used to convert the noise to range [0, 1])
		public float MaxNoiseHeight { get; private set; }

		// The starting points for each octave.
		public Vector2[] OctOffsets { get; private set; }


		public PerlinGenerator(int width, int height, float scale, int seed,
			int octaves, float lacunarity, float persistance, Vector2 offset) {
			Width = width;
			Height = height;
			Scale = (scale <= 0) ? 0.001f : scale;
			Seed = seed;
			Octaves = octaves;
			Lacunarity = lacunarity;
			Persistance = persistance;
			Offset = offset;


			GetOctaves();
			FindRange();
		}

		/*
			Gets the noise value for a given tile. (Range: [0, 1])
		*/
		public float this[int x, int y] => Get(x, y).Lerp(MinNoiseHeight, MaxNoiseHeight);


		/*
			Gets the octave offsets.
		*/
		private void GetOctaves() {
			var rng = new Random(Seed);

			OctOffsets = new Vector2[Octaves];

			for (int i = 0; i < Octaves; i++) {
				float offsetX = rng.Next(-100000, 100000) + Offset.X;
				float offsetY = rng.Next(-100000, 100000) + Offset.Y;
				OctOffsets[i] = new Vector2(offsetX, offsetY);
			}
		}

		/*
			Estimates the min and max noise values.
			Achieves this by sampling a subset of the area.
		*/
		private void FindRange() {

			// Get the range for a sample of the grid.
			var maxNoiseSampled = float.MinValue;
			var minNoiseSampled = float.MaxValue;

			// Get the distance between each sample tile.
			int stepX = Width / SAMPLE_SIZE, stepY = Height / SAMPLE_SIZE;

			// Get the minimum and maximum of the sample tiles.
			for (int y = 0; y < SAMPLE_SIZE; y++) {
				for (int x = 0; x < SAMPLE_SIZE; x++) {
					float noiseHeight = Get(x * stepX, y * stepY);

					if (noiseHeight > maxNoiseSampled) maxNoiseSampled = noiseHeight;
					if (noiseHeight < minNoiseSampled) minNoiseSampled = noiseHeight;
				}
			}
			MinNoiseHeight = minNoiseSampled * 1.15f;
			MaxNoiseHeight = maxNoiseSampled * 1.15f;
		}

		/*
			Tries out and prints different formulas for estimating the min and max noise values.
		*/
		private void TestFindRange() {
			
			// Get the largest range mathematically possible.
			float maxNoiseLim, minNoiseLim;
			maxNoiseLim = 0;
			for (var o = 0; o < Octaves; o++) {
				maxNoiseLim += (float)Math.Pow(Persistance, o);
			}
			minNoiseLim = -maxNoiseLim;


			/*
				Get the actual range for this grid.
			*/
			var maxNoiseReal = float.MinValue;
			var minNoiseReal = float.MaxValue;

			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {
					float noiseHeight = Get(x, y);
                    
					if (noiseHeight > maxNoiseReal) maxNoiseReal = noiseHeight;
					if (noiseHeight < minNoiseReal) minNoiseReal = noiseHeight;
				}
			}
			// Set the ranges.
			MaxNoiseHeight = maxNoiseReal;
			MinNoiseHeight = minNoiseReal;

			// Get the range for a sample of the grid.
			var maxNoiseSampled = float.MinValue;
			var minNoiseSampled = float.MaxValue;

			// Get the distance between each sample tile.
			int stepX = Width / SAMPLE_SIZE, stepY = Height / SAMPLE_SIZE;

			for (int y = 0; y < SAMPLE_SIZE; y++) {
				for (int x = 0; x < SAMPLE_SIZE; x++) {
					float noiseHeight = Get(x * stepX, y * stepY);

					if (noiseHeight > maxNoiseSampled) maxNoiseSampled = noiseHeight;
					if (noiseHeight < minNoiseSampled) minNoiseSampled = noiseHeight;
				}
			}
			var minNoiseAdjust = minNoiseSampled * 1.15f;
			var maxNoiseAdjust = maxNoiseSampled * 1.15f;


			// Get the coefficient for estimating. (GridSize - SampleSize) / COEF_GRID_SIZE_MAX
			var coef = Utils.Clamp((Width * Height - SAMPLE_SIZE * SAMPLE_SIZE) / COEF_GRID_SIZE_MAX);
			var minNoiseHeight = minNoiseLim * coef + minNoiseSampled * (1 - coef);
			var maxNoiseHeight = maxNoiseLim * coef + maxNoiseSampled * (1 - coef);

			var coef2 = Utils.SmootStep(coef);
			var minNoiseHeight2 = minNoiseLim * coef2 + minNoiseSampled * (1 - coef2);
			var maxNoiseHeight2 = maxNoiseLim * coef2 + maxNoiseSampled * (1 - coef2);

			var coef3 = (float) Math.Pow(coef, 0.5);
			var minNoiseHeight3 = minNoiseLim * coef3 + minNoiseSampled * (1 - coef3);
			var maxNoiseHeight3 = maxNoiseLim * coef3 + maxNoiseSampled * (1 - coef3);

			
			Logger.Log($@"
			Max:       ({minNoiseLim:N3}, {maxNoiseLim:N3})
			Actual:    ({MinNoiseHeight:N3}, {MaxNoiseHeight:N3})
			Sample:    ({minNoiseSampled:N3}, {maxNoiseSampled:N3})
			SAdjust:   ({minNoiseAdjust:N3}, {maxNoiseAdjust:N3})
			CoefAdj:   ({minNoiseHeight:N3}, {maxNoiseHeight:N3}) {coef:N3}
			CoefAdj2:  ({minNoiseHeight2:N3}, {maxNoiseHeight2:N3}) {coef2:N3}
			CoefAdj3:  ({minNoiseHeight3:N3}, {maxNoiseHeight3:N3}) {coef3:N3}");
		}

		/*
			Gets the noise value for a given tile.
		*/
		private float Get(int x, int y) {
			float frequency = 1;
			float noiseHeight = 0;
			float amplitude = 1;
			for (int i = 0; i < Octaves; i++) {
				float sampleX = (float)x / Scale * frequency + OctOffsets[i].X;
				float sampleY = (float)y / Scale * frequency + OctOffsets[i].Y;
				//float perlinNoise = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
				float perlinNoise = Perlin.GetNoise(sampleX, sampleY) * 2;// - 1;
				//float perlinNoise = (float)rng.NextDouble() * 2 - 1;
				noiseHeight += perlinNoise * amplitude;

				amplitude *= Persistance;
				frequency *= Lacunarity;
			}

			return noiseHeight;
		}


		/* 
			This generates a grid of perlin noises for an area.
			This does it all in one go, as opposed to the generator, which generates them on demand.
			Based on: https://github.com/HiGal/2D_terrain_generation/blob/master/Assets/Noise.cs
		*/
		public static float[,] GenerateNoiseMap(int width, int height, float scale, int seed, int octaves, float lacunarity, float persistance, Vector2 offset) {
			var rng = new Random(seed);
			float[,] noiseMap = new float[width, height];
			Vector2[] octOffsets = new Vector2[octaves];

			for (int i = 0; i < octaves; i++) {
				float offsetX = rng.Next(-100000, 100000) + offset.X;
				float offsetY = rng.Next(-100000, 100000) + offset.Y;
				octOffsets[i] = new Vector2(offsetX, offsetY);
			}

			if (scale <= 0) {
				scale = 0.001f;
			}

			float maxNoiseHeight = float.MinValue;
			float minNoiseHeight = float.MaxValue;

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float frequency = 1;
					float noiseHeight = 0;
					float amplitude = 1;
					for (int i = 0; i < octaves; i++) {
						float sampleX = (float)x / scale * frequency + octOffsets[i].X;
						float sampleY = (float)y / scale * frequency + octOffsets[i].Y;
						//float perlinNoise = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
						float perlinNoise = Perlin.GetNoise(sampleX, sampleY) * 2 - 1;
						//float perlinNoise = (float)rng.NextDouble() * 2 - 1;
						noiseHeight += perlinNoise * amplitude;

						amplitude *= persistance;
						frequency *= lacunarity;
					}

					if (noiseHeight > maxNoiseHeight) {
						maxNoiseHeight = noiseHeight;
					} else if (noiseHeight < minNoiseHeight) {
						minNoiseHeight = noiseHeight;
					}
					noiseMap[x, y] = noiseHeight;



				}
			}

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					noiseMap[x, y] = Utils.Clamp(Utils.Lerp(noiseMap[x, y], minNoiseHeight, maxNoiseHeight));
				}
			}

			return noiseMap;
		}
	}
}
