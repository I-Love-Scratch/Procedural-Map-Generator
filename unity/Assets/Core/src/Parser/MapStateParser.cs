using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft;

namespace terrainGenerator {

	/*
		Class for encoding/decoding MapStates.
	*/
	public class MapStateParser {
		
		// The serialization settings to use.
		private JsonSerializerSettings Settings { get; }

		public MapStateParser() {

			// Create the settings.
			var settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.Auto;
			settings.DefaultValueHandling = DefaultValueHandling.Ignore;
			settings.Converters.Add(new StringEnumConverter());
			settings.Converters.Add(new ResourceLocationConverter());
			Settings = settings;
		}

		/* 
			Encodes the map state into a JSON string.
		*/
		public string Encode(MapState obj) {
			return JsonConvert.SerializeObject(obj, Settings);
		}

		/* 
			Decodes a JSON string into a map state.
		*/
		public MapState Decode(string src) {
			return JsonConvert.DeserializeObject<MapState>(src, Settings);
		}

	}
}
