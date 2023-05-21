using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using terrainGenerator;

namespace TerrainUnity {

	/*
		A script used to run test functions in code from unity.
	*/
	public class TestComponent : MonoBehaviour {

		/*
			Run the test code.
		*/
		void Start() {
			Debug.Log($"[TestComponent.Start()] Start");
			Program.Main(new string[0]);
			Debug.Log($"[TestComponent.Start()] End");
		}

		void Update() {

		}


	}
}
