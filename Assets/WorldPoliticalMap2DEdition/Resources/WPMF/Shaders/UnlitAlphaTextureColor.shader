// Used by cities material
Shader "World Political Map 2D/Unlit Alpha Texture Color" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white"
    }

   	SubShader {
       Tags {
       	"Queue"="Transparent+6" // Draw over highlight
       }
       Lighting On
       ZWrite Off
       Blend SrcAlpha OneMinusSrcAlpha
       Offset -1, -1
       Material {
              Emission [_Color]
       }
       Pass {
          SetTexture [_MainTex] {
            Combine Texture * Primary, Texture * Primary
          }
       }
   } 
    
}