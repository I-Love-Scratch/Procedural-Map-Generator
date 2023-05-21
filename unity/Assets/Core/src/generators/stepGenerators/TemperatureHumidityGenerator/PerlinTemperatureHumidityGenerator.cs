using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Generates map humidity and temperature using perlin.
	*/
	[Serializable]
	public class PerlinTemperatureHumidityGenerator : StepGeneratorBase, ITemperatureHumidityGenerator {

		[SerializeField]
		[Range(0f, 1000f)]
		[Tooltip("How zoomed out the perlin maps should be.")]
		public float scale = 200.0f;

		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.None;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.TemperatureHumidity;

		public PerlinTemperatureHumidityGenerator(float scale = 200.0f) {
			this.scale = scale;
		}
		public PerlinTemperatureHumidityGenerator() : this(200.0f) { }

		/*
			Generates a perlin map for both temperature and humidity, then sets tiles.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {
			var rng = new System.Random(Seed);
			var temperatureGen = PerlinGenerator.GenerateNoiseMap(state.Width, state.Height, scale, rng.Next(0, int.MaxValue),
				7, 2.0f, 0.4f, new System.Numerics.Vector2(1, 1));
			var humidityGen = PerlinGenerator.GenerateNoiseMap(state.Width, state.Height, scale, rng.Next(0, int.MaxValue),
				7, 2.0f, 0.5f, new System.Numerics.Vector2(1, 1));

			for (int y = 0; y < state.Height; ++y) for (int x = 0; x < state.Width; ++x) {
				var t = grid[x, y];
				t.Set(temperatureGen[x, y], humidityGen[x, y]);
			}

			return grid;
		}
	}
}
