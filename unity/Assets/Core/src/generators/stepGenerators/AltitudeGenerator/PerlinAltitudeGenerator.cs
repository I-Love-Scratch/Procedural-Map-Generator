using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Generates map altitudes using perlin.
	*/
	[Serializable]
	public class PerlinAltitudeGenerator : StepGeneratorBase, IAltitudeGenerator {

		[SerializeField]
		[Range(0f, 1000f)]
		[Tooltip("How zoomed out the perlin map should be.")]
		public float scale = 150.0f;

		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.None;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.Altitude;

		public PerlinAltitudeGenerator(float scale = 150.0f) {
			this.scale = scale;
		}
		public PerlinAltitudeGenerator() : this(150.0f) { }

		/*
			Generates a noisemap with perlin and sets the altitudes.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {
			var rng = new System.Random(Seed);
			var altitudeGen = PerlinGenerator.GenerateNoiseMap(state.Width, state.Height, scale, rng.Next(0, int.MaxValue),
				7, 2.0f, 0.6f, new System.Numerics.Vector2(1, 1));
			for (int y = 0; y < state.Height; ++y) for (int x = 0; x < state.Width; ++x) {
					float noiseVal = altitudeGen[x, y];

					grid[x, y].Set(altitudeGen[x, y]);
			}

			return grid;
		}
	}
}
