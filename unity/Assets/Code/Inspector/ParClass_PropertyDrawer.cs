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

	[CustomPropertyDrawer(typeof(RuntimeGenerator.ParClass))]
	public class ParClass_PropertyDrawer : PropertyDrawer {

		public override VisualElement CreatePropertyGUI(SerializedProperty property) {
			//Debug.Log("[ParClass_PropertyDrawer.CreatePropertyGUI]");
			// Create a new VisualElement to be the root the property UI
			var container = new VisualElement();
			
		
		
			// Create drawer UI using C#
			
			//var popup = new UnityEngine.UIElements.PopupWindow();
			//popup.text = "Tire Details";
			//popup.Add(new Label("This is a custom inspector"));
			container.Add(new PropertyField(property.FindPropertyRelative("Name"), "the name works"));
			container.Add(new PropertyField(property.FindPropertyRelative("Width"), "width yo"));
			container.Add(new PropertyField(property.FindPropertyRelative("Height"), "height n stuff"));
			container.Add(new PropertyField(property.FindPropertyRelative("Seed"), "seeeed"));
		
			//popup.Add(new PropertyField(property.FindPropertyRelative("m_AirPressure"), "Air Pressure (psi)"));
			//popup.Add(new PropertyField(property.FindPropertyRelative("m_ProfileDepth"), "Profile Depth (mm)"));
			//container.Add(popup);
			
			// Return the finished UI
			return container;
		}
		//base.OnGUI(position, property, label);

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			//Debug.Log("[ParClass_PropertyDrawer.OnGUI]");

			EditorGUI.BeginProperty(position, label, property);

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			var amountRect = new Rect(position.x, position.y, position.width, position.height);

			EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("Name"), new GUIContent("the name works"));

			EditorGUI.EndProperty();

		}

}
}
