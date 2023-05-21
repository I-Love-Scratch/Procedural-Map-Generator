using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;
using System.IO;
using UnityEngine;

namespace terrainGenerator {

	/*
		Class for encoding/decoding GameMaps.
	*/
	public class GameMapParser {

		public const string DEFAULT_PATH = "Data/Maps/";
		public const string GRID_FILE_EXTENSION = "map";
		public const string THUMBNAIL_FILE_EXTENSION = "png";
		public const string STATE_FILE_EXTENSION = "json";

		// The parser for the grid.
		private readonly GridParser _gridParser;
		// The parser for the map state.
		private readonly MapStateParser _mapStateParser;

		public GameMapParser(GridParser gridParser = null, MapStateParser mapStateParser = null) {
			_gridParser = gridParser ?? new GridParser();
			_mapStateParser = mapStateParser ?? new MapStateParser();
		}


		/* 
			Encodes the map data into an image.
			This is meant to be the reversable version that allows maps to be loaded from image later on.
		*/
		public void Encode(GameMap map, string path = DEFAULT_PATH) {
			// Get the base filename of the map.
			var baseFilePath = GetBaseFilePath(map.State.Name, path);

			/// If the folder doesn't exist, create it.
			if(!Directory.Exists(baseFilePath)) Directory.CreateDirectory(baseFilePath);


			/// Create the grid file.
			using(var fs = new FileStream(GetGridFileName(baseFilePath), FileMode.Create)) {
				_gridParser.Encode(map.Grid, map.State, fs);
			}

			_storeResources(map);

			/// Create the state file.
			var stateStr = _mapStateParser.Encode(map.State);

			// Save the state as JSON
			using(StreamWriter sw = File.CreateText(GetStateFileName(baseFilePath))) {
				sw.WriteLine(stateStr);
			}


			/// Create the thumbnail.
			var thumb = ImageConversion.EncodeToPNG(map.ToPrettyImage());
			File.WriteAllBytes(GetThumbnailFileName(baseFilePath), thumb);

			clearResourceTiles(map.State);
		}


		/* 
			Decodes the image into a grid.
			@param name : The name of the map. (Don't include file extensions)
			@param path : The path to the map, excluding the name.
		*/
		public GameMap Decode(string name, string path = DEFAULT_PATH) {
			/// Get the base filename of the map.
			var baseFilePath = GetBaseFilePath(name, path);


			/// If the folder doesn't exist, throw exception.
			if(!Directory.Exists(baseFilePath)) throw new ArgumentException(nameof(name), $"No map of name \"{name}\" found at path.");


			/// Read the state from JSON
			MapState state;
			using (StreamReader sr = File.OpenText(GetStateFileName(baseFilePath))) {
				state = _mapStateParser.Decode(sr.ReadToEnd());
			}

			/// Get the grid.
			Grid grid;
			using(var fs = new FileStream(GetGridFileName(baseFilePath), FileMode.Open)) {
				grid = _gridParser.Decode(fs, state);
			}

			getResources(grid, state);

			return new GameMap(grid, state);
		}

		/* 
			Store resources in gameMap state based on grid.
		*/
		private void _storeResources(GameMap gameMap) {
			//clear all resource tile lists
			clearResourceTiles(gameMap.State);

			//fill them
			for (var x = 0; x < gameMap.Grid.Width; x++) for (var y = 0; y < gameMap.Grid.Height; y++) {
				foreach (var res in gameMap.Grid[x, y].Resources) {
					var type = res.Resource;
					var density = res.Density;
					gameMap.State.Resources[type.Id].Tiles.Add(new ResourceLocation(x, y, density));
				}
			}
		}

		/* 
			Clear all the information about what tiles a resource is found in from state.
		*/
		private void clearResourceTiles(MapState state) {
			foreach (var v in state.Resources.Values) {
				v.Tiles.Clear();
			}
		}

		/* 
			Get resources from gameMap state and store in grid.
		*/
		private void getResources(Grid grid, MapState state) {
			foreach (var (i, res) in state.Resources) {
				foreach (var t in res.Tiles) {
					grid[t.X, t.Y].Resources.Add(new TileResource(res, t.Density));
				}
			}
			clearResourceTiles(state);
		}


		/*
			Gets the path for the map's files, excluding file extensions.
			@param name : The name of the map.
			@param name : The path to the folder we wish to save the map.

			[ADD] Checks for empty or null.
			[ADD] Checks to remove '..' and illegal characters.
			[ADD] In different part: Code to confirm path and maybe create if not.
		*/
		private string GetBaseFilePath(string name, string path) {
			// Ensure that path ends with a '/'
			if (path.Length > 0 && !path.EndsWith("/") && !path.EndsWith("\\")) path += "/";

			// Get the base filepath of the map.
			return $"{path}{name}";
		}

		private string GetGridFileName(string baseFilePath) => $"{baseFilePath}/grid.{GRID_FILE_EXTENSION}";
		private string GetStateFileName(string baseFilePath) => $"{baseFilePath}/state.{STATE_FILE_EXTENSION}";
		private string GetThumbnailFileName(string baseFilePath) => $"{baseFilePath}/thumb.{THUMBNAIL_FILE_EXTENSION}";
	}

	/*
		Version that stores the grid as a series of images, each pixel corresponding to a tile.
	*/
	public class GameMapParser_ImageArray {

		public const string DEFAULT_PATH = "Data/Maps/"; // "Assets/Data/Maps/"
		public const string GRID_FILE_EXTENSION = "png";
		public const string STATE_FILE_EXTENSION = "json";

		private readonly GridParser_ImageArray _gridParser;
		private readonly MapStateParser _mapStateParser;

		public GameMapParser_ImageArray(GridParser_ImageArray gridParser = null, MapStateParser mapStateParser = null) {
			_gridParser = gridParser ?? new GridParser_ImageArray();
			_mapStateParser = mapStateParser ?? new MapStateParser();
		}


		/* 
			Encodes the map data into an image.
			This is meant to be the reversable version that allows maps to be loaded from image later on.
		*/
		public void Encode(GameMap map, string path = DEFAULT_PATH) {
			// Get the base filename of the map.
			var baseFilePath = GetBaseFilePath(map.State.Name, path);

			var gridPics = _gridParser.Encode(map.Grid, map.State);

			map.State.GridImgCount = gridPics.Length;

			var stateStr = _mapStateParser.Encode(map.State);

			// Save the state as JSON
			using(StreamWriter sw = File.CreateText(GetStateFileName(baseFilePath))) {
				sw.WriteLine(stateStr);
			}

			// Save the grid as PNGs
			for(int i = 0; i < gridPics.Length; ++i) {
				var gridPic = gridPics[i];
				var data = ImageConversion.EncodeToPNG(gridPic);
				//var data = gridPic.GetRawTextureData();
				File.WriteAllBytes(GetGridFileName(baseFilePath, i), data);
			}
		}


		/* 
			Decodes the image into a grid.
			@param name : The name of the map. (Don't include file extensions)
			@param path : The path to the map, excluding the name.
		*/
		public GameMap Decode(string name, string path = DEFAULT_PATH) {
			// Get the base filename of the map.
			var baseFilePath = GetBaseFilePath(name, path);

			// Read the state from JSON
			MapState state;
			using(StreamReader sr = File.OpenText(GetStateFileName(baseFilePath))) {
				state = _mapStateParser.Decode(sr.ReadToEnd());
			}

			// Read the pngs from file.
			var gridPics = new Texture2D[state.GridImgCount];

			for(int i = 0; i < state.GridImgCount; ++i) {
				var fileData = File.ReadAllBytes(GetGridFileName(baseFilePath, i));
				Texture2D tex = new Texture2D(state.Width, state.Height);
				tex.LoadImage(fileData);
				gridPics[i] = tex;

			}

			var grid = _gridParser.Decode(gridPics, state);

			//var fileData = File.ReadAllBytes(GetGridFileName(baseFilePath));
			//Texture2D tex = new Texture2D(state.Width, state.Height);
			//tex.LoadImage(fileData);
			//var grid = _gridParser.Decode(tex, state);

			return new GameMap(grid, state);
		}

		


		/*
			Gets the path for the map's files, excluding file extensions.
			@param name : The name of the map.
			@param name : The path to the folder we wish to save the map.

			[ADD] Checks for empty or null.
			[ADD] Checks to remove '..' and illegal characters.
			[ADD] In different part: Code to confirm path and maybe create if not.
		*/
		private string GetBaseFilePath(string name, string path) {
			// Ensure that path ends with a '/'
			if(path.Length > 0 && !path.EndsWith("/") && !path.EndsWith("\\")) path += "/";

			// Get the base filepath of the map.
			return $"{path}{name}";
		}

		private string GetGridFileName(string baseFilePath, int nr) => $"{baseFilePath}.{nr}.{GRID_FILE_EXTENSION}";
		private string GetGridFileName(string baseFilePath) => $"{baseFilePath}.{GRID_FILE_EXTENSION}";
		private string GetStateFileName(string baseFilePath) => $"{baseFilePath}.{STATE_FILE_EXTENSION}";
	}
}
