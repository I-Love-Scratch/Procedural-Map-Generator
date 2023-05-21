using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Numerics;

namespace terrainGenerator {
	
	/*
		Object with information on the biometype
	*/
	public class BiomeTypeObj {
		public BiomeType biomeType;
		public Color32 color;
		public float spreadMod, ageRate, ageModMin, prefferedAltitude;
		public int biomeSpawnMin, biomeSpawnMax;

		/*
			Set parameters
			@param biomeType:         the enum type of the biometype
			@param color:             the color the biometype is displayed as
			@param spreadMod:         how fast this biometype spreads
			@param ageRate:           how fast this biometype ages
			@param ageModMin:         the minimum spread rate as biome ages
			@param biomeSpawnMin:     the minimum number of seeds that spawn of this type
			@param biomeSpawnMax:     the maximum number of seeds that spawn of this type
			@param prefferedAltitude: the altitude the biome spreads fastest at
		*/
		public BiomeTypeObj(BiomeType biomeType, Color32 color, float spreadMod, float ageRate, float ageModMin,
			int biomeSpawnMin, int biomeSpawnMax, float prefferedAltitude) {
			this.biomeType = biomeType;
			this.color = color;
			this.spreadMod = spreadMod;
			this.ageRate = ageRate;
			this.ageModMin = ageModMin;
			this.biomeSpawnMin = biomeSpawnMin;
			this.biomeSpawnMax = biomeSpawnMax;
			this.prefferedAltitude = prefferedAltitude;
		}
	}

}
