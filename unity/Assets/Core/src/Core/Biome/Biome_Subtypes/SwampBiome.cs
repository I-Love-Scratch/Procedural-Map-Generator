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
		Represents an instance of a swamp biome.
	*/
	public interface ISwampBiome : IBiome { 
	}

	/*
		A class representing a swamp biome. 
	*/
	internal class SwampBiome : Biome, ISwampBiome {

		public SwampBiome(BiomeTypeObj type) : base(type) { }
	}
}
