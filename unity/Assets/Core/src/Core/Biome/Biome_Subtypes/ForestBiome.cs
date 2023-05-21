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
		Represents an instance of a forest biome.
	*/
	public interface IForestBiome : IBiome { 
	}

	/*
		A class representing a forest biome. 
	*/
	internal class ForestBiome : Biome, IForestBiome {

		public ForestBiome(BiomeTypeObj type) : base(type) { }
	}
}
