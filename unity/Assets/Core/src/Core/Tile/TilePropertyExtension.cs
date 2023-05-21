using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator {
	/*
		Extension methods for TileProperty.
	*/
	public static partial class TilePropertyExtension {

		/*
			Encode a given property of the tile.
		*/
		public static int Encode(this TileProperty type, IMapState state, int mask, Tile t) {
			if(!_parsers.TryGetValue(type, out var parser)) throw new ArgumentOutOfRangeException(nameof(type), "Type had no parser!");
			return parser.Encoder(state, mask, t);
		}


		/*
			Decode a given property of the tile.
		*/
		public static void Decode(this TileProperty type, IMapState state, int mask, Tile t, int val) {
			if(!_parsers.TryGetValue(type, out var parser)) throw new ArgumentOutOfRangeException(nameof(type), "Type had no parser!");
			parser.Decoder(state, mask, t, val);
		}
	}


	/*
		Fields and background functionality.
	*/
	public static partial class TilePropertyExtension {

		/*
			Class for encoding/decoding a property of a tile.
		*/
		public class TilePropertyParser {

			// The property type this parses.
			public TileProperty PropertyType { get; }

			// The encoding function.
			public Func<IMapState, int, Tile, int> Encoder { get; }

			// The decoding function.
			public Action<IMapState, int, Tile, int> Decoder { get; }


			public TilePropertyParser(
				TileProperty type,
				Func<IMapState, int, Tile, int> encoder, 
				Action<IMapState, int, Tile, int> decoder
			) {
				PropertyType = type;
				Encoder = encoder;
				Decoder = decoder;
			}
		}


		/*
			The parsers for each TileProperty.
		*/
		private static Dictionary<TileProperty, TilePropertyParser> _parsers = _getParsers();

		/*
			Creates a dictionary mapping TileProperties to parsers.
		*/
		private static Dictionary<TileProperty, TilePropertyParser> _getParsers() {
			var lst = new List<TilePropertyParser> {
				new TilePropertyParser(TileProperty.Biome, EncodeBiome, DecodeBiome),
				new TilePropertyParser(TileProperty.Altitude, EncodeAltitude, DecodeAltitude),
				new TilePropertyParser(TileProperty.Humidity, EncodeHumidity, DecodeHumidity),
				new TilePropertyParser(TileProperty.Temperature, EncodeTemperature, DecodeTemperature),
				new TilePropertyParser(TileProperty.WindDirection, EncodeWindAngle, DecodeWindAngle),
				new TilePropertyParser(TileProperty.WindMagnitude, EncodeWindMagnitude, DecodeWindMagnitude),
			};

			return lst.ToDictionary(v => v.PropertyType);
		}

	}


	/*
		Encoders and decoders.
	*/
	public static partial class TilePropertyExtension {


		private static int EncodeAltitude(IMapState state, int mask, Tile t) {
			// Get altitude relative to what BiomeType it is.
			var relAlt = t.BiomeType switch {
				BiomeType.Mountain => t.altitude.Lerp(state.MountainLevel, 1),
				BiomeType.Ocean    => t.altitude.Lerp(0,                   state.OceonLevel),
				BiomeType.Empty    => t.altitude,
				_                  => t.altitude.Lerp(state.OceonLevel,    state.MountainLevel),
			};

			// Scale the relative altitude to the size we have allocated altitude.
			var alt = (int) Math.Min(relAlt.Scale(0, mask + 1), mask);

			return alt;
		}
		private static void DecodeAltitude(IMapState state, int mask, Tile t, int val) {
			// Extract the altitude. (Is relative to the biome it belongs to)
			var relAlt = Utils.Lerp(val, 0, mask);

			// Scale the altitude based on biome.
			var alt = t.BiomeType switch {
				BiomeType.Mountain => relAlt.Scale(state.MountainLevel, 1),
				BiomeType.Ocean    => relAlt.Scale(0, state.OceonLevel),
				BiomeType.Empty    => relAlt,
				_                  => relAlt.Scale(state.OceonLevel, state.MountainLevel),
			};

			// Fill in data.
			t.altitude = alt;
		}


		private static int EncodeBiome(IMapState state, int mask, Tile t)
			=> t.Biome.Id;
		private static void DecodeBiome(IMapState state, int mask, Tile t, int val)
			=> t.Biome = state.GetBiome(val);


		private static int EncodeTemperature(IMapState state, int mask, Tile t)
			=> (int) Math.Min(t.temperature.Scale(0, mask + 1), mask);
		private static void DecodeTemperature(IMapState state, int mask, Tile t, int val)
			=> t.temperature = ((float) val).Lerp(0, mask);

		private static int EncodeWindAngle(IMapState state, int mask, Tile t)
			=> (int)Math.Floor((t.windAngle / (2 * Math.PI)) * mask);
		private static void DecodeWindAngle(IMapState state, int mask, Tile t, int val)
			=> t.windAngle = (float)(2 * Math.PI) * Utils.Lerp(val & mask, 0, mask);

		private static int EncodeWindMagnitude(IMapState state, int mask, Tile t)
			=> (int)Math.Floor(t.windMagnitude * mask);
		private static void DecodeWindMagnitude(IMapState state, int mask, Tile t, int val)
			=> t.windMagnitude = Utils.Lerp(val & mask, 0, mask);

		private static int EncodeHumidity(IMapState state, int mask, Tile t)
			=> (int) Math.Min(t.humidity.Scale(0, mask + 1), mask);
		private static void DecodeHumidity(IMapState state, int mask, Tile t, int val)
			=> t.humidity = ((float) val).Lerp(0, mask);

	}
}
