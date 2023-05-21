using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Places resources, each occupying only 1 tile.
		This is an old generator, but is still functional.
	*/
	[Serializable]
	public class PrimitiveResourceGenerator : StepGeneratorBase, IResourceGenerator {
		
		// How many times a resource is attempted to be placed before giving up.
		const int MAX_TRIES = 200;

		// A dictionary of resource types and their distribution rate.
		public Dictionary<ResourceType, float> ResourceDist = new Dictionary<ResourceType, float>{ 
			{ ResourceType.Gold, 1f },
			{ ResourceType.Iron, 1.5f },
			{ ResourceType.Copper, 2f },
			{ ResourceType.Berries, 3f },
			{ ResourceType.Cotton, 1.5f },
			{ ResourceType.Spice, 1f },
		};


		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.Biomes;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.Resources;

		private System.Random rng;

		public PrimitiveResourceGenerator() { }


		/*
			Places resorces, checking if biomes are valid each time.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {

			rng = new System.Random(Seed);

			var scale = ShouldUseMapScaleFactor ? state.MapScalingFactor : 1;

			// Clean the ResourceDist and scale it to the map scaling.
			var resourceDist = new Dictionary<ResourceType, float>();
			foreach(var (type, amount) in ResourceDist) {
				// Ignore any invalid types and skip Empty.
				resourceDist[type] = amount * scale;
			}

			// Start planting the resources.
			foreach(var (type, amount) in resourceDist) {
				var amountLeft = amount;

				// Get method for finding a valid tile.
				_tileFinder tileFinder = type switch {
					ResourceType.Gold => _findValidGoldTile,
					ResourceType.Iron => _findValidIronTile,
					ResourceType.Copper => _findValidCopperTile,
					ResourceType.Berries => _findValidBerriesTile,
					ResourceType.Cotton => _findValidCottonTile,
					ResourceType.Spice => _findValidSpiceTile,
					_ => null,
				};

				if(tileFinder == null) {
					Logger.Log($"Skipping resource of type {type}");
					continue;
				}

				// Try until we run out of tries or fill our quota.
				int i = MAX_TRIES;
				while(i > -1 && amountLeft > 0.01f) {
					var pos = tileFinder(grid, ref i);

					if(pos == (-1, -1)) continue;

					// Get density of resource spot.
					float density = 0.5f;
					amountLeft -= density;

					var t = grid[pos];

					var res = new TileResource(state.AddResource(type), density);

					t.Resource = res;
				}
			}

			return grid;
		}


		// Searches for a valid tile in the grid, counts down i, stops when i is 0 or pos is found.
		delegate (int, int) _tileFinder(Grid grid, ref int i);

		private (int, int) _findValidGoldTile(Grid grid, ref int i) {
			while(--i > -1) {
				var pos = (rng.Next(grid.Width), rng.Next(grid.Height));
				var t = grid[pos];
				if(t.Resource != null) continue;
				if(t.BiomeType == BiomeType.Stone) return pos;
			}
			return (-1, -1);
		}
		private (int, int) _findValidIronTile(Grid grid, ref int i) {
			while(--i > -1) {
				var pos = (rng.Next(grid.Width), rng.Next(grid.Height));
				var t = grid[pos];
				if(t.Resource != null) continue;
				if(t.BiomeType == BiomeType.Stone) return pos;
			}
			return (-1, -1);
		}
		private (int, int) _findValidCopperTile(Grid grid, ref int i) {
			while(--i > -1) {
				var pos = (rng.Next(grid.Width), rng.Next(grid.Height));
				var t = grid[pos];
				if(t.Resource != null) continue;
				if(t.BiomeType == BiomeType.Stone) return pos;
			}
			return (-1, -1);
		}

		private (int, int) _findValidBerriesTile(Grid grid, ref int i) {
			while(--i > -1) {
				var pos = (rng.Next(grid.Width), rng.Next(grid.Height));
				var t = grid[pos];
				if(t.Resource != null) continue;
				if(t.BiomeType == BiomeType.Grasslands || t.BiomeType == BiomeType.Forest || t.BiomeType == BiomeType.Swamp) 
					return pos;
			}
			return (-1, -1);
		}
		private (int, int) _findValidCottonTile(Grid grid, ref int i) {
			while(--i > -1) {
				var pos = (rng.Next(grid.Width), rng.Next(grid.Height));
				var t = grid[pos];
				if(t.Resource != null) continue;
				if(t.BiomeType == BiomeType.Grasslands || t.BiomeType == BiomeType.Forest || t.BiomeType == BiomeType.Swamp)
					return pos;
			}
			return (-1, -1);
		}
		private (int, int) _findValidSpiceTile(Grid grid, ref int i) {
			while(--i > -1) {
				var pos = (rng.Next(grid.Width), rng.Next(grid.Height));
				var t = grid[pos];
				if(t.Resource != null) continue;
				if(t.BiomeType == BiomeType.Grasslands || t.BiomeType == BiomeType.Forest || t.BiomeType == BiomeType.Swamp)
					return pos;
			}
			return (-1, -1);
		}

	}
}
