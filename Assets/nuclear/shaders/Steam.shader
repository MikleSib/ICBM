// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.36 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.36;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:1,olmd:1,culm:2,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:4795,x:34945,y:32170,varname:node_4795,prsc:2|emission-8619-OUT,voffset-5-OUT;n:type:ShaderForge.SFN_Tex2d,id:6074,x:32235,y:32601,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:2393,x:33923,y:32018,varname:node_2393,prsc:2|A-6074-RGB,B-2053-RGB,C-797-RGB,D-3161-OUT,E-1685-OUT;n:type:ShaderForge.SFN_VertexColor,id:2053,x:32647,y:32623,varname:node_2053,prsc:2;n:type:ShaderForge.SFN_Color,id:797,x:32029,y:32909,ptovrint:True,ptlb:Color,ptin:_TintColor,varname:_TintColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_NormalVector,id:4470,x:32237,y:32177,prsc:2,pt:False;n:type:ShaderForge.SFN_Fresnel,id:7255,x:32521,y:32318,varname:node_7255,prsc:2|NRM-4470-OUT,EXP-3501-OUT;n:type:ShaderForge.SFN_Slider,id:3501,x:32160,y:32411,ptovrint:False,ptlb:exponent,ptin:_exponent,varname:_exponent,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0.1,cur:1.150583,max:8;n:type:ShaderForge.SFN_Slider,id:3161,x:32591,y:32847,ptovrint:False,ptlb:Power,ptin:_Power,varname:_Power,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:2,max:64;n:type:ShaderForge.SFN_NormalVector,id:9239,x:33101,y:32415,prsc:2,pt:False;n:type:ShaderForge.SFN_Slider,id:542,x:32977,y:32803,ptovrint:False,ptlb:Displace_power,ptin:_Displace_power,varname:_Displace_power,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:16;n:type:ShaderForge.SFN_Multiply,id:5,x:33284,y:32450,varname:node_5,prsc:2|A-9239-OUT,B-542-OUT;n:type:ShaderForge.SFN_DepthBlend,id:6911,x:32867,y:32062,varname:node_6911,prsc:2|DIST-6199-OUT;n:type:ShaderForge.SFN_Slider,id:6199,x:32505,y:32106,ptovrint:False,ptlb:Burn_radius,ptin:_Burn_radius,varname:_Burn_radius,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.5,max:256;n:type:ShaderForge.SFN_Clamp01,id:1874,x:33083,y:32134,varname:node_1874,prsc:2|IN-852-OUT;n:type:ShaderForge.SFN_Add,id:1685,x:34157,y:32473,varname:node_1685,prsc:2|A-7255-OUT,B-1874-OUT;n:type:ShaderForge.SFN_OneMinus,id:852,x:33097,y:31985,varname:node_852,prsc:2|IN-6911-OUT;n:type:ShaderForge.SFN_Tex2d,id:2255,x:33104,y:32997,ptovrint:False,ptlb:Normal,ptin:_Normal,varname:_Normal,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:81cc49f209284c346ab6d148f1fa42a2,ntxv:3,isnm:False|MIP-5811-OUT;n:type:ShaderForge.SFN_Append,id:2529,x:33643,y:32536,varname:node_2529,prsc:2|A-7209-R,B-7209-G;n:type:ShaderForge.SFN_Multiply,id:4532,x:34087,y:32313,varname:node_4532,prsc:2|A-2529-OUT,B-2752-OUT,C-1283-OUT;n:type:ShaderForge.SFN_Slider,id:2752,x:33740,y:32815,ptovrint:False,ptlb:refract,ptin:_refract,varname:_refract,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_NormalVector,id:1298,x:33094,y:33261,prsc:2,pt:False;n:type:ShaderForge.SFN_Multiply,id:7070,x:33333,y:33091,varname:node_7070,prsc:2|A-2255-RGB,B-1298-OUT;n:type:ShaderForge.SFN_ComponentMask,id:7209,x:33519,y:32721,varname:node_7209,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-7070-OUT;n:type:ShaderForge.SFN_Slider,id:5811,x:32731,y:33155,ptovrint:False,ptlb:Refraction_blur,ptin:_Refraction_blur,varname:_Refraction_blur,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:2,max:12;n:type:ShaderForge.SFN_Fresnel,id:1283,x:33707,y:32185,varname:node_1283,prsc:2|NRM-4470-OUT,EXP-2428-OUT;n:type:ShaderForge.SFN_Slider,id:2428,x:33320,y:32219,ptovrint:False,ptlb:Refract_factor,ptin:_Refract_factor,varname:_Refract_factor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0.1,cur:1.018187,max:3;n:type:ShaderForge.SFN_Tex2d,id:2649,x:34246,y:32612,ptovrint:False,ptlb:Height_grad,ptin:_Height_grad,varname:node_2649,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:c67db1177f0521a40aad4c281600a6aa,ntxv:0,isnm:False|UVIN-2935-OUT,MIP-1486-OUT;n:type:ShaderForge.SFN_FragmentPosition,id:4420,x:33608,y:33061,varname:node_4420,prsc:2;n:type:ShaderForge.SFN_Vector1,id:1332,x:33576,y:33231,varname:node_1332,prsc:2,v1:1;n:type:ShaderForge.SFN_Append,id:7629,x:33881,y:33043,varname:node_7629,prsc:2|A-1332-OUT,B-4420-Y;n:type:ShaderForge.SFN_Multiply,id:2935,x:34182,y:33073,varname:node_2935,prsc:2|A-7629-OUT,B-7140-OUT;n:type:ShaderForge.SFN_Slider,id:7140,x:33951,y:33228,ptovrint:False,ptlb:height_size,ptin:_height_size,varname:node_7140,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:0.01;n:type:ShaderForge.SFN_Multiply,id:7038,x:34446,y:32257,varname:node_7038,prsc:2|A-1685-OUT,B-2649-R;n:type:ShaderForge.SFN_SwitchProperty,id:8619,x:34681,y:32212,ptovrint:False,ptlb:Use_h_grad,ptin:_Use_h_grad,varname:node_8619,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-1526-OUT,B-7901-OUT;n:type:ShaderForge.SFN_Add,id:7736,x:34556,y:32401,varname:node_7736,prsc:2|A-1526-OUT,B-2649-R;n:type:ShaderForge.SFN_Clamp01,id:1526,x:34378,y:32086,varname:node_1526,prsc:2|IN-2393-OUT;n:type:ShaderForge.SFN_Vector1,id:1486,x:34133,y:32784,varname:node_1486,prsc:2,v1:0;n:type:ShaderForge.SFN_Multiply,id:7901,x:34594,y:31985,varname:node_7901,prsc:2|A-2393-OUT,B-2649-R;proporder:6074-797-3501-3161-542-6199-2255-2752-5811-2428-2649-7140-8619;pass:END;sub:END;*/

Shader "Almgp/Steam" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _TintColor ("Color", Color) = (0.5,0.5,0.5,1)
        _exponent ("exponent", Range(0.1, 8)) = 1.150583
        _Power ("Power", Range(0, 64)) = 2
        _Displace_power ("Displace_power", Range(0, 16)) = 1
        _Burn_radius ("Burn_radius", Range(0, 256)) = 0.5
        _Normal ("Normal", 2D) = "bump" {}
        _refract ("refract", Range(0, 1)) = 1
        _Refraction_blur ("Refraction_blur", Range(0, 12)) = 2
        _Refract_factor ("Refract_factor", Range(0.1, 3)) = 1.018187
        _Height_grad ("Height_grad", 2D) = "white" {}
        _height_size ("height_size", Range(0, 0.01)) = 0
        [MaterialToggle] _Use_h_grad ("Use_h_grad", Float ) = 0
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
            Blend One One
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _CameraDepthTexture;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _TintColor;
            uniform float _exponent;
            uniform float _Power;
            uniform float _Displace_power;
            uniform float _Burn_radius;
            uniform sampler2D _Height_grad; uniform float4 _Height_grad_ST;
            uniform float _height_size;
            uniform fixed _Use_h_grad;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float4 vertexColor : COLOR;
                float4 projPos : TEXCOORD3;
                UNITY_FOG_COORDS(4)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                v.vertex.xyz += (v.normal*_Displace_power);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float sceneZ = max(0,LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)))) - _ProjectionParams.g);
                float partZ = max(0,i.projPos.z - _ProjectionParams.g);
////// Lighting:
////// Emissive:
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float node_1685 = (pow(1.0-max(0,dot(i.normalDir, viewDirection)),_exponent)+saturate((1.0 - saturate((sceneZ-partZ)/_Burn_radius))));
                float3 node_2393 = (_MainTex_var.rgb*i.vertexColor.rgb*_TintColor.rgb*_Power*node_1685);
                float3 node_1526 = saturate(node_2393);
                float2 node_2935 = (float2(1.0,i.posWorld.g)*_height_size);
                float4 _Height_grad_var = tex2Dlod(_Height_grad,float4(TRANSFORM_TEX(node_2935, _Height_grad),0.0,0.0));
                float3 emissive = lerp( node_1526, (node_2393*_Height_grad_var.r), _Use_h_grad );
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0,0,0,1));
                return finalRGBA;
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
