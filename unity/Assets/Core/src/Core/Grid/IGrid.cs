using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace terrainGenerator {

	/*
		Grid containing all tiles in a map
	*/
	public interface IGrid : IReadOnlyGrid {

		/*
			Get tile at x,y in the grid
		*/
		Tile this[int x, int y] { get; set; }
		Tile this[(int, int) pos] { set; }
		

		/*
			Get the i-th tile in the grid
		*/
		Tile this[int i] { set; }
	}

}
