using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator {
	/*
		A utility classs with helpful functions
	*/
	public static class Utils {


		/*
			Converts a raw integer to Color32.
		*/
		public static Color32 ToColor32(this int raw) {
			Color32 c = new Color32();
			c.b = (byte) (raw & 255);
			raw >>= 8;
			c.g = (byte) (raw & 255);
			raw >>= 8;
			c.r = (byte) (raw & 255);
			raw >>= 8;
			c.a = (byte) (raw & 255);
			return c;
		}
		public static Color32 ToColor32(this uint raw) {
			Color32 c = new Color32();
			c.b = (byte) (raw & 255);
			raw >>= 8;
			c.g = (byte) (raw & 255);
			raw >>= 8;
			c.r = (byte) (raw & 255);
			raw >>= 8;
			c.a = (byte) (raw & 255);
			return c;
		}

		/*
			Converts a Color32 to its raw integer representation.
		*/
		public static int ToRawInt(this Color32 c) {
			var raw = 0;
			raw += c.a;
			raw <<= 8;
			raw += c.r;
			raw <<= 8;
			raw += c.g;
			raw <<= 8;
			raw += c.b;
			return raw;
		}
		/*
			Converts an unsigned int to the corresponding RGBA color.
		*/
		public static uint ToRawUInt(this Color32 c) {
			uint raw = 0;
			raw += c.a;
			raw <<= 8;
			raw += c.r;
			raw <<= 8;
			raw += c.g;
			raw <<= 8;
			raw += c.b;
			return raw;
		}



		/*
			Returns how far along the range between min and max val is [0.0, 1.0]
		*/
		public static float Lerp(this float val, float min, float max)
			=> ((val - min) / (max - min)).Clamp();

		/*
			@param val: how far along range between min and max [0.0, 1.0]
			@param min: start of the range
			@param max: end of the range
			@return: point at range between min and max [min, max]
			example: Scale(0.5f, 10, 20) => 15.0f
		*/
		public static float Scale(this float val, float min, float max)
			=> val * (max - min) + min;

		/*
			Clamps a value between the min and max values.
		*/
		public static float Clamp(this float val, float min = 0.0f, float max = 1.0f)
			=> Math.Min(Math.Max(val, min), max);

		/*
			Return an angle as a negative value if greater than PI (180 degrees)
			Ranges: [0, 2*PI] => [-PI, PI]
		*/
		public static float GetNegativeAngle(this float angle)
			=> (angle > Math.PI) ? angle - (2 * Mathf.PI) : angle;


		/*
			Applies smoothstep to a value.
			Both input and output are [0,1].
		*/
		public static float SmootStep(this float val) {
			var x = Clamp(val);
			return x * x * (3 - 2 * x);
		}


		/*
			Groups the value into (steps+1) steps.
			@param val   : A number [0, 1]
			@param steps : A number of steps to group the value into. 
			               Ex.: if set to 20, val will be divided 21 into steps of 5%
			@returns A number [0, 1] that is val rounded to the nearest step.
			Ex.: (0.37, 10) => 0.3
			Ex.: (0.37, 20) => 0.35
		*/
		public static float Steps(this float val, int steps)
			=> MathF.Round(val * steps) / steps;


		/*
			Gets the orthoganal distance between 2 points.
		*/
		public static float DistanceToSimple(this (int x, int y) f, (int x, int y) t)
			=> Math.Abs(f.x - t.x) + Math.Abs(f.y - t.y); 

		/*
			Gets the pythagoral distance between 2 points.
		*/
		public static float DistanceTo(this (int x, int y) f, (int x, int y) t)
			=> MathF.Sqrt(MathF.Pow(f.x - t.x, 2) + MathF.Pow(f.y - t.y, 2));

		/*
			Gets the pythagoral distance to a point from origo (0, 0).
		*/
		public static float Distance(this (int x, int y) d)
			=> MathF.Sqrt(d.x * d.x + d.y * d.y);


		/*
			Normalize the grid to range [0,1]
		*/
		public static float[,] Normalize(this float[,] grid, float min, float max) {
			int w = grid.GetLength(0), h = grid.GetLength(1);
			for(int y = 0; y < h; ++y) {
				for(int x = 0; x < w; ++x) {
					grid[x, y] = grid[x, y].Lerp(min, max);
				}
			}
			return grid;
		}
		/*
			Normalize the grid to range [0,1]
		*/
		public static float[,] Normalize(this float[,] grid) {
			int w = grid.GetLength(0), h = grid.GetLength(1);
			float min = float.MaxValue, max = float.MinValue;
			for(int y = 0; y < h; ++y) {
				for(int x = 0; x < w; ++x) {
					var v = grid[x, y];
					if(v < min) min = v;
					if(v > max) max = v;
				}
			}
			return Normalize(grid, min, max);
		}

		/*
			Applies a function to every value in a grid.
		*/
		public static T[,] Apply<T>(this T[,] grid, Func<T, T> f) {
			int w = grid.GetLength(0), h = grid.GetLength(1);
			for(int y = 0; y < h; ++y) {
				for(int x = 0; x < w; ++x) {
					grid[x, y] = f(grid[x, y]);
				}
			}
			return grid;
		}


		/*
			Gets a new matrix containing only the tiles in the range.
			@param grid : The grid to get the range within.
			@param from : Inclusive lower bound of the range.
			@param to   : Exclusive upper bound of the range.
			@returns    : A rectangular range within the original grid.
		*/
		public static float[,] GetRange(this float[,] grid, (int x, int y) from, (int x, int y) to) {
			int w = to.x - from.x, 
			    h = to.y - from.y;
			float[,] res = new float[w,h];

			for(int y = from.y; y < to.y; ++y) {
				for(int x = from.x; x < to.x; ++x) {
					res[x, y] = grid[x, y];
				}
			}
			return res;
		}


		/*
			Returns true if coordinates are out of bounds.
		*/
		public static bool OutOfBounds(int x, int y, int width, int height)
			=> x < 0 || x >= width || y < 0 || y >= height;

		/*
			Returns true if coordinates are at the border.
		*/
		public static bool AtBorder(int x, int y, int width, int height)
			=> x == 0 || x == width - 1 || y == 0 || y == height - 1;

		/*
			Gets all tiles in a circle around a point.
		*/
		public static IEnumerable<(int, int)> CircleAround(this (int x, int y) c, float range, int width = int.MaxValue, int height = int.MaxValue, bool returnOrigo = false) {
			foreach((int x, int y) o in CircleOffset(range, returnOrigo)) {
				var (x, y) = (c.x + o.x, c.y + o.y);
				if(0 <= x && x < width && 0 <= y && y < height)
					yield return (x, y);
			}
		}
		
		/*
			Returns all offsets in a circle of the given range.
		*/
		public static IEnumerable<(int, int)> CircleOffset(float range, bool returnOrigo = false) {
			

			//Return origo?
			if(returnOrigo) yield return (0, 0);

			/*
			Explanation:

			For each x, we know that (x,0) is within range, because:
			  Dist(x, 0) = sqrt(x^2 + 0^2)
			             = sqrt(x^2)
						 = x
			Since x is at most range, it will always be in range.
			*/

			for(int x = 1; x <= range; ++x) {
				bool inRange = true; // Indicates that we are still in range.

				// (x, 0) is always in range.
				//Return the (x,0) set
				yield return ( x,  0);
				yield return (-x,  0);
				yield return ( 0,  x);
				yield return ( 0, -x);

				for(int y = 1; y <= x - 1; ++y) {
					// Stop if we're further out than the range.
					if((x, y).Distance() > range) {
						inRange = false;
						break;
					}
					//Return the (x,y) set
					yield return ( x,  y);
					yield return ( x, -y);
					yield return (-x,  y);
					yield return (-x, -y);

					yield return ( y,  x);
					yield return ( y, -x);
					yield return (-y,  x);
					yield return (-y, -x);
				}

				if(inRange && (x, x).Distance() <= range) {
					//Return the (x,x) set
					yield return ( x,  x);
					yield return ( x, -x);
					yield return (-x,  x);
					yield return (-x, -x);
				}
			}

		}


		/*
			Returns point at t along given bezier curve.
		*/
		public static (float, float) BezierCurve(float t, float p0x, float p0y, float p1x, float p1y, float p2x, float p2y) {
			var lin1x = (1 - t) * p0x + t * p1x;
			var lin1y = (1 - t) * p0y + t * p1y;
			var lin2x = (1 - t) * p1x + t * p2x;
			var lin2y = (1 - t) * p1y + t * p2y;
			return ((1 - t)*lin1x + t*lin2x, (1 - t) * lin1y + t * lin2y);
		}

		/*
			Returns all points along given bezier curve.
		*/
		public static IEnumerable<(int, int)> BezierRange(int p0x, int p0y, int p1x, int p1y, int p2x, int p2y) {

			//distance between p0 and p1 + distance between p1 and p2
			var maxCurveLength = Pythagoras(p0x, p0y, p1x, p1y) + Pythagoras(p1x, p1y, p2x, p2y);
			var increment = 1 / (maxCurveLength * 2);
			(float, float) coords;

			//for each increment, return bezier curve results
			for (float t = 0.0f; t < 1.0f; t += increment) {
				coords = BezierCurve(t, p0x, p0y, p1x, p1y, p2x, p2y);
				yield return ((int)coords.Item1, (int)coords.Item2);
			}
			//one last time for t = 1
			coords = BezierCurve(1.0f, p0x, p0y, p1x, p1y, p2x, p2y);
			yield return ((int)coords.Item1, (int)coords.Item2);

		}

		/*
			Returns point between points a and b.
		*/
		public static (int, int) Between((int, int) a, (int, int) b)
			=> BetweenDivide(a, b, 2);

		/*
			Returns point between points a and b, at specified div.
			ie. at div = 4, the returned point is 25% of the way from a to b
		*/
		public static (int, int) BetweenDivide((int x, int y) a, (int x, int y) b, int div) {
			var x = a.x + (b.x - a.x) / div;
			var y = a.y + (b.y - a.y) / div;
			return (x, y);
		}

		/*
			Returns point perpendicular to midpoint between points a and b,
			at specified distance ratio from midpoint, and specified direction.
			A distance ratio of 1 means distance from midpoint to returned point = distance from midpoint to a.
		*/
		public static (int, int) PerpendicularPoint((int x, int y) a, (int x, int y) b, float distRatio = 1, bool rightHand = true) {
			var directionMultiplier = rightHand ? 1 : -1;

			var (x, y) = Between(a, b); //midpoint position

			//perpendicular offset
			x += (int)(((b.y - a.y) / 2) * distRatio * directionMultiplier);
			y += (int)(((b.x - a.x) / 2) * distRatio * directionMultiplier * (-1));

			return (x, y);
		}

		/*
			Returns length of diagonal line between two points.
		*/
		public static float Pythagoras(float x1, float y1, float x2, float y2) {
			var x = MathF.Abs(x1 - x2);
			var y = MathF.Abs(y1 - y2);
			return MathF.Sqrt(x*x + y*y);
		}

		/*
			Return all coordinates in a diamond pattern around the origin, extension method.
		*/
		public static IEnumerable<(int, int)> DiamondAround(this (int x, int y) c, int range, (int x, int y) cap) {
			for(var y = Math.Max(0, c.y - range); y <= Math.Min(cap.y - 1, c.y + range); y++) {
				var w = range - Math.Abs(c.y - y);
				for(var x = Math.Max(0, c.x - w); x <= Math.Min(cap.x - 1, c.x + w); x++) {
					yield return (x, y);
				}
			}
		}

		/*
			Return all coordinates in a diamond pattern around the origin.
		*/
		public static IEnumerable<(int, int)> DiamondAround(int cx, int cy, int steps, int width = int.MaxValue, int height = int.MaxValue, bool returnOrigo = false)
			=> DiamondAroundRange(cx, cy, 1, steps, width, height, returnOrigo: returnOrigo);

		/*
			Return all coordinates in a diamond pattern around the origin, whose distance from the origin is between min and max.
		*/
		public static IEnumerable<(int, int)> DiamondAroundRange(int cx, int cy, int minRange, int maxRange, int width = int.MaxValue, int height = int.MaxValue, bool returnOrigo = false) {
			if (returnOrigo)
				yield return (cx, cy);

			/* Get top-right triangle, then use 4 rotations
			 * 
			 *	     0
			 *	   x 0 0
			 *	 x x x x x	
			 *	   x x x
			 *	     x
			*/
			for (var y = 1; y <= maxRange; y++) {
				for (var x = Math.Max(0, minRange - y); x <= maxRange - y; x++) {
					if (!OutOfBounds(cx + x, cy + y, width, height))
						yield return (cx + x, cy + y);
					if (!OutOfBounds(cx + y, cy - x, width, height))
						yield return (cx + y, cy - x);
					if (!OutOfBounds(cx - x, cy - y, width, height))
						yield return (cx - x, cy - y);
					if (!OutOfBounds(cx - y, cy + x, width, height))
						yield return (cx - y, cy + x);
				}
			}
		}

		/*
			Finds positions in a square shaped pattern around the center.
		*/
		public static IEnumerable<(int, int)> SqareAround(this (int x, int y) c, int range, (int x, int y) cap, bool includeSelf = false) {
			for(var y = Math.Max(0, c.y - range); y <= Math.Min(cap.y - 1, c.y + range); y++) {
				//var w = range - Math.Abs(c.y - y);
				for(var x = Math.Max(0, c.x - range); x <= Math.Min(cap.x - 1, c.x + range); x++) {
					if(c != (x, y) || includeSelf)
						yield return (x, y);
				}
			}
		}



		/*
			Returns all integers between from and to.
		*/
		public static IEnumerable<int> Range(int from, int to) {
			if (from > to) {
				for (var i = from; i >= to; i--) {
					yield return i;
				}
			} else {
				for (var i = from; i <= to; i++) {
					yield return i;
				}
			}
		}

		/*
			Returns all points in a line between f and t.
		*/
		public static IEnumerable<(int, int)> Range(int fx, int fy, int tx, int ty) {
			var xRange = Range(fx, tx).ToArray();
			var yRange = Range(fy, ty).ToArray();
			var xLen = Math.Abs(tx - fx);
			var yLen = Math.Abs(ty - fy);

			if (xLen > yLen) {
				for (var x = 0; x < xLen; x++) {
					int y = (int)(((float)yLen / (float)xLen) * x);
					yield return (xRange[x], yRange[y]);
				}
			} else if (xRange.Length < yRange.Length) {
				for (var y = 0; y < yLen; y++) {
					int x = (int)(((float)xLen / (float)yLen) * y);
					yield return (xRange[x], yRange[y]);
				}
			} else {
				for (var i = 0; i < xLen; i++) {
					yield return (xRange[i], yRange[i]);
				}
			}
		}

		/*
			Generates a Diamond Square noise map of the given size.
			Based on: https://gist.github.com/awilki01/83b65ad852a0ab30192af07cda3d7c0b
		*/
		public static float[,] GenerateDiamondSquareMap(int width, int height, float randomness, int seed) {
			
			// Get closest size that has at least enough space for the whole grid.
			var size = Math.Max(width, height) - 1;
			size = ( 1 << (int) Math.Ceiling(Math.Log(size, 2)) ) + 1;

			var rng = new System.Random(seed);
			var map = new float[size, size];

			//set corner values
			map[0, 0] = rng.Next(0, size);
			map[size - 1, 0] = rng.Next(0, size);
			map[0, size - 1] = rng.Next(0, size);
			map[size - 1, size - 1] = rng.Next(0, size);

			// Get a diamound square of the map.
			var res = DiamondSquareIterate(map, randomness, rng, 0);

			// If the dimensions don't match the DS map, extract the desired map
			if(width != height || width != size)
				res = res.GetRange((0, 0), (width, height));

			return res;
		}

		/*
			Generates a noise map using Diamond Square for the last levels, and Perlin for any levels above that.
		*/
		public static float[,] GeneratePerlinDiamondSquareMap(
			int width, int height, int dsDepth, 
			float randomness, int seed, 
			float scale = 150.0f, int octaves = 7, float lacunarity = 2.0f, float persistance = 0.6f

		) {
			var rng = new System.Random(seed);

			// Get closest size that has at least enough space for the whole grid.
			var size = Math.Max(width, height) - 1;
			var depth = (int) Math.Ceiling(Math.Log(size, 2));
			size = (1 << depth) + 1;

			// Get the depths to do in Perlin and DS.
			dsDepth = Math.Min(dsDepth, depth);
			int pDepth = depth - dsDepth;

			// Generate perlin map for basis values.
			var map = PerlinGenerator.GenerateNoiseMap(size, size, scale, rng.Next(0, int.MaxValue),
				octaves, lacunarity, persistance, new System.Numerics.Vector2(1, 1));

			map = map.Apply(v => v * size);

			// Get a diamound square of the map.
			var res = DiamondSquareIterate(map, randomness, rng, pDepth);

			// If the dimensions don't match the DS map, extract the desired map
			if(width != height || width != size)
				res = res.GetRange((0, 0), (width, height));

			return res;
		}


		/*
			Given a float matrix, iterate over it with DiamondSquare until we've filled in the blanks.
			startDepth is how many levels we've already done. Set it to 0 to DiamondSquare the whole grid.
			Based on: https://gist.github.com/awilki01/83b65ad852a0ab30192af07cda3d7c0b
		*/
		public static float[,] DiamondSquareIterate(float[,] map, float randomness, System.Random rng, int startDepth) {
			
			var width = map.GetLength(0);

			// If width != height, or width not 2^n-1
			var tmp = Math.Log(width - 1, 2);
			if(width != map.GetLength(1) || tmp != Math.Floor(tmp)) {
				throw new ArgumentException(nameof(width));
			}


			// Half tileWidth for each depth we've already covered.
			int tileWidth = width - 1 >> startDepth;

			while(tileWidth > 1) {
				int halfSide = tileWidth / 2;

				//square
				for(var y = halfSide; y < (width - 1); y += tileWidth) {
					for(var x = halfSide; x < (width - 1); x += tileWidth) {

						var A = map[x - halfSide, y - halfSide];
						var B = map[x - halfSide, y + halfSide];
						var C = map[x + halfSide, y - halfSide];
						var D = map[x + halfSide, y + halfSide];

						map[x, y] = ((A + B + C + D) / 4) + (float) ((rng.NextDouble() * 2 - 1) * halfSide * randomness);
					}
				}

				//diamond
				for(var y = 0; y < (width - 1) + 1; y += halfSide) {
					for(var x = y % tileWidth == 0 ? halfSide : 0; x < (width - 1) + 1; x += tileWidth) {
						float sum = 0;
						int num = 0;
						int x2, y2;

						x2 = x + halfSide; y2 = y;
						if(!OutOfBounds(x2, y2, width, width)) { sum += map[x2, y2]; num++; }
						x2 = x - halfSide; y2 = y;
						if(!OutOfBounds(x2, y2, width, width)) { sum += map[x2, y2]; num++; }
						x2 = x; y2 = y + halfSide;
						if(!OutOfBounds(x2, y2, width, width)) { sum += map[x2, y2]; num++; }
						x2 = x; y2 = y - halfSide;
						if(!OutOfBounds(x2, y2, width, width)) { sum += map[x2, y2]; num++; }

						map[x, y] = (sum / num) + (float) ((rng.NextDouble() * 2 - 1) * halfSide * randomness) / 2;
					}
				}
				tileWidth /= 2;
			}

			//normalize between 0 and 1
			return map.Normalize();
		}

	}
}
