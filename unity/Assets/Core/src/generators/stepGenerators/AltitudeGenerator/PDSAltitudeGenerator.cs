using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Generates map altitudes using a combination of Perlin and DiamondSquare.
	*/
	[Serializable]
	public class PDSAltitudeGenerator : StepGeneratorBase, IAltitudeGenerator {
		
		[SerializeField]
		[Range(0f, 2f)]
		[Tooltip("How much you want each tile to deviate from the average.")]
		public float randomness = 1.0f;

		[SerializeField]
		[Range(0, 9)]
		[Tooltip("How many layers you want to have done with DiamondSquare.\nEach time the map size doubles, you get 1 more layer.\nAll layers before the last few are done with Perlin.\nThe more layers are done with DS, the more smooth and less chaotic the map looks.")]
		public int dsDepth = 4; // how many layers to do with DiamondSquare.


		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.None;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.Altitude;

		public PDSAltitudeGenerator(float randomness = 1.0f, int dsDepth = 4) {
			this.randomness = randomness;
			this.dsDepth = dsDepth;
		}
		public PDSAltitudeGenerator() : this(1, 4) { }

		/*
			Generates a perlin map first, then fills in the details with diamond square and sets the altitudes.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {
			var rng = new System.Random(Seed);

			var aGrid = Utils.GeneratePerlinDiamondSquareMap(state.Width, state.Height, dsDepth, randomness, rng.Next(0, int.MaxValue));
			
			//set noise map values to tiles
			for (var x = 0; x < state.Width; x++) {
				for (var y = 0; y < state.Height; y++) {
					grid[x, y].Set(aGrid[x, y]);
				}
			}

			return grid;
		}

	}
}
