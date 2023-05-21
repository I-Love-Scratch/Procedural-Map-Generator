using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Generates rivers, but not lakes.
		Finds random stone point, then closest ocean point. Sets curve shape regardless of altitude,
		then adjusts altitude of each tile so the river never flows upwards.
	*/
	[Serializable]
	public class ClosestOceanRiverGenerator : StepGeneratorBase, IRiverLakeGenerator {
		const int GIVE_UP_AMOUNT = 10000000; //how many times to search for stone before giving up
		const int MAX_TRIES_MULITPLIER = 5; //how many times to attempt to generating a river before giving up
		const int RIVER_HALF_WIDTH = 1; //half of a river's width, exluding the centre tile

		[SerializeField]
		[Tooltip("Whether river tiles should have altitude adjusted to ensure they never flow upwards.")]
		public bool adjustAltitude = true;

		[SerializeField]
		public int numRiversWanted, minRiverLength, maxRiverLength, numCurves;

		[SerializeField]
		public float curveSize;

		int riversGenerated;
		IGeneratorState par;
		System.Random rng;


		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.Altitude | MapAspects.OceanMountain;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.RiverLake;


		public ClosestOceanRiverGenerator(int numRiversWanted = 1,
			int minRiverLength = 50, int maxRiverLength = 1000,
			int numCurves = 2, float curveSize = 1) {
			this.numRiversWanted = numRiversWanted;
			this.minRiverLength = minRiverLength;
			this.maxRiverLength = maxRiverLength;
			this.numCurves = numCurves;
			this.curveSize = curveSize;
		}
		public ClosestOceanRiverGenerator() : this(1, 50, 1000, 2, 1) { }

		/*
			For each river, looks for a start point, then finds closest ocean and makes curve.
		*/
		public override Grid Generate(Grid grid, IGeneratorState par) {
			this.par = par;
			rng = new System.Random(Seed);

			riversGenerated = 0;
			int tries = 0;

			// Spawn rivers
			while (riversGenerated < numRiversWanted && tries < (numRiversWanted * MAX_TRIES_MULITPLIER)) {
				tries++;

				//find start point of new river
				var startPoint = findStartPoint(grid);
				if(startPoint == (-1, -1)) continue; //finding start point failed

				//find end point of new river
				var endPoint = findEndPoint(startPoint, grid);
				if(endPoint == (-1, -1)) continue; //finding end point failed


				//get all river tiles based on start and end point
				var bezierPoints = getBezierPoints(startPoint, endPoint);
				var riverTiles = getRiverTiles(bezierPoints, grid);

				if (riverTiles.Count > 0) { //if not empty
					riversGenerated++; //increment number of rivers

					//set river tiles in grid
					setRiverBiomes(riverTiles, grid);

					//adjust altitude if chosen
					if (adjustAltitude)
						adjustRiverAltitudes(riverTiles, grid);
				}
			}
			return grid;
		}

		//get the coordinates of a stone tile, where a river can start
		//returns (-1, -1) on failure
		(int, int) findStartPoint(Grid grid) {
			//random coords
			int startX = -1, startY = -1;
			for (var i = 0; i < GIVE_UP_AMOUNT; i++) {
				int x = rng.Next(0, par.Width);
				int y = rng.Next(0, par.Height);
				if (grid[x, y].Biome.BiomeType == BiomeType.Stone) {
					startX = x;
					startY = y;
					break;
				}
			}

			return (startX, startY);
		}

		//get the coordinates of the closest ocean tile, where a river can end
		//returns (-1, -1) on failure
		(int, int) findEndPoint((int, int) start, Grid grid) {
			for (var i = minRiverLength; i < maxRiverLength; i++) { //for each valid river length
				foreach (var pos in Utils.DiamondAroundRange( //check this range
					start.Item1, start.Item2, i, i, grid.Width, grid.Height)) {
					if (grid[pos].BiomeType == BiomeType.Ocean) { //if ocean tile, return pos
						return pos;
					}
				}
			}
			return (-1, -1); //no valid point found
		}

		//get bezier curve points of river, based only on startPoint and endPoint
		List<(int, int)> getBezierPoints((int, int) startPoint, (int, int) endPoint) {
			var bezierPoints = new List<(int, int)>();

			bool rightHand;

			var p0 = (-1, -1);
			var p1 = (-1, -1);
			var p2 = startPoint;

			//for each curve, add p0 and p1, as p2 is just the next curve's p0
			for (var i = 0; i < numCurves; i++) {
				//rightHand alternates
				if (i % 2 == 0) 
					rightHand = true;
				else
					rightHand = false;

				p0 = p2; //get p0, which is just the last curve's p2
				p2 = Utils.BetweenDivide(p0, endPoint, numCurves - i); //get p2, by dividing the line
				p1 = Utils.PerpendicularPoint(p0, p2, curveSize, rightHand); //get p1, the perpendicular point
				bezierPoints.Add(p0); //add p0
				bezierPoints.Add(p1); //add p1
			}
			bezierPoints.Add(p2); //add final p2

			return bezierPoints;
		}

		//get all tiles in river, using bezier curve points
		List<(int, int)> getRiverTiles(List<(int, int)> bezierPoints, Grid grid) {
			var riverTiles = new List<(int, int)>();

			//for each bezier curve
			for (var i = 0; i < bezierPoints.Count() - 2; i += 2) {
				var (xp0, yp0) = bezierPoints[i];
				var (xp1, yp1) = bezierPoints[i + 1];
				var (xp2, yp2) = bezierPoints[i + 2];

				//get all tiles in range
				foreach (var (x, y) in Utils.BezierRange(xp0, yp0, xp1, yp1, xp2, yp2)) {
					if (Utils.OutOfBounds(x, y, grid.Width, grid.Height) || //out of bounds!
						grid[(x, y)].BiomeType == BiomeType.River || //hit other river!
						grid[(x, y)].BiomeType == BiomeType.Mountain) //hit mountain!
						return new List<(int, int)>(); //return empty

					riverTiles.Add((x, y)); //add tile

					//add nearby tiles based on river width
					foreach (var pos in Utils.DiamondAround(x, y, RIVER_HALF_WIDTH, grid.Width, grid.Height)) {
						if (!riverTiles.Contains(pos))
							riverTiles.Add(pos);
					}
				}
			}
			return riverTiles;
		}

		//set biomes on river tiles in grid
		void setRiverBiomes(List<(int, int)> riverTiles, Grid grid) {
			foreach (var pos in riverTiles) {
				if (grid[pos].BiomeType != BiomeType.Ocean) //if not ocean
					grid[pos].Biome = par.GetDefaultBiome(BiomeType.River); //set to river
			}
		}

		//adjust altitudes of tiles where needed, so river never flows up
		void adjustRiverAltitudes(List<(int, int)> riverTiles, Grid grid) {
			float lowestSoFar = 1.0f;
			foreach (var pos in riverTiles) {
				if (grid[pos].altitude < lowestSoFar)
					lowestSoFar = grid[pos].altitude;
				else
					grid[pos].altitude = lowestSoFar;
			}
		}

	}
}
