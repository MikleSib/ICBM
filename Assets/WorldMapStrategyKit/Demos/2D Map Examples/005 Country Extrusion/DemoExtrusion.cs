using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace WorldMapStrategyKit {
	
	public class DemoExtrusion : MonoBehaviour {

		WMSK map;

		void Start() {
			map = WMSK.instance;

			int USAIndex = map.GetCountryIndex("United States of America");
			Region region = map.GetCountry(USAIndex).mainRegion;

			map.RegionGenerateExtrudeGameObject("Extruded USA", region, 1f, Color.gray);

			map.FlyToCountry(USAIndex, 4f, 0.2f);
				
		}

	}

}

