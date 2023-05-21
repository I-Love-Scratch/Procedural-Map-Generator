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
		Represents an instance of a Stone biome.
	*/
	public interface IStoneBiome : IBiome { 
	}

	/*
		A class representing a Stone biome. 
	*/
	internal class StoneBiome : Biome, IStoneBiome {

		public StoneBiome(BiomeTypeObj type) : base(type) { }
	}
}
