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
		Represents an instance of a river biome.
	*/
	public interface IRiverBiome : IBiome { 
	}

	/*
		A class representing a river biome. 
	*/
	internal class RiverBiome : Biome, IRiverBiome {

		public RiverBiome(BiomeTypeObj type) : base(type) { }
	}
}
