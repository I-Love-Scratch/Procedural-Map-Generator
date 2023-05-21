using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;
using Newtonsoft.Json;

namespace terrainGenerator {

	/*
		Represents an instance of a biome.
	*/
	public interface IBiome {
		/*
			The id that identifies a biome instance on the map.
		*/
		int Id { get; }
		/*
			The object that contains information about the biome type.
		*/
		BiomeTypeObj BiomeTypeObject { get; }
		/*
			The number of tiles that have this biome.
		*/
		int TileCount { get; set; }
		/*
			The type of biome this is.
		*/
		BiomeType BiomeType { get; }
		/*
			The default color of this biome.
		*/
		Color32 Color { get; }
	}

	/*
		A class representing a biome. 
	*/
	public class Biome : IBiome {
		/*
			Default empty biome
		*/
		public static readonly Biome DEFAULT = new Biome(BiomeType.Empty.GetObj());

		/*
			The id that identifies a biome instance on the map.
		*/
		public int Id { get; set; } 

		/*
			The object that contains information about the biome type.
		*/
		[JsonIgnore]
		public BiomeTypeObj BiomeTypeObject { get; set; }

		/*
			The number of tiles that have this biome.
		*/
		public int TileCount { get; set; } 

		/*
			The type of biome this is.
		*/
		public BiomeType BiomeType { 
			get => BiomeTypeObject.biomeType;
			set => BiomeTypeObject = value.GetObj();
		}

		/*
			The default color of this biome.
		*/
		[JsonIgnore]
		public Color32 Color => BiomeTypeObject.color;

		/*
			Set parameters and increment id
		*/
		public Biome(BiomeTypeObj type) {
			BiomeTypeObject = type;
		}


		/*
			Creates a Biome object for the given BiomeType, returns it as a Biome.
		*/
		public static Biome Create(BiomeType type) {
			var typeObj = type.GetObj();
			var biome = type switch {
				BiomeType.Ocean => new OceanBiome(typeObj),
				BiomeType.Mountain => new MountainBiome(typeObj),
				BiomeType.Sand => new SandBiome(typeObj),
				BiomeType.Stone => new StoneBiome(typeObj),
				BiomeType.Desert => new DesertBiome(typeObj),
				BiomeType.Savanna => new SavannaBiome(typeObj),
				BiomeType.Jungle => new JungleBiome(typeObj),
				BiomeType.Grasslands => new GrasslandsBiome(typeObj),
				BiomeType.Forest => new ForestBiome(typeObj),
				BiomeType.Swamp => new SwampBiome(typeObj),
				BiomeType.Tundra => new TundraBiome(typeObj),
				BiomeType.Boreal => new BorealBiome(typeObj),
				BiomeType.Arctic => new ArcticBiome(typeObj),
				BiomeType.River => new RiverBiome(typeObj),
				BiomeType.Lake => new LakeBiome(typeObj),
				_ => new Biome(typeObj),
			};
			return biome;
		}
	}
}
