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
		Represents a resource present on a specific tile
	*/
	public interface ITileResource { 

		// The density of the resource on this tile. (in range [0, 1])
		float Density { get; set; }

		// The type of resource this is.
		ResourceType ResourceType { get; }

		// The resource blob this belongs to.
		IResource Resource { get; }
	}


	/*
		A class representing a resource present on a specific tile
	*/
	public class TileResource : ITileResource {

		// The density of the resource on this tile. (in range [0, 1])
		public float Density { get; set; }

		// The type of resource this is.
		public ResourceType ResourceType => Resource?.ResourceType ?? ResourceType.Empty;

		// The resource blob this belongs to.
		public IResource Resource { get; set; }


		public TileResource(IResource resource, float density = 1f) {
			Resource = resource;
			Density = density;
		}
	}
}
