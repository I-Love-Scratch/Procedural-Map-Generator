using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator {

	/*
		Class for encoding/decoding grids.
		Encodes/decodes between a grid and a binary file.

		In the binary file, each tile is converted to a sequence of bits, 
		representing the properties of the tile, which are written in sequential order, 
		making it possible to find where a given tile is stored.
	*/
	public class GridParser {
		// All different tile properties in order.
		private static readonly IReadOnlyList<TileProperty> PROPERTIES = Enum.GetValues(typeof(TileProperty))
			.Cast<TileProperty>()
			.Where(v => v != TileProperty.None)
			.ToList();


		/*
			Encodes the map data into a binary file.
		*/
		public void Encode(Grid g, MapState state, FileStream fs) {
			var w = g.Width;

			// Get a list of all the properties to store, in order.
			var tileConf = ((IMapState) state).TileStorageConfig;
			var props = PROPERTIES
				.Where(v => tileConf.ContainsKey(v))
				.Select(v => (v, tileConf[v]))
				.ToList();


			float totalWidth = props.Sum(v => v.Item2);

			for(int i = 0; i < g.Length; ++i) {
				var t = g[i];

				// The raw int representing the numeric representation of the tile.
				ulong raw = 0;
				int rawWidth = 0;


				// Encode each property in order.
				foreach(var (p, bits) in props) {
					var mask = (1 << bits) - 1;

					// Shift the number to make room for this property.
					raw <<= bits;

					// Encode the value and store it.
					raw += (uint) (p.Encode(state, mask, t) & mask);

					// Incease bitCount and check if it goes out of bounds.
					rawWidth += bits;

					// Write out the excess data.
					while(rawWidth > 7) {
						var padding = rawWidth - 8;
						var mask2 = (ulong)(1 << 8) - 1;
						mask2 <<= padding;
						var val = (byte)((raw & mask2) >> padding);

						fs.WriteByte(val);
						rawWidth -= 8;
					}
				}

				// If there is any data to spare, write it out.
				if(rawWidth > 0) {
					ulong mask = (ulong)((1 << rawWidth) - 1);
					var padding = 8 - rawWidth;
					fs.WriteByte((byte)((raw & mask) << padding));
				}

			}

		}

		/* 
			Decodes the binary file into a grid.
		*/
		public Grid Decode(FileStream fs, MapState state) {
			var g = new Grid(state.Width, state.Height);


			// Get a list of all the properties to store, in order.
			var tileConf = ((IMapState) state).TileStorageConfig;
			var props = PROPERTIES
				.Where(v => tileConf.ContainsKey(v))
				.Select(v => (v, tileConf[v]))
				.ToList();

			int totalWidth = props.Sum(v => v.Item2);
			int tileWidthBytes = (int) MathF.Ceiling(totalWidth / 8f);


			var w = state.Width;


			for(int i = 0; i < g.Length; ++i) {
				var t = g[i] = new Tile();


				ulong raw = 0;
				int rawWidth = 0;
				int remDat = tileWidthBytes;

				byte[] read = new byte[1];

				// Decode each property in order.
				foreach(var (p, bits) in props) {
					// Keep raw filled up.
					while(rawWidth < 32 && remDat > 0) {
						fs.Read(read, 0, 1);
						raw = (raw << 8) | read[0];
						rawWidth += 8; remDat -= 1;
					}

					var mask = (1 << bits) - 1;
					var mask2 = (ulong)mask << (rawWidth - bits);

					// Extract and decode the value.
					var val = (int)((raw & mask2) >> (rawWidth - bits));
					p.Decode(state, mask, t, val);

					// Shift the number to move the next property in place..
					//raw >>= bits;
					rawWidth -= bits;
				}

				//- Shouldn't need to move until rawWidth is 0, should always be 0 at the end.
			}

			return g;
		}

	}


	/*
		Version that splits the data into an array of images.
		The number of images matches the number of bits pr. tile / 32 rouded up.
	*/
	public class GridParser_ImageArray {
		
		public const int ENCODED_ALTITUDE_BITS		= 8;
		public const int ENCODED_BIOME_BITS			= 6;
		public const int ENCODED_WIND_ANGLE_BITS    = 4;
		public const int ENCODED_WIND_MAGNITUDE_BITS = 4;

		public const int ENCODED_ALTITUDE_MASK		= (1 << ENCODED_ALTITUDE_BITS) - 1;
		public const int ENCODED_BIOME_MASK			= (1 << ENCODED_BIOME_BITS) - 1;
		public const int ENCODED_WIND_ANGLE_MASK	= (1 << ENCODED_WIND_ANGLE_BITS) - 1;
		public const int ENCODED_WIND_MAGNITUDE_MASK = (1 << ENCODED_WIND_MAGNITUDE_BITS) - 1;

		// Number of bits in a pixel.
		public const int PIXEL_SIZE = 32;


		private static readonly IReadOnlyList<TileProperty> PROPERTIES = Enum.GetValues(typeof(TileProperty))
			.Cast<TileProperty>()
			.Where(v => v != TileProperty.None)
			.ToList();


		/*
			Encodes the map data into images.
			This is meant to be the reversable version that allows maps to be loaded from image later on.
		*/
		public Texture2D[] Encode(Grid g, MapState state) {
			var w = g.Width;

			// Get a list of all the properties to store, in order.
			var tileConf = ((IMapState) state).TileStorageConfig;
			var props = PROPERTIES
				.Reverse()
				.Where(v => tileConf.ContainsKey(v))
				.Select(v => (v, tileConf[v]))
				.ToList();


			float totalWidth = props.Sum(v => v.Item2);
			var imgCount = Mathf.CeilToInt(totalWidth / PIXEL_SIZE);

			var pics = new Texture2D[imgCount];
			for(int i = 0; i < imgCount; ++i) pics[i] = new Texture2D(g.Width, g.Height);

			var cols = new List<Color32[]>();
			for(int i = 0; i < imgCount; ++i) cols.Add(new Color32[g.Length]);
			//var cols = new Color32[g.Length, imgCount];

			for(int i = 0; i < g.Length; ++i) {
				var t = g[i];

				// The raw int representing the numeric representation of the tile.
				ulong raw = 0;
				int bitCount = 0; // # of bits we've added so far.
				int imgNum = 0; // The next image to save to.


				// Encode each property in order.
				foreach(var (p, bits) in props) {
					var mask = (1 << bits) - 1;

					// Shift the number to make room for this property.
					raw <<= bits;

					// Encode the value and store it.
					raw += (uint) (p.Encode(state, mask, t) & mask);

					// Incease bitCount and check if it goes out of bounds.
					bitCount += bits;
					if(bitCount >= PIXEL_SIZE) {
						ulong numMask = ((1ul << PIXEL_SIZE) - 1) << (bitCount - PIXEL_SIZE);
						var num = (uint) ((raw & numMask) >> (bitCount - PIXEL_SIZE));
						raw = raw & ~numMask;
						bitCount -= PIXEL_SIZE;

						// Store the data in the image.
						cols[imgNum++][i] = num.ToColor32();
					}
				}

				if(bitCount > 0) cols[imgNum][i] = ((int) raw).ToColor32();
				//colors[i] = raw.ToColor32();
			}

			// Encode pics
			for(int i = 0; i < imgCount; ++i) 
				pics[i].SetPixels32(cols[i]);

			return pics;
		}

		/* 
			Decodes the image into a grid.
		*/
		public Grid Decode(Texture2D[] pics, MapState state) {
			var g = new Grid(state.Width, state.Height);


			// Get a list of all the properties to store, in order.
			var tileConf = ((IMapState) state).TileStorageConfig;
			var props = PROPERTIES
				.Where(v => tileConf.ContainsKey(v))
				.Select(v => (v, tileConf[v]))
				.ToList();

			int totalWidth = props.Sum(v => v.Item2);
			var imgCount = Mathf.CeilToInt((float) totalWidth / PIXEL_SIZE);

			// Check that we get the correct number of images.
			if(pics.Length != imgCount)
				throw new ArgumentException(nameof(pics), $"Incorrect number of images, recieved {pics.Length}, expected {imgCount}");

			// Extract the pixels.
			var cols = new List<Color32[]>();
			foreach(var pic in pics) cols.Add(pic.GetPixels32());


			var w = state.Width;


			for(int i = 0; i < g.Length; ++i) {
				// The raw int representing the numeric representation of the tile.
				//var c = pixelArr[i];
				//var raw = c.ToRawInt();

				var t = g[i] = new Tile();


				int j = cols.Count - 1;
				ulong raw = cols[j--][i].ToRawUInt();
				int rawWidth = totalWidth % PIXEL_SIZE;

				// Decode each property in order.
				foreach(var (p, bits) in props) {
					// Append the next pixel to the raw value.
					if(rawWidth < PIXEL_SIZE && j > -1) {
						raw |= ((ulong) cols[j--][i].ToRawUInt()) << rawWidth;
						rawWidth += PIXEL_SIZE;
					}

					var mask = (1 << bits) - 1;

					// Extract and decode the value.
					var val = (int)(raw & (ulong)mask);
					p.Decode(state, mask, t, val);

					// Shift the number to move the next property in place..
					raw >>= bits;
					rawWidth -= bits;
				}
			}

			return g;
		}

	}

}
