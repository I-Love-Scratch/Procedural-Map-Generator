using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator {

	/*
		Read only interface for Grid.
	*/
	public interface IReadOnlyGrid {
		/*
			The width of the grid.
		*/
		int Width { get; }
		/*
			The height of the grid.
		*/
		int Height { get; }
		/*
			The length of the grid. (Total number of tiles)
		*/
		int Length { get; }

		/*
			Get tile at x,y in the grid
		*/
		Tile this[int x, int y] { get; }
		public Tile this[(int, int) pos] { get; }

		/*
			Get the i-th tile in the grid
		*/
		Tile this[int i] { get; }


		/*
			Converts the grid to a bitmap where the biomes are very easy to tell apart. 
		*/
		Texture2D ToDebugImage(float oceonCutOff = Tile.OCEAN_ALT_CUTOFF, float mountianCutOff = Tile.MOUNTAIN_ALT_CUTOFF);

		/*
			Converts the grid to a bitmap where the biome colors are nice to look at. 
		*/
		Texture2D ToPrettyImage(float oceonCutOff = Tile.OCEAN_ALT_CUTOFF, float mountianCutOff = Tile.MOUNTAIN_ALT_CUTOFF);

		/* 
			Returns color based on biome and AltitudeColorMod.
			@param colorPallet : The color pallet that maps biomes to colors.
		*/
		Texture2D ToImage(IReadOnlyDictionary<BiomeType, Color32> colorPallet, float oceonCutOff = Tile.OCEAN_ALT_CUTOFF, float mountianCutOff = Tile.MOUNTAIN_ALT_CUTOFF);


		/*
			Creates an image depicting the altitude of the map.
		*/
		Texture2D ToAltitudeMap();

		/*
			Creates an image depicting the temperature of the map.
		*/
		Texture2D ToTemperatureMap();

		/*
			Creates an image depicting the humidity of the map.
		*/
		Texture2D ToHumidityMap();

		/*
			Creates a map colored by the 3 main factors.
		*/
		Texture2D ToFactorMap();

		/*
			Creates an image depicting the wind strength and direction of the map.
		*/
		Texture2D ToWindMap();

	}


}
