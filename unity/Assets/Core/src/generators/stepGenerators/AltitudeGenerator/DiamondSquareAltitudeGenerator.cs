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
	public class DiamondSquareAltitudeGenerator : StepGeneratorBase, IAltitudeGenerator {
		[SerializeField]
		[Range(0f, 2f)]
		[Tooltip("How much you want the tiles to deviate from their average.")]
		public float randomness; 


		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.None;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.Altitude;

		public DiamondSquareAltitudeGenerator(float randomness = 1.0f) {
			this.randomness = randomness;
		}
		public DiamondSquareAltitudeGenerator() : this(1) { }

		/*
			Generates a diamond square heighmap and sets the altitudes.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {

			//generate noise map
			var noise = Utils.GenerateDiamondSquareMap(state.Width, state.Height, randomness, Seed);
			//set noise map values to tiles
			for (var x = 0; x < state.Width; x++) {
				for (var y = 0; y < state.Height; y++) {
					grid[x, y].Set(noise[x, y]);
				}
			}

			return grid;
		}

	}
}
