using UnityEngine;
using WorldMapStrategyKit;


namespace WorldMapStrategyKit {

	public class HexMapProcedural : MonoBehaviour {

		WMSK map;
		GUIStyle labelStyle;
		int countryIndex;
		Color countryColor = new Color(0.698f, 0.396f, 0.094f);

		void Start() {

			// UI Setup - non-important, only for this demo
			labelStyle = new GUIStyle();
			labelStyle.alignment = TextAnchor.MiddleLeft;
			labelStyle.normal.textColor = Color.white;

			// setup GUI resizer - only for the demo
			GUIResizer.Init(800, 500); 

			// Real useful code:

			// 1) Get a reference to the WMSK API
			map = WMSK.instance;

			// 2) Remove any existing map; to prevent loading geodata files at start, check the toggle DontLoadGeodataAtStart in WMSK inspector
			map.ClearAll();

			// 3) Create country based on cells defined by columns and rows
			int[,] cells = new int[,]
			{
				{ 20, 10 },
				{ 21, 10 },
				{ 20, 11 },
				{ 21, 11 }
			};
			countryIndex = CreateHexCountry("My country", cells);

			// Focus on country
			map.FlyToCountry(countryIndex, 2f, 0.2f);

			// Optional: Fill the new country with a color
			map.ToggleCountrySurface(countryIndex, true, countryColor);

			// Optional: allow country expansion using click on other cells
			map.OnCellClick += CellClicked;
		}

		void OnGUI() {
			GUIResizer.AutoResize();
			GUI.Box(new Rect(10, 10, 460, 40), "Click on any cell to add it to the country", labelStyle);
		}


		/// <summary>
		/// Creates a country with a name and list of cell columns and rows.
		/// </summary>
		/// <returns>The country index.</returns>
		int CreateHexCountry(string name, int[,] cells) {
			// 1) Create the empty country
			int countryIndex = map.CountryCreate("My country", "Continent");

			// 2) Add cells to the country, building its frontiers
			for (int k = 0; k < cells.Length / 2; k++) {
				int cellIndex = map.GetCellIndex(cells[k, 0], cells[k, 1]);
				map.CountryTransferCell(countryIndex, cellIndex, false); // false = don't redraw because we'll redraw only once when all cells are added
			}

			// 3) Update the map
			map.Redraw();

			return countryIndex;
		}


		/// <summary>
		/// This function is hooked at Start() function and will be called when user clicks on a cell
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="button">Mouse button index.</param>
		void CellClicked(int cellIndex, int button) {

			// if cell already is contained by country then cancel
			Vector2 cellCenter = map.GetCellPosition(cellIndex);
			int c = map.GetCountryIndex(cellCenter);
			if (c == countryIndex) return;

			// Transfer cell to the country shape
			map.CountryTransferCell(countryIndex, cellIndex, true);	// true = redraw

			// Recolor the country so any new separated region gets the same color
			map.ToggleCountrySurface(countryIndex, true, countryColor);
		}



	}

}

