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
		Represents an instance of an arctic biome.
	*/
	public interface IArcticBiome : IBiome { 
	}

	/*
		A class representing an arctic biome. 
	*/
	internal class ArcticBiome : Biome, IArcticBiome {

		public ArcticBiome(BiomeTypeObj type) : base(type) { }
	}
}
