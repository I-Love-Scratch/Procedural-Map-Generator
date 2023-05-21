using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Generates map humidity and temperature using diamond square.
	*/
	[Serializable]
	public class DiamondSquareTemperatureHumidityGenerator : StepGeneratorBase, ITemperatureHumidityGenerator {
		[SerializeField]
		[Range(0f, 2f)]
		[Tooltip("How much temperature should vary.")]
		public float tRandomness;

		[SerializeField]
		[Range(0f, 2f)]
		[Tooltip("How much humidity should vary.")]
		public float hRandomness;

		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.None;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.TemperatureHumidity;

		public DiamondSquareTemperatureHumidityGenerator(float tRandomness = 1.0f, float hRandomness = 1.0f) {
			this.tRandomness = tRandomness;
			this.hRandomness = hRandomness;
		}
		public DiamondSquareTemperatureHumidityGenerator() : this(1, 1) { }

		/*
			Generates a diamond square map for both temperature and humidity, then sets tiles.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {
			var rng = new System.Random(Seed);

			var temperatureGen = Utils.GenerateDiamondSquareMap(state.Width, state.Height,
				tRandomness, rng.Next(0, int.MaxValue));
			var humidityGen = Utils.GenerateDiamondSquareMap(state.Width, state.Height,
				hRandomness, rng.Next(0, int.MaxValue));

			for (int y = 0; y < state.Height; ++y) for (int x = 0; x < state.Width; ++x) {
				var t = grid[x, y];
				t.Set(temperatureGen[x, y], humidityGen[x, y]);
			}

			return grid;
		}
	}
}
