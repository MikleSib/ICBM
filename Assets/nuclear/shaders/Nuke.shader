// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.36 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.36;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:3,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:1,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:1,fgcg:0.6798174,fgcb:0.4338235,fgca:1,fgde:0.0005,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:34933,y:32828,varname:node_3138,prsc:2|emission-457-OUT,alpha-5814-OUT,disp-1130-OUT,tess-7739-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:34085,y:32558,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Tex2d,id:5519,x:32275,y:32763,ptovrint:False,ptlb:Emmis,ptin:_Emmis,varname:_Emmis,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:1382e9beefab1644e8de31d5ca2244ef,ntxv:0,isnm:False|UVIN-3673-UVOUT;n:type:ShaderForge.SFN_Tex2d,id:7453,x:32159,y:33051,varname:_Height,prsc:2,tex:e4518fbdd2e3af140b118e35d7bab35b,ntxv:0,isnm:False|UVIN-3673-UVOUT,MIP-6983-OUT,TEX-620-TEX;n:type:ShaderForge.SFN_NormalVector,id:5532,x:32850,y:32590,prsc:2,pt:False;n:type:ShaderForge.SFN_Slider,id:7739,x:34087,y:33517,ptovrint:False,ptlb:tesselation_factor,ptin:_tesselation_factor,varname:_tesselation_factor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:6.564103,max:32;n:type:ShaderForge.SFN_Multiply,id:457,x:34370,y:32866,varname:node_457,prsc:2|A-7241-RGB,B-2901-OUT;n:type:ShaderForge.SFN_Slider,id:326,x:31064,y:33007,ptovrint:False,ptlb:Speed,ptin:_Speed,varname:_Speed,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-10,cur:-0.5,max:10;n:type:ShaderForge.SFN_Panner,id:3673,x:31996,y:32661,varname:node_3673,prsc:2,spu:0,spv:1|UVIN-7413-OUT,DIST-2154-OUT;n:type:ShaderForge.SFN_TexCoord,id:8004,x:31441,y:32259,varname:node_8004,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Time,id:8028,x:31107,y:32769,varname:node_8028,prsc:2;n:type:ShaderForge.SFN_Multiply,id:2154,x:31455,y:32850,varname:node_2154,prsc:2|A-8028-TSL,B-326-OUT;n:type:ShaderForge.SFN_Append,id:7413,x:31601,y:32612,varname:node_7413,prsc:2|A-7619-OUT,B-5019-OUT;n:type:ShaderForge.SFN_Multiply,id:7619,x:32121,y:32205,varname:node_7619,prsc:2|A-8004-U,B-6662-OUT;n:type:ShaderForge.SFN_Multiply,id:5019,x:32145,y:32352,varname:node_5019,prsc:2|A-8004-V,B-7471-OUT;n:type:ShaderForge.SFN_Slider,id:6662,x:31816,y:32352,ptovrint:False,ptlb:U,ptin:_U,varname:_U,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-16,cur:2.225862,max:16;n:type:ShaderForge.SFN_Slider,id:7471,x:31816,y:32502,ptovrint:False,ptlb:V,ptin:_V,varname:_V,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-16,cur:1.556958,max:16;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:4416,x:32867,y:32784,varname:node_4416,prsc:2|IN-9257-OUT,IMIN-9888-OUT,IMAX-7468-OUT,OMIN-2080-OUT,OMAX-2863-OUT;n:type:ShaderForge.SFN_Vector1,id:9888,x:32441,y:32886,varname:node_9888,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:7468,x:32601,y:33013,varname:node_7468,prsc:2,v1:1;n:type:ShaderForge.SFN_Slider,id:2080,x:32578,y:33096,ptovrint:False,ptlb:Displace_min,ptin:_Displace_min,varname:_Displace_min,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-2,cur:-0.2169179,max:2;n:type:ShaderForge.SFN_Slider,id:2863,x:32847,y:33073,ptovrint:False,ptlb:Displace_max,ptin:_Displace_max,varname:_Displace_max,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-2,cur:0.05286796,max:2;n:type:ShaderForge.SFN_Multiply,id:1130,x:33209,y:32717,varname:node_1130,prsc:2|A-5532-OUT,B-4416-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:620,x:31932,y:33469,ptovrint:False,ptlb:Height_map,ptin:_Height_map,varname:_Height_map,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:e4518fbdd2e3af140b118e35d7bab35b,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:1646,x:32159,y:33237,varname:_node_1646,prsc:2,tex:e4518fbdd2e3af140b118e35d7bab35b,ntxv:0,isnm:False|UVIN-8058-OUT,MIP-6983-OUT,TEX-620-TEX;n:type:ShaderForge.SFN_Blend,id:9257,x:32368,y:33183,varname:node_9257,prsc:2,blmd:10,clmp:False|SRC-7453-R,DST-1646-R;n:type:ShaderForge.SFN_Slider,id:4802,x:31102,y:33149,ptovrint:False,ptlb:Speed 2,ptin:_Speed2,varname:_Speed2,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-10,cur:0.3,max:10;n:type:ShaderForge.SFN_Multiply,id:5144,x:31442,y:33020,varname:node_5144,prsc:2|A-8028-TSL,B-4802-OUT;n:type:ShaderForge.SFN_Panner,id:8723,x:31771,y:33005,varname:node_8723,prsc:2,spu:0,spv:1|UVIN-7413-OUT,DIST-5144-OUT;n:type:ShaderForge.SFN_Slider,id:6983,x:31714,y:33348,ptovrint:False,ptlb:blur,ptin:_blur,varname:_blur,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:2.402375,max:12;n:type:ShaderForge.SFN_Multiply,id:8058,x:31924,y:32907,varname:node_8058,prsc:2|A-8723-UVOUT,B-4575-OUT;n:type:ShaderForge.SFN_Vector1,id:4575,x:31794,y:33207,varname:node_4575,prsc:2,v1:1.3;n:type:ShaderForge.SFN_Tex2d,id:8582,x:33558,y:33798,ptovrint:False,ptlb:Gradient,ptin:_Gradient,varname:_Gradient,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-7539-OUT;n:type:ShaderForge.SFN_Append,id:7539,x:33267,y:33555,varname:node_7539,prsc:2|A-7120-OUT,B-2253-OUT;n:type:ShaderForge.SFN_Vector1,id:7120,x:32810,y:33264,varname:node_7120,prsc:2,v1:0;n:type:ShaderForge.SFN_Power,id:8242,x:32527,y:33499,varname:node_8242,prsc:2|VAL-9257-OUT,EXP-8252-OUT;n:type:ShaderForge.SFN_Slider,id:8252,x:32188,y:33633,ptovrint:False,ptlb:emmis_exp,ptin:_emmis_exp,varname:_emmis_exp,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0.1,cur:0.4307254,max:5;n:type:ShaderForge.SFN_Clamp01,id:2253,x:33020,y:33596,varname:node_2253,prsc:2|IN-8242-OUT;n:type:ShaderForge.SFN_Multiply,id:4020,x:33764,y:33461,varname:node_4020,prsc:2|A-9066-OUT,B-8582-RGB,C-9257-OUT;n:type:ShaderForge.SFN_Slider,id:4558,x:32771,y:33448,ptovrint:False,ptlb:Emmis_power,ptin:_Emmis_power,varname:_Emmis_power,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:2.967281,max:512;n:type:ShaderForge.SFN_Tex2d,id:9902,x:33566,y:32541,varname:_node_9902,prsc:2,tex:e4518fbdd2e3af140b118e35d7bab35b,ntxv:0,isnm:False|UVIN-8435-OUT,TEX-620-TEX;n:type:ShaderForge.SFN_Multiply,id:8435,x:32791,y:32435,varname:node_8435,prsc:2|A-3673-UVOUT,B-6028-OUT;n:type:ShaderForge.SFN_Vector1,id:6028,x:31530,y:32718,varname:node_6028,prsc:2,v1:0.6;n:type:ShaderForge.SFN_Add,id:3153,x:33769,y:33006,varname:node_3153,prsc:2|A-4830-OUT,B-4020-OUT;n:type:ShaderForge.SFN_Lerp,id:2901,x:34052,y:32903,varname:node_2901,prsc:2|A-4020-OUT,B-3153-OUT,T-8961-OUT;n:type:ShaderForge.SFN_Slider,id:8961,x:33822,y:33238,ptovrint:False,ptlb:Steam_blend,ptin:_Steam_blend,varname:_Steam_blend,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.5857007,max:1;n:type:ShaderForge.SFN_Color,id:2471,x:33853,y:32504,ptovrint:False,ptlb:steam_cloud_color,ptin:_steam_cloud_color,varname:_steam_cloud_color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:4830,x:33737,y:32334,varname:node_4830,prsc:2|A-9902-RGB,B-2471-RGB;n:type:ShaderForge.SFN_ConstantClamp,id:9066,x:33156,y:33405,varname:node_9066,prsc:2,min:0.15,max:512|IN-4558-OUT;n:type:ShaderForge.SFN_Slider,id:5814,x:34439,y:33101,ptovrint:False,ptlb:Opacity,ptin:_Opacity,varname:_Opacity,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;proporder:7241-7739-5519-326-6662-7471-2080-2863-620-4802-6983-8582-8252-4558-8961-2471-5814;pass:END;sub:END;*/

Shader "Almgp/Nuke" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _tesselation_factor ("tesselation_factor", Range(1, 32)) = 6.564103
        _Emmis ("Emmis", 2D) = "white" {}
        _Speed ("Speed", Range(-10, 10)) = -0.5
        _U ("U", Range(-16, 16)) = 2.225862
        _V ("V", Range(-16, 16)) = 1.556958
        _Displace_min ("Displace_min", Range(-2, 2)) = -0.2169179
        _Displace_max ("Displace_max", Range(-2, 2)) = 0.05286796
        _Height_map ("Height_map", 2D) = "white" {}
        _Speed2 ("Speed 2", Range(-10, 10)) = 0.3
        _blur ("blur", Range(0, 12)) = 2.402375
        _Gradient ("Gradient", 2D) = "white" {}
        _emmis_exp ("emmis_exp", Range(0.1, 5)) = 0.4307254
        _Emmis_power ("Emmis_power", Range(0, 512)) = 2.967281
        _Steam_blend ("Steam_blend", Range(0, 1)) = 0.5857007
        _steam_cloud_color ("steam_cloud_color", Color) = (0.5,0.5,0.5,1)
        _Opacity ("Opacity", Range(0, 1)) = 1
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma hull hull
            #pragma domain domain
            #pragma vertex tessvert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "Tessellation.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 5.0
            uniform float4 _TimeEditor;
            uniform float4 _Color;
            uniform float _tesselation_factor;
            uniform float _Speed;
            uniform float _U;
            uniform float _V;
            uniform float _Displace_min;
            uniform float _Displace_max;
            uniform sampler2D _Height_map; uniform float4 _Height_map_ST;
            uniform float _Speed2;
            uniform float _blur;
            uniform sampler2D _Gradient; uniform float4 _Gradient_ST;
            uniform float _emmis_exp;
            uniform float _Emmis_power;
            uniform float _Steam_blend;
            uniform float4 _steam_cloud_color;
            uniform float _Opacity;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            #ifdef UNITY_CAN_COMPILE_TESSELLATION
                struct TessVertex {
                    float4 vertex : INTERNALTESSPOS;
                    float3 normal : NORMAL;
                    float4 tangent : TANGENT;
                    float2 texcoord0 : TEXCOORD0;
                };
                struct OutputPatchConstant {
                    float edge[3]         : SV_TessFactor;
                    float inside          : SV_InsideTessFactor;
                    float3 vTangent[4]    : TANGENT;
                    float2 vUV[4]         : TEXCOORD;
                    float3 vTanUCorner[4] : TANUCORNER;
                    float3 vTanVCorner[4] : TANVCORNER;
                    float4 vCWts          : TANWEIGHTS;
                };
                TessVertex tessvert (VertexInput v) {
                    TessVertex o;
                    o.vertex = v.vertex;
                    o.normal = v.normal;
                    o.tangent = v.tangent;
                    o.texcoord0 = v.texcoord0;
                    return o;
                }
                void displacement (inout VertexInput v){
                    float4 node_8028 = _Time + _TimeEditor;
                    float2 node_7413 = float2((v.texcoord0.r*_U),(v.texcoord0.g*_V));
                    float2 node_3673 = (node_7413+(node_8028.r*_Speed)*float2(0,1));
                    float4 _Height = tex2Dlod(_Height_map,float4(TRANSFORM_TEX(node_3673, _Height_map),0.0,_blur));
                    float2 node_8058 = ((node_7413+(node_8028.r*_Speed2)*float2(0,1))*1.3);
                    float4 _node_1646 = tex2Dlod(_Height_map,float4(TRANSFORM_TEX(node_8058, _Height_map),0.0,_blur));
                    float node_9257 = ( _node_1646.r > 0.5 ? (1.0-(1.0-2.0*(_node_1646.r-0.5))*(1.0-_Height.r)) : (2.0*_node_1646.r*_Height.r) );
                    float node_9888 = 0.0;
                    v.vertex.xyz += (v.normal*(_Displace_min + ( (node_9257 - node_9888) * (_Displace_max - _Displace_min) ) / (1.0 - node_9888)));
                }
                float Tessellation(TessVertex v){
                    return _tesselation_factor;
                }
                float4 Tessellation(TessVertex v, TessVertex v1, TessVertex v2){
                    float tv = Tessellation(v);
                    float tv1 = Tessellation(v1);
                    float tv2 = Tessellation(v2);
                    return float4( tv1+tv2, tv2+tv, tv+tv1, tv+tv1+tv2 ) / float4(2,2,2,3);
                }
                OutputPatchConstant hullconst (InputPatch<TessVertex,3> v) {
                    OutputPatchConstant o = (OutputPatchConstant)0;
                    float4 ts = Tessellation( v[0], v[1], v[2] );
                    o.edge[0] = ts.x;
                    o.edge[1] = ts.y;
                    o.edge[2] = ts.z;
                    o.inside = ts.w;
                    return o;
                }
                [domain("tri")]
                [partitioning("fractional_odd")]
                [outputtopology("triangle_cw")]
                [patchconstantfunc("hullconst")]
                [outputcontrolpoints(3)]
                TessVertex hull (InputPatch<TessVertex,3> v, uint id : SV_OutputControlPointID) {
                    return v[id];
                }
                [domain("tri")]
                VertexOutput domain (OutputPatchConstant tessFactors, const OutputPatch<TessVertex,3> vi, float3 bary : SV_DomainLocation) {
                    VertexInput v = (VertexInput)0;
                    v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
                    v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
                    v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
                    v.texcoord0 = vi[0].texcoord0*bary.x + vi[1].texcoord0*bary.y + vi[2].texcoord0*bary.z;
                    displacement(v);
                    VertexOutput o = vert(v);
                    return o;
                }
            #endif
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
////// Lighting:
////// Emissive:
                float4 node_8028 = _Time + _TimeEditor;
                float2 node_7413 = float2((i.uv0.r*_U),(i.uv0.g*_V));
                float2 node_3673 = (node_7413+(node_8028.r*_Speed)*float2(0,1));
                float4 _Height = tex2Dlod(_Height_map,float4(TRANSFORM_TEX(node_3673, _Height_map),0.0,_blur));
                float2 node_8058 = ((node_7413+(node_8028.r*_Speed2)*float2(0,1))*1.3);
                float4 _node_1646 = tex2Dlod(_Height_map,float4(TRANSFORM_TEX(node_8058, _Height_map),0.0,_blur));
                float node_9257 = ( _node_1646.r > 0.5 ? (1.0-(1.0-2.0*(_node_1646.r-0.5))*(1.0-_Height.r)) : (2.0*_node_1646.r*_Height.r) );
                float2 node_7539 = float2(0.0,saturate(pow(node_9257,_emmis_exp)));
                float4 _Gradient_var = tex2D(_Gradient,TRANSFORM_TEX(node_7539, _Gradient));
                float3 node_4020 = (clamp(_Emmis_power,0.15,512)*_Gradient_var.rgb*node_9257);
                float2 node_8435 = (node_3673*0.6);
                float4 _node_9902 = tex2D(_Height_map,TRANSFORM_TEX(node_8435, _Height_map));
                float3 emissive = (_Color.rgb*lerp(node_4020,((_node_9902.rgb*_steam_cloud_color.rgb)+node_4020),_Steam_blend));
                float3 finalColor = emissive;
                return fixed4(finalColor,_Opacity);
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Back
            
            CGPROGRAM
            #pragma hull hull
            #pragma domain domain
            #pragma vertex tessvert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "Tessellation.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 5.0
            uniform float4 _TimeEditor;
            uniform float _tesselation_factor;
            uniform float _Speed;
            uniform float _U;
            uniform float _V;
            uniform float _Displace_min;
            uniform float _Displace_max;
            uniform sampler2D _Height_map; uniform float4 _Height_map_ST;
            uniform float _Speed2;
            uniform float _blur;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.pos = UnityObjectToClipPos(v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            #ifdef UNITY_CAN_COMPILE_TESSELLATION
                struct TessVertex {
                    float4 vertex : INTERNALTESSPOS;
                    float3 normal : NORMAL;
                    float4 tangent : TANGENT;
                    float2 texcoord0 : TEXCOORD0;
                };
                struct OutputPatchConstant {
                    float edge[3]         : SV_TessFactor;
                    float inside          : SV_InsideTessFactor;
                    float3 vTangent[4]    : TANGENT;
                    float2 vUV[4]         : TEXCOORD;
                    float3 vTanUCorner[4] : TANUCORNER;
                    float3 vTanVCorner[4] : TANVCORNER;
                    float4 vCWts          : TANWEIGHTS;
                };
                TessVertex tessvert (VertexInput v) {
                    TessVertex o;
                    o.vertex = v.vertex;
                    o.normal = v.normal;
                    o.tangent = v.tangent;
                    o.texcoord0 = v.texcoord0;
                    return o;
                }
                void displacement (inout VertexInput v){
                    float4 node_8028 = _Time + _TimeEditor;
                    float2 node_7413 = float2((v.texcoord0.r*_U),(v.texcoord0.g*_V));
                    float2 node_3673 = (node_7413+(node_8028.r*_Speed)*float2(0,1));
                    float4 _Height = tex2Dlod(_Height_map,float4(TRANSFORM_TEX(node_3673, _Height_map),0.0,_blur));
                    float2 node_8058 = ((node_7413+(node_8028.r*_Speed2)*float2(0,1))*1.3);
                    float4 _node_1646 = tex2Dlod(_Height_map,float4(TRANSFORM_TEX(node_8058, _Height_map),0.0,_blur));
                    float node_9257 = ( _node_1646.r > 0.5 ? (1.0-(1.0-2.0*(_node_1646.r-0.5))*(1.0-_Height.r)) : (2.0*_node_1646.r*_Height.r) );
                    float node_9888 = 0.0;
                    v.vertex.xyz += (v.normal*(_Displace_min + ( (node_9257 - node_9888) * (_Displace_max - _Displace_min) ) / (1.0 - node_9888)));
                }
                float Tessellation(TessVertex v){
                    return _tesselation_factor;
                }
                float4 Tessellation(TessVertex v, TessVertex v1, TessVertex v2){
                    float tv = Tessellation(v);
                    float tv1 = Tessellation(v1);
                    float tv2 = Tessellation(v2);
                    return float4( tv1+tv2, tv2+tv, tv+tv1, tv+tv1+tv2 ) / float4(2,2,2,3);
                }
                OutputPatchConstant hullconst (InputPatch<TessVertex,3> v) {
                    OutputPatchConstant o = (OutputPatchConstant)0;
                    float4 ts = Tessellation( v[0], v[1], v[2] );
                    o.edge[0] = ts.x;
                    o.edge[1] = ts.y;
                    o.edge[2] = ts.z;
                    o.inside = ts.w;
                    return o;
                }
                [domain("tri")]
                [partitioning("fractional_odd")]
                [outputtopology("triangle_cw")]
                [patchconstantfunc("hullconst")]
                [outputcontrolpoints(3)]
                TessVertex hull (InputPatch<TessVertex,3> v, uint id : SV_OutputControlPointID) {
                    return v[id];
                }
                [domain("tri")]
                VertexOutput domain (OutputPatchConstant tessFactors, const OutputPatch<TessVertex,3> vi, float3 bary : SV_DomainLocation) {
                    VertexInput v = (VertexInput)0;
                    v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
                    v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
                    v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
                    v.texcoord0 = vi[0].texcoord0*bary.x + vi[1].texcoord0*bary.y + vi[2].texcoord0*bary.z;
                    displacement(v);
                    VertexOutput o = vert(v);
                    return o;
                }
            #endif
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 normalDirection = i.normalDir;
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
