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
		Represents an instance of a desert biome.
	*/
	public interface IDesertBiome : IBiome { 
	}

	/*
		A class representing a desert biome. 
	*/
	internal class DesertBiome : Biome, IDesertBiome {

		public DesertBiome(BiomeTypeObj type) : base(type) { }
	}
}
