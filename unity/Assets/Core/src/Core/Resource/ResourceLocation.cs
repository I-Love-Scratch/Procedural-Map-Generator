using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;
using Newtonsoft.Json;

namespace terrainGenerator {

	/*
		Represents a single tile where a resource is present
	*/
	public class ResourceLocation {
		// The x coordinate of the tile on the grid.
		public int X { get; set; }

		// The y coordinate of the tile on the grid.
		public int Y { get; set; }

		// The density of this resource on the tile.
		public float Density { get; set; }

		public ResourceLocation(int x, int y, float density) {
			X = x;
			Y = y;
			Density = density;
		}

		/*
			Convert to string format for json file.
			Ex.: ResourceLocation(1, 2, 0.237) => "1-2-0.24"
		*/
		public override string ToString()
			=> $"{X}-{Y}-{Math.Round(Density, 2)}";

		/*
			Set variables based on string from json file.
			Ex.: "7-2-0.55" => ResourceLocation(7, 2, 0.55)
		*/
		static public ResourceLocation FromString(string str) {
			var parts = str.Split('-');
			var res = new ResourceLocation(
				int.Parse(parts[0]),
				int.Parse(parts[1]),
				float.Parse(parts[2]));
			return res;
		}

	}

	
}
