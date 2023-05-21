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
		Represents an instance of a tundra biome.
	*/
	public interface ITundraBiome : IBiome { 
	}

	/*
		A class representing a tundra biome. 
	*/
	internal class TundraBiome : Biome, ITundraBiome {

		public TundraBiome(BiomeTypeObj type) : base(type) { }
	}
}
