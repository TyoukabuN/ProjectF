// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "CatLikeCoding/Rendering/TheFirstLight"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct VertexData {
				float4 position : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};
		
			struct Interpolators {
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
			};

			Interpolators MyVertexProgram (VertexData v) {
				Interpolators i;
				i.uv = TRANSFORM_TEX(v.uv, _MainTex);
				i.position = UnityObjectToClipPos(v.position);
				//使用动态批处理的时候,unity会默认合并一些地数的游戏对象
				//批处理动态游戏对象在每个顶点都有一定开销，因此批处理仅会应用于总共包含不超过 900 个顶点属性且不超过 300 个顶点的网格。
				//如果着色器使用顶点位置、法线和单个 UV，最多可以批处理 300 个顶点，
				//而如果着色器使用顶点位置、法线、UV0、UV1 和切线，则只能批处理 180 个顶点。
				//为了合并网格,需要将网格的顶点从模型空间转换到世界空间
				//这同样会影响到法线
				//如果你使用开启了Dynamic batching,且你的游戏对象符合上述dynamic batching的条件的话
				//使用使用下面的指令会
				i.normal = UnityObjectToWorldNormal(normalize(v.normal));
				i.normal = normalize(i.normal);
				return i;
			}

			float4 MyFragmentProgram(Interpolators i) :SV_Target
			{
				i.normal = normalize(i.normal);
				return float4(i.normal * 0.5 + 0.5,1);
			}

			ENDCG
		}
    }
    FallBack "Diffuse"
}
