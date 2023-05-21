using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Numerics;

namespace terrainGenerator {

	/*
		Grid containing all tiles in a map
	*/
	public class Grid : IGrid {
		Tile[,] grid; //grid of tiles
		int w, h, l;  //width, height, length

		/*
			Initialize grid of empty tiles based on width and height
		*/
		public Grid(int w, int h, IBiome biome = null) {
			biome ??= Biome.DEFAULT;
			grid = new Tile[w, h];
			this.w = w;
			this.h = h;
			this.l = w * h;
			//fill grid with empty tiles
			for (var x = 0; x < w; x++) {
				for (var y = 0; y < h; y++) {
					grid[x, y] = new Tile(biome); //default tile has empty biome
				}
			}
		}

		/*
			Get tile at x,y in the grid
		*/
		public Tile this[int x, int y] {
			get {
				if (x < 0 || x >= w || y < 0 || y >= h) return null;
				return grid[x, y];
			}
			set => grid[x, y] = value;
		}
		public Tile this[(int,int) pos] {
			get => this[pos.Item1, pos.Item2];
			set => this[pos.Item1, pos.Item2] = value;
		}


		/*
			Get the i-th tile in the grid
		*/
		public Tile this[int i] {
			get => grid[i % Width, i / Width];
			set => grid[i % Width, i / Width] = value;
		}


		/*
			The width of the grid.
		*/
		public int Width => w;
		/*
			The height of the grid.
		*/
		public int Height => h;
		/*
			The length of the grid. (Total number of tiles)
		*/
		public int Length => l;



		/*
			Converts the grid to a bitmap where the biomes are very easy to tell apart. 
		*/
		public Texture2D ToDebugImage(float oceonCutOff = Tile.OCEAN_ALT_CUTOFF, float mountianCutOff = Tile.MOUNTAIN_ALT_CUTOFF)
			=> ToImage(BiomeExtension.BiomeColorPallet_VisibleBorders, oceonCutOff, mountianCutOff);

		/*
			Converts the grid to a bitmap where the biome colors are nice to look at. 
		*/
		public Texture2D ToPrettyImage(float oceonCutOff = Tile.OCEAN_ALT_CUTOFF, float mountianCutOff = Tile.MOUNTAIN_ALT_CUTOFF)
			=> ToImage(BiomeExtension.BiomeColorPallet_Updated, oceonCutOff, mountianCutOff);

		/* 
			Returns color based on biome and AltitudeColorMod.
			@param colorPallet : The color pallet that maps biomes to colors.
		*/
		public Texture2D ToImage(IReadOnlyDictionary<BiomeType, Color32> colorPallet, float oceonCutOff = Tile.OCEAN_ALT_CUTOFF, float mountianCutOff = Tile.MOUNTAIN_ALT_CUTOFF) {
			Texture2D pic = new Texture2D(Width, Height);
			Color c;
			var w = Width;
			for (int i = 0; i < Length; i++) {
				var t = this[i];
				var alt = t.GetAltitudeColorMod(oceonCutOff, mountianCutOff);
				var biomeColor = colorPallet.GetColor(t.Biome.BiomeType);
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
			Creates an image depicting the altitude of the map.
		*/
		public Texture2D ToAltitudeMap() {
			Texture2D pic = new Texture2D(Width, Height);
			var w = Width;
			for (int i = 0; i < Length; i++) {
				var c = new Color32(0, (byte)(this[i].altitude.Steps(10) * 255), 0, 255);
				pic.SetPixel(i % w, i / w, c);
			}
			return pic;
		}

		/*
			Creates an image depicting the temperature of the map.
		*/
		public Texture2D ToTemperatureMap() {
			Texture2D pic = new Texture2D(Width, Height);
			var w = Width;
			for (int i = 0; i < Length; i++) {
				var c = new Color32((byte)(this[i].temperature.Steps(10) * 255), 0, 0, 255);
				pic.SetPixel(i % w, i / w, c);
			}
			return pic;
		}

		/*
			Creates an image depicting the humidity of the map.
		*/
		public Texture2D ToHumidityMap() {
			Texture2D pic = new Texture2D(Width, Height);
			var w = Width;
			for (int i = 0; i < Length; i++) {
				var t = this[i];
				var c = (t.BiomeType.IsWaterSource()) 
				  ? new Color32(50, 175, 125, 255)
				  : new Color32(0, 0, (byte)(t.humidity.Steps(10) * 255), 255);
				pic.SetPixel(i % w, i / w, c);
			}
			return pic;
		}

		/*
			Creates a map colored by the 3 main factors.
		*/
		public Texture2D ToFactorMap(
			float biomeCutoffA = GenerationUtils.BIOME_CUTOFF_A, float biomeCutoffB = GenerationUtils.BIOME_CUTOFF_B,
			float biomeCutoffC = GenerationUtils.BIOME_CUTOFF_C, float biomeCutoffD = GenerationUtils.BIOME_CUTOFF_D,
			float oceonCutOff = Tile.OCEAN_ALT_CUTOFF, float mountianCutOff = Tile.MOUNTAIN_ALT_CUTOFF,
			bool drawExtremeAltitudes = false
		) {
			// Gets the index 
			float getIndex(float val) {
				if(val < biomeCutoffB)
					return (val < biomeCutoffA) ? 0 : 0.25f;
				if(val > biomeCutoffC)
					return (val > biomeCutoffD) ? 1 : 0.75f;
				return 0.5f;
			}

			Texture2D pic = new Texture2D(Width, Height);
			var w = Width;
			for (int i = 0; i < Length; i++) {
				var t = this[i];
				Color32 c;
				if(drawExtremeAltitudes && t.altitude < oceonCutOff)
					c = new Color32(50, 175, 125, 255);
				else if(drawExtremeAltitudes && t.altitude > mountianCutOff)
					c = new Color32(75, 125, 200, 255);
				else
					c = new Color32(
						(byte) (getIndex(t.temperature) * 255),
						(byte) (getIndex(t.altitude.Lerp(oceonCutOff, mountianCutOff)) * 255),
						(byte) (getIndex(t.humidity) * 255), 
						255
					);
				pic.SetPixel(i % w, i / w, c);
			}
			return pic;
		}

		/*
			Creates an image depicting the wind strength and direction of the map.
		*/
		public Texture2D ToWindMap() {
			Texture2D pic = new Texture2D(Width, Height);
			var w = Width;
			for (int i = 0; i < Length; i++) {
				var t = this[i];
				var mag = t.windMagnitude;
				var red = 0;
				var blue = 0;
				var green = 0;

				var angle = t.windAngle.GetNegativeAngle();

				if (angle > 0) {
					red = (byte)(Mathf.Sqrt(angle.Lerp(0, Mathf.PI / 2)).Steps(10) * 255);
				} else {
					blue = (byte)(Mathf.Sqrt(angle.Lerp(0, -Mathf.PI / 2)).Steps(10) * 255);
				}

				green = (byte)(t.windMagnitude.Steps(10) * 255);

				var c = new Color32((byte)red, (byte)green, (byte)blue, 255);
				pic.SetPixel(i % w, i / w, c);
			}
			return pic;
		}

		Texture2D IReadOnlyGrid.ToFactorMap() => ToFactorMap();
	}
}
