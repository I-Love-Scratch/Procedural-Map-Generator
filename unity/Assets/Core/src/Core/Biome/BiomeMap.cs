using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace terrainGenerator {
	
	/*
		Maps coordinates to a biome. 
	*/
	public class BiomeMap {
		private Biome[,,] _map;

		public BiomeMap(Biome[,,] map) {
			if(map == null) throw new ArgumentNullException(nameof(map));
			_map = map;
		}

		/*
			Get the biome at a specific coordinate.
		*/
		public Biome this[int x, int y, int z] => _map[x, y, z];
	}
}
