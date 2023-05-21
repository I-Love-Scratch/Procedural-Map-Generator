using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace terrainGenerator { 

	/*
		Counts the distribution of values.
	*/
	public class DistributionCounter {
		
		public int Steps { get; } // Number of steps we divide the values into.
		public float Min { get; } // The smallest value allowed in the range.
		public float Max { get; } // The largest value in the range.


		private int[] counts; // The list counting instances in each step.

		public DistributionCounter(int steps, float min = 0f, float max = 1f) {
			if (steps < 2) throw new ArgumentException(nameof(steps), "Cannot be less than 2");
			if (max <= min) throw new ArgumentException(nameof(max), "Maximum must be greater than minimum.");
			
			Steps = steps;
			Min = min;
			Max = max;
			counts = new int[steps];
		}

		/*
			Increases the count for the step this value belongs to. 
		*/
		public void Add(float val, int count = 1) {
			if (count < 1) throw new ArgumentException(nameof(count), "You cannot add fewer than 1.");

			// Find the step this value belongs to, then increment it.
			int index = (int) Utils.Clamp(Utils.Lerp(val, Min, Max) * Steps, 0, Steps - 1);
			counts[index] += count;
		}

		/*
			Gets the count for a given index. 
		*/
		public int this[int i] => counts[i];

		/*
			Gets the sum of values we've counted. 
		*/
		public int Sum => counts.Sum();


		/*
			Gets the distribution of values into these steps in the range. 
		*/
		public float[] GetDistribution() {
			float sum = Sum;
			if (sum == 0) return new float[Steps];
			return counts.Select(v => v / sum).ToArray();
		}

		/*
			Gets the lowest value of the step. 
		*/
		public float GetStepStart(int i)
			=> Utils.Scale((float)i / Steps, Min, Max);


	}
}
