using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Parameters used for map generation.
	*/
	[Serializable]
	public class GenParams {

		// Default tile storage configurations.
		public static Dictionary<TileProperty, int> DEFAULT_TILE_STORAGE_CONFIG
			=> new Dictionary<TileProperty, int> {
				{ TileProperty.Biome, 8 },
				{ TileProperty.Altitude, 8 },
				{ TileProperty.Temperature, 5 },
				{ TileProperty.Humidity, 5 },
				{ TileProperty.WindDirection, 6 },
				{ TileProperty.WindMagnitude, 4 },
			};


		[field: SerializeField]
		[field: Tooltip("The name of the generated map. (Used for things like saving and loading)")]
		public string Name { get; set; } = "ImABigDummyWhoForgotToNameMyMap";


		[field: SerializeField]
		[field: Min(64)]
		[field: Tooltip("The width of the map.")]
		public int Width { set; get; } 


		[field: SerializeField]
		[field: Min(64)]
		[field: Tooltip("The height of the map.")]
		public int Height { set; get; } 


		[field: SerializeField]
		[field: Tooltip("The seed to use for the map.\n-1 will cause it to pick a random seed.")]
		public int Seed { set; get; } = -1; 

		[field: SerializeField]
		[field: Tooltip("Should the biome cap automatically increase as biomes are added.")]
		public bool DynamicBiomeCap { get; set; } = true;


		/*
			Scaling factor for the map.
			
			Used as a multiplier for parameters in the step generators.
			- Leave as negative to generate a factor automatically from map size.
			- Set it to a positive value to manually set factor.
			- Setting it to 1 leaves all parameters unchanged.

			NOTE: MapScalingFactor is not magical, each step generator needs to manually make use of it where applicable.
		*/
		[field: SerializeField]
		[field: Tooltip("Informs other step generators how much larger than normal this map is.\nUsed for scaling parameters of step generators so it doesn't have to be done manually on each when changing the size of the map.\nExample: If you change the map to be 4x as big, the number of rivers can be multiplied by 4 automatically when running instead of manually changing it.\nIf value is less than 0, it will be set automatically based on map size.")]
		public float MapScalingFactor { get; set; } = -1;



		[field: SerializeField]
		[field: Tooltip("Controls what tile properties are saved to file and how many bits allocated to each.")]
		public Dictionary<TileProperty, int> TileStorageConfig { get; set; } = DEFAULT_TILE_STORAGE_CONFIG;


	}


}