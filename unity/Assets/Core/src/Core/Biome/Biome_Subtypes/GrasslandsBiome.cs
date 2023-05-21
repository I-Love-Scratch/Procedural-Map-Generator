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
		Represents an instance of a grasslands biome.
	*/
	public interface IGrasslandsBiome : IBiome { 
	}

	/*
		A class representing a grasslands biome. 
	*/
	internal class GrasslandsBiome : Biome, IGrasslandsBiome {

		public GrasslandsBiome(BiomeTypeObj type) : base(type) { }
	}
}

