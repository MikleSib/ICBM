using System.Collections.Generic;
using UnityEngine;


namespace WorldMapStrategyKit {

    public class ProceduralMap : MonoBehaviour {

        WMSK map;

        void Start() {

            // 1) Get a reference to the WMSK API
            map = WMSK.instance;

            // 2) Remove any existing map; to prevent loading geodata files at start, check the toggle DontLoadGeodataAtStart in WMSK inspector
            map.ClearAll();

            // 3) Create country based on points
            Vector2[] points =
            {
                new Vector2(0.05f, 0.12f),
                new Vector2(0.07f, 0.18f),
                new Vector2(0.23f, 0.25f),
                new Vector2(0.3f, 0.2f),
                new Vector2(0.31f, 0.11f),
                new Vector2(0.28f, 0.14f),
                new Vector2(0.25f, 0.09f),
                new Vector2(0.2f, 0.1f)

            };
            int countryIndex = CreateCountry("My country", points);

            //CreateProvince("My Province", countryIndex, points);

            // 4) Draw the map
            map.Redraw();

            // Optional: Fill the new country with a color
            Color countryColor = new Color(0.698f, 0.396f, 0.094f);
            map.ToggleCountrySurface(countryIndex, true, countryColor);
        }


        /// <summary>
        /// Creates a country with a name and list of points.
        /// </summary>
        /// <returns>The country index.</returns>
        int CreateCountry(string name, Vector2[] points) {
            // 1) Initialize a new country
            Country country = new Country(name, "Continent", 1);

            // 2) Define the land region for this country with a list of points with coordinates from -0.5, -0.5 (bottom/left edge of map) to 0.5, 0.5 (top/right edge of map)
            // Note: the list of points should be expressed in clock-wise order
            Region region = new Region(country, 0);

            region.UpdatePointsAndRect(points);

            // 3) Add the region to the country (a country can have multiple regions, like islands)
            country.regions.Add(region);

            // 4) Add the new country to the map
            int countryIndex = map.CountryAdd(country);

            return countryIndex;
        }

        /// <summary>
        /// Creates a province with a name and list of points.
        /// </summary>
        /// <returns>The country index.</returns>
        int CreateProvince(string name, int countryIndex, Vector2[] points) {
            // 1) Initialize a new province
            Province province = new Province(name, countryIndex, 0);

            // 2) Define the land region for this country with a list of points with coordinates from -0.5, -0.5 (bottom/left edge of map) to 0.5, 0.5 (top/right edge of map)
            // Note: the list of points should be expressed in clock-wise order
            Region region = new Region(province, 0);

            region.UpdatePointsAndRect(points);

            // 3) Add the region to the province (a province can also have multiple regions, like islands)
            province.regions = new List<Region>();
            province.regions.Add(region);

            // 4) Add the new country to the map
            int provinceIndex = map.ProvinceAdd(province);
            return provinceIndex;
        }


    }

}

