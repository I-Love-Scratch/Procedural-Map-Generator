using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Makes biome blobs using cellular automata rules.
		This generator is old and unoptimized, but fully functional.
	*/
	[Serializable]
	public class ExpandingBiomeGenerator : StepGeneratorBase, IBiomeGenerator {

		/*
			Class that contains information about a biome only relevant for this generator.
		*/
		public class BiomeInfo {
			public int Id; //get biome id
			public float SpreadRate; //spreadrate of instance, used in tandem with biometype spreadrate
			public float AgeRate; //agerate of instance, used in tandem with biometype agerate
		}

		// Dictionary mapping an id to a biome.
		Dictionary<int, BiomeInfo> _biomeInfo = new Dictionary<int, BiomeInfo>();
		// List of coordinates of all seeds.
		List<(int x, int y)> seedCoords = new List<(int x, int y)>();

		protected IGeneratorState par; //parameter for the generation
		protected System.Random rng; //random number generator
		private Grid _grid;


		// Map aspects needed for this step generator to work.
		public override MapAspects RequiredMapAspects => MapAspects.Altitude | MapAspects.OceanMountain;

		// Map aspects this generator applies.
		public override MapAspects AddedMapAspects => MapAspects.Biomes;


		/*
			Spawns biome seeds, the expands them with cellular automata rules.
		*/
		public override Grid Generate(Grid grid, IGeneratorState state) {
			this.par = state;
			rng = new System.Random(Seed);

			_biomeInfo = new Dictionary<int, BiomeInfo>();
			seedCoords = new List<(int x, int y)>();

			_grid = grid;

			SpawnBiomeSeeds();
			ExpandBiomes();
			FillBlanks();
			RemoveStraySeeds();

			return grid;
		}


		/*
			Spawn biome seeds on map based on biome types
		*/
		void SpawnBiomeSeeds() {
			//get list of all biome types 
			var biomeTypes = Enum.GetValues(typeof(BiomeType))
				.Cast<BiomeType>()
				.Skip(1)
				.Select(t => t.GetObj())
				.ToList();

			var scale = ShouldUseMapScaleFactor ? par.MapScalingFactor : 1;

			//spawn multiple seeds for each biome type
			foreach(var type in biomeTypes) {
				var cap = rng.Next(type.biomeSpawnMin, type.biomeSpawnMax) * scale;
				for(var i = 0; i <= cap; i++) {
					spawnSeed(type);
				}
			}
		}

		public const int MAX_SEED_SPAWN_TRIES = 100;
		/*
			Place a seed
		*/
		void spawnSeed(BiomeTypeObj type) {
			const float MAX_SPREAD_M = 1.0f; //max possible spread mod
			const float MIN_SPREAD_M = 0.1f; //min possible spread mod
			const float MAX_AGE_R = 1.0f; //max possible age rate
			const float MIN_AGE_R = 0.5f; //min possible age rate

			//randomise spread/age rate based on biome spread/age rates
			var spreadMod = type.spreadMod * Utils.Scale((float)rng.NextDouble(), MIN_SPREAD_M, MAX_SPREAD_M);
			var ageRate = type.ageRate * Utils.Scale((float)rng.NextDouble(), MIN_AGE_R, MAX_AGE_R);

			//finds random empty tile
			Tile t = null;
			int x = 0, y = 0;
			for(int i = 0; i < MAX_SEED_SPAWN_TRIES; ++i) {
				x = rng.Next(0, par.Width);
				y = rng.Next(0, par.Height);
				t = _grid[x, y];
				if(t.Biome.BiomeType == BiomeType.Empty) break;
			}

			if(t == null || t.Biome.BiomeType != BiomeType.Empty) return;

			//set tile biome
			var biome = par.AddBiome(type.biomeType);
			var bInfo = new BiomeInfo{
				Id = biome.Id,
				SpreadRate = spreadMod,
				AgeRate = ageRate,
			};

			_biomeInfo[bInfo.Id] = bInfo;
			t.Set(biome);
			seedCoords.Add((x, y)); //add seed coords to list
			//biomeList.Add(biome);
		}

		/*
			Spread biomes
		*/
		void ExpandBiomes() {
			var steps = 100; //number of steps in cellular automaton
			Grid newGrid = new Grid(_grid.Width, _grid.Height); //copy of grid, gets applied at end of step
															  //loop through steps, loop through each tile
			for (var s = 0; s < steps; s++) {
				for (var x = 0; x < par.Width; x++) {
					for (var y = 0; y < par.Height; y++) {

						var t = _grid[x, y]; //get tile from coords

						//try to spread from all neighbor tiles
						newGrid[x, y] = trySpread(t, x - 1, y)
							?? trySpread(t, x + 1, y)
							?? trySpread(t, x, y - 1)
							?? trySpread(t, x, y + 1)
							?? trySpread(t, x + 1, y + 1, 0.7f)
							?? trySpread(t, x + 1, y - 1, 0.7f)
							?? trySpread(t, x - 1, y + 1, 0.7f)
							?? trySpread(t, x - 1, y - 1, 0.7f)
							?? t;
					}
				}
				//apply new grid by swapping places
				var tmp = _grid;
				_grid = newGrid;
				newGrid = tmp; //this works because we always overwrite every pixel in newGrid

			}
		}

		/* 
			* Try to spread biome from tile at [x,y] to t
			* @param t       : The tile to spread to.
			* @param diagMod : A multiplier for chance to spread diagonally in range [0, 1]
		*/
		const float ALT_DIFF_WEIGHT = 2.0f;
		const float ALT_PREF_DIST = 0.3f;
		const float ALT_PREF_MAX = 0.3f;
		int[] altModArr = new int[20];
		Tile trySpread(Tile t, int x, int y, float diagMod = 1) {
			var t2 = _grid[x, y];
			if (t2 == null) return null;
			//get rate of spread from t2 to t, can be 0
			var overgrowRate = t2.Biome.BiomeType.OvergrowRate(t.Biome.BiomeType);

			var altDiffMod = Utils.Clamp(1 - ((float)Math.Pow(Math.Abs(t.altitude - t2.altitude), 0.5) * ALT_DIFF_WEIGHT), 0, 1);
			if (overgrowRate > 0) altModArr[(int)((altDiffMod - 0.00001f) * 20)]++;

			var altPrefMod = 1 - Utils.Clamp(
				Math.Abs(t2.Biome.BiomeTypeObject.prefferedAltitude - t.altitude) / ALT_PREF_DIST) * ALT_PREF_MAX;

			if(!_biomeInfo.TryGetValue(t2.Biome.Id, out var bInfo)) return null;			
			//var bInfo = _biomeInfo[t2.Biome.Id];
			var ageMod = Math.Max(100 - t2.age * bInfo.AgeRate, t2.Biome.BiomeTypeObject.ageModMin) / 100f;
			var spreadRate = bInfo.SpreadRate * ageMod;

			//random chance to spread based on SpreadRate, overgrowRate, and diagMod
			if (overgrowRate > 0 &&
				rng.NextDouble() < spreadRate * overgrowRate * diagMod * altDiffMod * altPrefMod) {
				return t2.Spread(t); //actually spread
			}

			//if no spread, return null
			return null;
		}


		/*
			Replace all empty tiles with soil biome 
		*/
		void FillBlanks() {
			Tile t;
			IBiome b = par.GetDefaultBiome(BiomeType.Soil);
			var bInfo = new BiomeInfo{
				Id = b.Id,
				SpreadRate = 0,
				AgeRate = 0,
			};
			_biomeInfo[bInfo.Id] = bInfo;

			for (var x = 0; x < par.Width; x++) {
				for (var y = 0; y < par.Height; y++) {
					t = _grid[x, y];
					if (t.Biome.BiomeType == BiomeType.Empty) {
						t.Set(b);
					}
				}
			}
		}

		/*
			Remove seeds that never got to spread
		*/
		void RemoveStraySeeds() {
			foreach (var c in seedCoords) {
				Tile t = _grid[c.x, c.y];
				if (t.Biome.TileCount < 2) { //check if alone
					if (t.Biome.TileCount < 1) Logger.Log($"This should not happen! {t.Biome.TileCount}");
					//set biome to same as surroundings
					t.Set((_grid[c.x + 1, c.y]
						?? _grid[c.x - 1, c.y]
						?? _grid[c.x, c.y + 1]
						?? _grid[c.x, c.y - 1]).Biome);
				}
			}
		}
	}
}
