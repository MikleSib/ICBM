﻿// World Map Strategy Kit for Unity - Main Script
// (C) Kronnect Technologies SL
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WMSK

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WorldMapStrategyKit {

    public partial class WMSK : MonoBehaviour {

        public enum ViewportMode {
            None,
            Viewport3D,
            Terrain,
            MapPanel
        }


        #region Internal variables

        const string MAPPER_CAM = "WMSKMapperCam";
        const string MAPPER_CAM_WRAP = "WMSKMapperCamWrap";

        // resources
        Material fogOfWarMat;

        // Overlay & Viewport
        RenderTexture overlayRT, overlayRTwrapped;
        Camera _currentCamera, _wrapCamera, mapperCam;
        GameObject _wrapCameraObj;
        Material viewportMat;
        ViewportMode viewportMode;

        // Terrain support
        Material terrainMat;
        Terrain terrain;
        Vector3 lastMainCameraPos;
        Quaternion lastMainCameraRot;
        RaycastHit[] terrainHits;

        // Earth effects
        float earthLastElevation = -1;
        const int EARTH_ELEVATION_STRIDE = 256;
        int heightmapTextureWidth, heightmapTextureHeight;
        int viewportColliderNeedsUpdate;
        float[] viewportElevationPoints;
        Color32[] heightMapColors;
        Texture2D currentHeightmapTexture;
        float renderViewportOffsetX, renderViewportOffsetY, renderViewportScaleX, renderViewportScaleY, _renderViewportElevationFactor;
        Vector3 renderViewportClip0, renderViewportClip1;
        float renderViewportClipWidth, renderViewportClipHeight;
        bool lastRenderViewportGood;
        float _renderViewportScaleFactor;
        Vector3 lastRenderViewportRotation, lastRenderViewportPosition;
        List<Region> extrudedRegions;
        Vector2[] viewportUV;
        Vector3[] viewportElevationPointsAdjusted;
        int[] viewportIndices;

        public float renderViewportScaleFactor { get { return renderViewportIsEnabled ? _renderViewportScaleFactor : transform.localScale.y / (lastDistanceFromCamera + 1f); } }

        public float renderViewportElevationFactor { get { return _renderViewportElevationFactor; } }

        // Curvature
        Mesh quadPrefab, flexQuadPrefab, flexQuad;
        float flexQuadCurvature;
        float[] curvatureOffsets;
        float currentCurvature;


        public Camera currentCamera {
            get {
                if (_currentCamera == null) {
                    SetupViewport();
                }
                return _currentCamera;
            }
        }

        public bool renderViewPortIsTerrain {
            get { return viewportMode == ViewportMode.Terrain; }
        }

        #endregion

        #region Viewport mesh building

        /// <summary>
        /// Build an extruded mesh for the viewport
        /// </summary>
        void EarthBuildMesh() {
            // Real Earth relief is only available when viewport is enabled
            if (_renderViewport == null || _renderViewport == gameObject)
                return;

            EarthGetElevationInfo();

            EarthUpdateElevation();
            earthLastElevation = _earthElevation;

            // Updates objects elevation
            UpdateViewportObjectsTransformAndVisibility();
        }

        void EarthGetElevationInfo() {
            int size = heightmapTextureWidth * heightmapTextureHeight;
            if (viewportElevationPoints == null || viewportElevationPoints.Length != size || _heightMapTexture == null || _heightMapTexture.width != heightmapTextureWidth || _heightMapTexture != currentHeightmapTexture) {

                // Get elevation info
                if (_heightMapTexture == null) {
                    _heightMapTexture = Resources.Load<Texture2D>("WMSK/Textures/EarthHeightMap");  // default
                }

                heightMapColors = _heightMapTexture.GetPixels32();
                currentHeightmapTexture = _heightMapTexture;
                heightmapTextureWidth = _heightMapTexture.width;
                heightmapTextureHeight = _heightMapTexture.height;
                size = heightmapTextureWidth * heightmapTextureHeight;
                if (viewportElevationPoints == null || viewportElevationPoints.Length != size) {
                    viewportElevationPoints = new float[size];
                }

            } else if (earthLastElevation >= 0) { // data already loaded
                return;
            }

            float baseElevation = 24.0f / 255.0f;
            int tw = _heightMapTexture.width;
            int extrudedRegionCount = extrudedRegions != null ? extrudedRegions.Count : 0;

            if (extrudedRegionCount > 0) {
                Vector2 p;
                for (int e = 0; e < extrudedRegionCount; e++) {
                    Region region = extrudedRegions[e];
                    int j0 = (int)((region.rect2D.yMin + 0.5) * heightmapTextureHeight);
                    int j1 = (int)((region.rect2D.yMax + 0.5) * heightmapTextureHeight);
                    if (j1 >= heightmapTextureHeight) j1 = heightmapTextureHeight - 1;
                    int k0 = (int)((region.rect2D.xMin + 0.5) * heightmapTextureWidth);
                    int k1 = (int)((region.rect2D.xMax + 0.5) * heightmapTextureWidth);
                    if (k1 >= heightmapTextureWidth) k1 = heightmapTextureWidth - 1;
                    for (int j = j0; j <= j1; j++) {
                        int jj = j * heightmapTextureWidth;
                        p.y = (j + 0.5f) / heightmapTextureHeight - 0.5f;
                        for (int k = k0; k <= k1; k++) {
                            p.x = (k + 0.5f) / heightmapTextureWidth - 0.5f;
                            if (region.Contains(p)) {
                                viewportElevationPoints[jj + k] = region.extrusionAmount;
                            }
                        }
                    }
                }
            } else {
                for (int j = 0; j < heightmapTextureHeight; j++) {
                    int jj = j * heightmapTextureWidth;
                    int texjj = (j * _heightMapTexture.height / heightmapTextureHeight) * _heightMapTexture.width;
                    for (int k = 0; k < heightmapTextureWidth; k++) {
                        int pos = texjj + k * tw / heightmapTextureWidth;
                        float gCol = heightMapColors[pos].r / 255f - baseElevation;
                        if (gCol < 0)
                            gCol = 0;
                        viewportElevationPoints[jj + k] = gCol;
                    }
                }
            }

            // Remind to compute cell costs again
            cellsCostsComputed = false;

            // Create and assign a quad mesh
            MeshFilter mf = _renderViewport.GetComponent<MeshFilter>();
            Mesh mesh = mf.sharedMesh;
            if (mesh == null) {
                mesh = new Mesh();
                if (disposalManager != null)
                    disposalManager.MarkForDisposal(mesh);
            }
            mesh.Clear();
            mesh.vertices = new Vector3[] {
                new Vector2 (-0.5f, 0.5f),
                new Vector2 (0.5f, 0.5f),
                new Vector2 (0.5f, -0.5f),
                new Vector2 (-0.5f, -0.5f)
            };
            mesh.SetIndices(new int[] { 0, 1, 2, 3 }, MeshTopology.Quads, 0);
            mesh.uv = new Vector2[] {
                new Vector2 (0, 1),
                new Vector2 (1, 1),
                new Vector2 (1, 0),
                new Vector2 (0, 0)
            };
            mesh.RecalculateNormals();
            mf.sharedMesh = mesh;
        }

        /// <summary>
        /// Similar to EarthGetElevationInfo but feeds from Terrain heightmap itself
        /// </summary>
        void TerrainGetElevationData() {
            if (viewportElevationPoints == null || viewportElevationPoints.Length == 0) {
                viewportElevationPoints = new float[heightmapTextureWidth * heightmapTextureHeight];
            }

            if (terrain.terrainData == null) {
                Debug.LogError("Terrain does not have heightmap information (TerrainData is missing!). For a world heightmap, you can use the TerrainData from the demo scenes of WMSK.");
                return;
            }
            int sizeX = terrain.terrainData.heightmapResolution;
            int sizeY = terrain.terrainData.heightmapResolution;
            float[,] heights = terrain.terrainData.GetHeights(0, 0, sizeX, sizeY);

            for (int j = 0; j < heightmapTextureHeight; j++) {
                int jj = j * heightmapTextureWidth;
                int hj = j * sizeY / heightmapTextureHeight;
                for (int k = 0; k < heightmapTextureWidth; k++) {
                    int hk = k * sizeX / heightmapTextureWidth;
                    float gCol = heights[hj, hk];
                    viewportElevationPoints[jj + k] = gCol;
                }
            }
        }

        void EarthUpdateElevation() {
            try {
                EarthUpdateElevationInt();
            } catch { }
        }

        void EarthUpdateElevationInt() {

            // Curvature
            currentCurvature = Mathf.Lerp(_renderViewportCurvatureMinZoom, _renderViewportCurvature, lastKnownZoomLevel);

            // Compute MIP
            int visibleElevationColumns = Mathf.FloorToInt(_renderViewportRect.width * heightmapTextureWidth);
            if (visibleElevationColumns < 1)
                return;

            int mip = Mathf.CeilToInt(visibleElevationColumns / (float)EARTH_ELEVATION_STRIDE);
            int earthElevationHeight = heightmapTextureHeight;
            int earthElevationWidth = heightmapTextureWidth;

            int arrayLength;

            // Get window rect
            float dy = renderViewportClipHeight / (float)earthElevationHeight;
            float dx = renderViewportClipWidth / (float)earthElevationWidth;
            int rmin = int.MaxValue;
            int rmax = int.MinValue;
            for (int j = 0; j < earthElevationHeight; j++) {
                float j0 = renderViewportClip1.y + dy * j;
                float j1 = renderViewportClip1.y + dy * (j + 1.0f);
                if ((j0 >= 0f && j0 <= 1.0f) || (j1 >= 0f && j1 <= 1.0f) || (j0 < 0f && j1 > 1.0f)) {
                    if (j < rmin)
                        rmin = j;
                    if (j > rmax)
                        rmax = j;
                }
            }
            int cmin = int.MaxValue;
            int cmax = int.MinValue;
            int cols = _wrapHorizontally ? earthElevationWidth * 2 : earthElevationWidth;
            for (int k = 0; k < cols; k++) {
                float k0 = renderViewportClip0.x + dx * k;
                float k1 = renderViewportClip0.x + dx * (k + 1.0f);
                if ((k0 >= 0f && k0 <= 1.0f) || (k1 >= 0f && k1 <= 1.0f) || (k0 < 0f && k1 > 1.0f)) {
                    if (k < cmin)
                        cmin = k;
                    if (k > cmax)
                        cmax = k;
                }
            }

            if (cmin >= cols)
                cmin = 0;
            if (rmin >= earthElevationHeight)
                rmin = 0;
            if (cmax < 0)
                cmax = cols - 1;
            if (rmax < 0)
                rmax = earthElevationHeight - 1;
            if (rmax < earthElevationHeight - 1)
                rmax++;
            if (cmax < cols - 1)
                cmax++;

            int cmin0 = cmin;
            int rmin0 = rmin;
            do {
                rmin = (rmin0 / mip) * mip;
                cmin = (cmin0 / mip) * mip;
                rmax = Mathf.CeilToInt(rmax / (float)mip) * mip;
                cmax = Mathf.CeilToInt(cmax / (float)mip) * mip;

                arrayLength = 0;
                int rangeY = (rmax - rmin) / mip + 1;
                int rangeX = (cmax - cmin) / mip + 1;
                arrayLength = Mathf.Max(rangeY * rangeX, 0);
                if (arrayLength > 65000)
                    mip++;
            } while (arrayLength > 65000);

            // Compute surface vertices and uv
            _renderViewportScaleFactor = transform.localScale.y / (lastDistanceFromCamera + 1f);
            _renderViewportElevationFactor = _earthElevation * _renderViewportScaleFactor;

            int arrayIndex = -1;
            if (viewportUV == null || viewportUV.Length != arrayLength) {
                viewportUV = new Vector2[arrayLength];
                viewportElevationPointsAdjusted = new Vector3[arrayLength];
            }
            Vector2 uv;
            Vector3 v;
            int earthElevationWidthMinus1 = earthElevationWidth - mip;
            int earthElevationHeightMinus1 = earthElevationHeight - mip;

            if (curvatureOffsets == null || curvatureOffsets.Length <= cmax) {
                curvatureOffsets = new float[cmax + 1];
            }
            if (currentCurvature != 0) {
                for (int k = cmin; k <= cmax; k += mip) {
                    float kk0 = renderViewportClip0.x + dx * (float)k;
                    float k0;
                    if (kk0 <= 0) {
                        k0 = 0;
                    } else if (kk0 >= 1) {
                        k0 = 1;
                    } else {
                        k0 = kk0;
                    }
                    float x = k0 - 0.5f;
                    curvatureOffsets[k] = Mathf.Cos(x * Mathf.PI) * currentCurvature;
                }
            } else {
                for (int k = cmin; k <= cmax; k += mip) {
                    curvatureOffsets[k] = 0;
                }
            }

            for (int j = rmin; j <= rmax; j += mip) {
                float jj0 = renderViewportClip1.y + dy * (float)j;
                float j0;
                if (jj0 <= 0) {
                    j0 = 0;
                } else if (jj0 >= 1) {
                    j0 = 1;
                } else {
                    j0 = jj0;
                }
                uv.y = j0;
                v.y = j0 - 0.5f;

                int jj = earthElevationWidth;
                if (j < earthElevationHeightMinus1) {
                    jj *= j;
                } else {
                    jj *= earthElevationHeightMinus1;
                }

                for (int k = cmin; k <= cmax; k += mip) {
                    float kk0 = renderViewportClip0.x + dx * (float)k;
                    float k0;
                    if (kk0 <= 0) {
                        k0 = 0;
                    } else if (kk0 >= 1) {
                        k0 = 1;
                    } else {
                        k0 = kk0;
                    }

                    arrayIndex++;

                    // add uv mapping
                    uv.x = k0;
                    viewportUV[arrayIndex] = uv;

                    if (_renderViewportElevationFactor != 0) {
                        // add vertex location
                        int kw = (_wrapHorizontally && k >= earthElevationWidth) ? k - earthElevationWidth : k;
                        int pos = jj;
                        if (kw < earthElevationWidthMinus1) {
                            pos += kw;
                        } else {
                            pos += earthElevationWidthMinus1;
                        }
                        float elev = viewportElevationPoints[pos];
                        // as this pos get clamped at borders, interpolate with previous row or col
                        if (j == rmin && rmin < earthElevationHeightMinus1) {
                            float jj1 = renderViewportClip1.y + dy * (j + mip);
                            float t = (j0 - jj0) / (jj1 - jj0);
                            if (t > 0) {
                                float elev1 = viewportElevationPoints[pos + earthElevationWidth];
                                elev = t >= 1f ? elev1 : elev * (1f - t) + elev1 * t;
                            }
                        } else if (j == rmax && rmax > 0) {
                            float jj1 = renderViewportClip1.y + dy * (j - mip);
                            float t = (jj0 - j0) / (jj0 - jj1);
                            if (t > 0) {
                                float elev1 = viewportElevationPoints[pos - earthElevationWidth];
                                elev = t >= 1f ? elev1 : elev * (1f - t) + elev1 * t;
                            }
                            //} else if (j < rmax) {  // commented out to avoid artifacts on the edges
                            //    float elev1 = viewportElevationPoints[pos + earthElevationWidth];
                            //    elev = (elev + elev1) * 0.5f;
                        }

                        if (k == cmin && cmin < earthElevationWidthMinus1) {
                            float kk1 = renderViewportClip0.x + dx * (kw + mip);
                            float t = (k0 - kk0) / (kk1 - kk0);
                            if (t > 0) {
                                float elev1 = viewportElevationPoints[pos + 1];
                                elev = t >= 1f ? elev1 : elev * (1f - t) + elev1 * t;
                            }
                        } else if (k == cmax && cmax > 0 && pos > 0) {
                            float kk1 = renderViewportClip0.x + dx * (kw - mip);
                            float t = (kk0 - k0) / (kk0 - kk1);
                            if (t > 0) {
                                float elev1 = viewportElevationPoints[pos - 1];
                                elev = t >= 1f ? elev1 : elev * (1f - t) + elev1 * t;
                            }
                        //} else if (k < cmax) { // commented out to avoid artifacts on the edges
                        //    float elev1 = viewportElevationPoints[pos + 1];
                        //    elev = (elev + elev1) * 0.5f;
                        }
                        v.z = -elev * _renderViewportElevationFactor;
                    } else {
                        v.z = 0;
                    }
                    v.x = k0 - 0.5f;

                    v.z += curvatureOffsets[k];
                    viewportElevationPointsAdjusted[arrayIndex] = v;
                }
            }

            // Set surface geometry
            int h = (rmax - rmin) / mip;
            int w = (cmax - cmin) / mip;
            int row = w + 1;
            int bindex = 0;
            int viewportIndicesLength = w * h * 6;
            if (viewportIndices == null || viewportIndices.Length != viewportIndicesLength) {
                viewportIndices = new int[viewportIndicesLength];
            }
            for (int j = 0; j < h; j++) {
                int pos = j * row;
                int posEnd = pos + w;
                while (pos < posEnd) {
                    viewportIndices[bindex++] = pos + 1;
                    viewportIndices[bindex++] = pos;
                    viewportIndices[bindex++] = pos + row + 1;
                    viewportIndices[bindex++] = pos;
                    viewportIndices[bindex++] = pos + row;
                    viewportIndices[bindex++] = pos + row + 1;
                    pos++;
                }
            }

            // Create and assign mesh
            if (arrayLength > 0 && arrayLength <= 65000) {
                MeshFilter mf = _renderViewport.GetComponent<MeshFilter>();
                Mesh mesh = mf.sharedMesh;
                if (mesh == null) {
                    mesh = new Mesh();
                    if (disposalManager != null)
                        disposalManager.MarkForDisposal(mesh);
                }
                if (mesh.vertexCount != viewportElevationPointsAdjusted.Length) {
                    mesh.Clear();
                }
                mesh.vertices = viewportElevationPointsAdjusted;
                mesh.uv = viewportUV;
                mesh.SetIndices(viewportIndices, MeshTopology.Triangles, 0);
                mesh.RecalculateNormals();
                mf.sharedMesh = mesh;
            }
            viewportColliderNeedsUpdate = 5;
        }

        #endregion


        #region Render viewport setup

        void AssignRenderViewport(GameObject o) {
            terrain = null;
            viewportMat = null;

            if (o == null || o == gameObject) {
                viewportMode = ViewportMode.None;
                _renderViewport = gameObject;
                return;
            }

            terrain = o.GetComponent<Terrain>();

            // Is it a terrain?
            if (terrain != null) {
                viewportMode = ViewportMode.Terrain;
                _renderViewport = o;
                _renderViewportUIPanel = null;
                return;
            }

            RectTransform rt = o.GetComponent<RectTransform>();
            // Is it a Map Panel?
            if (o.GetComponent<MapPanel>() != null) {
                viewportMode = ViewportMode.MapPanel;
                _renderViewport = o;
                _renderViewportUIPanel = rt;
                return;
            }

            // Is it a regular panel used to sync viewport?
            if (rt != null) {
                _renderViewportUIPanel = rt;
            }
            if (_renderViewportUIPanel != null) {
                GameObject vp = GameObject.Find("Viewport");
                if (vp == null) {
                    vp = Instantiate(Resources.Load<GameObject>("WMSK/Prefabs/Viewport"));
                    vp.name = "Viewport";
                    transform.position += new Vector3(500, 500, -500); // keep normal map out of camera
                }
                o = vp;
            }

            // Assume it's the 3D viewport
            _renderViewport = o;
            viewportMode = ViewportMode.Viewport3D;
        }

        void DetachViewport() {
            viewportMode = ViewportMode.None;
            if (overlayRT != null) {
                if (_currentCamera != null && _currentCamera.targetTexture != null) {
                    _currentCamera.targetTexture = null;
                }
                RenderTexture.active = null;
                overlayRT.Release();
                DestroyImmediate(overlayRT);
                overlayRT = null;
            }
            if (overlayRTwrapped != null) {
                overlayRTwrapped.Release();
                DestroyImmediate(overlayRTwrapped);
                overlayRTwrapped = null;
            }
            _currentCamera = cameraMain; // Camera main;
            if (_currentCamera == null) {
                Debug.LogWarning("Camera main not found. Ensure you have a camera in the scene tagged as MainCamera.");
            }
            if (overlayLayer != null) {
                DestroyMapperCam();
            }
            if (_renderViewport != gameObject) {
                AssignRenderViewport(gameObject);
                CenterMap();
            }
        }


        void SetupViewport() {

            if (extrudedRegions == null) {
                extrudedRegions = new List<Region>();
            }

            if (!gameObject.activeInHierarchy) {
                return;
            }

            // Check correct window rect
            if (Application.isPlaying && (_windowRect.width == 0 || _windowRect.height == 0)) {
                _windowRect = new Rect(-0.5f, -0.5f, 1, 1);
            }

            // Assigns / updates viewport object
            AssignRenderViewport(_renderViewport);

            if (viewportMode == ViewportMode.None) {
                DetachViewport();
                return;
            }

            // Setup additional cameras and render texture
            int imageWidth, imageHeight;
            imageWidth = Camera.main.pixelWidth;
            if (imageWidth < 1024)
                imageWidth = 1024;
            imageWidth = (int)(imageWidth * _renderViewportResolution);
            imageWidth = (imageWidth / 2) * 2;
            _renderViewportResolutionMaxRTWidth = Mathf.Clamp(_renderViewportResolutionMaxRTWidth, 1024, 8192);
            if (imageWidth > _renderViewportResolutionMaxRTWidth) {
                imageWidth = _renderViewportResolutionMaxRTWidth;
            }
            imageHeight = imageWidth / 2;

            FilterMode filterMode = _renderViewportFilterMode;
            if (filterMode == FilterMode.Trilinear && _wrapHorizontally) {
                filterMode = FilterMode.Bilinear;
            }

            if (overlayRT != null && (overlayRT.width != imageWidth || overlayRT.height != imageHeight || overlayRT.filterMode != filterMode)) {
                if (_currentCamera != null && _currentCamera.targetTexture != null) {
                    _currentCamera.targetTexture = null;
                }
                RenderTexture.active = null;
                overlayRT.Release();
                DestroyImmediate(overlayRT);
                overlayRT = null;
                if (overlayRTwrapped != null) {
                    if (_wrapCamera != null && _wrapCamera.targetTexture == overlayRTwrapped) {
                        _wrapCamera.targetTexture = null;
                    }
                    overlayRTwrapped.Release();
                    DestroyImmediate(overlayRTwrapped);
                    overlayRTwrapped = null;
                }
            }

            GameObject overlayLayer = GetOverlayLayer(true);
            if (overlayRT == null) {
                overlayRT = new RenderTexture(imageWidth, imageHeight, 24, RenderTextureFormat.ARGB32);
                //overlayRT.hideFlags = HideFlags.DontSave;	// don't add to the disposal manager
                overlayRT.filterMode = filterMode; // FilterMode.Trilinear; -> trilinear causes blurry issues with NGUI
                overlayRT.anisoLevel = 0;
                overlayRT.useMipMap = filterMode == FilterMode.Trilinear;
            }

            // Camera
            GameObject camObj = GameObject.Find(MAPPER_CAM);
            if (camObj == null) {
                camObj = new GameObject(MAPPER_CAM, typeof(Camera));
            }
            camObj.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            camObj.layer = overlayLayer.layer;
            mapperCam = camObj.GetComponent<Camera>();
            float aspect = 1f;
            switch (viewportMode) {
                case ViewportMode.MapPanel: {
                        Rect wsRect = GetWorldRect(_renderViewportUIPanel.GetComponent<RectTransform>());
                        if (wsRect.size.x != 0 && wsRect.size.y != 0) {
                            aspect = wsRect.size.x / wsRect.size.y;
                        }
                    }
                    break;
                default: {
                        Vector3 rvScale = _renderViewport.transform.localScale;
                        if (rvScale.x != 0 && rvScale.y != 0) {
                            aspect = rvScale.x / rvScale.y;
                        }
                    }
                    break;
            }
            mapperCam.aspect = aspect;
            mapperCam.cullingMask = 1 << camObj.layer;
            mapperCam.clearFlags = CameraClearFlags.SolidColor;
            mapperCam.backgroundColor = Misc.ColorClear;
            mapperCam.targetTexture = overlayRT;
            mapperCam.nearClipPlane = renderViewPortIsTerrain ? 0.3f : 0.01f;
            mapperCam.farClipPlane = Mathf.Min(cameraMain.farClipPlane, 1000);
            mapperCam.renderingPath = _renderViewportRenderingPath;
            mapperCam.enabled = true;

            if (_wrapHorizontally) {
                mapperCam.allowMSAA = false;
            }

            if (_currentCamera != mapperCam) {
                _currentCamera = mapperCam;
                CenterMap();
            }

            // Wrapper camera
            _wrapCameraObj = GameObject.Find(MAPPER_CAM_WRAP);
            if (_wrapCameraObj == null) {
                _wrapCameraObj = Instantiate(camObj);
                _wrapCameraObj.hideFlags = HideFlags.HideInHierarchy;
                if (disposalManager != null) disposalManager.MarkForDisposal(_wrapCameraObj);
                _wrapCameraObj.layer = overlayLayer.layer;
                _wrapCameraObj.name = MAPPER_CAM_WRAP;
            }
            if (_wrapCamera == null) {
                _wrapCamera = _wrapCameraObj.GetComponent<Camera>();
                _wrapCamera.tag = "Untagged";
                _wrapCamera.aspect = mapperCam.aspect;
                _wrapCamera.cullingMask = 1 << camObj.layer;
                _wrapCamera.clearFlags = CameraClearFlags.SolidColor;
                _wrapCamera.backgroundColor = Misc.ColorClear;
                _wrapCamera.nearClipPlane = renderViewPortIsTerrain ? 0.3f : 0.01f;
                _wrapCamera.farClipPlane = Mathf.Min(cameraMain.farClipPlane, 1000);
                _wrapCamera.renderingPath = _renderViewportRenderingPath;
            }

            // Specific support depending on viewport type
            switch (viewportMode) {
                case ViewportMode.Terrain: {
                        // Additionals setup steps for Terrain support
                        if (terrainMat == null) {
                            terrainMat = Instantiate(Resources.Load<Material>("WMSK/Materials/Terrain"));
                        }
                        SRP.ConfigureTerrainShader(terrainMat);
                        if (disposalManager != null)
                            disposalManager.MarkForDisposal(terrainMat);
#if !UNITY_2019_3_OR_NEWER
                        terrain.materialType = Terrain.MaterialType.Custom;
#endif
                        terrain.materialTemplate = terrainMat;
                        Shader.SetGlobalTexture("_WMSK_Overlay", overlayRT);
                    }
                    break;

                case ViewportMode.MapPanel: {
                        CheckViewportScaleAndCurvature();
                        MapPanel mapPanel = _renderViewport.GetComponent<MapPanel>();
                        if (mapPanel != null) {
                            viewportMat = mapPanel.material;
                            if (viewportMat != null) {
                                Shader shader = _wrapHorizontally ? Shader.Find("WMSK/UI Viewport Wrapped") : Shader.Find("WMSK/UI Viewport");
                                viewportMat.shader = shader;
                                viewportMat.mainTexture = overlayRT;
                                mapPanel.SetMaterialDirty();
                            }
                        }
                    }
                    break;

                default: {
                        SetupViewportUIPanel();
                        CheckViewportScaleAndCurvature();

                        // Setup viewport material and shader
                        Renderer viewportRenderer = _renderViewport.GetComponent<Renderer>();
                        if (viewportRenderer != null) {
                            viewportMat = viewportRenderer.sharedMaterial;
                            if (viewportMat != null) {
                                viewportMat.shader = _wrapHorizontally ? Shader.Find("WMSK/Lit Viewport Wrapped") : Shader.Find("WMSK/Lit Viewport");
                                SRP.Configure(viewportMat);
                                viewportMat.mainTexture = overlayRT;
                                if (_renderViewportLightingMode == VIEWPORT_LIGHTING_MODE.Unlit) {
                                    viewportMat.EnableKeyword("WMSK_VIEWPORT_UNLIT");
                                } else {
                                    viewportMat.DisableKeyword("WMSK_VIEWPORT_UNLIT");
                                }
                            }
                            PointerTrigger pt = _renderViewport.GetComponent<PointerTrigger>() ?? _renderViewport.AddComponent<PointerTrigger>();
                            pt.map = this;
                        }

                    }
                    break;
            }

            if (_wrapHorizontally) {
                if (overlayRTwrapped == null) {
                    overlayRTwrapped = new RenderTexture(imageWidth, imageHeight, 24, RenderTextureFormat.ARGB32);
                    //overlayRTwrapped.hideFlags = HideFlags.DontSave;   // don't add to the disposal manager
                    overlayRTwrapped.filterMode = filterMode; // FilterMode.Trilinear; -> trilinear causes blurry issues with NGUI
                    overlayRTwrapped.anisoLevel = 0;
                    overlayRTwrapped.useMipMap = filterMode == FilterMode.Trilinear;
                }
                _wrapCamera.targetTexture = overlayRTwrapped;
                viewportMat.SetTexture("_WrappedTex", overlayRTwrapped);
                UpdateWrapCam();
            } else {
                ToggleWrapCamera(false);
            }

            // Setup 3d surface, cloud and other visual effects
            UpdateViewport();

            // Shot!
            mapperCam.Render();
        }


        void DestroyMapperCam() {
            if (isMiniMap) {
                return;
            }
            mapperCam = null;

            GameObject o = GameObject.Find(MAPPER_CAM);
            if (o != null) {
                DestroyImmediate(o);
            }
            o = GameObject.Find(MAPPER_CAM_WRAP);
            if (o != null) {
                DestroyImmediate(o);
            }
        }


        /// <summary>
        /// Ensure the proportions of the main map fit the aspect ratio of the render viewport
        /// </summary>
        void CheckViewportScaleAndCurvature() {
            if (viewportMode == ViewportMode.None || renderViewPortIsTerrain)
                return;

            Vector3 scale = new Vector3(transform.localScale.y * 2f, transform.localScale.y, 1f);
            if (transform.localScale != scale) {
                if (scale.x != 0 && scale.y != 0) {
                    transform.localScale = scale;
                }
            }
        }

        void SyncMapperCamWithMainCamera() {

            if (mapperCam != null) {
                Quaternion camRot = cameraMain.transform.rotation;
                Vector3 camPos = cameraMain.transform.position;
                bool cameraHasMoved = camPos != lastMainCameraPos || camRot != lastMainCameraRot || !Application.isPlaying;
                if (cameraHasMoved) {
                    lastMainCameraPos = camPos;
                    lastMainCameraRot = camRot;
                    if (terrain.terrainData == null)
                        return;
                    float sx = terrain.terrainData.size.x * 0.5f;
                    float sz = terrain.terrainData.size.z * 0.5f;
                    transform.position = terrain.transform.position + new Vector3(sx, WMSK_TERRAIN_MODE_Y_OFFSET, sz);
                    transform.rotation = Misc.QuaternionX90;
                    transform.localScale = new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.z, 1f);
                    Vector3 center = new Vector3(sx, 0, sz);
                    Vector4 data = transform.position - center;
                    data.w = _renderViewportTerrainAlpha;
                    Shader.SetGlobalVector(ShaderIds.TerrainWMSKData, data);
                    Vector3 deltaPos = terrain.transform.position + center - lastMainCameraPos;
                    _currentCamera.transform.position = transform.position - deltaPos;
                    _currentCamera.transform.rotation = lastMainCameraRot;

                    UpdateViewportObjectsVisibility();
                }
            }
        }

        void SyncMainCameraWithMapperCam() {

            if (mapperCam != null) {
                Shader.SetGlobalMatrix(ShaderIds.TerrainWMSKClip, _currentCamera.projectionMatrix * _currentCamera.worldToCameraMatrix);
            }

            Transform t = _currentCamera.transform;
            cameraMain.transform.rotation = t.rotation;
            cameraMain.transform.position = t.position + new Vector3(0, -WMSK_TERRAIN_MODE_Y_OFFSET, 0);
        }

        #endregion


        #region Wrap camera setup

        void UpdateWrapCam() {
            if (_wrapCameraObj == null || !renderViewportIsEnabled)
                return;

            // Reduce floating-point errors
            Vector3 apos = transform.position;
            transform.position -= apos;
            _currentCamera.transform.position -= apos;

            // Get clip bounds
            Vector3 v0 = _currentCamera.WorldToViewportPoint(transform.TransformPoint(Misc.Vector3left * 0.5f));
            Vector3 v1 = _currentCamera.WorldToViewportPoint(transform.TransformPoint(Misc.Vector3right * 0.5f));

            float x0 = v0.x;
            float x1 = v1.x;
            if ((x0 < 0 && x1 > 1) || (x0 >= 0 && x1 <= 1)) {
                // disable wrap cam as current camera is not over the edges or the zoom is too far
                ToggleWrapCamera(false);
                transform.position += apos;
                _currentCamera.transform.position += apos;
                return;
            }

            if (x0 > 1) {
                // shifts current camera to the other side of the map
                Vector3 v = new Vector3(x1 - x0 + 0.5f, 0.5f, v0.z);
                _currentCamera.transform.position = _currentCamera.ViewportToWorldPoint(v);
                _currentCamera.transform.position -= _currentCamera.transform.forward * lastDistanceFromCamera;
            } else if (x1 < 0) {
                // shifts current camera to the other side of the map
                Vector3 v = new Vector3(x0 - x1 + 0.5f, 0.5f, v0.z);
                _currentCamera.transform.position = _currentCamera.ViewportToWorldPoint(v);
                _currentCamera.transform.position -= _currentCamera.transform.forward * lastDistanceFromCamera;
            }

            if (x0 > 0) {
                // wrap on the left
                Vector3 v = new Vector3(x1 - x0 + 0.499f, 0.5f, v0.z);
                _wrapCameraObj.transform.position = _currentCamera.ViewportToWorldPoint(v);
            } else if (x1 < 1) {
                // wrap on the right
                Vector3 v = new Vector3(x0 - x1 + 0.501f, 0.5f, v0.z);
                _wrapCameraObj.transform.position = _currentCamera.ViewportToWorldPoint(v);
            }

            _wrapCameraObj.transform.rotation = _currentCamera.transform.rotation;
            _wrapCameraObj.transform.position -= _currentCamera.transform.forward * lastDistanceFromCamera;

            // Restore positions
            transform.position += apos;
            _currentCamera.transform.position += apos;
            _wrapCameraObj.transform.position += apos;

            if (!_wrapCamera.enabled) {
                ToggleWrapCamera(true);
            }
        }

        void ToggleWrapCamera(bool enabled) {
            if (_wrapCamera != null) {
                _wrapCamera.enabled = enabled;
            }
            if (viewportMat != null) {
                viewportMat.SetFloat("_WrapEnabled", enabled ? 1f : 0f);
            }
        }
        #endregion

        #region Viewport FX

        void UpdateCloudLayer() {
            if (renderViewPortIsTerrain || _renderViewport == null || _renderViewport == gameObject)
                return;

            Transform t = _renderViewport.transform.Find("CloudLayer1");
            if (t == null) {
                Debug.Log("Cloud layer not found under Viewport gameobject. Remove it and create it again from prefab.");
                return;
            }
            Renderer renderer = t.GetComponent<MeshRenderer>();
            renderer.enabled = _earthCloudLayer;

            if (lastDistanceFromCamera <= 0)
                return;

            // Compute cloud layer position and texture scale and offset
            Vector3 clip0 = _currentCamera.WorldToViewportPoint(transform.TransformPoint(-0.5f, 0.5f, 0));
            Vector3 clip1 = _currentCamera.WorldToViewportPoint(transform.TransformPoint(0.5f, -0.5f, 0));

            float dx = clip1.x - clip0.x;
            float scaleX = 1.0f / dx;
            float offsetX = -clip0.x / dx;
            float dy = clip0.y - clip1.y;
            float scaleY = 1.0f / dy;
            float offsetY = -clip0.y / dy;

            t.transform.localPosition = new Vector3(0, 0, _earthCloudLayerElevation * (_renderViewportElevationFactor + 0.01f));
            Material cloudMat = renderer.sharedMaterial;
            SRP.Configure(cloudMat);
            Vector2 scale = new Vector2(scaleX, scaleY);
            cloudMat.mainTextureScale = scale;
            cloudMat.SetVector("_TextureScale", scale); // for LWRP
            float brightness = Mathf.Clamp01((lastDistanceFromCamera + t.transform.localPosition.z - 5f) / 5f);
            renderer.enabled = _earthCloudLayer && brightness > 0f; // optimization: hide cloud layer entirely if it's 100% transparent
            cloudMat.SetFloat("_Brightness", brightness * _earthCloudLayerAlpha);
            earthMat.SetFloat("_CloudShadowStrength", _earthCloudLayer ? _earthCloudLayerShadowStrength * _earthCloudLayerAlpha : 0f);
            CloudLayerAnimator cla = t.GetComponent<CloudLayerAnimator>();
            cla.earthMat = earthMat;
            cla.cloudMainTextureOffset = new Vector2(offsetX, offsetY);
            cla.speed = _earthCloudLayerSpeed;
            cla.Update();

            UpdateCurvature(t, renderer.sharedMaterial);
        }


        void UpdateFogOfWarLayer() {
            if (renderViewPortIsTerrain || _renderViewport == null || _renderViewport == gameObject)
                return;

            Transform t = _renderViewport.transform.Find("FogOfWarLayer");
            if (t == null) {
                Debug.Log("Fog of War layer not found under Viewport gameobject. Remove it and create it again from prefab.");
                return;
            }
            Renderer renderer = t.GetComponent<MeshRenderer>();
            renderer.enabled = _fogOfWarLayer;

            if (lastDistanceFromCamera <= 0)
                return;

            // Compute fog layer position and texture scale and offset
            float elevationFactor = _earthElevation * 100.0f / lastDistanceFromCamera;
            float absElevation = Mathf.Abs(_fogOfWarLayerElevation);
            t.transform.localPosition = new Vector3(0, 0, _earthCloudLayerElevation * absElevation * elevationFactor * 0.99f); // make it behind clouds
            t.transform.localScale = new Vector3(1f + 0.05f * absElevation, 1f + 0.05f * absElevation, 1f);
            if (fogOfWarMat == null) {
                fogOfWarMat = Instantiate(Resources.Load<Material>("WMSK/Materials/FogOfWar"));
                if (disposalManager != null)
                    disposalManager.MarkForDisposal(fogOfWarMat);
            }
            renderer.sharedMaterial = fogOfWarMat;
            fogOfWarMat.mainTextureScale = new Vector2(renderViewportScaleX, renderViewportScaleY);
            fogOfWarMat.mainTextureOffset = new Vector2(renderViewportOffsetX, renderViewportOffsetY);
            fogOfWarMat.SetColor("_EmissionColor", _fogOfWarColor);

            UpdateCurvature(t, renderer.sharedMaterial);
        }


        void UpdateSun() {
            if (!_sunUseTimeOfDay || _sun == null)
                return;
            _sun.transform.rotation = _renderViewport.transform.rotation;
            _sun.transform.Rotate(Vector3.up, 180f + _timeOfDay * 360f / 24f, Space.Self);
        }

        void UpdateCurvature(Transform layer, Material mat) {
            if (layer == null)
                return;

#if UNITY_EDITOR
#if UNITY_2018_3_OR_NEWER
            UnityEditor.PrefabInstanceStatus prefabInstanceStatus = UnityEditor.PrefabUtility.GetPrefabInstanceStatus(_renderViewport);
            if (prefabInstanceStatus != UnityEditor.PrefabInstanceStatus.NotAPrefab) {
                UnityEditor.PrefabUtility.UnpackPrefabInstance(_renderViewport, UnityEditor.PrefabUnpackMode.Completely, UnityEditor.InteractionMode.AutomatedAction);
            }
#else
			UnityEditor.PrefabType prefabType = UnityEditor.PrefabUtility.GetPrefabType (_renderViewport);
			if (prefabType != UnityEditor.PrefabType.None && prefabType != UnityEditor.PrefabType.DisconnectedPrefabInstance && prefabType != UnityEditor.PrefabType.DisconnectedModelPrefabInstance) {
				UnityEditor.PrefabUtility.DisconnectPrefabInstance (_renderViewport);
			}
#endif
#endif

            MeshFilter mf = layer.GetComponent<MeshFilter>();
            if (mf == null)
                return;
            if (currentCurvature == 0) {
                // Disable
                if (quadPrefab == null) {
                    quadPrefab = Instantiate(Resources.Load<GameObject>("WMSK/Prefabs/Quad").GetComponent<MeshFilter>().sharedMesh);
                }
                flexQuad = quadPrefab;
            } else {
                // Enable
                if (flexQuadPrefab == null) {
                    flexQuadPrefab = Resources.Load<Mesh>("WMSK/Meshes/PlaneMesh");
                }
                flexQuad = flexQuadPrefab;
                if (flexQuadCurvature != currentCurvature) {
                    // Updates flex quad z-positions
                    Vector3[] vertices = flexQuad.vertices;
                    for (int k = 0; k < vertices.Length; k++) {
                        vertices[k].z = Mathf.Cos(vertices[k].x * 3.1415927f) * currentCurvature;
                    }
                    flexQuad.vertices = vertices;
                    mf.sharedMesh = null;
                    flexQuadCurvature = currentCurvature;
                }
            }
            if (mf.sharedMesh == null || mf.sharedMesh != flexQuad) {
                mf.mesh = flexQuad;
            }
        }

        #endregion


        #region internal viewport API

        void UpdateViewport() {

            if (renderViewPortIsTerrain) {
                if (earthLastElevation < 0) {
                    earthLastElevation = 1f;
                    TerrainGetElevationData();
                }
                return;
            }

            // Update wrapping
            if (_wrapHorizontally) {
                UpdateWrapCam();
            }

            if (viewportMode == ViewportMode.MapPanel) {
                return;
            }

            // Calculates viewport rect
            ComputeViewportRect();

            // Generates 3D surface
            EarthBuildMesh();

            // Updates cloud layer
            UpdateCloudLayer();

            // Update fog layer
            UpdateFogOfWarLayer();
        }


        /// <summary>
        /// Updates renderViewportRect field
        /// </summary>
        void ComputeViewportRect(bool useSceneViewWindow = false) {
            if (!useSceneViewWindow && lastRenderViewportGood && Application.isPlaying)
                return;

            lastRenderViewportGood = true;


#if UNITY_EDITOR
            Vector3 oldPos = _currentCamera.transform.position;
            Quaternion oldRot = _currentCamera.transform.rotation;
            float oldFoV = _currentCamera.fieldOfView;
            if (useSceneViewWindow && UnityEditor.SceneView.lastActiveSceneView != null) {
                Camera sceneCam = UnityEditor.SceneView.lastActiveSceneView.camera;
                if (sceneCam != null) {
                    oldPos = _currentCamera.transform.position;
                    oldRot = _currentCamera.transform.rotation;
                    _currentCamera.transform.position = sceneCam.transform.position;
                    _currentCamera.transform.rotation = sceneCam.transform.rotation;
                    _currentCamera.fieldOfView = sceneCam.fieldOfView;
                }
            }
#endif

            // Get clip rect
            if (!_enableFreeCamera) {
                _currentCamera.transform.forward = transform.forward;
            }
            Vector3 topLeft = transform.TransformPoint(-0.5f, 0.5f, 0);
            renderViewportClip0 = _currentCamera.WorldToViewportPoint(topLeft);
            Vector3 bottomRight = transform.TransformPoint(0.5f, -0.5f, 0);
            renderViewportClip1 = _currentCamera.WorldToViewportPoint(bottomRight);
            renderViewportClipWidth = renderViewportClip1.x - renderViewportClip0.x;
            renderViewportClipHeight = renderViewportClip0.y - renderViewportClip1.y;

            // Computes and saves current viewport scale, offset and rect
            renderViewportScaleX = 1.0f / renderViewportClipWidth;
            renderViewportOffsetX = -renderViewportClip0.x / renderViewportClipWidth;
            renderViewportScaleY = 1.0f / renderViewportClipHeight;
            renderViewportOffsetY = -renderViewportClip0.y / renderViewportClipHeight;
            _renderViewportRect = new Rect(renderViewportOffsetX - 0.5f, renderViewportOffsetY + 0.5f, renderViewportScaleX, renderViewportScaleY);

            if (_wrapHorizontally && renderViewportClip0.x > 0) {   // need to offset clip0x and clip1x to extract correct heights later
                renderViewportClip0.x -= renderViewportClipWidth;
                renderViewportClip1.x = renderViewportClip0.x + renderViewportClipWidth;
            }
#if UNITY_EDITOR
            _currentCamera.transform.position = oldPos;
            _currentCamera.transform.rotation = oldRot;
            _currentCamera.fieldOfView = oldFoV;
#endif
        }

        #endregion

        #region UI Fitter


        Vector3[] wc;
        Vector3 panelUIOldPosition;
        Vector2 panelUIOldSize;


        void FitViewportToUIPanel() {

            if (viewportMode != ViewportMode.Viewport3D || _renderViewportUIPanel == null)
                return;

            if (Application.isPlaying && panelUIOldPosition == _renderViewportUIPanel.position && panelUIOldSize == _renderViewportUIPanel.sizeDelta) {
                return;
            }

            // Check if positions are different
            Rect rect = GetWorldRect(_renderViewportUIPanel);
            Camera cam = cameraMain;
            float zDistance = cam.farClipPlane - 10f;
            Vector3 bl = new Vector3(rect.xMin, rect.yMax, zDistance);
            Vector3 tr = new Vector3(rect.xMax, rect.yMin, zDistance);
            Vector3 br = new Vector3(rect.xMax, rect.yMax, zDistance);
            bl = cam.ScreenToWorldPoint(bl);
            br = cam.ScreenToWorldPoint(br);
            tr = cam.ScreenToWorldPoint(tr);

            Transform t = _renderViewport.transform;

            Vector3 pos = (bl + tr) * 0.5f;
            float width = Vector3.Distance(bl, br);
            float height = Vector3.Distance(br, tr);

            t.position = pos;
            t.localScale = new Vector3(width, height, 1f);
            t.forward = cam.transform.forward;

            if (!flyToActive && panelUIOldSize.x == 0) {
                CenterMap();
            }

#if UNITY_EDITOR
            if (!Application.isPlaying && panelUIOldSize != _renderViewportUIPanel.sizeDelta) {
                SetupViewport();
            }
#endif

            panelUIOldPosition = _renderViewportUIPanel.position;
            panelUIOldSize = _renderViewportUIPanel.sizeDelta;

        }

        Rect GetWorldRect(RectTransform rt) {
            if (rt == null) return Rect.zero;
            if (wc == null || wc.Length < 4) wc = new Vector3[4];
            rt.GetWorldCorners(wc);
            return new Rect(wc[0].x, wc[0].y, wc[2].x - wc[0].x, wc[2].y - wc[0].y);
        }

        void SetupViewportUIPanel() {
            ToggleUIPanel(false);
        }

        void ToggleUIPanel(bool visible) {
            if (_renderViewportUIPanel == null)
                return;
            Image img = _renderViewportUIPanel.GetComponent<Image>();
            if (img != null) {
                img.enabled = visible;
            }
        }

        #endregion

    }
}
