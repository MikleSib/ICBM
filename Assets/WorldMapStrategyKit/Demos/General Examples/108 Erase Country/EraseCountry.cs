using UnityEngine;


namespace WorldMapStrategyKit {
	public class EraseCountry : MonoBehaviour {

		WMSK map;
		public Color eraseColor;

		void Start () {
			// Get a reference to the World Map API
			map = WMSK.instance;

			map.OnCountryClick += EraseCountryWithColor;
		}

		void OnGUI () {
			GUIResizer.AutoResize ();
			Rect rect = new Rect (10, 10, 500, 20);
			GUI.Box (rect, "");
			GUI.Label (rect, "  Click on a region to remove it.");
		}

		void EraseCountryWithColor (int countryIndex, int regionIndex, int buttonIndex) {
			// Hide country frontiers
			Country country = map.countries [countryIndex];
			// Erase country from background texture
			map.RegionErase (country.regions, eraseColor);
			// Redraw frontiers
			map.Redraw (true);
		}

	}
}

