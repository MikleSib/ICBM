using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using WorldMapStrategyKit.MapGenerator;
using WorldMapStrategyKit.MapGenerator.Geom;

namespace WorldMapStrategyKit {

	public partial class WMSK_Editor : MonoBehaviour {

		#region Map generation

		Dictionary<Segment, MapRegion> cellNeighbourHit;
		Dictionary<Segment, MapRegion> territoryNeighbourHit;
		List<Segment> territoryFrontiers;
		StringBuilder sb;
		HashSet<string> usedNames = new HashSet<string>();

		Point[] centers;
		VoronoiFortune voronoi;

		public void ApplySeed() {
			UnityEngine.Random.InitState(seed);
			if (octavesBySeed || noiseOctaves == null || noiseOctaves.Length == 0) {
				int octaves = UnityEngine.Random.Range(5, 8);
				noiseOctaves = new NoiseOctave[octaves];
				for (int k = 0; k < octaves; k++) {
					NoiseOctave octave = new NoiseOctave();
					octave.amplitude = 1f / (k + 1f);
					octave.frequency = Mathf.Pow(2, k) * (1f + UnityEngine.Random.value);
					noiseOctaves[k] = octave;
				}
			}
		}


		void VoronoiTesselation() {
			bool usesUserDefinedSites = false;
			if (voronoiSites != null && voronoiSites.Count > 0) {
				numProvinces = voronoiSites.Count;
				usesUserDefinedSites = true;
			}
			if (centers == null || centers.Length != numProvinces) {
				centers = new Point[numProvinces];
			}
			for (int k = 0; k < centers.Length; k++) {
				if (usesUserDefinedSites) {
					Vector2 p = voronoiSites[k];
					centers[k] = new Point(p.x, p.y);
				} else {
					centers[k] = new Point(UnityEngine.Random.Range(-0.49f, 0.49f), UnityEngine.Random.Range(-0.49f, 0.49f));
				}
			}

			if (voronoi == null) {
				voronoi = new VoronoiFortune();
			}
			for (int k = 0; k < goodGridRelaxation; k++) {
				voronoi.AssignData(centers);
				voronoi.DoVoronoi();
				if (k < goodGridRelaxation - 1) {
					for (int j = 0; j < numProvinces; j++) {
						Point centroid = voronoi.cells[j].centroid;
						centers[j] = (centers[j] + centroid) / 2;
					}
				}
			}

			// Make cell regions: we assume cells have only 1 region but that can change in the future
			for (int k = 0; k < voronoi.cells.Length; k++) {
				VoronoiCell voronoiCell = voronoi.cells[k];
				Vector2 center = voronoiCell.center.vector3;
				MapProvince cell = new MapProvince(center);
				MapRegion cr = new MapRegion(cell);
				if (edgeNoise > 0) {
					cr.polygon = voronoiCell.GetPolygon(voronoiCell.center, edgeMaxLength, edgeNoise);
				} else {
					cr.polygon = voronoiCell.GetPolygon();
				}
				if (cr.polygon != null) {
					// Add segments
					int segmentsCount = voronoiCell.segments.Count;
					for (int i = 0; i < segmentsCount; i++) {
						Segment s = voronoiCell.segments[i];
						if (!s.deleted) {
							if (edgeNoise > 0) {
								cr.segments.AddRange(s.subdivisions);
							} else {
								cr.segments.Add(s);
							}
						}
					}
					cell.region = cr;
					mapProvinces.Add(cell);
				}
			}
		}


		void CreateMapProvinces() {

			numProvinces = Mathf.Clamp(numProvinces, Mathf.Max(numCountries, 2), MAX_CELLS);
			if (mapProvinces == null) {
				mapProvinces = new List<MapProvince>(numProvinces);
			} else {
				mapProvinces.Clear();
			}

			VoronoiTesselation();

			ProvincesFindNeighbours();

			ProvincesUpdateBounds();
		}


		void ProvincesUpdateBounds() {
			// Update cells polygon
			for (int k = 0; k < mapProvinces.Count; k++) {
				ProvincesUpdateBounds(mapProvinces[k]);
			}
		}


		void ProvincesUpdateBounds(MapProvince cell) {
			if (cell.region.polygon.contours.Count == 0)
				return;
			Vector2[] points = cell.region.polygon.contours[0].GetVector2Points();
			cell.region.points = points;
			// Update bounding rect
			float minx, miny, maxx, maxy;
			minx = miny = float.MaxValue;
			maxx = maxy = float.MinValue;
			int pointCount = points.Length;
			for (int p = 0; p < pointCount; p++) {
				Vector2 point = points[p];
				if (point.x < minx)
					minx = point.x;
				if (point.x > maxx)
					maxx = point.x;
				if (point.y < miny)
					miny = point.y;
				if (point.y > maxy)
					maxy = point.y;
			}
			float rectWidth = maxx - minx;
			float rectHeight = maxy - miny;
			cell.region.rect2D = new Rect(minx, miny, rectWidth, rectHeight);
			cell.region.rect2DArea = rectWidth * rectHeight;
		}


		/// <summary>
		/// Must be called after changing one cell geometry.
		/// </summary>
		void UpdateProvinceGeometry(MapProvince prov, WorldMapStrategyKit.MapGenerator.Geom.Polygon poly) {
			// Copy new polygon definition
			prov.region.polygon = poly;
			// Update segments list
			prov.region.segments.Clear();
			List<Segment> segmentCache = new List<Segment>(cellNeighbourHit.Keys);
			WorldMapStrategyKit.MapGenerator.Geom.Contour contour0 = poly.contours[0];
			int pointCount = contour0.points.Count;
			int segmentCacheCount = segmentCache.Count;
			for (int k = 0; k < pointCount; k++) {
				Segment s = contour0.GetSegment(k);
				bool found = false;
				// Search this segment in the segment cache
				for (int j = 0; j < segmentCacheCount; j++) {
					Segment o = segmentCache[j];
					if ((Point.EqualsBoth(o.start, s.start) && Point.EqualsBoth(o.end, s.end)) || (Point.EqualsBoth(o.end, s.start) && Point.EqualsBoth(o.start, s.end))) {
						prov.region.segments.Add(o);
						o.territoryIndex = prov.countryIndex;
						found = true;
						break;
					}
				}
				if (!found)
					prov.region.segments.Add(s);
			}
			// Refresh neighbours
			ProvincesUpdateNeighbours();
			// Refresh rect2D
			ProvincesUpdateBounds(prov);

			// Refresh territories
			FindCountryFrontiers();
			UpdateCountryBoundaries();
		}

		void ProvincesUpdateNeighbours() {
			int cellCount = mapProvinces.Count;
			for (int k = 0; k < cellCount; k++) {
				mapProvinces[k].region.neighbours.Clear();
			}
			ProvincesFindNeighbours();
		}


		void ProvincesFindNeighbours() {

			if (cellNeighbourHit == null) {
				cellNeighbourHit = new Dictionary<Segment, MapRegion>(50000);
			} else {
				cellNeighbourHit.Clear();
			}
			int cellsCount = mapProvinces.Count;
			for (int k = 0; k < cellsCount; k++) {
				MapProvince cell = mapProvinces[k];
				MapRegion region = cell.region;
				int numSegments = region.segments.Count;
				for (int i = 0; i < numSegments; i++) {
					Segment seg = region.segments[i];
					if (cellNeighbourHit.ContainsKey(seg)) {
						MapRegion neighbour = cellNeighbourHit[seg];
						if (neighbour != region) {
							if (!region.neighbours.Contains(neighbour)) {
								region.neighbours.Add(neighbour);
								neighbour.neighbours.Add(region);
							}
						}
					} else {
						cellNeighbourHit[seg] = region;
					}
				}
			}
		}


		void FindCountryFrontiers() {

			if (mapCountries == null || mapCountries.Count == 0)
				return;

			if (territoryFrontiers == null) {
				territoryFrontiers = new List<Segment>(cellNeighbourHit.Count);
			} else {
				territoryFrontiers.Clear();
			}
			if (territoryNeighbourHit == null) {
				territoryNeighbourHit = new Dictionary<Segment, MapRegion>(50000);
			} else {
				territoryNeighbourHit.Clear();
			}
			int terrCount = mapCountries.Count;
			Connector[] connectors = new Connector[terrCount];
			for (int k = 0; k < terrCount; k++) {
				connectors[k] = new Connector();
				MapCountry territory = mapCountries[k];
				territory.provinces.Clear();
				if (territory.region == null) {
					MapRegion territoryRegion = new MapRegion(territory);
					territory.region = territoryRegion;
				}
				mapCountries[k].region.neighbours.Clear();
			}

			int cellCount = mapProvinces.Count;
			for (int k = 0; k < cellCount; k++) {
				MapProvince cell = mapProvinces[k];
				if (cell.countryIndex >= terrCount)
					continue;
				bool validCell = cell.visible && cell.countryIndex >= 0;
				if (validCell)
					mapCountries[cell.countryIndex].provinces.Add(cell);
				MapRegion region = cell.region;
				int numSegments = region.segments.Count;
				for (int i = 0; i < numSegments; i++) {
					Segment seg = region.segments[i];
					if (seg.border) {
						if (validCell) {
							territoryFrontiers.Add(seg);
							int territory1 = cell.countryIndex;
							connectors[territory1].Add(seg);
							seg.territoryIndex = territory1;
						}
						continue;
					}
					if (territoryNeighbourHit.ContainsKey(seg)) {
						MapRegion neighbour = territoryNeighbourHit[seg];
						MapProvince neighbourCell = (MapProvince)neighbour.entity;
						int territory1 = cell.countryIndex;
						int territory2 = neighbourCell.countryIndex;
						if (territory2 != territory1) {
							territoryFrontiers.Add(seg);
							if (validCell) {
								connectors[territory1].Add(seg);
								seg.territoryIndex = (territory2 >= 0) ? -1 : territory1;   // if segment belongs to a visible cell and valid territory2, mark this segment as disputed. Otherwise make it part of territory1
								if (seg.territoryIndex < 0) {
									// add territory neigbhours
									MapRegion territory1Region = mapCountries[territory1].region;
									MapRegion territory2Region = mapCountries[territory2].region;
									if (!territory1Region.neighbours.Contains(territory2Region)) {
										territory1Region.neighbours.Add(territory2Region);
									}
									if (!territory2Region.neighbours.Contains(territory1Region)) {
										territory2Region.neighbours.Add(territory1Region);
									}
								}
							}
							if (territory2 >= 0) {
								connectors[territory2].Add(seg);
							}
						}
					} else {
						territoryNeighbourHit.Add(seg, region);
						seg.territoryIndex = cell.countryIndex;
					}
				}
			}

			for (int k = 0; k < terrCount; k++) {
				mapCountries[k].region = new MapRegion(mapCountries[k]);
				if (mapCountries[k].provinces.Count > 0) {
					mapCountries[k].region.polygon = connectors[k].ToPolygonFromLargestLineStrip();
				} else {
					mapCountries[k].region.polygon = null;
				}
			}

		}



		void CreateMapCountries() {

			numCountries = Mathf.Clamp(numCountries, 1, Mathf.Min(numProvinces, MAX_TERRITORIES));

			if (mapCountries == null) {
				mapCountries = new List<MapCountry>(numCountries);
			} else {
				mapCountries.Clear();
			}

			for (int k = 0; k < mapProvinces.Count; k++) {
				mapProvinces[k].countryIndex = -1;
			}

			int cellsCount = mapProvinces.Count;
			for (int c = 0; c < numCountries; c++) {
				MapCountry territory = new MapCountry(c.ToString());
				int territoryIndex = mapCountries.Count;
				int p = UnityEngine.Random.Range(0, cellsCount);
				int z = 0;
				while ((mapProvinces[p].countryIndex != -1 || !mapProvinces[p].visible || mapProvinces[p].ignoreTerritories) && z++ <= mapProvinces.Count) {
					p++;
					if (p >= mapProvinces.Count)
						p = 0;
				}
				if (z > mapProvinces.Count)
					break; // no more territories can be found - this should not happen
				MapProvince prov = mapProvinces[p];
				prov.countryIndex = territoryIndex;
				territory.capitalCenter = prov.center;
				territory.provinces.Add(prov);
				mapCountries.Add(territory);
			}

			// Continue conquering cells
			int[] territoryCellIndex = new int[mapCountries.Count];

			// Iterate one cell per country (this is not efficient but ensures balanced distribution)
			bool remainingCells = true;
			int territoriesCount = mapCountries.Count;
			while (remainingCells) {
				while (remainingCells) {
					remainingCells = false;
					for (int k = 0; k < territoriesCount; k++) {
						MapCountry territory = mapCountries[k];
						int territoryCellsCount = territory.provinces.Count;
						for (int p = territoryCellIndex[k]; p < territoryCellsCount; p++) {
							MapRegion cellRegion = territory.provinces[p].region;
							int neighboursCount = cellRegion.neighbours.Count;
							for (int n = 0; n < neighboursCount; n++) {
								MapRegion otherRegion = cellRegion.neighbours[n];
								MapProvince otherProv = (MapProvince)otherRegion.entity;
								if (otherProv.countryIndex == -1 && otherProv.visible && !otherProv.ignoreTerritories) {
									otherProv.countryIndex = k;
									territory.provinces.Add(otherProv);
									remainingCells = true;
									p = territory.provinces.Count;
									break;
								}
							}
							if (p < territoryCellsCount) // no free neighbours left for this cell
								territoryCellIndex[k]++;
						}
					}
				}
				// Check if there's any other cell without territory
				for (int k = 0; k < cellsCount; k++) {
					MapProvince cell = mapProvinces[k];
					if (cell.countryIndex == -1 && cell.visible && !cell.ignoreTerritories) {
						int territoryIndex = UnityEngine.Random.Range(0, territoriesCount);
						cell.countryIndex = territoryIndex;
						mapCountries[territoryIndex].provinces.Add(cell);
						remainingCells = true;
						break;
					}
				}
			}

			FindCountryFrontiers();
			UpdateCountryBoundaries();
		}

		void UpdateCountryBoundaries() {
			if (mapCountries == null)
				return;

			// Update territory region
			int terrCount = mapCountries.Count;
			for (int k = 0; k < terrCount; k++) {
				MapCountry territory = mapCountries[k];
				MapRegion territoryRegion = territory.region;

				if (territoryRegion.polygon == null) {
					continue;
				}
				territoryRegion.points = territoryRegion.polygon.contours[0].GetVector2Points();
				List<Point> points = territoryRegion.polygon.contours[0].points;
				int pointCount = points.Count;
				for (int j = 0; j < pointCount; j++) {
					Point p0 = points[j];
					Point p1;
					if (j == pointCount - 1) {
						p1 = points[0];
					} else {
						p1 = points[j + 1];
					}
					territoryRegion.segments.Add(new Segment(p0, p1));
				}

				// Update bounding rect
				float minx, miny, maxx, maxy;
				minx = miny = float.MaxValue;
				maxx = maxy = float.MinValue;
				int terrPointCount = territoryRegion.points.Length;
				for (int p = 0; p < terrPointCount; p++) {
					Vector2 point = territoryRegion.points[p];
					if (point.x < minx)
						minx = point.x;
					if (point.x > maxx)
						maxx = point.x;
					if (point.y < miny)
						miny = point.y;
					if (point.y > maxy)
						maxy = point.y;
				}
				float rectWidth = maxx - minx;
				float rectHeight = maxy - miny;
				territoryRegion.rect2D = new Rect(minx, miny, rectWidth, rectHeight);
				territoryRegion.rect2DArea = rectWidth * rectHeight;
			}
		}



		#endregion

		#region Drawing stuff

		/// <summary>
		/// Generate and replace provinces, city and country data
		/// </summary>
		public void GenerateMap() {

			try {

				UnityEngine.Random.InitState(seed);
				GenerateHeightMap();
				CreateMapProvinces();
				AssignHeightMapToProvinces(true);
				CreateMapCountries();
				CreateMapCities();

				UnityEngine.Random.InitState(seedNames);
				// Replace countries
				int mapCountriesCount = mapCountries.Count;
				List<Country> newCountries = new List<Country>(mapCountriesCount);
				for (int k = 0; k < mapCountriesCount; k++) {
					MapCountry c = mapCountries[k];
					Vector2[] points = c.region.points;
					if (points == null || points.Length < 3)
						continue;
					string name = GetUniqueRandomName(0, 4, usedNames);
					Country newCountry = new Country(name, "World", k);
					Region region = new Region(newCountry, newCountry.regions.Count);
					newCountry.regions.Add(region);
					region.UpdatePointsAndRect(points);
					map.RefreshCountryGeometry(newCountry);
					newCountries.Add(newCountry);
				}
				map.countries = newCountries.ToArray();
				countryChanges = true;

				// Replace provinces
				usedNames.Clear();
				int mapProvincesCount = mapProvinces.Count;
				List<Province> newProvinces = new List<Province>(mapProvincesCount);
				Province[] provinces = _map.provinces;
				if (provinces == null) {
					provinces = new Province[0];
				}
				for (int k = 0; k < mapProvincesCount; k++) {
					MapProvince c = mapProvinces[k];
					if (!c.visible) {
						continue;
					}
					Vector2[] points = c.region.points;
					if (points == null || points.Length < 3)
						continue;
					int countryIndex = map.GetCountryIndex(c.center);
					if (countryIndex < 0)
						continue;

					string name = GetUniqueRandomName(0, 4, usedNames);
					Province newProvince = new Province(name, countryIndex, k);
					newProvince.regions = new List<Region>();
					Region region = new Region(newProvince, newProvince.regions.Count);
					newProvince.attrib["mapColor"] = c.color;
					newProvince.regions.Add(region);
					region.UpdatePointsAndRect(points);
					map.RefreshProvinceGeometry(newProvince);
					newProvinces.Add(newProvince);

					Country country = map.countries[countryIndex];
					List<Province> countryProvinces = country.provinces != null ? new List<Province>(country.provinces) : new List<Province>();
					countryProvinces.Add(newProvince);
					country.provinces = countryProvinces.ToArray();
				}
				map.provinces = newProvinces.ToArray();
				provinceChanges = true;

				// Replace cities
				usedNames.Clear();
				int mapCitiesCount = mapCities.Count;
				List<City> newCities = new List<City>(mapCitiesCount);
				string provinceName = "";
				for (int k = 0; k < mapCitiesCount; k++) {
					MapCity c = mapCities[k];
					City newCity;
					Province prov = map.GetProvince(c.unity2DLocation);
					if (prov == null)
						continue;
					provinceName = prov != null ? prov.name : "";
					string name = GetUniqueRandomName(0, 4, usedNames);
					newCity = new City(name, provinceName, prov.countryIndex, c.population, c.unity2DLocation, c.cityClass, k);
					newCities.Add(newCity);
				}
				map.cities = newCities.ToArray();
				cityChanges = true;

				// Generate textures
				GenerateWorldTexture();

				// Apply some complementary style
				if (changeStyle && heightGradientPreset != HeightMapGradientPreset.Custom) {
					map.frontiersColor = Color.black;
					map.frontiersColorOuter = Color.black;
					map.provincesColor = new Color(0.5f, 0.5f, 0.5f, 0.35f);
					if (heightGradientPreset == HeightMapGradientPreset.BlackAndWhite || heightGradientPreset == HeightMapGradientPreset.Grayscale) {
						map.countryLabelsColor = Color.black;
						map.countryLabelsShadowColor = new Color(0.9f, 0.9f, 0.9f, 0.75f);
					} else {
						map.countryLabelsColor = Color.white;
						map.countryLabelsShadowColor = new Color(0.1f, 0.1f, 0.1f, 0.75f);
					}
				}
				if (_map.earthStyle.isScenicPlus()) {
					_map.earthStyle = EARTH_STYLE.Natural;
				}


				// Save map data
				SaveGeneratedMapData();

				map.showFrontiers = true;
				map.showCountryNames = true;
				map.waterLevel = seaLevel;
				map.Redraw(true);

			} catch (Exception ex) {
				if (!Application.isPlaying) {
					Debug.LogError("Error generating map: " + ex.ToString());
#if UNITY_EDITOR
					EditorUtility.DisplayDialog("Error Generating Map", "An error occured while generating map. Try choosing another 'Seed' value, reducing 'Border Curvature' amount or number of provinces.", "Ok");
#endif
				}
			}

		}


		#endregion


		#region Cities stuff

		void CreateMapCities() {

			if (mapCountries == null)
				return;

			int countryCount = mapCountries.Count;
			if (mapCities == null) {
				mapCities = new List<MapCity>();
			} else {
				mapCities.Clear();
			}
			int totalCityCount = 0;
			for (int k = 0; k < countryCount; k++) {
				int cityCount = UnityEngine.Random.Range(numCitiesPerCountryMin, numCitiesPerCountryMax + 1);

				MapCountry country = mapCountries[k];
				if (!country.visible)
					continue;
				int provincesCount = country.provinces.Count;
				country.hasCapital = false;
				for (int j = 0; j < provincesCount; j++) {
					country.provinces[j].hasCapital = false;
				}
				Vector2 pos;
				int countryCityCount = 0;
				for (int q = 0; q < cityCount; q++) {
					if (countryCityCount >= cityCount)
						break;
					for (int j = 0; j < provincesCount; j++) {
						if (countryCityCount >= cityCount)
							break;

						MapProvince prov = country.provinces[j];
						if (!prov.visible)
							continue;
						Rect rect = prov.region.rect2D;
						if (rect.width > 0.00001f && rect.height > 0.00001f) {
							do {
								pos.x = rect.xMin + UnityEngine.Random.value * rect.width;
								pos.y = rect.yMin + UnityEngine.Random.value * rect.height;
							} while (!prov.region.Contains(pos.x, pos.y));

							CITY_CLASS cityClass;
							if (!country.hasCapital) {
								country.hasCapital = true;
								cityClass = CITY_CLASS.COUNTRY_CAPITAL;
							} else if (!prov.hasCapital) {
								prov.hasCapital = true;
								cityClass = CITY_CLASS.REGION_CAPITAL;
							} else {
								cityClass = CITY_CLASS.CITY;
							}
							MapCity c = new MapCity("City" + totalCityCount++, j, k, 0, pos, cityClass);
							mapCities.Add(c);
							countryCityCount++;
						}
					}
				}
			}
		}

		#endregion

		#region

#if UNITY_EDITOR
		public string GetGenerationMapOutputPath() {
			string rootFolder;
			string path = "";
			string[] paths = AssetDatabase.GetAllAssetPaths();
			for (int k = 0; k < paths.Length; k++) {
				if (paths[k].EndsWith("WorldMapStrategyKit")) {
					rootFolder = paths[k];
					path = rootFolder + "/Resources/WMSK/Geodata/" + outputFolder;
					break;
				}
			}
			return path;
		}
#endif

		void SaveGeneratedMapData() {

#if UNITY_EDITOR

			if (string.IsNullOrEmpty(outputFolder)) {
				outputFolder = "CustomMap";
			}

			string path = GetGenerationMapOutputPath();
			if (string.IsNullOrEmpty(path)) {
				Debug.LogError("Could not find WMSK folder.");
				return;
			}

			Directory.CreateDirectory(path);
			if (!Directory.Exists(path)) {
				EditorUtility.DisplayDialog("Invalid output folder", "The path " + path + " is no valid.", "Ok");
				return;
			}

			// Set Geodata folder to custom path
			_map.geodataResourcesPath = "WMSK/Geodata/" + outputFolder;

			// Save country data
			string fullPathName = path + "/" + GetCountryGeoDataFileName();
			string data = _map.GetCountryGeoData();
			File.WriteAllText(fullPathName, data, Encoding.UTF8);
			AssetDatabase.ImportAsset(fullPathName);
			countryChanges = false;

			// Save province data
			fullPathName = path + "/" + GetProvinceGeoDataFileName();
			data = _map.GetProvinceGeoData();
			File.WriteAllText(fullPathName, data, Encoding.UTF8);
			AssetDatabase.ImportAsset(fullPathName);
			provinceChanges = false;

			// Save cities
			fullPathName = path + "/" + GetCityGeoDataFileName();
			data = _map.GetCityGeoData();
			File.WriteAllText(fullPathName, data, Encoding.UTF8);
			AssetDatabase.ImportAsset(fullPathName);
			cityChanges = false;

			// Save heightmap
			byte[] bytes = heightMapTexture.EncodeToPNG();
			string outputFile = path + "/heightmap.png";
			File.WriteAllBytes(outputFile, bytes);
			AssetDatabase.ImportAsset(outputFile);
			TextureImporter timp = (TextureImporter)AssetImporter.GetAtPath(outputFile);
			timp.isReadable = true;
			timp.SaveAndReimport();

			// Save background texture
			bytes = backgroundTexture.EncodeToPNG();
			outputFile = path + "/worldColors.png";
			File.WriteAllBytes(outputFile, bytes);
			AssetDatabase.ImportAsset(outputFile);
			timp = (TextureImporter)AssetImporter.GetAtPath(outputFile);
			timp.isReadable = true;
			timp.SaveAndReimport();

			// Save normal map texture
			if (generateNormalMap) {
				bytes = heightMapTexture.EncodeToPNG();
				outputFile = path + "/normalMap.png";
				File.WriteAllBytes(outputFile, bytes);
				AssetDatabase.ImportAsset(outputFile);
				timp = (TextureImporter)AssetImporter.GetAtPath(outputFile);
				timp.textureType = TextureImporterType.NormalMap;
				timp.heightmapScale = normalMapBumpiness;
				timp.convertToNormalmap = true;
				timp.isReadable = true;
				timp.SaveAndReimport();
				_map.earthBumpMapTexture = Resources.Load<Texture2D>(_map.geodataResourcesPath + "/normalMap");
				_map.earthBumpEnabled = true;
			} else {
				_map.earthBumpMapTexture = null;
				_map.earthBumpEnabled = false;
			}

			// Save water mask texture
			bytes = waterMaskTexture.EncodeToPNG();
			outputFile = path + "/waterMask.png";
			File.WriteAllBytes(outputFile, bytes);
			AssetDatabase.ImportAsset(outputFile);
			timp = (TextureImporter)AssetImporter.GetAtPath(outputFile);
			timp.isReadable = true;
			timp.SaveAndReimport();

			// Set textures
			_map.SetWaterMask(Resources.Load<Texture2D>(_map.geodataResourcesPath + "/waterMask"));
			_map.SetEarthTexture(Resources.Load<Texture2D>(_map.geodataResourcesPath + "/worldColors"));
			_map.SetHeightmapTexture(Resources.Load<Texture2D>(_map.geodataResourcesPath + "/heightmap"));

#endif

		}

		#endregion





	}

}