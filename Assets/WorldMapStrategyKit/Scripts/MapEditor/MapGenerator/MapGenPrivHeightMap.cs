//#define SHOW_DEBUG_GIZMOS

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using WorldMapStrategyKit.MapGenerator.Geom;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WorldMapStrategyKit {

	public partial class WMSK_Editor : MonoBehaviour {

		float[] heights;
		Color[] heightColors, backgroundColors;
		int currentHeightmapWidth, currentHeightmapHeight;

		public void GenerateHeightMap (bool preview = false) {

			try {
				if (userHeightMapTexture != null) {
					Color[] hh = userHeightMapTexture.GetPixels ();
					heights = new float[hh.Length];
					for (int k = 0; k < hh.Length; k++) {
						heights [k] = hh [k].r;
					}
					return;
				}

				bool lowQuality = mapGenerationQuality == MapGenerationQuality.Draft || preview;
				currentHeightmapWidth = lowQuality ? 256 : this.heightMapWidth;
				currentHeightmapHeight = lowQuality ? 128 : this.heightMapHeight;

				int heightsSize = currentHeightmapWidth * currentHeightmapHeight;
				if (heights == null || heights.Length != heightsSize) {
					heights = new float[heightsSize];
				} else {
					for (int k = 0; k < heightsSize; k++) {
						heights [k] = 0;
					}
				}

				if (noiseOctaves == null)
					return;
			
				// Fetch some noise
				float ratio = (float)currentHeightmapWidth / currentHeightmapHeight;
				for (int n = 0; n < noiseOctaves.Length; n++) {
					if (noiseOctaves [n].disabled)
						continue;
					int c = 0;
					float frequency = noiseOctaves [n].frequency;
					float amplitude = noiseOctaves [n].amplitude;
					float xmult = frequency * ratio / currentHeightmapWidth;
					float ymult = frequency / currentHeightmapHeight;
					if (noiseOctaves [n].ridgeNoise) {
						for (int y = 0; y < currentHeightmapHeight; y++) {
							float yy = y * ymult;
							for (int x = 0; x < currentHeightmapWidth; x++, c++) {
								float noise = Mathf.PerlinNoise (x * xmult, yy);
								noise = 2f * (0.5f - Mathf.Abs (0.5f - noise));
								noise *= amplitude;
								if (n > 0) {
									noise *= heights [c];
								}
								heights [c] += noise;
							}
						}
					} else {
						for (int y = 0; y < currentHeightmapHeight; y++) {
							float yy = y * ymult;
							for (int x = 0; x < currentHeightmapWidth; x++, c++) {
								float noise = Mathf.PerlinNoise (x * xmult, yy) * amplitude;
								heights [c] += noise;
							}
						}
					}
				}

				// Apply exponent & island factor
				float maxHeight = float.MinValue;
				for (int k = 0; k < heights.Length; k++) {
					float h = heights [k];

					// island factor
					float x = k % currentHeightmapWidth;
					float y = k / currentHeightmapWidth;
					float nx = 2f * x / currentHeightmapWidth - 1f;
					float ny = 2f * y / currentHeightmapHeight - 1f;
					float dx = nx * nx;
					float dy = ny * ny;
					float d = dx + dy;

					float mask = 1f - d * islandFactor;
					if (mask < 0) {
						mask = 0;
					}
					h *= mask;

					// pow
					h = Mathf.Pow (h, noisePower);

					h = h + elevationShift;

					if (h > maxHeight) {
						maxHeight = h;
					}

					heights [k] = h;

				}

				// Normalize values
				if (maxHeight > 0) {
					for (int k = 0; k < heights.Length; k++) {
						if (heights [k] < 0) {
							heights [k] = 0;
						} else {
							heights [k] /= maxHeight;
						}
					}
				}

				if (heightMapTexture == null || heightMapTexture.width != currentHeightmapWidth || heightMapTexture.height != currentHeightmapHeight) {
					heightMapTexture = new Texture2D (currentHeightmapWidth, currentHeightmapHeight, TextureFormat.ARGB32, false);
				}
				if (heightColors == null || heightColors.Length != heights.Length) {
					heightColors = new Color[heights.Length];
				}

				if (preview) {
					for (int k = 0; k < heights.Length; k++) {
						heightColors [k].r = heightColors [k].g = heightColors [k].b = heightColors [k].a = heights [k];
					}
					heightMapTexture.SetPixels (heightColors);
					heightMapTexture.Apply ();
					return;
				}

				// Create water mask
				if (waterMaskTexture == null || waterMaskTexture.width != currentHeightmapWidth || waterMaskTexture.height != currentHeightmapHeight) {
					waterMaskTexture = new Texture2D (currentHeightmapWidth, currentHeightmapHeight, TextureFormat.ARGB32, false);
				}
				Color wc = new Color ();
				int heightsLength = heights.Length;
				for (int k = 0; k < heightsLength; k++) {
					wc.r = wc.g = wc.b = wc.a = heights [k];
					heightColors [k] = wc;
				}
				waterMaskTexture.SetPixels (heightColors);
				waterMaskTexture.Apply ();

			} catch (Exception ex) {
				Debug.LogError ("Error generating heightmap: " + ex.ToString ());
			}
		}

		void AssignHeightMapToProvinces (bool colorGroundCells) {
			if (heightMapTexture == null)
				return;

			if (currentHeightmapWidth == 0) {
				return;
			}
			if (heightGradient == null) {
				heightGradientPreset = HeightMapGradientPreset.Colored;
			}

			GradientColorKey[] colorKeys;
			switch (heightGradientPreset) {
			case HeightMapGradientPreset.Colored:
				heightGradient = new Gradient ();
				colorKeys = new GradientColorKey[4];
				colorKeys [0] = new GradientColorKey (Color.gray, 0f);
				colorKeys [1] = new GradientColorKey (new Color (0.133f, 0.545f, 0.133f), seaLevel);
				colorKeys [2] = new GradientColorKey (new Color (0.898f, 0.898f, 0.298f), (seaLevel + 1f) * 0.5f);
				colorKeys [3] = new GradientColorKey (Color.white, 1f);
				heightGradient.colorKeys = colorKeys;
				break;
			case HeightMapGradientPreset.ColoredLight:
				heightGradient = new Gradient ();
				colorKeys = new GradientColorKey[4];
				colorKeys [0] = new GradientColorKey (Color.gray, 0f);
				colorKeys [1] = new GradientColorKey (new Color (0.333f, 0.745f, 0.333f), seaLevel);
				colorKeys [2] = new GradientColorKey (new Color (0.998f, 0.998f, 0.498f), (seaLevel + 1f) * 0.5f);
				colorKeys [3] = new GradientColorKey (Color.white, 1f);
				heightGradient.colorKeys = colorKeys;
				break;
			case HeightMapGradientPreset.Grayscale:
				heightGradient = new Gradient ();
				colorKeys = new GradientColorKey[3];
				colorKeys [0] = new GradientColorKey (Color.black, 0f);
				colorKeys [1] = new GradientColorKey (Color.gray, seaLevel);
				colorKeys [2] = new GradientColorKey (Color.white, 1f);
				heightGradient.colorKeys = colorKeys;
				break;
			case HeightMapGradientPreset.BlackAndWhite:
				heightGradient = new Gradient ();
				colorKeys = new GradientColorKey[3];
				colorKeys [0] = new GradientColorKey (Color.black, 0f);
				colorKeys [1] = new GradientColorKey (Color.white, seaLevel);
				colorKeys [2] = new GradientColorKey (Color.white, 1f);
				heightGradient.colorKeys = colorKeys;
				break;
			}

			int provCount = mapProvinces.Count;
			for (int k = 0; k < provCount; k++) {
				MapProvince prov = mapProvinces [k];
				int x = (int)Mathf.Clamp ((prov.center.x + 0.5f) * currentHeightmapWidth, 0, currentHeightmapWidth - 1);
				int y = (int)Mathf.Clamp ((prov.center.y + 0.5f) * currentHeightmapHeight, 0, currentHeightmapHeight - 1);
				int j = y * currentHeightmapWidth + x;
				float h = heights [j];
				if (h < seaLevel) {
					prov.visible = false;
				} else {
					prov.visible = true;
					prov.color = colorGroundCells ? heightGradient.Evaluate (h) : Color.white;
				}
			}
		}


		void GenerateWorldTexture () {
			
			// Create background texture
			int backgroundTextureWidth = mapGenerationQuality == MapGenerationQuality.Draft ? 256 : this.backgroundTextureWidth;
			int backgroundTextureHeight = mapGenerationQuality == MapGenerationQuality.Draft ? 128 : this.backgroundTextureHeight;

			if (backgroundTexture == null || backgroundTexture.width != backgroundTextureWidth || backgroundTexture.height != backgroundTextureHeight) {
				backgroundTexture = new Texture2D (backgroundTextureWidth, backgroundTextureHeight, TextureFormat.RGBA32, true);
			}
			int bufferLen = backgroundTextureWidth * backgroundTextureHeight;
			if (backgroundColors == null || backgroundColors.Length != bufferLen) {
				backgroundColors = new Color[bufferLen];
			}
			Color backColor = seaColor;
			backColor.a = 0;
			backgroundColors.Fill<Color> (backColor);

			int provincesCount = _map.provinces.Length;
			for (int k = 0; k < provincesCount; k++) {
				Province prov = _map.provinces [k];
				if (prov.regions == null) {
					_map.ReadProvincePackedString (prov);
					if (prov.regions == null) {
						continue;
					}
				}
				Region region = prov.regions [0];
				if (gradientPerPixel) {
					_map.RegionPaintHeights (backgroundColors, backgroundTextureWidth, backgroundTextureHeight, region, heights, seaLevel, currentHeightmapWidth, currentHeightmapHeight, heightGradient);
				} else {
					Color provColor = prov.attrib ["mapColor"];
					provColor.a = 1;
					_map.RegionPaint (backgroundColors, backgroundTextureWidth, backgroundTextureHeight, region, provColor);
				}
			}

			backgroundTexture.SetPixels (backgroundColors);
			backgroundTexture.Apply ();

			// Set height to 0 out of land areas
			if (heightMapTexture != null) {
				if (currentHeightmapWidth == backgroundTextureWidth && currentHeightmapHeight == backgroundTextureHeight) {
					for (int k = 0; k < backgroundColors.Length; k++) {
						if (backgroundColors [k].a == 0) {
							heights [k] = 0;
							heightColors [k] = Misc.ColorClear;
						} else {
							heightColors [k].r = heightColors [k].g = heightColors [k].b = heightColors [k].a = heights [k];
						}
					}
				} else {
					for (int k = 0, y = 0; y < currentHeightmapHeight; y++) {
						int backy = y * backgroundTextureHeight / currentHeightmapHeight;
						int backyy = backy * backgroundTextureWidth;
						for (int x = 0; x < currentHeightmapWidth; x++, k++) {
							int backx = x * backgroundTextureWidth / currentHeightmapWidth;
							if (backgroundColors [backyy + backx].a == 0) {
								heights [k] = 0;
								heightColors [k] = Misc.ColorClear;
							} else {
								heightColors [k].r = heightColors [k].g = heightColors [k].b = heightColors [k].a = heights [k];
							}
						}
					}
				}
				heightMapTexture.SetPixels (heightColors);
				heightMapTexture.Apply ();
			}
		}



	}
}