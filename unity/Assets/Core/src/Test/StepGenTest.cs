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

	/*
		An object used to test a step generator,
	*/
	public class StepGenTest {

		// The step generator to test on.
		public IStepGenerator Generator { get; set; }

		// The name to display in the output.
		public string Name { get; set; }

		// A note to display on the output if defined.
		public string Notes { get; set; }

		// A list for the results on different tests..
		public List<TimeSpan> Results { get; set; }

		public StepGenTest(IStepGenerator generator, string name = null, string notes = null) {
			Generator = generator;
			Name = name;
			Notes = notes;
		}
	}
}
