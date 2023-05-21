using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using terrainGenerator.Generator;


namespace terrainGenerator {

	/*
		A utility class for generation tools
	*/
	public static partial class GenerationUtils {



		private static readonly Biome ______Desert = new Biome(BiomeType.Desert.GetObj());
		private static readonly Biome _____Savanna = new Biome(BiomeType.Savanna.GetObj());
		private static readonly Biome ______Jungle = new Biome(BiomeType.Jungle.GetObj());

		private static readonly Biome __Grasslands = new Biome(BiomeType.Grasslands.GetObj());
		private static readonly Biome ______Forest = new Biome(BiomeType.Forest.GetObj());
		private static readonly Biome _______Swamp = new Biome(BiomeType.Swamp.GetObj());

		private static readonly Biome ______Tundra = new Biome(BiomeType.Tundra.GetObj());
		private static readonly Biome ______Boreal = new Biome(BiomeType.Boreal.GetObj());
		private static readonly Biome ______Arctic = new Biome(BiomeType.Arctic.GetObj());


		/*
			A map of biomes indexed by the factors on the tile.
			Factors are: Altitude, Temprature, Humidity
		*/
		public static readonly BiomeMap BIOME_MATRIX = new BiomeMap(new Biome[5, 5, 5] {
			{
				{ ______Boreal, ______Boreal, ______Boreal, ______Arctic, ______Arctic },
				{ ______Tundra, ______Forest, ______Forest, ______Boreal, _______Swamp },
				{ ______Desert, ______Forest, ______Forest, _______Swamp, _______Swamp },
				{ ______Desert, ______Desert, ______Forest, _______Swamp, _______Swamp },
				{ ______Desert, ______Desert, ______Desert, ______Jungle, ______Jungle },
			},
			{
				{ ______Tundra, ______Boreal, ______Boreal, ______Arctic, ______Arctic },
				{ ______Tundra, ______Forest, ______Forest, ______Boreal, ______Boreal },
				{ _____Savanna, ______Forest, ______Forest, _______Swamp, _______Swamp },
				{ ______Desert, _____Savanna, ______Forest, _______Swamp, ______Jungle },
				{ ______Desert, ______Desert, _____Savanna, ______Jungle, ______Jungle },
			},
			{
				{ ______Tundra, ______Tundra, ______Tundra, ______Arctic, ______Arctic },
				{ ______Tundra, __Grasslands, ______Forest, ______Boreal, ______Boreal },
				{ _____Savanna, __Grasslands, ______Forest, _______Swamp, ______Jungle },
				{ _____Savanna, _____Savanna, ______Forest, ______Jungle, ______Jungle },
				{ ______Desert, ______Desert, _____Savanna, ______Jungle, ______Jungle },
			},
			{
				{ ______Tundra, ______Tundra, ______Tundra, ______Arctic, ______Arctic },
				{ ______Tundra, __Grasslands, ______Forest, ______Boreal, ______Boreal },
				{ __Grasslands, __Grasslands, __Grasslands, ______Forest, ______Forest },
				{ _____Savanna, __Grasslands, __Grasslands, ______Forest, ______Forest },
				{ _____Savanna, _____Savanna, _____Savanna, __Grasslands, ______Jungle },
			},
			{
				{ ______Tundra, ______Tundra, ______Arctic, ______Arctic, ______Arctic },
				{ ______Tundra, __Grasslands, ______Forest, ______Boreal, ______Arctic },
				{ __Grasslands, __Grasslands, __Grasslands, __Grasslands, __Grasslands },
				{ __Grasslands, __Grasslands, __Grasslands, __Grasslands, __Grasslands },
				{ _____Savanna, _____Savanna, _____Savanna, __Grasslands, __Grasslands },
			},
		});

		public static readonly BiomeMap BIOME_MATRIX_SIMPLIFIED = new BiomeMap(new Biome[5, 5, 5] {
			{
				{ ______Arctic, ______Arctic, ______Arctic, ______Arctic, ______Arctic },
				{ ______Forest, ______Forest, ______Forest, _______Swamp, _______Swamp },
				{ ______Desert, ______Forest, ______Forest, _______Swamp, _______Swamp },
				{ ______Desert, ______Desert, ______Desert, _______Swamp, _______Swamp },
				{ ______Desert, ______Desert, ______Desert, ______Jungle, ______Jungle },
			},
			{
				{ ______Arctic, ______Arctic, ______Arctic, ______Arctic, ______Arctic },
				{ ______Forest, ______Forest, ______Forest, ______Arctic, ______Arctic },
				{ ______Forest, ______Forest, ______Forest, _______Swamp, _______Swamp },
				{ ______Desert, _____Savanna, ______Forest, _______Swamp, ______Jungle },
				{ ______Desert, ______Desert, _____Savanna, ______Jungle, ______Jungle },
			},
			{
				{ ______Arctic, ______Arctic, ______Arctic, ______Arctic, ______Arctic },
				{ ______Arctic, __Grasslands, ______Forest, ______Arctic, ______Arctic },
				{ _____Savanna, __Grasslands, ______Forest, _______Swamp, ______Jungle },
				{ _____Savanna, _____Savanna, ______Forest, ______Jungle, ______Jungle },
				{ ______Desert, ______Desert, _____Savanna, ______Jungle, ______Jungle },
			},
			{
				{ ______Arctic, ______Arctic, ______Arctic, ______Arctic, ______Arctic },
				{ ______Arctic, __Grasslands, ______Forest, ______Arctic, ______Arctic },
				{ __Grasslands, __Grasslands, ______Forest, ______Forest, ______Jungle },
				{ ______Desert, __Grasslands, ______Forest, ______Jungle, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, _____Savanna, ______Jungle },
			},
			{
				{ ______Arctic, ______Arctic, ______Arctic, ______Arctic, ______Arctic },
				{ ______Arctic, ______Arctic, ______Arctic, ______Arctic, ______Arctic },
				{ __Grasslands, __Grasslands, __Grasslands, ______Forest, ______Forest },
				{ ______Desert, __Grasslands, __Grasslands, ______Forest, ______Forest },
				{ ______Desert, _____Savanna, _____Savanna, _____Savanna, ______Jungle },
			},
		});
		public static readonly BiomeMap BIOME_MATRIX_SIMPLIFIED_2 = new BiomeMap(new Biome[5, 5, 5] {
			{
				{ ______Arctic, ______Arctic, ______Arctic, ______Arctic, ______Arctic },
				{ __Grasslands, ______Forest, ______Forest, ______Forest, _______Swamp },
				{ __Grasslands, ______Forest, ______Forest, ______Forest, _______Swamp },
				{ ______Desert, ______Desert, ______Forest, ______Forest, ______Jungle },
				{ ______Desert, ______Desert, _____Savanna, _____Savanna, ______Jungle },
			},

			{
				{ ______Arctic, ______Arctic, ______Arctic, ______Arctic, ______Arctic },
				{ __Grasslands, ______Forest, ______Forest, ______Forest, _______Swamp },
				{ __Grasslands, ______Forest, ______Forest, ______Forest, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, __Grasslands, ______Jungle },
				{ ______Desert, ______Desert, _____Savanna, _____Savanna, __Grasslands },
			},
			{
				{ ______Arctic, ______Arctic, ______Arctic, ______Arctic, ______Arctic },
				{ __Grasslands, ______Forest, ______Forest, ______Forest, _______Swamp },
				{ __Grasslands, ______Forest, ______Forest, ______Forest, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, __Grasslands, ______Jungle },
				{ ______Desert, ______Desert, _____Savanna, _____Savanna, __Grasslands },
			},
			{
				{ ______Arctic, ______Arctic, ______Arctic, ______Arctic, ______Arctic },
				{ __Grasslands, ______Forest, ______Forest, ______Forest, _______Swamp },
				{ __Grasslands, ______Forest, ______Forest, ______Forest, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, __Grasslands, ______Jungle },
				{ ______Desert, ______Desert, _____Savanna, _____Savanna, __Grasslands },
			},

			{
				{ ______Arctic, ______Arctic, ______Arctic, ______Arctic, ______Arctic },
				{ ______Arctic, ______Forest, ______Forest, ______Forest, ______Forest },
				{ _____Savanna, __Grasslands, ______Forest, ______Forest, ______Forest },
				{ _____Savanna, __Grasslands, __Grasslands, __Grasslands, __Grasslands },
				{ ______Desert, _____Savanna, _____Savanna, __Grasslands, __Grasslands },
			},
		});


		public static readonly BiomeMap BIOME_MATRIX_2 = new BiomeMap(new Biome[5, 5, 5] {
			{
				{ __Grasslands, _______Swamp, _______Swamp, _______Swamp, _______Swamp },
				{ ______Desert, _______Swamp, _______Swamp, _______Swamp, _______Swamp },
				{ ______Desert, ______Desert, _______Swamp, _______Swamp, _______Swamp },
				{ ______Desert, ______Desert, ______Desert, ______Desert, _______Swamp },
				{ ______Desert, ______Desert, ______Desert, ______Desert, _____Savanna },
			},
			{
				{ __Grasslands, __Grasslands, _______Swamp, _______Swamp, _______Swamp },
				{ __Grasslands, __Grasslands, _______Swamp, _______Swamp, _______Swamp },
				{ ______Desert, ______Desert, ______Desert, _______Swamp, _______Swamp },
				{ ______Desert, ______Desert, ______Desert, _____Savanna, _____Savanna },
				{ ______Desert, ______Desert, ______Desert, _____Savanna, _____Savanna },
			},
			{
				{ __Grasslands, __Grasslands, __Grasslands, _______Swamp, _______Swamp },
				{ __Grasslands, __Grasslands, __Grasslands, _______Swamp, _______Swamp },
				{ ______Desert, ______Desert, __Grasslands, _______Swamp, _______Swamp },
				{ ______Desert, ______Desert, _____Savanna, _____Savanna, _____Savanna },
				{ ______Desert, ______Desert, _____Savanna, _____Savanna, _____Savanna },
			},
			{
				{ __Grasslands, __Grasslands, __Grasslands, _______Swamp, _______Swamp },
				{ __Grasslands, __Grasslands, __Grasslands, _______Swamp, _______Swamp },
				{ __Grasslands, __Grasslands, __Grasslands, _____Savanna, _____Savanna },
				{ ______Desert, ______Desert, _____Savanna, _____Savanna, _____Savanna },
				{ ______Desert, ______Desert, _____Savanna, _____Savanna, _____Savanna },
			},
			{
				{ __Grasslands, __Grasslands, __Grasslands, __Grasslands, _______Swamp },
				{ __Grasslands, __Grasslands, __Grasslands, _____Savanna, _____Savanna },
				{ __Grasslands, __Grasslands, _____Savanna, _____Savanna, _____Savanna },
				{ __Grasslands, __Grasslands, _____Savanna, _____Savanna, _____Savanna },
				{ ______Desert, _____Savanna, _____Savanna, _____Savanna, _____Savanna },
			},
		});

		public static readonly BiomeMap BIOME_MATRIX_TROPICAL = new BiomeMap(new Biome[5, 5, 5] {
			{
				{ ______Desert, ______Desert, ______Desert, ______Desert, ______Jungle },
				{ ______Desert, ______Desert, ______Desert, _____Savanna, ______Jungle },
				{ ______Desert, ______Desert, _____Savanna, _____Savanna, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, _____Savanna, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, _____Savanna, ______Jungle },
			},
			{
				{ ______Desert, ______Desert, ______Desert, _____Savanna, ______Jungle },
				{ ______Desert, ______Desert, _____Savanna, _____Savanna, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, _____Savanna, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, _____Savanna, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, _____Savanna, ______Jungle },
			},
			{
				{ ______Desert, ______Desert, _____Savanna, _____Savanna, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, _____Savanna, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, _____Savanna, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, _____Savanna, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, ______Jungle, ______Jungle },
			},
			{
				{ ______Desert, _____Savanna, _____Savanna, _____Savanna, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, _____Savanna, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, _____Savanna, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, ______Jungle, ______Jungle },
				{ ______Desert, _____Savanna, ______Jungle, ______Jungle, ______Jungle },
			},
			{
				{ ______Desert, _____Savanna, _____Savanna, _____Savanna, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, _____Savanna, ______Jungle },
				{ ______Desert, _____Savanna, _____Savanna, ______Jungle, ______Jungle },
				{ ______Desert, _____Savanna, ______Jungle, ______Jungle, ______Jungle },
				{ ______Desert, ______Jungle, ______Jungle, ______Jungle, ______Jungle },
			},
		});

		public static readonly BiomeMap BIOME_MATRIX_NORDIC = new BiomeMap(new Biome[5, 5, 5] {
			{
				{ ______Tundra, ______Tundra, ______Arctic, ______Arctic, ______Arctic },
				{ ______Tundra, ______Tundra, ______Arctic, ______Arctic, ______Arctic },
				{ ______Tundra, ______Tundra, ______Boreal, ______Forest, ______Forest },
				{ __Grasslands, __Grasslands, __Grasslands, ______Forest, ______Forest },
				{ __Grasslands, __Grasslands, __Grasslands, ______Forest, ______Forest },
			},
			{
				{ ______Tundra, ______Tundra, ______Arctic, ______Arctic, ______Arctic },
				{ ______Tundra, ______Tundra, ______Boreal, ______Arctic, ______Arctic },
				{ ______Tundra, ______Boreal, ______Boreal, ______Boreal, ______Forest },
				{ __Grasslands, __Grasslands, ______Boreal, ______Forest, ______Forest },
				{ __Grasslands, __Grasslands, __Grasslands, ______Forest, ______Forest },
			},
			{
				{ ______Tundra, ______Tundra, ______Boreal, ______Arctic, ______Arctic },
				{ ______Tundra, ______Tundra, ______Boreal, ______Arctic, ______Arctic },
				{ ______Boreal, ______Boreal, ______Boreal, ______Boreal, ______Boreal },
				{ __Grasslands, __Grasslands, ______Boreal, ______Forest, ______Forest },
				{ __Grasslands, __Grasslands, ______Boreal, ______Forest, ______Forest },
			},
			{
				{ ______Tundra, ______Tundra, ______Boreal, ______Arctic, ______Arctic },
				{ ______Tundra, ______Boreal, ______Boreal, ______Boreal, ______Arctic },
				{ ______Boreal, ______Boreal, ______Boreal, ______Boreal, ______Boreal },
				{ __Grasslands, ______Boreal, ______Boreal, ______Boreal, ______Forest },
				{ __Grasslands, __Grasslands, ______Boreal, ______Forest, ______Forest },
			},
			{
				{ ______Tundra, ______Boreal, ______Boreal, ______Boreal, ______Arctic },
				{ ______Boreal, ______Boreal, ______Boreal, ______Boreal, ______Boreal },
				{ ______Boreal, ______Boreal, ______Boreal, ______Boreal, ______Boreal },
				{ ______Boreal, ______Boreal, ______Boreal, ______Boreal, ______Boreal },
				{ __Grasslands, ______Boreal, ______Boreal, ______Boreal, ______Forest },
			},
		});



	}

	/*
		This section is for the fields/properties.
		That way it's a bit easier to find what you're looking for.
	*/
	public static partial class GenerationUtils {
		public const float OCEON_SAND_PADDING = 0.03f;     // The default altitude above ocean to padd with sand.
		public const float MOUNTIAN_STONE_PADDING = 0.03f; // The default altitude below mountain to padd with stone.


		public const float BIOME_CUTOFF_A = 0.30f; // The default cut-off where a value is very low.
		public const float BIOME_CUTOFF_B = 0.45f; // The default cut-off where a value is low.
		public const float BIOME_CUTOFF_C = 0.55f; // The default cut-off where a value is high.
		public const float BIOME_CUTOFF_D = 0.70f; // The default cut-off where a value is very high.


	}


	/*
		This section is for spooky reflection stuff.
	*/
	public static partial class GenerationUtils {

		/*
			Maps sub-interfaces of IStepGenerator to their implementations.
		*/
		private static Dictionary<Type, List<Type>> _generators = _makeGeneratorDict();

		/*
			List of implementations by their name.
		*/
		private static Dictionary<string, Type> _generatorsByName;


		/*
			Map each subinterfaces of IStepGenerator to all their implementations.
		*/
		private static Dictionary<Type, List<Type>> _makeGeneratorDict() {
			var root = typeof(IStepGenerator);
			//var assembly = Assembly.GetCallingAssembly();
			var assembly = Assembly.GetAssembly(root);

			// Get all types branching off the root interface.
			var types = assembly.GetTypes().Where(t => root.IsAssignableFrom(t)).ToList();

			// Get all the interfaces for different step-generators.
			var interfaces = types.Where(t => t.IsInterface && t != root).ToList();

			// Get all concrete implementations of this interface.
			var instances = types.Where(
				t => t.IsClass 
				&& !t.IsAbstract 
				&& !t.IsGenericTypeDefinition
				&& t.GetConstructor(Type.EmptyTypes) != null
			).ToList();

			// Create new, empty list of types.
			var res = new Dictionary<Type, List<Type>>();
			foreach(var i in interfaces) res[i] = new List<Type>();


			_generatorsByName = new Dictionary<string, Type>();
			// For each implementation, map it to each sub-interface it implements.
			foreach(var t in instances) {
				foreach(var i in interfaces) {
					if(i.IsAssignableFrom(t))
						res[i].Add(t);
				}

				// Map instances by name.
				_generatorsByName[t.Name] = t;
			}

			return res;
		}

		/*
			Gets a list of implementations of the given interface.
		*/
		public static IReadOnlyList<Type> GetImplementations<T>() where T : IStepGenerator {
			return _generators.GetValueOrDefault(typeof(T), new List<Type>());
		}

		/*
			Gets a list of implementations of the given interface.
		*/
		public static Dictionary<string, T> GetImplementationsInstanceDict<T>(bool includeNull = true) where T : class, IStepGenerator {
			var res = new Dictionary<string, T>();
			if(includeNull) res["none"] = null;

			if(_generators.TryGetValue(typeof(T), out var lst)) {
				foreach(var t in lst) {
					res[t.Name] = (T) Activator.CreateInstance(t);
				}
			}

			return res;
		}
		public static Dictionary<string, object> GetImplementationsInstanceDict(Type type, bool includeNull = true) {
			var res = new Dictionary<string, object>();
			if(includeNull) res["none"] = null;

			if(_generators.TryGetValue(type, out var lst)) {
				foreach(var t in lst) {
					res[t.Name.ToLower()] = Activator.CreateInstance(t);
				}
			}

			return res;
		}
		public static Dictionary<string, object> GetImplementationsInstanceDict(string typeName, bool includeNull = true) 
			=> GetImplementationsInstanceDict(Assembly.GetAssembly(typeof(IStepGenerator)).GetType(typeName), includeNull);


		/*
			Gets an implementation by name.
		*/
		public static IStepGenerator GetImplementationInstance<T>(string typeName) where T : IStepGenerator {
			var t = _generatorsByName[typeName.ToLower()];
			return (T) Activator.CreateInstance(t);
		}
		public static IStepGenerator GetImplementationInstance(string typeName)
			=> GetImplementationInstance<IStepGenerator>(typeName);
	}
}
