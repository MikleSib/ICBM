// World Strategy Kit for Unity - Main Script
// (C) Kronnect Technologies SL
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WMSK

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WorldMapStrategyKit.PathFinding;


namespace WorldMapStrategyKit {
    public enum TERRAIN_CAPABILITY {
        Any = 1,
        OnlyGround = 2,
        OnlyWater = 4,
        Air = 8
    }

    public partial class WMSK : MonoBehaviour {

        byte[] earthRouteMatrix;
        // bit 1 for custom elevation, bit 2 for ground without elevation restrictions, bit 3 for water without elevation restrictions
        float[] _customRouteMatrix;
        // optional values for custom validation
        float earthRouteMatrixWithElevationMinAltitude, earthRouteMatrixWithElevationMaxAltitude;
        byte computedMatrixBits;

        FastBitArray earthWaterMask;
        const byte EARTH_WATER_MASK_OCEAN_LEVEL_MAX_ALPHA = 16;
        // A lower alpha value in texture means water

        int earthWaterMaskWidth, earthWaterMaskHeight;
        int EARTH_ROUTE_SPACE_WIDTH = 2048;
        // both must be power of 2
        int EARTH_ROUTE_SPACE_HEIGHT = 1024;
        PathFinderFast finder;
        PathFinderAdminEntity finderCountries;
        PathFinderAdminEntity finderProvinces;
        PathFinderCells finderCells;
        int lastMatrix;
        Texture2D pathFindingCustomMatrixCostTexture;
        bool cellsCostsComputed;
        CellCosts[] _cellsCosts;

        float[] customRouteMatrix {
            get {
                if (_customRouteMatrix == null || _customRouteMatrix.Length == 0) {
                    PathFindingCustomRouteMatrixReset();
                }
                return _customRouteMatrix;
            }
        }

        void PathFindingPrewarm() {
            CheckRouteWaterMask();
            Task.Run(PathFindingPrewarmBackground);
        }

        void PathFindingPrewarmBackground() { 
            ComputeRouteMatrix(TERRAIN_CAPABILITY.OnlyGround, 0, 1.0f);
            ComputeRouteMatrix(TERRAIN_CAPABILITY.OnlyWater, 0, 1.0f);
            if (_showGrid) {
                ComputeCellsCostsInfo();
            }
            PathFindingPrewarmCountryPositions();
        }

        // Returns true if water mask buffer has been created; false if it was already created
        bool CheckRouteWaterMask() {

            if (earthWaterMask != null)
                return false;

            // Get water mask info
            Texture2D waterMap = null;
            if (waterMask == null) {
                if (_earthStyle.isScenicPlus()) {
                    waterMap = (Texture2D)earthMat.GetTexture("_TerrestrialMap");
                }
            } else {
                waterMap = waterMask;
            }
            if (waterMap == null) {
                Debug.LogError("Water mask texture could not be loaded");
                return false;
            }
            earthWaterMaskHeight = waterMap.height;
            earthWaterMaskWidth = waterMap.width;
            Color32[] colors = waterMap.GetPixels32();
            earthWaterMask = new FastBitArray(colors, _waterMaskLevel);

            // Remind to compute cell costs again
            cellsCostsComputed = false;

            return true;
        }

        void ComputeRouteMatrix(TERRAIN_CAPABILITY terrainCapability, float minAltitude, float maxAltitude) {

            bool computeMatrix = false;
            byte thisMatrix = 0;

            // prepare water mask data
            bool checkTerrainCapability = terrainCapability == TERRAIN_CAPABILITY.OnlyGround || terrainCapability == TERRAIN_CAPABILITY.OnlyWater;
            if (checkTerrainCapability) {

                // prepare matrix
                if (earthRouteMatrix == null) {
                    earthRouteMatrix = new byte[EARTH_ROUTE_SPACE_WIDTH * EARTH_ROUTE_SPACE_HEIGHT];
                    computedMatrixBits = 0;
                }

                computeMatrix = CheckRouteWaterMask();
                thisMatrix = (byte)terrainCapability;

                // check elevation data if needed
                bool checkElevation = minAltitude > 0f || maxAltitude < 1.0f;

                if (checkElevation) {
                    if (viewportElevationPoints == null) {
                        Debug.LogError("Viewport needs to be initialized before calling using Path Finding functions.");
                        return;
                    }
                    if (minAltitude != earthRouteMatrixWithElevationMinAltitude || maxAltitude != earthRouteMatrixWithElevationMaxAltitude) {
                        computeMatrix = true;
                        earthRouteMatrixWithElevationMinAltitude = minAltitude;
                        earthRouteMatrixWithElevationMaxAltitude = maxAltitude;
                    }
                } else {
                    if ((computedMatrixBits & thisMatrix) == 0) {
                        computeMatrix = true;
                        computedMatrixBits |= thisMatrix;   // mark computedMatrixBits
                    }
                }

                // Compute route
                if (computeMatrix) {
                    int jj_waterMask = 0, kk_waterMask;
                    int jj_terrainElevation = 0, kk_terrainElevation;
                    bool dry = false;
                    float elev = 0;
                    for (int j = 0; j < EARTH_ROUTE_SPACE_HEIGHT; j++) {
                        int jj = j * EARTH_ROUTE_SPACE_WIDTH;
                        if (checkTerrainCapability)
                            jj_waterMask = (int)((j * (float)earthWaterMaskHeight / EARTH_ROUTE_SPACE_HEIGHT)) * earthWaterMaskWidth;
                        if (checkElevation)
                            jj_terrainElevation = ((int)(j * (float)heightmapTextureHeight / EARTH_ROUTE_SPACE_HEIGHT)) * heightmapTextureWidth;
                        for (int k = 0; k < EARTH_ROUTE_SPACE_WIDTH; k++) {
                            bool setBit = false;
                            // Check altitude
                            if (checkElevation) {
                                kk_terrainElevation = (int)(k * (float)heightmapTextureWidth / EARTH_ROUTE_SPACE_WIDTH);
                                elev = viewportElevationPoints[jj_terrainElevation + kk_terrainElevation];
                            }
                            if (elev >= minAltitude && elev <= maxAltitude) {
                                if (checkTerrainCapability) {
                                    kk_waterMask = (int)(k * (float)earthWaterMaskWidth / EARTH_ROUTE_SPACE_WIDTH);
                                    dry = !earthWaterMask.GetBit(jj_waterMask + kk_waterMask);
                                }
                                if (terrainCapability == TERRAIN_CAPABILITY.OnlyGround && dry ||
                                                         terrainCapability == TERRAIN_CAPABILITY.OnlyWater && !dry) {
                                    setBit = true;
                                }
                            }
                            if (setBit) {   // set navigation bit
                                earthRouteMatrix[jj + k] |= thisMatrix;
                            } else {        // clear navigation bit
                                earthRouteMatrix[jj + k] &= (byte)(byte.MaxValue ^ thisMatrix);
                            }
                        }
                    }
                }
            }

            if (finder == null) {
                if (_customRouteMatrix == null || !_pathFindingEnableCustomRouteMatrix) {
                    PathFindingCustomRouteMatrixReset();
                }
                finder = new PathFinderFast(earthRouteMatrix, thisMatrix, EARTH_ROUTE_SPACE_WIDTH, EARTH_ROUTE_SPACE_HEIGHT, _customRouteMatrix);
            } else {
                if (computeMatrix || thisMatrix != lastMatrix) {
                    lastMatrix = thisMatrix;
                    finder.SetCalcMatrix(earthRouteMatrix, thisMatrix);
                }
            }
        }


        void ComputeCellsCostsInfo() {

            if (cellsCostsComputed || _cellsCosts == null)
                return;

            CheckRouteWaterMask();

            int cellsCount = cells.Length;
            bool usesViewport = renderViewportIsEnabled && viewportElevationPoints != null;
            for (int k = 0; k < cellsCount; k++) {
                if (cells[k] == null)
                    continue;
                float x = cells[k].center.x + 0.5f;
                float y = cells[k].center.y + 0.5f;
                int px = (int)(x * earthWaterMaskWidth);
                int py = (int)(y * earthWaterMaskHeight);
                bool water = earthWaterMask.GetBit(py * earthWaterMaskWidth + px);
                _cellsCosts[k].isWater = water;

                if (usesViewport) {
                    px = (int)(x * heightmapTextureWidth);
                    py = (int)(y * heightmapTextureHeight);
                    float elev = viewportElevationPoints[py * heightmapTextureWidth + px];
                    _cellsCosts[k].altitude = elev;
                }
            }


            cellsCostsComputed = true;

            if (finderCells == null) {
                finderCells = new PathFinderCells(_cellsCosts, _gridColumns, _gridRows);
            } else {
                finderCells.SetCustomCellsCosts(_cellsCosts);
            }

        }



        /// <summary>
        /// Used by FindRoute method to satisfy custom positions check
        /// </summary>
        float FindRoutePositionValidator(int location) {
            if (_customRouteMatrix == null) {
                PathFindingCustomRouteMatrixReset();
            }
            float cost = 1;
            if (OnPathFindingCrossPosition != null) {
                int y = location / EARTH_ROUTE_SPACE_WIDTH;
                int x = location - y * EARTH_ROUTE_SPACE_WIDTH;
                Vector2 position = MatrixCostPositionToMap2D(x, y);
                cost = OnPathFindingCrossPosition(position);
            }
            _customRouteMatrix[location] = cost;
            return cost;
        }

        /// <summary>
        /// Used by FindRoute method in country-country mode
        /// </summary>
        /// <returns>The extra cross cost.</returns>
        float FindRouteCountryValidator(int countryIndex) {
            if (OnPathFindingCrossCountry != null) {
                return OnPathFindingCrossCountry(countryIndex);
            }
            return 0;
        }

        /// <summary>
        /// Used by FindRoute method in province-province mode
        /// </summary>
        /// <returns>The extra cross cost.</returns>
        float FindRouteProvinceValidator(int provinceIndex) {
            if (OnPathFindingCrossProvince != null) {
                return OnPathFindingCrossProvince(provinceIndex);
            }
            return 0;
        }

        /// <summary>
        /// Used by FindRoute method in cell mode
        /// </summary>
        /// <returns>The extra cross cost.</returns>
        float FindRouteCellValidator(int cellIndex) {
            if (OnPathFindingCrossCell != null) {
                return OnPathFindingCrossCell(cellIndex);
            }
            return 0;
        }

        Point Map2DToMatrixCostPosition(Vector2 position) {
            int x = (int)((position.x + 0.5f) * EARTH_ROUTE_SPACE_WIDTH);
            int y = (int)((position.y + 0.5f) * EARTH_ROUTE_SPACE_HEIGHT);
            return new Point(x, y);
        }

        Vector2 MatrixCostPositionToMap2D(Point position) {
            return MatrixCostPositionToMap2D(position.X, position.Y);
        }

        Vector2 MatrixCostPositionToMap2D(int k, int j) {
            float x = (k + 0.5f) / EARTH_ROUTE_SPACE_WIDTH - 0.5f;
            float y = (j + 0.5f) / EARTH_ROUTE_SPACE_HEIGHT - 0.5f;
            return new Vector2(x, y);
        }

        int PointToRouteMatrixIndex(Point p) {
            return p.Y * EARTH_ROUTE_SPACE_WIDTH + p.X;
        }

        void UpdatePathfindingMatrixCostTexture() {
            if (pathFindingCustomMatrixCostTexture == null) {
                pathFindingCustomMatrixCostTexture = new Texture2D(EARTH_ROUTE_SPACE_WIDTH, EARTH_ROUTE_SPACE_HEIGHT);
                if (disposalManager != null) disposalManager.MarkForDisposal(pathFindingCustomMatrixCostTexture); // pathFindingCustomMatrixCostTexture.hideFlags = HideFlags.DontSave;
                pathFindingCustomMatrixCostTexture.filterMode = FilterMode.Point;
            }
            int len = customRouteMatrix.Length;
            float maxValue = 0;
            for (int k = 0; k < len; k++) {
                if (_customRouteMatrix[k] > maxValue)
                    maxValue = _customRouteMatrix[k];
            }
            if (maxValue <= 0) maxValue = 1;
            Color[] colors = new Color[_customRouteMatrix.Length];
            Color white = Color.white;
            Color black = Color.black;
            Color c = Color.white;
            for (int k = 0; k < colors.Length; k++) {
                if (_customRouteMatrix[k] < 0) {
                    colors[k] = white;
                } else if (_customRouteMatrix[k] == 0) {
                    colors[k] = black;
                } else {
                    float t = _customRouteMatrix[k] / maxValue;
                    if (t > 1f) t = 1f;
                    c.g = c.b = t;
                    colors[k] = c;
                }
            }
            pathFindingCustomMatrixCostTexture.SetPixels(colors);
            pathFindingCustomMatrixCostTexture.Apply();
        }


    }

}