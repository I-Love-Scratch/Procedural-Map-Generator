using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using terrainGenerator;
using terrainGenerator.Generator;
using System;

namespace TerrainUnity {

	using Object = UnityEngine.Object;

	/*
		Generated maps on demand and displays them.
	*/
	public class RuntimeGenerator : MonoBehaviour {

		// The unity grid object to display the resulting map on.
		public GameObject gridObj;

		// The grid script for gridObj.
		private Grid grid;

		[Serializable]
		public class ParClass {
			public string Name = $"RuntimeMap_0";
			public int Width = 513;
			public int Height = 513;
			public int Seed = 9999;
		}

		//public int testNum = 5;

		//public ParClass parClass = new ParClass();

		// The generator builder used to create the maps.
		[SerializeReference]
		public GeneratorBuilder generatorBuilder = new GeneratorBuilder(new GenParams {
			Name = "Test",
			Width = 513,
			Height = 513,
			Seed = 9999,
		}) {
			AltitudeGenerator = new PDSAltitudeGenerator(2, 9),
			/*AltitudeSmoothener = new AltitudeAverageSmoothener(new List<(int, int, bool)> {
				(2, 5, true),
				(3, 2, true),
				(3, 1, false),
			}),*/
			RiverLakeGenerator = new RiverLakeGenerator(6, 6, 16),
			OceanMountianGenerator = new BalancedOceanMountianGenerator(0.2f, 0.05f, 50),
			//WindGenerator = new WindGenerator(),
			TemperatureHumidityGenerator = new EnvironmentalTemperatureHumidityGenerator(160, 6, dispersionSteps: 2, dispersionRange: 4, dispersionProp: 0.15f),
			BiomeGenerator = new FactorBasedBiomeGenerator(GenerationUtils.BIOME_MATRIX_SIMPLIFIED_2),
			MapSmoothenerGenerator = new CellularAutomataSmoothener(3, 4),
		};

		// Start is called before the first frame update
		void Start() {
			grid = gridObj.GetComponent<Grid>();
		}

		/*
			Generate a new map using the generator and display it.
		*/
		public void generate() {
			Debug.Log("Button pressed. Generating...");

			//GenParams par = new GenParams {
			//	Name = parClass.Name,
			//	Width = parClass.Width,
			//	Height = parClass.Height,
			//	Seed = parClass.Seed,
			//};


			var gen = generatorBuilder.Build();

			var map = gen.Generate();


			grid.SetMap(map);

			Debug.Log("Finished generating");
		}

		// Update is called once per frame
		void Update() {

		}
	}
}
