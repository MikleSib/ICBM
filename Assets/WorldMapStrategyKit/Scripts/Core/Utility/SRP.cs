using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WorldMapStrategyKit {

    public static class SRP {

        static bool isLWRP {
            get {
#if UNITY_2019_1_OR_NEWER
                if (GraphicsSettings.renderPipelineAsset == null) return false;
                string pipe = GraphicsSettings.renderPipelineAsset.name;
                return pipe.Contains("LWRP") || pipe.Contains("Lightweight") || pipe.Contains("Universal") || pipe.Contains("URP");
#else
                return false;
				#endif
            }
        }

        public static void Configure(Material mat) {

            if (mat == null || mat.shader == null) return;

            string n = mat.shader.name;
            int mat_LWRP_PrefixIndex = n.IndexOf("LWRP");
            bool LWRP = isLWRP;
            if (mat_LWRP_PrefixIndex < 0 && LWRP) {
                int i = n.LastIndexOf('/');
                if (i >= 0) {
                    n = n.Substring(i + 1);
                }
                string sn = "Shader Graphs/LWRP " + n;
                Shader comp = Shader.Find(sn);
                if (comp != null) {
                    mat.shader = comp;
                }
            } else if (mat_LWRP_PrefixIndex >= 0 && !LWRP) {
                string sn = "WMSK/" + n.Substring(mat_LWRP_PrefixIndex + 5);
                Shader comp = Shader.Find(sn);
                if (comp != null) {
                    mat.shader = comp;
                }
            }
        }

        public static void ConfigureTerrainShader(Material mat) {

            if (mat == null || mat.shader == null) return;

            string n = mat.shader.name;
            int mat_LWRP_PrefixIndex = n.IndexOf("URP");
            bool LWRP = isLWRP;
            if (mat_LWRP_PrefixIndex < 0 && LWRP) {
                int i = n.LastIndexOf('/');
                if (i >= 0) {
                    n = n.Substring(i + 1);
                }
                string sn = "WMSK/URP/" + n;
                Shader comp = Shader.Find(sn);
                if (comp != null) {
                    mat.shader = comp;
                } else {
                    Debug.LogError("World Map Strategy Kit: URP compatible terrain shader not found. Please import the URP terrain shaders package from WMSK/Resources/WMSK/Shaders/LWRP/TerrainShaders folder.");
                }
            } else if (mat_LWRP_PrefixIndex >= 0 && !LWRP) {
                string sn = "WMSK" + n.Substring(mat_LWRP_PrefixIndex + 3);
                Shader comp = Shader.Find(sn);
                if (comp != null) {
                    mat.shader = comp;
                }
            }
        }

    }
}