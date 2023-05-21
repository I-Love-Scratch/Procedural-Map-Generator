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
		Represents an instance of a boreal biome.
	*/
	public interface IBorealBiome : IBiome { 
	}

	/*
		A class representing a boreal biome. 
	*/
	internal class BorealBiome : Biome, IBorealBiome {

		public BorealBiome(BiomeTypeObj type) : base(type) { }
	}
}
