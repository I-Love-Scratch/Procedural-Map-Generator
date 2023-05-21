using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using UnityEngine;

namespace terrainGenerator {

	/*
		Interface for a game map.
	*/
	public interface IGameMap { 
		/*
			The grid containing all the tiles of this map.
		*/
		IReadOnlyGrid Grid { get; }

		/*
			The overall state of this map.
		*/
		IMapState State { get; }

		/*
			Returns color based on biome and AltitudeColorMod
		*/
		Texture2D ToImage();

		/*
			Converts the grid to a bitmap where the biomes are very easy to tell apart. 
		*/
		Texture2D ToDebugImage();

		/*
			Converts the grid to a bitmap where the biome colors are nice to look at. 
		*/
		Texture2D ToPrettyImage();

		/* 
			Returns color based on biome and AltitudeColorMod.
			@param colorPallet : The color pallet that maps biomes to colors.
		*/
		Texture2D ToImage(IReadOnlyDictionary<BiomeType, Color32> colorPallet);

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

		/* 
			Returns a texture that shows the resources on the map.
			On any tile that has no resource, it prints the usual pretty image.
		*/
		Texture2D ToResourceMap();

		/* 
			Returns an image depicting the map.
			Takes a function that is used to convert the tile into a color.
		*/
		Texture2D ToImage(Func<Tile, IMapState, Color> func);
	}
}
