using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using terrainGenerator;
using terrainGenerator.Generator;
using System;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TerrainUnity {

	using Object = UnityEngine.Object;

	[CustomPropertyDrawer(typeof(GeneratorBuilder))]
	public class GeneratorBuilder_PropertyDrawer : PropertyDrawer {


		private VisualElement root;
		private UnityEngine.UIElements.PopupWindow popup;
		private VisualElement child;

		private SerializedProperty _property;

		private SerializedProperty _propertyParameters;
		private SerializedProperty _propertyAltitudeGenerator;
		private SerializedProperty _propertyAltitudeSmoothenerGenerator;
		private SerializedProperty _propertyWaterMountianGenerator;
		private SerializedProperty _propertyRiverLakeGenerator;
		private SerializedProperty _propertyWindGenerator;
		private SerializedProperty _propertyTemperatureHumidityGenerator;
		private SerializedProperty _propertyBiomeGenerator;
		private SerializedProperty _propertyMapSmoothenerGenerator;
		private SerializedProperty _propertyNatureGenerator;
		private SerializedProperty _propertyResourceGenerator;
		private SerializedProperty _propertyArtifactGenerator;

		private VisualElement tab1;
		private VisualElement tab2;
		private bool showTab1 = true;

		public const string TAB_1_TITLE = "Parameters";
		public const string TAB_2_TITLE = "Generators";

		public override bool CanCacheInspectorGUI(SerializedProperty property) => false;

		public override VisualElement CreatePropertyGUI(SerializedProperty property) {
			Debug.Log("[GeneratorBuilder_PropertyDrawer.CreatePropertyGUI()]");

			// Create a new VisualElement to be the root the property UI
			var container = new VisualElement();
			root = container;
			popup = new UnityEngine.UIElements.PopupWindow();
			popup.text = "GeneratorBuilder";
			container.Add(popup);

			_property = property.Copy();
			_setProperties(_property.Copy());

			var button = new Button(() => {
				showTab1 = !showTab1;
				_createTabGui(property);
			});
			button.text = "Switch tab";
			popup.Add(button);
			
			tab2 = _createTab2Gui(_property.Copy());
			tab1 = _createTab1Gui(_property.Copy());

			//popup.Add(tab1);
			//popup.Add(tab2);

			_createTabGui(property);
			// Return the finished UI
			return container;
		}

		private void _setProperties(SerializedProperty property) {
			_propertyParameters = property.FindPropertyRelative("Parameters").Copy();
			_propertyAltitudeGenerator = property.FindPropertyRelative("AltitudeGenerator").Copy();
			_propertyAltitudeSmoothenerGenerator = property.FindPropertyRelative("AltitudeSmoothenerGenerator").Copy();
			_propertyWaterMountianGenerator = property.FindPropertyRelative("OceanMountianGenerator").Copy();
			_propertyRiverLakeGenerator = property.FindPropertyRelative("RiverLakeGenerator").Copy();
			_propertyWindGenerator = property.FindPropertyRelative("WindGenerator").Copy();
			_propertyTemperatureHumidityGenerator = property.FindPropertyRelative("TemperatureHumidityGenerator").Copy();
			_propertyBiomeGenerator = property.FindPropertyRelative("BiomeGenerator").Copy();
			_propertyMapSmoothenerGenerator = property.FindPropertyRelative("MapSmoothenerGenerator").Copy();
			_propertyNatureGenerator = property.FindPropertyRelative("NatureGenerator").Copy();
			_propertyResourceGenerator = property.FindPropertyRelative("ResourceGenerator").Copy();
			_propertyArtifactGenerator = property.FindPropertyRelative("ArtifactGenerator").Copy();
		}

		private VisualElement _createTabGui(SerializedProperty property) {
			try {
				if(child != null) popup.Remove(child);
			} catch(Exception e) {
				Debug.Log(e);
			}
			child = (showTab1) ? tab1 : tab2;
			popup.Add(child);
			return root;
		}



		private VisualElement _createTab1Gui(SerializedProperty property) {
			var container = new VisualElement();
			container.Add(new Label(TAB_1_TITLE));

			container.Add(new PropertyField(property.FindPropertyRelative("Parameters")));

			var endOfChildrenIteration = property.GetEndProperty();
			property.NextVisible(true);
			while(property.NextVisible(false) && !SerializedProperty.EqualContents(property, endOfChildrenIteration)) {
				if(property.propertyType != SerializedPropertyType.ManagedReference) continue;

				var type = Assembly.GetAssembly(typeof(IStepGenerator))
					.GetType(property.managedReferenceFieldTypename.Split(" ").Last());

				if(type != null && typeof(IStepGenerator).IsAssignableFrom(type)) {
					container.Add(new PropertyField(property));
				}
			}
			return container;
		}
		
		private VisualElement _createTab2Gui(SerializedProperty property) {
			var container = new VisualElement();

			container.Add(new Label(TAB_2_TITLE));

			var endOfChildrenIteration = property.GetEndProperty();
			property.NextVisible(true);
			while(property.NextVisible(false) && !SerializedProperty.EqualContents(property, endOfChildrenIteration)) {
				if(property.propertyType != SerializedPropertyType.ManagedReference) continue;

				var type = Assembly.GetAssembly(typeof(IStepGenerator))
					.GetType(property.managedReferenceFieldTypename.Split(" ").Last());

				if(type != null && typeof(IStepGenerator).IsAssignableFrom(type)) {
					container.Add(_addDropdownFor(property.Copy(), type));
				}
			}

			return container;
		}


		private DropdownField _addDropdownFor<T>(SerializedProperty prop, string name = null) where T : class, IStepGenerator {
			name ??= prop?.displayName;


			// Create dictionary of the types.
			var generators = GenerationUtils.GetImplementationsInstanceDict<T>();

			var names = new List<String>(generators.Keys);

			var cur = names.IndexOf(prop?.managedReferenceValue?.GetType()?.Name ?? "none");

			var dropdown = new DropdownField(name, names, cur);
			//dropdown.RegisterValueChangedCallback(evt => Debug.Log(evt.newValue));
			dropdown.RegisterValueChangedCallback(evt => {
				prop.managedReferenceValue = (T) generators[evt.newValue];
				Debug.Log(prop.serializedObject.ApplyModifiedProperties());
				prop.serializedObject.Update();
			});
			return dropdown;
		}
		private DropdownField _addDropdownFor(SerializedProperty prop, Type type, string name = null) {
			name ??= prop?.displayName;

			// Create dictionary of the types.
			var generators = GenerationUtils.GetImplementationsInstanceDict(type);

			var names = new List<String>(generators.Keys);

			var cur = names.IndexOf(prop?.managedReferenceValue?.GetType()?.Name?.ToLower() ?? "none");

			var dropdown = new DropdownField(name, names, cur);
			//dropdown.RegisterValueChangedCallback(evt => Debug.Log(evt.newValue));
			dropdown.RegisterValueChangedCallback(evt => {
				prop.managedReferenceValue = generators[evt.newValue];
				Debug.Log(prop.serializedObject.ApplyModifiedProperties());
				//prop.serializedObject.Update();
			});
			return dropdown;
		}
	}
}