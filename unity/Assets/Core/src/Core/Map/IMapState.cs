using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using terrainGenerator.Generator;

namespace terrainGenerator {

	/*
		 Interface for the state of the map.
	*/
	public interface IMapState {
		// Version number of the map.
		string Version { get; }

		// Name of the map.
		string Name { get; set; }

		// Altitude of ocean surface.
		float OceonLevel { get; set; }

		// Altitude of mountain base.
		float MountainLevel { get; set; }

		// Width of grid.
		int Width { get; }

		// Height of grid.
		int Height { get; }

		// The seed used for randomization.
		int Seed { get; }    


		// Controls what tile properties are saved and how many bits they each get.
		IReadOnlyDictionary<TileProperty, int> TileStorageConfig { get; set; }

		/*
			Scaling factor for the map.
			NOTE: MapScalingFactor is not magical, each step generator needs to manually make use of it where applicable.
		*/
		float MapScalingFactor { get; }

		/*
			The aspects of the map that have been defined.
		*/
		MapAspects MapAspects { get; }

		/*
			The biomes of the map.
		*/
		IReadOnlyDictionary<int, IBiome> Biomes { get; }

		/*
			Get a biome by id.
		*/
		IBiome GetBiome(int id);
	}

}
