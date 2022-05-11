Shader "TyoukabuN/Charactar/SandFade"
{
	Properties
	{
		_Color ("Color",Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MatCap ("MatCap (RGB)", 2D) = "white" {}
		_MatCapPower ("Power",Range(0.25,8.0)) = 2.0
		_Saturation("Saturation", Range(0,1)) = 1

		//_OutlineColor("OutLineColor",Color) = (0,0,0,0)
  //      _OutlinePower("OutlineWidth",Range(0.1,5)) = 2.47

        [Toggle()] _HitRimPreview("HitRimPreview",Float) = 0
		_HitRimPower ("_HitRimPower",Range(0,1)) = 1.0
		_RimColor("RimColor",Color) = (1,0.8816213,0.5330188,1)
        _HitRimParameter("HitRimParameter",Vector) = (0,0,0,0)
		_RimReducer ("RimReducer",float) = 1.3
		_RimPow ("_RimPow",float) = 3.0

		_SandColor("Sand Color",Color) = (1,1,1,1)
		_GradientNoiseMap ("Gradient Noise Map", 2D) = "white" {}
		_SandPerview ("SandPerview",Range(0,1.5)) = 0
		_SandFadeParameter ("SandParam",Vector) = (0,0,0,0)
		_WindDir("WindDir",Vector) = (1,0,0,0)

		_SandColorPctOffset("沙子颜色变化的阻力",Range(0,3)) = 0

	}
	
	Subshader
	{
		Tags { "RenderType"="Transparent" "Queue" = "Transparent"}

		Pass
		{
			Tags { "LightMode" = "Always" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_fog
				#include "UnityCG.cginc"
				struct v2f
				{
					float4 pos	: SV_POSITION;
					float2 uv 	: TEXCOORD0;
					float2 cap	: TEXCOORD1;
					UNITY_FOG_COORDS(2)
					float3 normalDir : TEXCOORD3;
					float3 viewDirection : TEXCOORD4;
					float3  vertex       : TEXCOORD5;
					fixed2 uv1 : TEXCOORD6;

				};
				
				uniform float4 _MainTex_ST;
				uniform float4 _SandFadeParameter;
				uniform sampler2D _GradientNoiseMap;
				uniform float4 _GradientNoiseMap_ST;

				fixed _FogSelfHeightOffset;
				fixed _FogSelfHeightOffset2;
				uniform float4 _WindDir;
				uniform float _SandColorPctOffset;
				uniform float _SandPerview;
				uniform float4 _SandColor;
				uniform float4 _Realtime;

				v2f vert (appdata_base v)
				{
					v2f o;
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.uv1 = TRANSFORM_TEX(v.texcoord, _GradientNoiseMap);

					//rimhit
					float3 posWorld = mul(unity_ObjectToWorld, v.vertex);
					o.normalDir = normalize(UnityObjectToWorldNormal(v.normal));
					o.viewDirection = normalize(_WorldSpaceCameraPos.xyz - posWorld.xyz);

					fixed posYPre =(posWorld.y - _SandFadeParameter.z)/_SandFadeParameter.w;

					fixed x = saturate((_Realtime.y-_SandFadeParameter.x)/(_SandFadeParameter.y-_SandFadeParameter.x));
					x = lerp(_SandPerview,x,step(0.0001,_SandFadeParameter.x));

					// fixed PI = 3.14159265359;
					//fixed inCirc = 1 - sqrt(1 - pow(x, 2));
					//fixed inSine = 1 - cos((x * PI) / 2);
					fixed easeInOutCubic = lerp(4 * x * x * x,1 - pow(-2 * x + 2, 3) / 2, step(x,0.5));

					fixed fader = posYPre + easeInOutCubic;
					fader *=  step(1,fader);
					v.vertex.xyz += _WindDir.xyz * dot(fader,fader);
					
					o.pos = UnityObjectToClipPos (v.vertex);
					half2 capCoord;
					
					float3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
					worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
					o.cap.xy = worldNorm.xy * 0.5 + 0.5;


					o.vertex = posWorld;


					UNITY_TRANSFER_FOG(o, o.pos);

					return o;
				}
				
				uniform sampler2D _MainTex;
				uniform sampler2D _MatCap;
				uniform fixed4 _Color;
				uniform half _MatCapPower;
				fixed _Saturation;


				fixed4 frag (v2f i) : COLOR
				{ 
					fixed posYPre =(i.vertex.y - _SandFadeParameter.z)/_SandFadeParameter.w;
					fixed4 noise = tex2D(_GradientNoiseMap, float2( i.uv1.x,saturate(posYPre) ));

					fixed x = saturate((_Realtime.y-_SandFadeParameter.x)/(_SandFadeParameter.y-_SandFadeParameter.x));
					x = lerp(_SandPerview,x,step(0.0001,_SandFadeParameter.x));

					//fixed PI = 3.14159265359;
					//fixed inCirc = 1 - sqrt(1 - pow(x, 2));
					//fixed inSine = 1 - cos((x * PI) / 2);
					fixed easeInOutCubic = lerp(4 * x * x * x, 1 - pow(-2 * x + 2, 3) / 2, step(x,0.5));

					fixed param = noise.r - easeInOutCubic+_WindDir.w;
					clip(param);

					fixed4 tex = tex2D(_MainTex, i.uv);
					fixed4 mc = tex2D(_MatCap, i.cap) * tex * _Color * _MatCapPower;
					mc.a = tex.a * _Color.a;
					UNITY_APPLY_FOG(i.fogCoord, mc);
					//saturation饱和度：首先根据公式计算同等亮度情况下饱和度最低的值：
					fixed gray = 0.2125 * mc.r + 0.7154 * mc.g + 0.0721 * mc.b;
					fixed3 grayColor = fixed3(gray, gray, gray);
					//根据Saturation在饱和度最低的图像和原图之间差值
					mc.rgb = lerp(grayColor, mc.rgb, _Saturation);

					//mc.rgb = lerp(mc.rgb,_SandColor.rgb,saturate(1-param-_SandColorPctOffset));
					float smoothC = smoothstep(0,1+_SandColorPctOffset,1-param);
					mc.rgb = lerp(mc.rgb,_SandColor,saturate(smoothC));

					return fixed4(mc.rgb,mc.a);
					//return fixed4(param,param,param,1);
				}
			ENDCG
		}
	}
	Fallback "X_Shader/A_Default/VertexLit"
}
