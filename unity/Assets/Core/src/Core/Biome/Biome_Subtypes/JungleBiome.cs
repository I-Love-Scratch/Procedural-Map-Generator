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
		Represents an instance of a jungle biome.
	*/
	public interface IJungleBiome : IBiome { 
	}

	/*
		A class representing a jungle biome. 
	*/
	internal class JungleBiome : Biome, IJungleBiome {

		public JungleBiome(BiomeTypeObj type) : base(type) { }
	}
}
