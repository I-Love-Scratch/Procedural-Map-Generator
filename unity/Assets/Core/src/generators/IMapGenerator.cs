using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace terrainGenerator.Generator {

	/*
		Interface for map generators.
	*/
	public interface IMapGenerator {

		/*
			The state of the map.
		*/
		IGeneratorState State { get; }

		/*
			The grid of the map.
		*/
		IGrid Grid { get; }

		/*
			Run all steps and return the resulting map.
		*/
		GameMap Generate();
		
		/*
			Get the map if it is ready.
		*/
		GameMap GetMap();


		/*
			Check if the current result is a valid map.
		*/
		bool MapIsValid();

	}

}