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
		Represents an instance of a Mountain biome.
	*/
	public interface IMountainBiome : IBiome { 
	}

	/*
		A class representing a Mountain biome. 
	*/
	internal class MountainBiome : Biome, IMountainBiome {

		public MountainBiome(BiomeTypeObj type) : base(type) { }
	}
}
