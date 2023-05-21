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
		Represents an instance of a lake biome.
	*/
	public interface ILakeBiome : IBiome { 
		/*
			The altitude of the water surface of the lake.
		*/
		float SurfaceAltitude { get; set; }
	}

	/*
		A class representing a lake biome. 
	*/
	internal class LakeBiome : Biome, ILakeBiome {
		/*
			The altitude of the water surface of the lake.
		*/
		public float SurfaceAltitude { get; set; }

		public LakeBiome(BiomeTypeObj type) : base(type) { }
	}
}
