using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.IO;

using terrainGenerator.Generator;
using UnityEngine;

namespace terrainGenerator.Test {
	using Random = System.Random;

	/*
		Class for running tests on the generators.
	*/
	public class GenTest {

		// The time format to use on the output.
		public string DurationFormat { get; set; } = @"hh\:mm\:ss\.ffff";

		// The time this test was started.
		public DateTime Started { get; private set; }

		// The time this test used.
		public TimeSpan TotalDuration { get; private set; }

		// The width of the durations.
		public int DurationWidth => DurationFormat.Replace("\\", "").Length;

		/*
			Run a set of generators on a set of parameters to test.
			Callback allows you to control what to do after each test.
		*/
		public void Run(
			List<GenParams> pars, 
			List<Func<GenParams, IMapGenerator>> generators, 
			Action<GenParams, IMapGenerator, GameMap> callback
		) {
			Started = DateTime.Now;
			foreach(var par in pars) {
				foreach(var genMaker in generators) {
					var generator = genMaker(par);
					var map = generator.Generate();
					callback(par, generator, map);
				}
			}
			TotalDuration = DateTime.Now - Started;
		}


		/*
			Benchmarks a bunch of step generators and returns their average performance.
			Tests every step generator on every size of map n times and returns a print of the results.
		*/
		public string Benchmark(
			List<StepGenTest> steps, List<int> sizes,
			GenParams par, int seed, int n, 
			bool toCSV = false
		) {
			var rnd = new Random(seed);
			// Set starting time.
			Started = DateTime.Now;

			// Prepare the steps.
			foreach(var step in steps) {
				step.Results = new List<TimeSpan>();
				step.Name ??= step.Generator.GetType().Name;
			}

			// For each size we want to test on.
			for(int i = 0; i < sizes.Count; ++i) {
				// Set the param size.
				var size = sizes[i];
				par.Width = par.Height = size;

				// Add a result for this test on each step.
				foreach(var step in steps) {
					step.Results.Add(TimeSpan.Zero);
				}

				// Run the tests.
				for(int j = 0; j < n; ++j) {
					// New seed.
					par.Seed = rnd.Next();

					// Create a fresh chain generator.
					var gen = new ChainMapGenerator(steps.Select(t => t.Generator).ToList(), par);

					// Run through the steps.
					while(!gen.Finished) {
						var start = DateTime.Now;
						var index = gen.NextIndex;
						gen.Next();

						// Add the time it took.
						steps[index].Results[i] += DateTime.Now - start;
					}
				}

				// Average the time each step took.
				foreach(var step in steps) {
					step.Results[i] /= n;
				}
			}

			// Set ending time.
			TotalDuration = DateTime.Now - Started;

			return toCSV ? ToCSV(steps, sizes, n) : ToTable(steps, sizes, n);
		}


		/*
			Prints the results as a table.
		*/
		private string ToTable(List<StepGenTest> steps, List<int> sizes, int n) {
			var sb = new StringBuilder();

			// Width of the name column. (Width of longest name).
			int nameColWidth = steps.Select(t => t.Name.Length).Max();
			// Width of data columns.
			int dataColWidth = DurationWidth;// 13;
			// Width of the table.
			int tableWidth = nameColWidth + 8 + sizes.Count * (dataColWidth + 3);

			/// 0) Write the metadata.
			sb
				.AppendLine($"Date      : {Started}")
				.AppendLine($"TotalTime : {TotalDuration}")
				.AppendLine($"N         : {n}")
				.AppendLine("".PadRight(tableWidth, '='));


			/// 1) Write the header.
			sb.Append($"{"Sizes".PadRight(nameColWidth)} ");
			foreach(var size in sizes) 
				sb.Append($"| {size.ToString().PadLeft(dataColWidth)} ");
			sb.Append("| Notes");


			/// 2) Write the divider.
			sb.Append($"\n{"".PadRight(tableWidth, '-')}");


			/// 3) Write out the results for each step.
			foreach(var step in steps) {
				sb.Append($"\n{step.Name.PadRight(nameColWidth)} ");

				// For each size.
				for(int i = 0; i < sizes.Count; ++i) {
					var time = step.Results[i];
					sb.Append($"| {time.ToString(DurationFormat).PadLeft(dataColWidth)} ");
				}

				// Print notes.
				sb.Append($"| {step.Notes}");
			}

			/// 4) Write the total for each size.
			sb.Append($"\n{"".PadRight(tableWidth, '-')}");
			sb.Append($"\n{"Total".PadRight(nameColWidth)} ");

			for(int i = 0; i < sizes.Count; ++i) {
				// Sum of each step for this size.
				TimeSpan sum = TimeSpan.Zero;
				foreach(var step in steps) sum += step.Results[i];

				sb.Append($"| {sum.ToString(DurationFormat).PadLeft(dataColWidth)} ");
			}
			
			return sb.ToString();
		}


		/*
			Prints the results as a CSV.
		*/
		private string ToCSV(List<StepGenTest> steps, List<int> sizes, int n) {
			var sb = new StringBuilder();

			/// 1) Write the header.
			sb.Append("Sizes");
			foreach(var size in sizes) sb.Append($", {size}");

			/// 2) Write out the results for each step.
			foreach(var step in steps) {
				sb.Append($"\n{step.Name}");

				// For each size.
				for(int i = 0; i < sizes.Count; ++i) 
					sb.Append($", {step.Results[i].ToString(DurationFormat)}");
			}

			/// 3) Write out the results for each step.
			sb.Append("\nTotal");
			for(int i = 0; i < sizes.Count; ++i) {
				// Sum of each step for this size.
				TimeSpan sum = TimeSpan.Zero;
				foreach(var step in steps) sum += step.Results[i];

				sb.Append($", {sum.ToString(DurationFormat)}");
			}

			return sb.ToString();
		}


	}


}
