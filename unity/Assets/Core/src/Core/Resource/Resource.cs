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
		Represents a resource blob
	*/
	public interface IResource { 
		// The id used to identify a resource blob on a map.
		int Id { get; }

		// The type of resource this is.
		ResourceType ResourceType { get; }

		// The default color of this resource.
		Color32 Color { get; }
	}


	/*
		A class representing a resource blob
	*/
	public class Resource : IResource {

		// The id used to identify a resource blob on a map.
		public int Id { get; set; }

		// The type of resource this is.
		public ResourceType ResourceType { get; set; }

		/*
			Returns the color of this instance
		*/
		public Color32 Color => new Color32(255, 255, 255, 255);

		/*
			A list of all tiles this resource is on
			Used only for saving to json
		*/
		public List<ResourceLocation> Tiles { get; set; } = new List<ResourceLocation>();


		public Resource(int id, ResourceType type) {
			Id = id;
			ResourceType = type;
		}

	}
}
