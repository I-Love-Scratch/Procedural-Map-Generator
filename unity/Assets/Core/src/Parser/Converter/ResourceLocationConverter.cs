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
using Newtonsoft.Json.Linq;
using terrainGenerator.Generator;

namespace terrainGenerator {

	/*
		Class for converting IStepGenerator instances to and from JSON.

        Creates an object with the name of the type as one property, and the actuall JSON as the other field.
	*/
	public class ResourceLocationConverter : JsonConverter<ResourceLocation> {

        public override void WriteJson(JsonWriter writer, ResourceLocation val, JsonSerializer serializer) {
            if(val == null) return; 
            writer.WriteValue(val.ToString());
        }

        public override ResourceLocation ReadJson(JsonReader reader, Type objectType, ResourceLocation existingValue, bool hasExistingValue, JsonSerializer serializer) {
			ResourceLocation res = ResourceLocation.FromString((string)reader.Value);
            return res;
        }
    }


}
