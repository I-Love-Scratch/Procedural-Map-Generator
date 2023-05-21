using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using UnityEngine;

namespace terrainGenerator.Generator {

	/*
		Builder class for map generators.
	*/
	[Serializable]
	public class GeneratorBuilder {
		[field: SerializeField]
		[field: Tooltip("The parameters of how the map should be.")]
		public GenParams Parameters;


		/*
			The map used as basis for the map we generate.
			Useful for improving existing maps or iterating on maps.
		*/
		public GameMap BasisMap = null;


		[field: SerializeReference]
		[field: Tooltip("The generator used to give the map altitude.")]
		public IAltitudeGenerator AltitudeGenerator;
		[field: SerializeReference]
		[field: Tooltip("The generator used to smoothen out the altitude of the map.")]
		public IAltitudeSmoothenerGenerator AltitudeSmoothenerGenerator;
		[field: SerializeReference]
		[field: Tooltip("The generator used to give the map ocean and mountain.")]
		public IOceanMountianGenerator OceanMountianGenerator;
		[field: SerializeReference]
		[field: Tooltip("The generator used to add rivers and lakes to the map.")]
		public IRiverLakeGenerator RiverLakeGenerator;
		[field: SerializeReference]
		[field: Tooltip("The generator used to add wind to the map.")]
		public IWindGenerator WindGenerator;
		[field: SerializeReference]
		[field: Tooltip("The generator used to generate temperature and humidity.")]
		public ITemperatureHumidityGenerator TemperatureHumidityGenerator;
		[field: SerializeReference]
		[field: Tooltip("The generator used to give the tiles a biome.")]
		public IBiomeGenerator BiomeGenerator;
		[field: SerializeReference]
		[field: Tooltip("The generator used to smoothen out the map a little. (Things like biome)")]
		public IMapSmoothenerGenerator MapSmoothenerGenerator;
		[field: SerializeReference]
		[field: Tooltip("The generator used to add resources to the map.")]
		public IResourceGenerator ResourceGenerator;
		[field: SerializeReference]
		[field: Tooltip("The generator used to add nature to the map.")]
		public INatureGenerator NatureGenerator;
		[field: SerializeReference]
		[field: Tooltip("The generator used to artifacts to the map.")]
		public IArtifactGenerator ArtifactGenerator;



		public GeneratorBuilder(GenParams par) {
			if (par == null) throw new ArgumentNullException(nameof(par));
			Parameters = par;
		}
		public GeneratorBuilder() { }


		/*
			Sets the generator parameters and returns the builder.
		*/
		public GeneratorBuilder With(GenParams par) {
			Parameters = par;
			return this;
		}



		/*
			These functions set the step generator and returns the builder.
		*/

		public GeneratorBuilder With(IAltitudeGenerator gen) {
			AltitudeGenerator = gen;
			return this;
		}

		public GeneratorBuilder With(IAltitudeSmoothenerGenerator gen) {
			AltitudeSmoothenerGenerator = gen;
			return this;
		}

		public GeneratorBuilder With(IOceanMountianGenerator gen) {
			OceanMountianGenerator = gen;
			return this;
		}

		public GeneratorBuilder With(IRiverLakeGenerator gen) {
			RiverLakeGenerator = gen;
			return this;
		}

		public GeneratorBuilder With(IWindGenerator gen) {
			WindGenerator = gen;
			return this;
		}

		public GeneratorBuilder With(ITemperatureHumidityGenerator gen) {
			TemperatureHumidityGenerator = gen;
			return this;
		}

		public GeneratorBuilder With(IBiomeGenerator gen) {
			BiomeGenerator = gen;
			return this;
		}

		public GeneratorBuilder With(IMapSmoothenerGenerator gen) {
			MapSmoothenerGenerator = gen;
			return this;
		}

		public GeneratorBuilder With(IResourceGenerator gen) {
			ResourceGenerator = gen;
			return this;
		}

		public GeneratorBuilder With(INatureGenerator gen) {
			NatureGenerator = gen;
			return this;
		}

		public GeneratorBuilder With(IArtifactGenerator gen) {
			ArtifactGenerator = gen;
			return this;
		}


		/*
			Builds a MapGenerator out of this GeneratorBuilder.
		*/
		public MapGenerator Build() {
			if (Parameters == null) throw new ArgumentNullException(nameof(Parameters));
			return new MapGenerator(this);
		}
	}

}