using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Generates rivers and lakes.
		Generates rivers first, then lone lakes. Rivers can spawn a lake if they end in a local minima,
		and lakes can overflow into a new river. This can cause a recursive chain reaction.
	*/
	[Serializable]
	public class RiverLakeGenerator : StepGeneratorBase, IRiverLakeGenerator {
		const int GIVE_UP_AMOUNT = 10000000; //how many times to search before giving up
		const int MAX_TRIES_MULITPLIER = 5; //how many times to attempt to generate before giving up

		const int RIVER_SIGHT_INCREMENT = 1; //the minimum increase in river sight when spreading down
		const int MIN_RIVER_POINTS = 20; //the smallest valid number of river points
		const int MAX_RIVER_POINTS = 800; //the highest valid number of river points
		const int RIVER_HALF_WIDTH = 1; //half of a river's width, exluding the centre tile
		const float MIN_RIVER_POINT_DISTANCE = 20.0f; //the smallest valid distance between two river points

		const float LAKE_DEPTH_INCREMENT = 0.001f; //the minimum increase in lake depth when raising surface
		const int MIN_LAKE_SIZE = 200; //the smallest valid number of lake tiles
		const int MAX_LAKE_SIZE = 8000; //the highest valid number of lake tiles
		const int OVERFLOW_SIGHT_MULTIPLIER = 2; //the sight multiplier when looking for river-from-lake spawn

		[SerializeField]
		[Tooltip("Whether rivers can spawn from stone (around mountains).")]
		public bool riversFromStone = true;

		[SerializeField]
		[Tooltip("Whether lakes can spawn in a valley without a river source.")]
		public bool loneLakes = true;

		[SerializeField]
		[Tooltip("Whether lakes can spawn from the end of rivers.")]
		public bool lakesFromRivers = true;

		[SerializeField]
		[Tooltip("Whether rivers can spawn from overflowing lakes.")]
		public bool riversFromLakes = true;

		[SerializeField]
		public int numRiversWanted, numLakesWanted, sight;

		int riversGenerated, lakesGenerated;
		IGeneratorState par;
		System.Random rng;


		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.Altitude | MapAspects.OceanMountain;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.RiverLake;


		public RiverLakeGenerator(int numRiversWanted = 1, int numLakesWanted = 1, int sight = 10) {
			this.numRiversWanted = numRiversWanted;
			this.numLakesWanted = numLakesWanted;
			this.sight = sight;
		}
		public RiverLakeGenerator() : this(1, 1, 10) { }

		/*
			Spawns rivers from stone, then lone lakes. Rivers can spawn lakes, and lakes can spawn rivers.
		*/
		public override Grid Generate(Grid grid, IGeneratorState par) {
			this.par = par;
			rng = new System.Random(Seed);

			riversGenerated = 0;
			lakesGenerated = 0;
			int tries = 0;

			// Spawn rivers from stone
			if (riversFromStone) {
				while (riversGenerated < numRiversWanted && tries < (numRiversWanted * MAX_TRIES_MULITPLIER)) {
					tries++;

					//search for stone to spawn river from
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

					//check if no stone found
					if (startX == -1 && startY == -1) {
						throw new ArgumentException(nameof(startX));
					}

					startRiver((startX, startY), grid);
				}
			}

			// Spawn lone lakes
			if (loneLakes) {
				tries = 0;
				while (lakesGenerated < numLakesWanted && tries < (numLakesWanted * MAX_TRIES_MULITPLIER)) {
					tries++;

					//search for non-water tile to begin lake-spawn-search from
					int startX = -1, startY = -1;
					for (var i = 0; i < GIVE_UP_AMOUNT; i++) {
						int x = rng.Next(0, par.Width);
						int y = rng.Next(0, par.Height);
						if (grid[x, y].Biome.BiomeType != BiomeType.Ocean &&
							grid[x, y].Biome.BiomeType != BiomeType.Lake &&
							grid[x, y].Biome.BiomeType != BiomeType.River) {
							startX = x;
							startY = y;
							break;
						}
					}

					//check if no non-water tile found
					if (startX == -1 && startY == -1) {
						throw new ArgumentException(nameof(startX));
					}

					searchForLoneLakeSpawn((startX, startY), grid);
				}
			}

			return grid;
		}

		//start river without specifying second point
		void startRiver((int, int) origin, Grid grid) {
			startRiver(origin, grid, (-1, -1));
		}

		//Start river at origin, spread down. Not actually generated until generateRiver is called
		void startRiver((int, int) origin, Grid grid, (int, int) secondPoint) {
			var riverPoints = new List<(int, int)>();
			riverPoints.Add(origin);
			var i = 0;

			//add second point
			if (secondPoint != (-1, -1)) {
				riverPoints.Add(secondPoint);
				i++;
			}

			//for each "point" in the river
			for (; i < MAX_RIVER_POINTS; i++) {

				//check surroundings for other river
				var nearbyRiverTiles = new List<(int, int)>();
				for (var j = 1; j <= sight; j++) {
					foreach (var t in Utils.DiamondAroundRange(riverPoints[i].Item1, riverPoints[i].Item2, j, j, grid.Width, grid.Height)) {
						if (grid[t].BiomeType == BiomeType.River &&
							grid[t].altitude < grid[riverPoints[i]].altitude) {
							nearbyRiverTiles.Add(t);
						}
					}
					if (nearbyRiverTiles.Count() != 0) break;
				}

				//nearby river found! snap to it
				if (nearbyRiverTiles.Count() != 0) {

					//find lowest river tile
					(int, int) lowest = (0, 0);
					var lowestAlt = 1.0f;
					foreach (var t in nearbyRiverTiles) {
						if (grid[t].altitude < lowestAlt) {
							lowest = t;
							lowestAlt = grid[t].altitude;
						}
					}
					//this tile is end of river
					riverPoints.Add(lowest);
					if (riverPoints.Count >= MIN_RIVER_POINTS) { //check if river is long enough
						generateRiver(riverPoints, grid); //generate river
					}
					break; //ends function
				}

				//check surroundings for lowest point
				var lowestPoint = riverPoints[i];

				//try several times with increased sight each try
				for (var j = 1; j <= sight; j++) {
					var lowestAround = lowestPointAtDist(riverPoints[i], RIVER_SIGHT_INCREMENT * j, grid);
					if (grid[lowestAround].altitude < grid[riverPoints[i]].altitude) {
						lowestPoint = lowestAround;
						break;
					}
				}

				//if still not found, make lake in local minima or abort
				if (lowestPoint == riverPoints[i]) {
					//check if generating lakes from rivers is allowed, and if river is long enough
					if (lakesFromRivers && riverPoints.Count >= MIN_RIVER_POINTS) { 
						if (generateLake(lowestPoint, grid)) { //if lake succeeds...
							generateRiver(riverPoints, grid); //generate river
						}
					}
					break; //ends function, no river or lake generated
				}

				//add lowest point to list
				riverPoints.Add(lowestPoint);

				//check if hit ocean, lake, or edge of map
				var (lastX, lastY) = riverPoints.Last();
				if (grid[lastX, lastY].Biome.BiomeType == BiomeType.Ocean ||
					grid[lastX, lastY].Biome.BiomeType == BiomeType.Lake ||
					Utils.AtBorder(lastX, lastY, par.Width, par.Height)) {
					if (riverPoints.Count >= MIN_RIVER_POINTS) { //check if river is long enough
						generateRiver(riverPoints, grid); //generate river
					}
					break; //ends function, no river generated
				}
				//river continues
			}
		}

		//Search for local minima to spawn lone lake
		void searchForLoneLakeSpawn((int, int) origin, Grid grid) {
			var lowestPoint = origin;

			//keep going down until no lower point is found
			while (true) {

				//find lowest point at sight edge
				var lowestAround = lowestPointAtDist(lowestPoint, sight, grid);

				//if hit water, abort
				if (grid[lowestAround].BiomeType == BiomeType.Ocean ||
					grid[lowestAround].BiomeType == BiomeType.Lake ||
					grid[lowestAround].BiomeType == BiomeType.River) {
					return;
				}

				//if no lower point on edge, try whole diamond, then generate lake
				if (lowestAround == lowestPoint) {
					lowestPoint = lowestNonWaterPointInRange(lowestPoint, sight, grid);
					generateLake(lowestPoint, grid); //generate lake, can fail
					return;
				}

				//otherwise keep going
				lowestPoint = lowestAround;
			}
		}

		//generate river based on riverPoints
		void generateRiver(List<(int, int)> riverPoints, Grid grid) {
			riversGenerated++; //river is guaranteed to generate

			//remove points that are too close and add points for bezier curves
			riverPoints = riverPointSetup(riverPoints);

			var riverBiome = par.GetDefaultBiome(BiomeType.River);
			float lowestAltSoFar = 1.0f;
			bool hitLake = false; //if the river hits a lake, sets tiles to lake, not river
			IBiome lakeBiome = null; //lake biome used if hitLake

			//for each bezier curve
			for (var i = 0; i < riverPoints.Count() - 2; i += 2) {
				var (xp0, yp0) = riverPoints[i];
				var (xp1, yp1) = riverPoints[i + 1]; //only this point is from the original riverPoints
				var (xp2, yp2) = riverPoints[i + 2];

				//get all tiles between two points
				foreach (var (x, y) in Utils.BezierRange(xp0, yp0, xp1, yp1, xp2, yp2)) {

					//find lowest point in diamond pattern with range 1 greater than river (closest land)
					var localLow = grid[lowestPointInRange((x, y), RIVER_HALF_WIDTH + 1, grid)].altitude;
					if (localLow < lowestAltSoFar) lowestAltSoFar = localLow;

					//set river biome (if biome allows) in diamond pattern, possibly lower altitude
					foreach (var p in Utils.DiamondAround(x, y, RIVER_HALF_WIDTH, grid.Width, grid.Height, returnOrigo: true)) {
						//if hit lake, all future tiles in this river will be lake tiles
						//does not apply if it's the first curve, in case river spawned from lake
						if (grid[p].BiomeType == BiomeType.Lake && i != 0) {
							hitLake = true;
							lakeBiome = grid[p].Biome;
						}
						//set tile
						setRiverTile(grid[p], riverBiome, lowestAltSoFar, hitLake, lakeBiome);
					}
				}
			}
		}

		// Generate lake from origin, can call startRiver if overflowing.
		/*
		 	Starting from the origin, neighboring tiles are added to lakeSet, as long as
			they are below the altitude limit. This repeats as the altitude limit increases.
			If the lake "overflows" (suddenly gets way too big) or hits ocean/a lake/edge of map,
			the previous altitude limit is used, thanks to savedLakeSet.
		*/
		bool generateLake((int, int) origin, Grid grid) {

			//lake can fail to generate, so lakesGenerated is incremented later

			var (cx, cy) = origin;
			var targetAlt = grid[cx, cy].altitude;

			var lakeSet = new HashSet<(int, int)>(); //a set of all found valid lake tiles
			var savedLakeSet = new HashSet<(int, int)>(); //saved lake set, in case we need to backpedal
			var visitedSet = new HashSet<(int, int)>(); //all tiles visited so far

			var nextLoopQueue = new List<(int, int)>(); //the edge of the current loop queue
			var edgeOfSavedLakeSet = new List<(int, int)>(); //the edge of the previous loop queue
			nextLoopQueue.Add(origin);

			bool overflow = false;
			bool hitOceanOrLake = false;

			//until lake overflows/hits ocean or lake/gets too big
			while (!overflow && !hitOceanOrLake && targetAlt < 1.0f) {

				targetAlt += LAKE_DEPTH_INCREMENT; //increment altitude limit

				var loopQueue = new Queue<(int, int)>(); //queue is filled with starting tile(s)
				nextLoopQueue.ForEach(o => loopQueue.Enqueue(o)); //edge of previous iteration added
				edgeOfSavedLakeSet = nextLoopQueue; //used for overflow river
				nextLoopQueue = new List<(int, int)>(); //empty nextLoopQueue
				savedLakeSet.UnionWith(lakeSet); //save lake set from previous altitude increment

				//loop through all tiles that might become lakes with this altitude limit
				while (loopQueue.Count != 0) {

					var pos = loopQueue.Dequeue();
					var (x, y) = pos;

					if (grid[x, y].Biome.BiomeType == BiomeType.Mountain) continue; //cannot spread to mountain

					if (grid[x, y].altitude > targetAlt) { //is above altitude limit
						nextLoopQueue.Add(pos); //add to next queue
						continue;
					}

					if (lakeSet.Count > MAX_LAKE_SIZE) { //overflow!
						overflow = true;
						break;
					}

					if (grid[x, y].Biome.BiomeType == BiomeType.Ocean || //hit ocean!
						grid[x, y].Biome.BiomeType == BiomeType.Lake) { //hit lake!
						hitOceanOrLake = true;
						break;
					}

					//add pos to lake set
					lakeSet.Add(pos);

					//add adjacent tiles to queue, if not already in set
					foreach (var neighbour in Utils.DiamondAround(x, y, 1, grid.Width, grid.Height)) {
						if (visitedSet.Add(neighbour)) loopQueue.Enqueue(neighbour);
					}
				}
			}
			//lake tiles are now decided, attempt to generate

			//if lake is not too small
			if (savedLakeSet.Count >= MIN_LAKE_SIZE) {

				lakesGenerated++; //lake successfully generated
								  //Debug.Log("added lake!");

				//make new lake biome
				var lakeBiome = par.AddBiome(BiomeType.Lake); 

				var maxAlt = 0.0f;
				//set all tiles in savedLakeSet to lakes
				foreach (var pos in savedLakeSet) {
					var t = grid[pos];
					t.Set(lakeBiome);
					if (t.altitude > maxAlt)
						maxAlt = t.altitude;
				}
				((ILakeBiome)lakeBiome).SurfaceAltitude = maxAlt; //set surface level

				//spawn new river on overflow, if rivers from lakes are allowed
				//look for lowest tile within range from the lake's edge
				//if (overflow && riversFromLakes) { //old
				if ((overflow || hitOceanOrLake) && riversFromLakes) {
						var lowestAlt = 1.0f; //lowest altitude found
					var lowestTile = (0, 0); //lowest point within range from the lake's edge
					var lowestEdge = (0, 0); //edge tile corresponding to lowestTile
					foreach (var i in edgeOfSavedLakeSet) { //for each edge tile of the lake
						var localLowestTile = lowestNonWaterPointInRange(i, sight * OVERFLOW_SIGHT_MULTIPLIER, grid); //lowest tile
						var localLowestAlt = grid[localLowestTile].altitude; //lowest tile's alt
						if (localLowestAlt < lowestAlt) { //if new lowest
							lowestAlt = localLowestAlt;
							lowestTile = localLowestTile;
							lowestEdge = i;
						}
					}
					startRiver(lowestEdge, grid, lowestTile); //start new river here (can fail)
				}
				return true; //lake generated!
			}
			return false; //too small to generate!
		}

		//Prepare river points for generation,
		//by removing points that are too close and adding points for bezier curves
		List<(int, int)> riverPointSetup(List<(int, int)> riverPoints) {

			var tmpRiverPoints = new List<(int, int)>(riverPoints);

			//remove points that are too close to each other
			for (var i = 0; i < tmpRiverPoints.Count() - 2; i++) {
				if (Utils.Pythagoras(tmpRiverPoints[i].Item1, tmpRiverPoints[i].Item2,
					tmpRiverPoints[i + 2].Item1, tmpRiverPoints[i + 2].Item2) < MIN_RIVER_POINT_DISTANCE) {
					tmpRiverPoints.Remove(tmpRiverPoints[i + 1]);
					i = 0;
				}
			}
			riverPoints = new List<(int, int)>(tmpRiverPoints);
			tmpRiverPoints = new List<(int, int)>();

			//generate points in between each existing point, these will be the p0/p2 values
			for (var i = 0; i < riverPoints.Count() - 1; i++) {
				tmpRiverPoints.Add(riverPoints[i]);
				tmpRiverPoints.Add(Utils.Between(riverPoints[i], riverPoints[i + 1]));
			}
			tmpRiverPoints.Add(riverPoints.Last());

			//inserts duplicates of first and last, needed for first and last curve
			tmpRiverPoints.Insert(0, tmpRiverPoints.First());
			tmpRiverPoints.Add(tmpRiverPoints.Last());

			riverPoints = new List<(int, int)>(tmpRiverPoints);
			return riverPoints;
		}

		//set given tile to river if biome allows, possibly change altitude
		//if river has hit lake, set to lake instead
		void setRiverTile(Tile t, IBiome riverBiome, float maxAlt, bool hitLake, IBiome lakeBiome) {
			if (t.Biome.BiomeType != BiomeType.Ocean &&
				t.Biome.BiomeType != BiomeType.River) {

				//set biome
				if (hitLake)
					t.Set(lakeBiome);
				else
					t.Set(riverBiome);

				//lower the altitude
				if (t.altitude > maxAlt) {
					t.altitude = maxAlt;
				}
			}
		}

		//returns lowest point at given distance from origin (diamond pattern), including origin
		(int, int) lowestPointAtDist((int, int) origin, int dist, Grid grid) {
			var (lowestX, lowestY) = origin;
			float lowestAlt = grid[lowestX, lowestY].altitude;

			//for each tile in a diamond pattern
			foreach (var (x, y) in Utils.DiamondAroundRange(lowestX, lowestY, dist, dist, par.Width, par.Height)) {

				//check if new lowest point
				float newAlt = grid[x, y].altitude;
				if (newAlt < lowestAlt) {
					(lowestX, lowestY) = (x, y);
					lowestAlt = newAlt;
				}
			}
			return (lowestX, lowestY);
		}

		//returns lowest point within given range from origin (diamond pattern), including origin
		(int, int) lowestPointInRange((int, int) origin, int radius, Grid grid) {

			var (lowestX, lowestY) = origin;
			float lowestAlt = grid[lowestX, lowestY].altitude;

			//for each tile in a diamond pattern
			foreach (var (x, y) in Utils.DiamondAround(lowestX, lowestY, radius, par.Width, par.Height)) {

				//check if new lowest point
				float newAlt = grid[x, y].altitude;
				if (newAlt < lowestAlt) {
					(lowestX, lowestY) = (x, y);
					lowestAlt = newAlt;
				}
			}
			return (lowestX, lowestY);
		}

		//returns lowest non-water point within given range from origin (diamond pattern), including origin
		(int, int) lowestNonWaterPointInRange((int, int) origin, int radius, Grid grid) {

			var (lowestX, lowestY) = origin;
			float lowestAlt = grid[lowestX, lowestY].altitude;

			//for each tile in a diamond pattern
			foreach (var (x, y) in Utils.DiamondAround(lowestX, lowestY, radius, par.Width, par.Height)) {

				//check if new lowest point
				var biomeType = grid[x, y].BiomeType;
				var newAlt = grid[x, y].altitude;
				if (biomeType != BiomeType.Lake &&
					biomeType != BiomeType.Ocean &&
					newAlt < lowestAlt) {
					(lowestX, lowestY) = (x, y);
					lowestAlt = newAlt;
				}
			}
			return (lowestX, lowestY);
		}
	}
}
