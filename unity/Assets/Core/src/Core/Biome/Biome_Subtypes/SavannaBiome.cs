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
		Represents an instance of a savanna biome.
	*/
	public interface ISavannaBiome : IBiome { 
	}

	/*
		A class representing a savanna biome. 
	*/
	internal class SavannaBiome : Biome, ISavannaBiome {

		public SavannaBiome(BiomeTypeObj type) : base(type) { }
	}
}
