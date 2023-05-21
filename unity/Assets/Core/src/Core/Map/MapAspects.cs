using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace terrainGenerator {
	/*
		The different aspects of a map. (Flags)
		Used for telling what aspects have been added or need to be added.
	*/
	[Flags]
	public enum MapAspects {
		None				= 0,
		Altitude			= 1,
		Wind				= 1 << 1,
		TemperatureHumidity	= 1 << 2,
		Biomes				= 1 << 3,
		Nature				= 1 << 4,
		Resources			= 1 << 5,
		Artifacts			= 1 << 6,
		OceanMountain		= 1 << 7,
		RiverLake			= 1 << 8,
	}
}