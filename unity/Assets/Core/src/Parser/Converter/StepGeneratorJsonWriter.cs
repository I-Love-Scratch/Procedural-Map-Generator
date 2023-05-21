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
	public class StepGeneratorJsonWriter : JsonConverter<IStepGenerator> {

        public const string TYPE_NAME_PROPERTY_NAME = "Type";
        public const string TYPE_DATA_PROPERTY_NAME = "Data";


        public override void WriteJson(JsonWriter writer, IStepGenerator val, JsonSerializer serializer) {
            if(val == null) return; 

            writer.WriteStartObject();

            // Writes the name of the type as a property.
            writer.WritePropertyName(TYPE_NAME_PROPERTY_NAME);
            writer.WriteValue(val.GetType().ToString());

            // Stores the actual JSON of the object as a second property.
            writer.WritePropertyName(TYPE_DATA_PROPERTY_NAME);
            JObject.FromObject(val, serializer).WriteTo(writer);

            writer.WriteEndObject();

            //writer.WriteValue(val.ToString());
        }

        public override IStepGenerator ReadJson(JsonReader reader, Type objectType, IStepGenerator existingValue, bool hasExistingValue, JsonSerializer serializer) {

            IStepGenerator res = null;

            while(reader.Read()) {

                if(reader.TokenType == JsonToken.EndObject) break;

                if(reader.TokenType == JsonToken.PropertyName) {

                    // Create instance of the given type.
                    if(reader.Value == TYPE_NAME_PROPERTY_NAME) {
                        reader.Read();
                        var name = (string) reader.Value;
                        res = GenerationUtils.GetImplementationInstance(name);

                    } 
                    
                    // Fill the instance.
                    else if(reader.Value == TYPE_DATA_PROPERTY_NAME) {
                        // Throw if we haven't found the type yet.
                        if(res == null) throw new Exception("No type was found in the JSON before the data.");

                        reader.Read();
                        serializer.Populate(reader, res);

                    }
                }

            }
            //JsonSerializer.Populate(JsonReader, Object);
            //string s = (string)reader.Value;

            return res;
        }
    }


}
