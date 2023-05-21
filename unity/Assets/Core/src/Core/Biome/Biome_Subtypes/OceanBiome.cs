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
		Represents an instance of a ocean biome.
	*/
	public interface IOceanBiome : IBiome { 
	}

	/*
		A class representing a ocean biome. 
	*/
	internal class OceanBiome : Biome, IOceanBiome {

		public OceanBiome(BiomeTypeObj type) : base(type) { }
	}
}
