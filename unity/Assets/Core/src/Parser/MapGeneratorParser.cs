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
using System.IO;

using terrainGenerator.Generator;


namespace terrainGenerator {

	/*
		Class for encoding/decoding GeneratorBuilder.
        [NOTE] Unused so far, found settings to help with the problem, keeping it for now in case it can be used as reference later.
	*/
	public class MapGeneratorParser {
		public const string DEFAULT_PATH = "Data/Test/";

		private JsonSerializerSettings Settings { get; }

		public MapGeneratorParser() {

			// Create the settings.
			var settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.TypeNameHandling = TypeNameHandling.Auto;
			settings.Converters.Add(new StringEnumConverter());
			//settings.Converters.Add(new StepGeneratorJsonWriter());
			Settings = settings;
		}
		

		/* 
			Encodes the map data into an image.
			This is meant to be the reversable version that allows maps to be loaded from image later on.
		*/
		public string Encode(GeneratorBuilder obj) {
			return JsonConvert.SerializeObject(obj, Settings);
		}

		public string EncodeFile(GeneratorBuilder obj, string name = null) {
			name ??= obj.Parameters.Name;
			var baseFilePath = $"{DEFAULT_PATH}{name}.json";
			var res = Encode(obj);

			using(StreamWriter sw = File.CreateText(baseFilePath)) {
				sw.WriteLine(res);
			}

			return name;
		}


		/* 
			Decodes the image into a grid.
		*/
		public GeneratorBuilder Decode(string src) {
			return JsonConvert.DeserializeObject<GeneratorBuilder>(src, Settings);
		}
		public GeneratorBuilder DecodeFile(string name) {
			using(StreamReader sr = File.OpenText($"{DEFAULT_PATH}{name}.json")) {
				return Decode(sr.ReadToEnd());
			}
		}

	}
}
