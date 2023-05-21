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
		Represents an instance of a Sand biome.
	*/
	public interface ISandBiome : IBiome { 
	}

	/*
		A class representing a Sand biome. 
	*/
	internal class SandBiome : Biome, ISandBiome {

		public SandBiome(BiomeTypeObj type) : base(type) { }
	}
}
