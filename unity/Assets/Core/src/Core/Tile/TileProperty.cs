using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator {
	/*
		The properties of a tile.
		Used for configuring what properties and how much space to use when exporting a map.
		IMPORTANT: The order they're defined in decides the order they're encoded/decoded.
	*/
	public enum TileProperty {
		None,
		Biome,
		Altitude, Temperature, Humidity,
		WindDirection, WindMagnitude,
	}
}
