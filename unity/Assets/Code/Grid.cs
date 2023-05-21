using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

using terrainGenerator;

namespace TerrainUnity {

	/*
		Displays a map in 3D.
	*/
	public class Grid : MonoBehaviour {
		// The default altitude to display ocean surface on.
		public const float DEFAULT_OCEAN_CUTOFF = 0.3f;
		// The default altitude to display mountain base on.
		public const float DEFAULT_MOUNTAIN_CUTOFF = 0.6f;
		// How tall the map is relative to size.
		const float HEIGHT_MULTIPLIER = 0.5f;

		// The parser used to read map files.
		static readonly GameMapParser _mapParser = new GameMapParser();

		// The types of image that gets displayed on 3D terrain.
		public enum ImageType { Pretty, Altitude, Temperature, Humidity, Factor, Wind, Resource }
		// The type of image that gets displayed on 3D terrain.
		public ImageType imageType = ImageType.Pretty;

		public GameObject terrainPrefab;
		public GameObject oceanPlane;

		// The name of the map to load.
		public string mapName = "TestMap_0";

		// The altitude to display ocean surface on.
		public float oceanCutoff = DEFAULT_OCEAN_CUTOFF;
		// The altitude to display mountain base on.
		public float mountainCutoff = DEFAULT_MOUNTAIN_CUTOFF;

		// The map to represent.
		private GameMap _map;

		// The Unity object representing the terrain.
		private GameObject terrainObj;
		// The Unity object representing the ocean plane.
		private GameObject oceanObj;

		// The Unity object representing the terrain.
		private GameObject TerrainObj => terrainObj ??= Instantiate(terrainPrefab, transform);
		// The Unity object representing the ocean plane.
		private GameObject OceanObj => oceanObj ??= Instantiate(oceanPlane, transform);



		/*
			Load and display a map from file.
		*/
		void Start() {
			Debug.Log($"[Grid.Start()] Start");

			if(!string.IsNullOrEmpty(mapName)) LoadMap(mapName);

			SetTerrain(_map, oceanCutoff, mountainCutoff);

			Debug.Log($"[Grid.Start()] End");
		}

		void Update() {

		}

		/*
			Load a map from file by name.
		*/
		void LoadMap(string name) {
			_map = _mapParser.Decode(name);
			Debug.Log($"[Grid.LoadMap()] Loaded: maps/{name}");
		}

		/*
			Displays the map.
		*/
		public void SetMap(GameMap map) {
			SetTerrain(map, oceanCutoff, mountainCutoff);
		}

		/*	
			Sets terrain data.

		 	- The unity terrain must be a square, with each side being 2^n + 1
			- The smallest square that fits this criteria is made, and the map data is put in
			- Any terrain coordinates outside the map dimesions are made into holes
		*/
		private void SetTerrain(GameMap map, float oceanCutoff, float mountainCutoff) {
			if(map == null) return;

			var grid = map.Grid;

			//dimension of the terrain map
			var terrainSize = Math.Max(grid.Width, grid.Height);
			var depth = (int)Math.Ceiling(Math.Log(terrainSize - 1, 2));
			terrainSize = (1 << depth) + 1;

			//data for heightmap
			var alts = new float[terrainSize, terrainSize];
			var holes = new bool[terrainSize - 1, terrainSize - 1];

			// Holes

			for (var x = 0; x < terrainSize - 1; x++) for (var y = 0; y < terrainSize - 1; y++) {
				holes[y, x] = x < (grid.Width - 1) && y < (grid.Height - 1);
			}

			// Altitudes

			for (var x = 0; x < grid.Width; x++) for (var y = 0; y < grid.Height; y++) {
				var t = grid[x, y];

				var relAlt = t.BiomeType switch {
					BiomeType.Mountain => t.altitude.Lerp(map.State.MountainLevel, 1),
					BiomeType.Ocean => t.altitude.Lerp(0, map.State.OceonLevel),
					_ => t.altitude.Lerp(map.State.OceonLevel, map.State.MountainLevel),
				};
				alts[y, x] = t.BiomeType switch {
					BiomeType.Mountain => relAlt.Scale(mountainCutoff, 1),
					BiomeType.Ocean => relAlt.Scale(0, oceanCutoff),
					_ => relAlt.Scale(oceanCutoff, mountainCutoff),
				};
			}

			// Biomes

			Texture2D tex = null;
			switch (imageType) {
				case ImageType.Pretty:		tex = map.ToPrettyImage(); break;
				case ImageType.Altitude:	tex = map.ToAltitudeMap(); break;
				case ImageType.Temperature:	tex = map.ToTemperatureMap(); break;
				case ImageType.Humidity:	tex = map.ToHumidityMap(); break;
				case ImageType.Factor:		tex = map.ToFactorMap(); break;
				case ImageType.Wind:		tex = map.ToWindMap(); break;
				case ImageType.Resource:	tex = map.ToResourceMap(); break;
				default:					tex = map.ToPrettyImage(); break;
			}
			
			//set alhpa values to 0.5 to reduce glossyness
			for (var x = 0; x < tex.width; x++) for (var y = 0; y < tex.height; y++) {
				var p = tex.GetPixel(x, y);
				p.a = 0.5f;
				tex.SetPixel(x, y, p);
			}
			tex.Apply();

			// lines texture up with heightmap, so each pixel matches an altitude point
			// the new texture has double the size, but pixels along the edges are removed
			var adjustedTex = new Texture2D((grid.Width * 2) - 2, (grid.Height * 2) - 2);

			for (var x = 0; x < adjustedTex.width; x++) for (var y = 0; y < adjustedTex.height; y++) {
				adjustedTex.SetPixel(x, y, tex.GetPixel((x + 1) / 2, (y + 1) / 2));
			}
			adjustedTex.Apply();
			adjustedTex.wrapMode = TextureWrapMode.Clamp;

			// Make terrain

			var terrainObj = TerrainObj;
			var terrainComp = terrainObj.GetComponent<Terrain>();
			
			//set terrain heightmap
			terrainComp.terrainData.heightmapResolution = terrainSize;
			terrainComp.terrainData.SetHeights(0, 0, alts);
			terrainComp.terrainData.SetHoles(0, 0, holes);

			//set terrain texture
			terrainComp.terrainData.terrainLayers[0].tileSize = new Vector2(grid.Width, grid.Height);
			terrainComp.terrainData.terrainLayers[0].diffuseTexture = adjustedTex;
			
			//set terrain variables
			terrainObj.transform.position = new Vector3(grid.Width / -2.0f, 0, grid.Height / -2.0f);
			terrainComp.terrainData.size = new Vector3(terrainSize, (terrainSize - 1) * HEIGHT_MULTIPLIER, terrainSize);

			//ocean plane
			var oceanObj = OceanObj;
			oceanObj.transform.position = new Vector3(0, oceanCutoff * terrainSize * HEIGHT_MULTIPLIER, 0);
			oceanObj.transform.localScale = new Vector3(grid.Width / 10.0f, 1, grid.Height / 10.0f);

		}

	}
}
