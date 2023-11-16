using UnityEngine;

namespace WorldMapStrategyKit {

    public partial class WMSK : MonoBehaviour {

        static partial class ShaderIds {
            public static int OutlineWidth = Shader.PropertyToID("_OutlineWidth");
            public static int OutlineColor = Shader.PropertyToID("_OutlineColor");
            public static int TerrainWMSKData = Shader.PropertyToID("_WMSK_Data");
            public static int TerrainWMSKClip = Shader.PropertyToID("_WMSK_Clip");
            public static int SunDirection = Shader.PropertyToID("_SunLightDirection");
            public static int Thickness = Shader.PropertyToID("_Thickness");
            public static int MaxPixelWidth = Shader.PropertyToID("_MaxPixelWidth");
            public static int OuterColor = Shader.PropertyToID("_OuterColor");

            public const string SKW_OUTLINE = "OUTLINE_ON";
        }
    }

}
