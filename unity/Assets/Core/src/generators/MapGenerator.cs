using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace terrainGenerator.Generator {

	/*
		 Generates a map step-by-step.
	*/
	public class MapGenerator : IMapGenerator {

		// The state of the map being generated.
		public GeneratorState State { get; }

		// The grid of the map being generated.
		public Grid Grid { get; protected set; }


		public IAltitudeGenerator AltitudeGenerator { get; }
		public IAltitudeSmoothenerGenerator AltitudeSmoothener { get; set; }
		public IOceanMountianGenerator OceanMountianGenerator { get; }
		public IRiverLakeGenerator RiverLakeGenerator { get; }
		public IWindGenerator WindGenerator { get; }
		public ITemperatureHumidityGenerator TemperatureHumidityGenerator { get; }
		public IBiomeGenerator BiomeGenerator { get; }
		public IMapSmoothenerGenerator SmoothenGenerator { get; }
		public IResourceGenerator ResourceGenerator { get; }
		public INatureGenerator NatureGenerator { get; }
		public IArtifactGenerator ArtifactGenerator { get; }



		/*
			Make a MapGenerator from a GeneratorBuilder. 
		*/
		public MapGenerator(GeneratorBuilder builder) {
			// Assign the generators.
			AltitudeGenerator = builder.AltitudeGenerator;
			AltitudeSmoothener = builder.AltitudeSmoothenerGenerator;
			OceanMountianGenerator = builder.OceanMountianGenerator;
			RiverLakeGenerator = builder.RiverLakeGenerator;
			WindGenerator = builder.WindGenerator;
			TemperatureHumidityGenerator = builder.TemperatureHumidityGenerator;
			BiomeGenerator = builder.BiomeGenerator;
			SmoothenGenerator = builder.MapSmoothenerGenerator;
			ResourceGenerator = builder.ResourceGenerator;
			NatureGenerator = builder.NatureGenerator;
			ArtifactGenerator = builder.ArtifactGenerator;

			// Create grid and state. If we have a basis map, use that.
			var basis = builder.BasisMap;
			if(basis != null) {
				// Create new state.
				State = GeneratorState.Create(builder.Parameters, basis.State);
				// Pass over the grid.
				Grid = basis.Grid;
			} else {
				// Create new state.
				State = GeneratorState.Create(builder.Parameters);
				// Create a new, empty grid.
				Grid = new Grid(State.Width, State.Height, State.GetDefaultBiome(BiomeType.Empty));
			}


			// Seed the algorithms.
			var rnd = new Random(State.Seed);
			AltitudeGenerator?.SetSeed(rnd.Next(0, int.MaxValue));
			AltitudeSmoothener?.SetSeed(rnd.Next(0, int.MaxValue));
			OceanMountianGenerator?.SetSeed(rnd.Next(0, int.MaxValue));
			RiverLakeGenerator?.SetSeed(rnd.Next(0, int.MaxValue));
			WindGenerator?.SetSeed(rnd.Next(0, int.MaxValue));
			TemperatureHumidityGenerator?.SetSeed(rnd.Next(0, int.MaxValue));
			BiomeGenerator?.SetSeed(rnd.Next(0, int.MaxValue));
			SmoothenGenerator?.SetSeed(rnd.Next(0, int.MaxValue));
			ResourceGenerator?.SetSeed(rnd.Next(0, int.MaxValue));
			NatureGenerator?.SetSeed(rnd.Next(0, int.MaxValue));
			ArtifactGenerator?.SetSeed(rnd.Next(0, int.MaxValue));

		}


		/*
			Handles using a generator on the map.
		*/
		private bool _useGenerator(IStepGenerator generator) {
			try {
				if(generator != null) Grid = generator.Use(Grid, State);
			} catch(Exception e) {
				Logger.Log(e);
				return false;
			}
			return true;
		}


		/*
			Generates altitude on the map. 
		*/
		public MapGenerator GenerateAltititude() {
			_useGenerator(AltitudeGenerator);
			return this;
		}


		/*
			Smoothens altitude on the map. 
		*/
		public MapGenerator SmoothenAltititude() {
			_useGenerator(AltitudeSmoothener);
			return this;
		}


		/*
			Generates oceans and mountains on the map. 
		*/
		public MapGenerator GenerateOceanMountianGenerator() {
			_useGenerator(OceanMountianGenerator);
			return this;
		}


		/*
			Generates rivers and lakes on the map. 
		*/
		public MapGenerator GenerateRiverLake() {
			_useGenerator(RiverLakeGenerator);
			return this;
		}
		

		/*
			Generates wind on the map. 
		*/
		public MapGenerator GenerateWind() {
			_useGenerator(WindGenerator);
			return this;
		}

		/*
			Generates temperature and humidity on the map. 
		*/
		public MapGenerator GenerateTemperatureHumidity() {
			_useGenerator(TemperatureHumidityGenerator);
			return this;
		}


		/*
			Generates biome on the map. 
		*/
		public MapGenerator GenerateBiomes() {
			_useGenerator(BiomeGenerator);
			return this;
		}


		/*
			Smoothens the map. 
		*/
		public MapGenerator GenerateSmootheness() {
			_useGenerator(SmoothenGenerator);
			return this;
		}

		/*
			Generates resources on the map. 
		*/
		public MapGenerator GenerateResources() {
			_useGenerator(ResourceGenerator);
			return this;
		}

		/*
			Generates nature on the map. 
		*/
		public MapGenerator GenerateNature() {
			_useGenerator(NatureGenerator);
			return this;
		}

		/*
			Generates artifacts on the map. 
		*/
		public MapGenerator GenerateArtifacts() {
			_useGenerator(ArtifactGenerator);
			return this;
		}


		/*
			Generates the map in the standard pipeline. 
		*/
		public GameMap Generate() {
			GenerateAltititude();
			SmoothenAltititude();
			GenerateOceanMountianGenerator();
			GenerateRiverLake();
			GenerateWind();
			GenerateTemperatureHumidity();
			GenerateBiomes();
			GenerateSmootheness();
			GenerateResources();
			GenerateNature();
			GenerateArtifacts();

			return GetMap();
		}

		/*
			Get the map generated by this generator.
		*/
		public GameMap GetMap() {
			if(!MapIsValid()) throw new InvalidOperationException("The map generated so far is not currently valid for return.");
			return GameMap.Create(Grid, State);
		}


		/*
			Check if the current result is a valid map.
		*/
		public bool MapIsValid() {
			// Check that all biomes are in the dict.
			var biomes = State.Biomes;
			for(int i = 0; i < Grid.Length; ++i) {
				var t = Grid[i];
				var b = t.Biome;
				if(b != biomes[b.Id]) {
					return false;
				}
			}

			return true;
		}


		IGeneratorState IMapGenerator.State => State;
		IGrid IMapGenerator.Grid => Grid;
	}
}