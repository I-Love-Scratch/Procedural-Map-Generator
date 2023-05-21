using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Makes resource blobs using lazy flood fill algorithm.
	*/
	[Serializable]
	public partial class FloodFillResourceGenerator : StepGeneratorBase, IResourceGenerator {

		// How many times a seed is attempted to be placed before giving up.
		const int MAX_TRIES = 200;

		// A dictionary of resource types and their parameters.
		public Dictionary<ResourceType, ResourceParams> Resources = new Dictionary<ResourceType, ResourceParams>{
			{ ResourceType.Gold,	new ResourceParams(new List<BiomeType>{BiomeType.Stone}, 1, 0.9f) },
			{ ResourceType.Iron,	new ResourceParams(new List<BiomeType>{BiomeType.Stone}, 2, 0.99f) },
			{ ResourceType.Copper,	new ResourceParams(new List<BiomeType>{BiomeType.Stone}, 3, 0.95f) },
			{ ResourceType.Berries, new ResourceParams(new List<BiomeType>{BiomeType.Jungle}, 3, 0.99f) },
			{ ResourceType.Cotton,	new ResourceParams(new List<BiomeType>{BiomeType.Jungle}, 2, 0.95f) },
			{ ResourceType.Spice,	new ResourceParams(new List<BiomeType>{BiomeType.Jungle}, 1, 0.999f) },
		};

		[SerializeField]
		[Range(1, 10)]
		[Tooltip("How many resources can be on a single tile.")]
		public int resourceOverlap = 1;

		[SerializeField]
		[Tooltip("Whether a resource can spread to a different biome than where it spawned.")]
		public bool escapeBiome = false;

		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.Biomes;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.Resources;

		private System.Random rng;

		public FloodFillResourceGenerator(int resourceOverlap = 1, bool escapeBiome = false) {
			this.resourceOverlap = resourceOverlap;
			this.escapeBiome = escapeBiome;
		}
		public FloodFillResourceGenerator() : this(1, false) { }

		/*
			Set resource seeds, then spread with lazy flood fill.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {

			// List of resource seeds added to the map.
			var seedList = new List<(TileResource, (int, int))>();

			rng = new System.Random(Seed);

			//set seeds in seedList
			placeResourceSeeds(grid, state, seedList);

			//spread into resource blobs, set values in grid
			floodFillResourceSpread(grid, state, seedList); 

			return grid;
		}


		private void placeResourceSeeds(Grid grid, IGeneratorState state, List<(TileResource, (int, int))> seedList) {
			var scale = ShouldUseMapScaleFactor ? state.MapScalingFactor : 1;
			// For each resource
			foreach (var (type, par) in Resources) {
				var amountLeft = par.numSpawns * scale;

				// Try until we run out of tries or fill our quota.
				int i = MAX_TRIES;
				while (i > -1 && amountLeft > 0.01f) {
					var pos = findValidResourceTile(grid, ref i, par.validBiomes);
					if (pos == (-1, -1)) continue;

					// Get density of resource spot.
					float density = (float)rng.NextDouble() * 0.5f + 0.5f; // [0.5, 1]
					amountLeft -= density;

					//make new resource
					var seedRes = new TileResource(state.AddResource(type), density);
					seedList.Add((seedRes, pos)); //Add to list
				}
			}
		}

		/*
			Lazy flood fill.
		*/
		private void floodFillResourceSpread(Grid grid, IGeneratorState state, List<(TileResource, (int, int))> seedList) {

			//TODO: shuffle seedList?

			//for each resource seed
			foreach (var (seedRes, seedPos) in seedList) {

				float chance = 1;
				float decay = Resources[seedRes.ResourceType].spreadDecay;
				var validBiomes = Resources[seedRes.ResourceType].validBiomes;

				var queue = new Queue<(int, int)>();
				queue.Enqueue(seedPos);
				var visited = new HashSet<(int, int)>();
				visited.Add(seedPos);
				
				//while queue is not empty
				while (queue.Count() > 0) {
					var p = queue.Dequeue(); //pop pos from queue

					//set resource in grid
					grid[p].Resource = new TileResource(seedRes.Resource, seedRes.Density * chance);

					if (chance >= rng.NextDouble()) { //spread chance
						//for 4 neighbors
						foreach (var newP in Utils.DiamondAround(p, 1, (grid.Width, grid.Height))) {
							var b = grid[newP].BiomeType;

							var spreadResource = (!visited.Contains(newP)   //if not visted
								&& grid[newP].Resources.Count < resourceOverlap); //and has space for more resources
							spreadResource = spreadResource && ( escapeBiome //if escapeBiome, check for invalid biomes
								? (b != BiomeType.Mountain && b != BiomeType.Ocean
									&& b != BiomeType.River && b != BiomeType.Lake)
								: (validBiomes.Contains(b))); //otherwise, check valid biome list
							
							if (spreadResource) {
								queue.Enqueue(newP);    //add neighbors to queue
								visited.Add(newP);      //set neighbors as visited
							}
						}
					}
					//decrease chance by decay factor
					chance *= decay;
				}
			}
		}

		/*
			Find resource seed spawn point.
		*/
		private (int, int) findValidResourceTile(Grid grid, ref int i, IList<BiomeType> validBiomeList) {
			while(--i > -1) { //until too many tries
				var pos = (rng.Next(grid.Width), rng.Next(grid.Height));
				var t = grid[pos];
				if(t.Resource != null) continue; //cannot spawn on another resource
				if (validBiomeList.Contains(t.BiomeType)) return pos; //valid biome
			}
			return (-1, -1);
		}

	}

	public partial class FloodFillResourceGenerator {
		/*
			A class representing resource parameters.
		*/
		public class ResourceParams {
			public float numSpawns;
			public float spreadDecay;
			public IList<BiomeType> validBiomes;

			public ResourceParams(IList<BiomeType> _validBiomes, float _numSpawns = 1, float _spreadDecay = 0.5f) {
				numSpawns = _numSpawns;
				spreadDecay = _spreadDecay;
				validBiomes = _validBiomes;
			}
		}
	}
}
