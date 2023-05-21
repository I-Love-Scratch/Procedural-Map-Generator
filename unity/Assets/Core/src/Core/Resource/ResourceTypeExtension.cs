using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;

namespace terrainGenerator {
	/*
		Extension methods for ResourceTypes.
	*/
	public static class ResourceTypeExtension {


		/* 
			Gets the color of the ResourceType.
		*/
		public static Color32 GetColor(this ResourceType type) => type switch {
			ResourceType.Gold    => new Color32(255, 204, 0, 255),
			ResourceType.Iron    => new Color32(153, 153, 102, 255),
			ResourceType.Copper  => new Color32(255, 153, 0, 255),
			ResourceType.Berries => new Color32(255, 51, 153, 255),
			ResourceType.Cotton  => new Color32(204, 204, 255, 255),
			ResourceType.Spice   => new Color32(255, 51, 0, 255),
			_                    => Color.black,
		};

	}

}
