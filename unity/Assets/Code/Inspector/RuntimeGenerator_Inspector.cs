using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using terrainGenerator;
using terrainGenerator.Generator;
using System;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TerrainUnity {

	using Object = UnityEngine.Object;

	[CustomEditor(typeof(RuntimeGenerator))]
	public class RuntimeGenerator_Inspector : Editor {

		public override VisualElement CreateInspectorGUI() {
			// Create a new VisualElement to be the root of our inspector UI
			VisualElement myInspector = new VisualElement();

			// Add a simple label
			//myInspector.Add(new Label("This is a custom inspector"));

			VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/RuntuneGenerator_Inspector.uxml");
			visualTree.CloneTree(myInspector);

			// Return the finished inspector UI
			return myInspector;
		}

	}
}
