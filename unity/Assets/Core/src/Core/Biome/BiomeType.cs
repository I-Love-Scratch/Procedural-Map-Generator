using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace terrainGenerator {
	/*
		The type of biomes.
	*/
	public enum BiomeType {
		Empty,                         // Used for tiles with no specific biome defined.
		Ocean, Mountain,               // Used for altitude extremes.
		Sand, Stone,                   // Sand is a buffer for ocean, stone is a buffer for mountain.
		Desert, Savanna, Jungle,       // Hot biomes.
		Grasslands, Forest, Swamp,     // Mid biomes.
		Tundra, Boreal, Arctic,        // Cold biomes.
		River, Lake,                   // Water source.
		Soil, Glacier,                 // Depricated.
	}

}
