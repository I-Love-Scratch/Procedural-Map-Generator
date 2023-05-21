using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Numerics;

namespace terrainGenerator {
	
	/*
		Extension methods for biome
	*/
	public static partial class BiomeExtension {

		/*
			Creates a new instance of the given type. 
		*/
		public static Biome NewInstance(this BiomeType type)
			=> Biome.Create(type);

		/*
			Gets a default Biome instance of the given type. 
		*/
		public static Biome GetDefaultInstance(this BiomeType type)
			=> _biomeInstances[type];


		/* 
			Gets a biome color from a color pallet, uses the default colors as fallback if none are found.
		*/
		public static Color32 GetColor(this IReadOnlyDictionary<BiomeType, Color32> colorPalet, BiomeType type) 
			=> colorPalet.GetValueOrDefault(type, type.GetColor());

		/*
			Returns the color of the given BiomeType.
		*/
		public static Color32 GetColor(this BiomeType type) => _biomeObj[type].color;

		/*
			Returns biome object from enum.
		*/
		public static BiomeTypeObj GetObj(this BiomeType type) => _biomeObj[type];

		/*
			Returns the rate a can overgrow b at
		*/
		public static float OvergrowRate(this BiomeType a, BiomeType b) {
			//no point overgrowing yourself
			if (a == b) return 0;
			//oceans and mountains cannot spread or be replaced
			if (a == BiomeType.Ocean || a == BiomeType.Mountain ||
				b == BiomeType.Ocean || b == BiomeType.Mountain) return 0;

			switch (a) {
				case BiomeType.Soil:
					return (b == BiomeType.Empty) ? 1 : 0;
				case BiomeType.Stone:
					return (b == BiomeType.Soil || b == BiomeType.Empty) ? 0.7f : 0;
				case BiomeType.Sand:
					return (b == BiomeType.Soil || b == BiomeType.Empty) ? 0.9f : 0;
				case BiomeType.Forest:
					switch (b) {
						case BiomeType.Sand: return 0.9f;
						case BiomeType.Stone: return 0.8f;
					}
					break;
				case BiomeType.Swamp:
					switch (b) {
						case BiomeType.Sand: return 0.6f;
						case BiomeType.Stone: return 0.6f;
						case BiomeType.Forest: return 0.2f;
					}
					break;
			}
			return 0;
		}

		/*
			Indicates whether a biome can spread.
		*/
		public static bool CanSpread(this BiomeType type) =>
			(type != BiomeType.Mountain) && (type != BiomeType.Stone) &&
			(type != BiomeType.Ocean) && (type != BiomeType.Sand) &&
			(type != BiomeType.River) && (type != BiomeType.Lake) &&
			(type != BiomeType.Glacier);
		
		/*
			Checks if the type is a source of water.
		*/
		public static bool IsWaterSource(this BiomeType type) =>
			(type == BiomeType.Ocean) || (type == BiomeType.River) || 
			(type == BiomeType.Lake) || (type == BiomeType.Glacier);
	}


	/*
		Extension methods for biome.
		This part contains fields, constants and private methods.
	*/
	public static partial class BiomeExtension {

		public static readonly Color32 JungleColor     = new Color32((byte)0, (byte)102, (byte)0, (byte) 255);
		public static readonly Color32 SavannaColor    = new Color32((byte)255, (byte)153, (byte)51, (byte) 255);
		public static readonly Color32 DesertColor     = new Color32((byte)255, (byte)204, (byte)0, (byte) 255);
		public static readonly Color32 GrasslandsColor = new Color32((byte)153, (byte)255, (byte)51, (byte) 255);
		public static readonly Color32 ForestColor     = new Color32((byte)51, (byte)204, (byte)51, (byte) 255);
		public static readonly Color32 SwampColor      = new Color32((byte)51, (byte)153, (byte)51, (byte) 255);
		public static readonly Color32 ArcticColor     = new Color32((byte)0, (byte)255, (byte)255, (byte) 255);
		public static readonly Color32 BorealColor     = new Color32((byte)51, (byte)153, (byte)255, (byte) 255);
		public static readonly Color32 TundraColor     = new Color32((byte)102, (byte)102, (byte)255, (byte) 255);
												     
		public static readonly Color32 RiverColor      = new Color32((byte)0, (byte)204, (byte)255, (byte) 255);
		public static readonly Color32 LakeColor       = new Color32((byte)0, (byte)102, (byte)255, (byte) 255);
		public static readonly Color32 GlacierColor    = new Color32((byte)153, (byte)204, (byte)255, (byte) 255);



		/*
			A color pallet that makes biomes more distinct.
		*/
		public static readonly IReadOnlyDictionary<BiomeType, Color32> BiomeColorPallet_VisibleBorders = new Dictionary<BiomeType, Color32> {
			{ BiomeType.Jungle,     new Color32(255, 0, 0, 255) },
			{ BiomeType.Savanna,    new Color32(255, 157, 0, 255) },
			{ BiomeType.Desert,     new Color32(255, 234, 0, 255) },

			{ BiomeType.Grasslands, new Color32(195, 255, 0, 255) },
			{ BiomeType.Forest,     new Color32(30, 255, 0, 255) },
			{ BiomeType.Swamp,      new Color32(0, 255, 179, 255) },

			{ BiomeType.Arctic,     new Color32(255, 0, 170, 255) },
			{ BiomeType.Boreal,     new Color32(234, 0, 255, 255) },
			{ BiomeType.Tundra,     new Color32(136, 0, 255, 255) },

			{ BiomeType.River,      new Color32(0, 204, 255, 255) },
			{ BiomeType.Lake,       new Color32(0, 102, 255, 255) },
			{ BiomeType.Glacier,    GlacierColor },

			{ BiomeType.Empty,      new Color32(255, 255, 255, 0) },
		};

		/*
			A color pallet that makes biomes look more nice.
		*/
		public static readonly IReadOnlyDictionary<BiomeType, Color32> BiomeColorPallet_HumanFriendly = new Dictionary<BiomeType, Color32> {
			{ BiomeType.Jungle,     new Color32(0, 102, 0, 255) },
			{ BiomeType.Savanna,    new Color32(255, 153, 51, 255) },
			{ BiomeType.Desert,     new Color32(255, 204, 0, 255) },

			{ BiomeType.Grasslands, new Color32(153, 255, 51, 255) },
			{ BiomeType.Forest,     new Color32(51, 204, 51, 255) },
			{ BiomeType.Swamp,      new Color32(51, 153, 51, 255) },

			{ BiomeType.Arctic,     new Color32(0, 255, 255, 255) },
			{ BiomeType.Boreal,     new Color32(51, 153, 255, 255) },
			{ BiomeType.Tundra,     new Color32(102, 102, 255, 255) },

			{ BiomeType.River,      new Color32(0, 204, 255, 255) },
			{ BiomeType.Lake,       new Color32(0, 102, 255, 255) },
			{ BiomeType.Glacier,    GlacierColor },

			{ BiomeType.Empty,      new Color32(255, 255, 255, 0) },
			//{ BiomeType., Color.FromArgb() },
		};

		/*
			A color pallet that makes biomes look more nice.
		*/
		public static readonly IReadOnlyDictionary<BiomeType, Color32> BiomeColorPallet_Updated = new Dictionary<BiomeType, Color32> {
			{ BiomeType.Jungle,     new Color32(0, 102, 0, 255) },
			{ BiomeType.Savanna,    new Color32(255, 153, 51, 255) },
			{ BiomeType.Desert,     new Color32(255, 204, 0, 255) },

			{ BiomeType.Grasslands, new Color32(153, 255, 51, 255) },
			{ BiomeType.Forest,     new Color32(51, 204, 51, 255) },
			{ BiomeType.Swamp,      new Color32(51, 153, 51, 255) },

			{ BiomeType.Arctic,     new Color32(102, 255, 204, 255) },
			{ BiomeType.Boreal,     new Color32(153, 255, 204, 255) },
			{ BiomeType.Tundra,     new Color32(204, 255, 204, 255) },

			{ BiomeType.River,      new Color32(0, 204, 255, 255) },
			{ BiomeType.Lake,       new Color32(0, 102, 255, 255) },
			{ BiomeType.Glacier,    new Color32(153, 204, 255, 255) },

			{ BiomeType.Mountain,   new Color32(255, 255, 255, 255) },
			{ BiomeType.Stone,		new Color32(153, 102, 51, 255) },
			{ BiomeType.Ocean,		new Color32(0, 0, 255, 255) },
			{ BiomeType.Sand,		new Color32(245, 245, 220, 255) },

			{ BiomeType.Empty,      new Color32(255, 255, 255, 0) },
			//{ BiomeType., Color.FromArgb() },
		};


		/*
			Default instances of each biome. 
		*/
		private static Dictionary<BiomeType, Biome> _biomeInstances = new Dictionary<BiomeType, Biome>();


		/*
			Maps the BiomeTypes to their parameters
		*/
		private static Dictionary<BiomeType, BiomeTypeObj> _biomeObj = new Dictionary<BiomeType, BiomeTypeObj> {
			{ BiomeType.Empty,     new BiomeTypeObj(BiomeType.Empty,    Color.white,     0, 0, 0, 0, 0, 0) },

			{ BiomeType.Ocean,     new BiomeTypeObj(BiomeType.Ocean,    Color.blue,      0, 0, 0, 0, 0, 0) },
			{ BiomeType.Mountain,  new BiomeTypeObj(BiomeType.Mountain, Color.black,     0, 0, 0, 0, 0, 0) },
			{ BiomeType.Soil,      new BiomeTypeObj(BiomeType.Soil,     new Color32(165, 42, 42, 155),     10, 0, 0, 8, 16, 0.6f) },
			{ BiomeType.Sand,      new BiomeTypeObj(BiomeType.Sand,     new Color32(245, 245, 220, 255),     4, 2, 5, 8, 16, 0.5f) },
			{ BiomeType.Stone,     new BiomeTypeObj(BiomeType.Stone,    Color.grey,      4, 2, 5, 8, 16, 0.7f) },

			{ BiomeType.Desert,     new BiomeTypeObj(BiomeType.Desert,     DesertColor,     0, 0, 0, 0, 0, 0) },
			{ BiomeType.Savanna,    new BiomeTypeObj(BiomeType.Savanna,    SavannaColor,    0, 0, 0, 0, 0, 0) },
			{ BiomeType.Jungle,     new BiomeTypeObj(BiomeType.Jungle,     JungleColor,     0, 0, 0, 0, 0, 0) },
			{ BiomeType.Grasslands, new BiomeTypeObj(BiomeType.Grasslands, GrasslandsColor, 0, 0, 0, 0, 0, 0) },
			{ BiomeType.Forest,    new BiomeTypeObj(BiomeType.Forest,      ForestColor,     3, 4, 5, 8, 16, 0.6f) },
			{ BiomeType.Swamp,     new BiomeTypeObj(BiomeType.Swamp,       SwampColor,      3, 4, 5, 8, 16, 0.5f) },
			{ BiomeType.Tundra,     new BiomeTypeObj(BiomeType.Tundra,     TundraColor,     0, 0, 0, 0, 0, 0) },
			{ BiomeType.Boreal,     new BiomeTypeObj(BiomeType.Boreal,     BorealColor,     0, 0, 0, 0, 0, 0) },
			{ BiomeType.Arctic,     new BiomeTypeObj(BiomeType.Arctic,     ArcticColor,     0, 0, 0, 0, 0, 0) },

			{ BiomeType.River,     new BiomeTypeObj(BiomeType.River,        RiverColor,      0, 0, 0, 0, 0, 0) },
			{ BiomeType.Lake,      new BiomeTypeObj(BiomeType.Lake,         LakeColor,      0, 0, 0, 0, 0, 0) },
			{ BiomeType.Glacier,   new BiomeTypeObj(BiomeType.Glacier,      GlacierColor,      0, 0, 0, 0, 0, 0) },
		};


		/*
			Constructor for the static class.
			Initializes important values.
		*/
		static BiomeExtension() {
			_createDefaltBiomeInstances();
			_createDefaltBiomeObjs();
		}

		/*
			Creates the dictionary of default Biome instances.
		*/
		private static void _createDefaltBiomeInstances() {
			foreach (var type in Enum.GetValues(typeof(BiomeType)).Cast<BiomeType>()) 
				_biomeInstances[type] = type.NewInstance();
		}

		/*
			Creates the dictionary of BiomeTypeObjs.
		*/
		private static void _createDefaltBiomeObjs() {
			foreach (var type in Enum.GetValues(typeof(BiomeType)).Cast<BiomeType>()) {
				if (!_biomeObj.ContainsKey(type)) 
					_biomeObj[type] = new BiomeTypeObj(type, new Color32(255, 255, 255, 0), 0, 0, 0, 0, 0, 0);
			}

		}

	}

}
