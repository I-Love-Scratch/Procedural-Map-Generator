using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace terrainGenerator.Generator {
	/*
		Interface for generating a step in the process.
	*/
	public interface IStepGenerator { 
		// The seed this step generator should use for randomization.
		int Seed { get; }

		// An id for the step generator.
		int Id { get; set; }

		// Indicates whether or not this step generator should scale its parameters with the map.
		bool ShouldUseMapScaleFactor { get; set; }

		// Map aspects needed for this step generator to work.
		MapAspects RequiredMapAspects { get; }

		// Map aspects this generator applies.
		MapAspects AddedMapAspects { get; }

		/*
			Set the seed of the step generator.
		*/
		void SetSeed(int seed);

		/*
			Generate on the provided map.
		*/
		Grid Generate(Grid grid, IGeneratorState state);

		/*
			Performs some checks before forwarding to Generate.
		*/
		Grid Use(Grid grid, IGeneratorState state);

	}


	/*
		Abstract base class for StepGenerators.
	*/
	public abstract class StepGeneratorBase : IStepGenerator {

		// An id for the step generator.
		public int Id { get; set; }

		// The seed this step generator should use for randomization.
		public int Seed { get; private set; }


		// Indicates whether or not this step generator should scale its parameters with the map.
		public bool ShouldUseMapScaleFactor { get; set; } = true;

		// Map aspects needed for this step generator to work.
		public abstract MapAspects RequiredMapAspects { get; }
		// Map aspects this generator applies.
		public abstract MapAspects AddedMapAspects { get; }


		/*
			Generate on the provided map.
		*/
		public abstract Grid Generate(Grid grid, IGeneratorState state);


		/*
			Set the seed of the step generator.
		*/
		public void SetSeed(int seed) => Seed = seed;


		/*
			Performs some checks before forwarding to Generate.
		*/
		public Grid Use(Grid grid, IGeneratorState state) {
			// If any required aspects are missing, throw exception.
			var missing = ~state.MapAspects & RequiredMapAspects;
			if(missing != MapAspects.None) 
				throw new ArgumentException(nameof(state), $"State is missing the following required map aspects: [{missing}]");

			var res = Generate(grid, state);

			if(res == null)
				throw new Exception($"Something went wrong with the process.");

			state.ApplyAspects(AddedMapAspects);
			return res;
		}

	}


	/*
		Interface for generating altitudes.
	*/
	public interface IAltitudeGenerator : IStepGenerator { }

	/*
		Interface for smoothening altitudes.
	*/
	public interface IAltitudeSmoothenerGenerator : IStepGenerator { }

	/*
		Interface for generating ocean and mountian areas.
	*/
	public interface IOceanMountianGenerator : IStepGenerator { }

	/*
		Interface for generating rivers and lakes.
	*/
	public interface IRiverLakeGenerator : IStepGenerator { }

	/*
		Interface for generating wind on the map.
	*/
	public interface IWindGenerator : IStepGenerator { }

	/*
		Interface for generating temperature and humidity on the map.
	*/
	public interface ITemperatureHumidityGenerator : IStepGenerator { }


	/*
		Interface for generating biomes.
	*/
	public interface IBiomeGenerator : IStepGenerator { }


	/*
		Interface for smoothening biomes.
	*/
	public interface IMapSmoothenerGenerator : IStepGenerator { }



	/*
		Interface for generating resources.
		Ex.: Iron, stone, gold, fish, etc.
	*/
	public interface IResourceGenerator : IStepGenerator { }


	/*
		Interface for generating nature.
		Ex.: Trees, flowers, etc.
	*/
	public interface INatureGenerator : IStepGenerator { }


	/*
		Interface for generating artifacts.
		Ex.: Abandoned ruins, natural wonder.
	*/
	public interface IArtifactGenerator : IStepGenerator { }

}