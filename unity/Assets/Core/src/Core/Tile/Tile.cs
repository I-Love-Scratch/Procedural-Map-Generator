using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator {
	/*
		A tile on the map, containing biome, altitude, age
	*/
	public class Tile {
		public const int NUM_ALT_GROUPS = 4;           // Number of altitude ranges to display
		public const float OCEAN_ALT_CUTOFF  = 0.4f;   // The default altitude below which is ocean
		public const float MOUNTAIN_ALT_CUTOFF = 0.8f; // The default altitude above which is mountain

		// The biome of the tile.
		public IBiome Biome;

		// The altitude of the tile.
		public float altitude;

		// The temperature of the tile.
		public float temperature;

		// The humidity of the tile.
		public float humidity;

		// The x component of the wind on the tile.
		public float windXComponent;

		// The y component of the wind on the tile.
		public float windYComponent;

		// The angle of the wind on the tile given in radians. (in range [0, 2*PI])
		public float windAngle;

		// The magnitude of the wind on the tile.
		public float windMagnitude;

		// The age of the tile, used for spreading biome.
		public int age; 

		// The top resource of the tile.
		public TileResource Resource {
			get => Resources.FirstOrDefault();
			set { if(value != null) Resources.Add(value); }
		}

		// The type of the top resource on this tile.
		public ResourceType ResourceType => Resource?.ResourceType ?? ResourceType.Empty;

		// A list of resources present on this tile.
		public List<TileResource> Resources = new List<TileResource>();

		// The type of biome this tile belongs to.
		public BiomeType BiomeType => Biome.BiomeType;


		/*
			Make tile based on biome, increment TileCount
		*/
		public Tile(IBiome biome = null) {
			Biome = biome ?? terrainGenerator.Biome.DEFAULT;
			Biome.TileCount++;
		}

		/*
			The brightness to give this tile based on its altitude.
		*/
		public float GetAltitudeColorMod(float oceonCutOff, float mountianCutOff) {
			if (Biome.BiomeType == BiomeType.Mountain) return 1;
			if (Biome.BiomeType == BiomeType.Ocean) return altitude
				.Lerp(0, oceonCutOff)
				.Steps(NUM_ALT_GROUPS)
				.Scale(0.15f, 0.6f);
			return (1f - altitude.Lerp(oceonCutOff, mountianCutOff))
				.Steps(NUM_ALT_GROUPS)
				.Scale(0.65f, 1.0f);
		}

		/*
			Set tile biome
		*/
		public void Set(IBiome biome) {
			this.Biome.TileCount--;
			biome.TileCount++;
			this.Biome = biome;
		}

		/*
			Set tile altitude
		*/
		public void Set(float altitude) {
			this.altitude = altitude;
		}

		/*
			Set tile temperature and humidity
		*/
		public void Set(float temperature, float humidity) {
			this.temperature = temperature;
			this.humidity = humidity;
		}

		/*
			Set tile wind direction and magnitude
		*/
		public void Set(float xComponent, float yComponent, float angle, float magnitude) {
			this.windXComponent = xComponent;
			this.windYComponent = yComponent;
			this.windAngle = angle;
			this.windMagnitude = magnitude;
		}


		/*
			Set fields based on another tile
		*/
		public void Set(Tile t) {
			this.Biome = t.Biome;
			this.Resource = t.Resource;
			this.altitude = t.altitude;
			this.age = t.age;
			this.windMagnitude = t.windMagnitude;
			this.windXComponent = t.windXComponent;
			this.windYComponent = t.windYComponent;
		}
		
		/*
			Return clone of tile
		*/
		public Tile Clone() {
			var t = new Tile();
			t.Set(this);
			return t;
		}
		
		/*
			Spread tile biome and increase age
		*/
		public Tile Spread(Tile target) {
			age++;
			var t = target.Clone();
			t.Set(Biome);
			t.age = age;
			return t;
		}

	}
}
