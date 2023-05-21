using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using terrainGenerator.Generator;
using UnityEngine;


namespace terrainGenerator {

	/*
		Class for a game map.
	*/
	public partial class GameMap : IGameMap {
		/*
			The grid containing all the tiles of this map.
		*/
		public Grid Grid { get; private set; }

		/*
			The overall state of this map.
		*/
		public MapState State { get; private set; }

		public GameMap(Grid grid, MapState state) {
			if (grid == null) throw new ArgumentNullException(nameof(grid));
			if (state == null) throw new ArgumentNullException(nameof(state));

			Grid = grid;
			State = state;
		}


		/*
			Returns color based on biome and AltitudeColorMod
		*/
		public Texture2D ToImage()
			=> Grid.ToPrettyImage(State.OceonLevel, State.MountainLevel);


		/*
			Converts the grid to a bitmap where the biomes are very easy to tell apart. 
		*/
		public Texture2D ToDebugImage()
			=> Grid.ToDebugImage(State.OceonLevel, State.MountainLevel);

		/*
			Converts the grid to a bitmap where the biome colors are nice to look at. 
		*/
		public Texture2D ToPrettyImage()
			=> Grid.ToPrettyImage(State.OceonLevel, State.MountainLevel);

		/* 
			Returns color based on biome and AltitudeColorMod.
			@param colorPallet : The color pallet that maps biomes to colors.
		*/
		public Texture2D ToImage(IReadOnlyDictionary<BiomeType, Color32> colorPallet)
			=> Grid.ToImage(colorPallet, State.OceonLevel, State.MountainLevel);


		/*
			Creates an image depicting the altitude of the map.
		*/
		public Texture2D ToAltitudeMap() => Grid.ToAltitudeMap();

		/*
			Creates an image depicting the temperature of the map.
		*/
		public Texture2D ToTemperatureMap() => Grid.ToTemperatureMap();

		/*
			Creates an image depicting the humidity of the map.
		*/
		public Texture2D ToHumidityMap() => Grid.ToHumidityMap();

		/*
			Creates a map colored by the 3 main factors.
		*/
		public Texture2D ToFactorMap() => Grid.ToFactorMap(oceonCutOff: State.OceonLevel, mountianCutOff: State.MountainLevel);
		
		/*
			Creates an image depicting the wind strength and direction of the map.
		*/
		public Texture2D ToWindMap() => Grid.ToWindMap();


		/* 
			Returns a texture that shows the resources on the map.
			On any tile that has no resource, it prints the usual pretty image.
		*/
		public Texture2D ToResourceMap() {
			Texture2D pic = new Texture2D(Grid.Width, Grid.Height);
			Color c;
			var w = Grid.Width;
			for(int i = 0; i < Grid.Length; i++) {
				var t = Grid[i];

				if(t.Resource != null) {
					var density = t.Resource.Density;
					var resourceColor = t.ResourceType.GetColor();
					c = new Color32(
						(byte)(resourceColor.r * density),
						(byte)(resourceColor.g * density),
						(byte)(resourceColor.b * density),
						255
					);
					pic.SetPixel(i % w, i / w, c);
					continue;
				}
				var alt = t.GetAltitudeColorMod(State.OceonLevel, State.MountainLevel);
				var biomeColor = BiomeExtension.BiomeColorPallet_Updated.GetColor(t.Biome.BiomeType);
				c = new Color32(
					(byte) (biomeColor.r * alt),
					(byte) (biomeColor.g * alt),
					(byte) (biomeColor.b * alt),
					255
				);
				pic.SetPixel(i % w, i / w, c);
			}
			return pic;
		}



		/* 
			Returns an image depicting the map.
			Takes a function that is used to convert the tile into a color.
		*/
		public Texture2D ToImage(Func<Tile, IMapState, Color> func) {
			Texture2D pic = new Texture2D(Grid.Width, Grid.Height);
			var w = Grid.Width;
			for(int i = 0; i < Grid.Length; i++) {
				var t = Grid[i];
				pic.SetPixel(i % w, i / w, func(t, State));
			}
			return pic;
		}
	}


	/*
		Some interface implementations & support stuff. 
	*/
	public partial class GameMap {

		/*
			Create a GameMap from the results of a generator.
		*/
		public static GameMap Create(Grid grid, GeneratorState genState) {
			return new GameMap(grid, MapState.Create(genState));
		}

		IReadOnlyGrid IGameMap.Grid => Grid;
		IMapState IGameMap.State => State;
	}
}
