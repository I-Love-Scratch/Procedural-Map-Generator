using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {
	/*
		Generates windmap based on local tiles only,
		meaning far-away tiles have no effect on a tile's wind.

		  a		relationship between a and d determines vertical tilt, determines x's wind direction
		b x c	relationship between b and c determines horizontal tilt, determines x's wind magnitude
		  d		greater sight means more tiles are considered
	*/
	[Serializable]
	public class LocalTilesWindGenerator : StepGeneratorBase, IWindGenerator {

		[SerializeField]
		[Range(1, 10)]
		[Tooltip("The radius around each tile where altitude is checked to determine wind.")]
		public int sight;

		[SerializeField]
		[Range(10, 80)]
		[Tooltip("How sensitive you want the wind to be to altitude changes.")]
		public float altitudeSensitivity;

		//the altitude difference required for max wind direction/magnitude
		private float maxWindAltDiff => 1 / altitudeSensitivity;


		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.Altitude;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.Wind;

		public LocalTilesWindGenerator(int sight = 4, float altitudeSensitivity = 40) {
			this.sight = sight;
			this.altitudeSensitivity = altitudeSensitivity;
		}
		public LocalTilesWindGenerator() : this(4, 40) { }

		/*
			Generates wind based on local altitude differences
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {
			int w = state.Width, h = state.Height;

			//apply wind to the tiles
			for (int x = 0; x < w; ++x) for (int y = 0; y < h; ++y) {

				var magnitude = getWindMagnitude(x, y, grid, state);
				var angle = getWindDirection(x, y, grid, state);
				var xComp = (float)Math.Cos(angle);
				var yComp = (float)Math.Sin(angle);

				grid[x, y].Set(xComp, yComp, angle, magnitude);
			}

			return grid;
		}

		//get wind magnitude, based on average altitude difference between tiles behind and ahead of tile
		float getWindMagnitude(int x, int y, Grid grid, IGeneratorState state) {
			var frontSum = 0.0f;
			var backSum = 0.0f;
			var numPairs = 0;
			foreach (var (x2, y2) in Utils.DiamondAround(x, y, sight, grid.Width, grid.Height)) {
				if (x2 > x) {
					numPairs++;
					frontSum += getAltForWind(grid[x2, y2], state);
				} else if (x2 < x) {
					backSum += getAltForWind(grid[x2, y2], state);
				} else {
					continue;
				}
			}

			var averageDiff = (frontSum - backSum) / numPairs;
			var windMagnitude = Math.Max(0.0000001f, 1 - Utils.Lerp(averageDiff, -maxWindAltDiff, maxWindAltDiff));
			return windMagnitude;
		}

		//get wind direction, based on average altitude difference between tiles north and south of tile
		float getWindDirection(int x, int y, Grid grid, IGeneratorState state) {
			var northSum = 0.0f;
			var southSum = 0.0f;
			var numPairs = 0;
			foreach (var (x2, y2) in Utils.DiamondAround(x, y, sight, grid.Width, grid.Height)) {
				if (y2 > y) {
					numPairs++;
					northSum += getAltForWind(grid[x2, y2], state);
				} else if (y2 < y) {
					southSum += getAltForWind(grid[x2, y2], state);
				} else {
					continue;
				}
			}

			var averageDiff = (northSum - southSum) / numPairs;
			var windAngleScale = Math.Max(0.0000001f, 1 - Utils.Lerp(averageDiff, -maxWindAltDiff, maxWindAltDiff));
			var windAngle = Utils.Scale(windAngleScale, -(float)(Math.PI / 2), (float)(Math.PI / 2));

			return windAngle;
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
	}
}
