using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace terrainGenerator {
	/*
	* Based on: https://en.wikipedia.org/wiki/Perlin_noise
	*/
	static class Perlin {

		/*
			Picks a point between 2 values based on where the weight is.
		*/
		public static float interpolate(float a0, float a1, float w) {
			return (a1 - a0) * w + a0;
		}

		/*
			Creates a pseudo-random value.
		*/
		static (float x, float y) randomGradient(int ix, int iy) {
			const uint w = 8 * sizeof(uint);
			const uint s = w / 2; // rotation width
			uint a = (uint)ix, b = (uint)iy;
			a *= 3284157443; b ^= a << (int)s | a >> (int)(w-s);
			b *= 1911520717; a ^= b << (int)s | b >> (int)(w -s);
			a *= 2048419325;
			double random = a * (Math.PI / ~(~0u >> 1)); // in [0, 2*Pi]
			
			return ( (float) Math.Cos(random), (float) Math.Sin(random) );
		}

		/*
			Creates a pseudo-random value.
		*/
		public static float dotGridGradient(int ix, int iy, float x, float y) {
			// Get gradient from integer coordinates
			(float gx, float gy) = randomGradient(ix, iy);

			// Compute the distance vector
			float dx = x - ix;
			float dy = y - iy;

			// Compute the dot-product
			return (dx * gx + dy * gy);
		}

		/*
			Gets the noise value for a position in 2D space.
			Value is in range [-1, 1]
		*/
		public static float GetNoise(float x, float y) {
			// Determine grid cell coordinates
			int x0 = (int)Math.Floor(x);
			int x1 = x0 + 1;
			int y0 = (int)Math.Floor(y);
			int y1 = y0 + 1;

			// Determine interpolation weights
			// Could also use higher order polynomial/s-curve here
			float sx = x - (float)x0;
			float sy = y - (float)y0;

			// Interpolate between grid point gradients
			float n0, n1, ix0, ix1, value;

			n0 = dotGridGradient(x0, y0, x, y);
			n1 = dotGridGradient(x1, y0, x, y);
			ix0 = interpolate(n0, n1, sx);

			n0 = dotGridGradient(x0, y1, x, y);
			n1 = dotGridGradient(x1, y1, x, y);
			ix1 = interpolate(n0, n1, sx);

			value = interpolate(ix0, ix1, sy);
			return value; // Will return in range -1 to 1. To make it in range 0 to 1, multiply by 0.5 and add 0.5
		}
	}
}
