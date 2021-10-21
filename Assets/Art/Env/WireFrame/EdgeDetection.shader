Shader "X_Shader/C_Charactar/EdgeDetection"
{
	Properties
	{
		_WireColor("线框颜色",Color) = (1,1,1,1)
		_WireWidth("线框的宽度",float) = 0.15
		_SlopeLengle("对角线的长度",float) = 0.04
		_ModelTop("模型高点",float) = 7.87
		_ModelBottom("模型低点",float) = -5.39


		_WaveTex("扫描线纹理",2D) = "white" {}
		_WaveTexScrollSpeed("扫描线纹理滚动速度",float) = 1
		_WaveColor("扫描线颜色",Color) = (1,1,1,1)
		[KeywordEnum(Off,On)]_WaveSwitch("扫描线开关",float) =  1
		_WaveFrequency("扫描线的频率",float) = 4.76
		_WaveLength("扫描线的波长",float) = 1
		_WaveSpeed("扫描线的速度",float) = 1.3

	}
	
	Subshader
	{
		Tags { "RenderType"="Transparent" "Queue" = "Transparent"}

		Pass
		{
			Tags { "LightMode" = "Always" } 
			Blend SrcAlpha OneMinusSrcAlpha
			//ColorMask RGB
			Cull Off
			ZTest Off
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members color)
#pragma exclude_renderers d3d11
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"
				struct v2f
				{
					float4 pos	: SV_POSITION;
					 float4	color :	COLOR;
					float4 uv 	: TEXCOORD0;
					float3 normal : TEXCOORD1;
					float4 wpos : TEXCOORD2;
					float4 vertex : TEXCOORD3;
					float3 colorMupti :TEXCOORD4;
					float4 screenPosition : TEXCOORD5;
					

				};
				
				
				uniform float4 _MainTex_ST;
				uniform float4 _XTime;
				uniform float4 _WireColor; 
				uniform float4 _WaveColor; 

				uniform sampler2D _WaveTex;
				uniform float4 _WaveTex_ST;
				uniform float _WaveFrequency; 
				uniform float _WaveLength; 
				uniform float _WireWidth; 
				uniform float _SlopeLengle; 
				uniform float _ModelTop; 
				uniform float _ModelBottom; 
				uniform float _WaveSpeed; 
				uniform float _WaveTexScrollSpeed; 
				uniform float _WaveSwitch; 
				

				 struct a2v
				 {
					 float4 vertex   : POSITION;  // The vertex position in model space.
					 float3 normal   : NORMAL;    // The vertex normal in model space.
					 float4 texcoord : TEXCOORD0; // The first UV coordinate
					 float4	color :	COLOR;
				 };


				v2f vert (a2v v)
				{
					v2f o;
					o.vertex = v.vertex;
					o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.uv.zw = TRANSFORM_TEX(v.texcoord, _WaveTex);

					o.pos = UnityObjectToClipPos (v.vertex);
					//o.normal = normalize(UnityObjectToWorldNormal(v.normal));
					o.normal = v.normal;
					o.wpos =  mul(unity_ObjectToWorld, v.vertex);
					o.color = v.color;
					o.colorMupti = v.color.rgb * v.color.a;
					o.screenPosition = ComputeScreenPos(o.pos);

					UNITY_TRANSFER_FOG(o, o.pos);

					return o;
				}
				

				fixed4 frag (v2f i) : COLOR
				{
					float3 color = i.color.rgb;

					float grad = min(color.r, min(color.g, color.b));
					float minColComp = min(color.r, min(color.g, color.b));
					float maxColComp = max(color.r, max(color.g, color.b));

					//col = smoothstep(0,wide,color.r + color.g) + smoothstep(0,wide,color.r + color.b) + smoothstep(0,wide,color.g + color.b);
					//return fixed4(col,col,col,1);
					//return fixed4(_WireColor.rgb,grad);
					//grad = max(color.r, max(color.g, color.b));

					float3 zero = float3(0,0,0);
					float3 dx = lerp(zero,float3(1,0,0),step(abs(minColComp-color.r),0.0001));
					float3 dy = lerp(zero,float3(0,1,0),step(abs(minColComp-color.g),0.0001));
					float3 dz = lerp(zero,float3(0,0,1),step(abs(minColComp-color.b),0.0001));

					float3 slopeDetection = dx + dy + dz;
					float3 colorMuptiSingle = i.colorMupti * slopeDetection;

					float3 Indices = i.colorMupti / i.color.rgb;
					float grad2 = max(colorMuptiSingle.r, max(colorMuptiSingle.g, colorMuptiSingle.b));
					float minIndice = grad2 / grad;


					float3 slopeDetection2 = step(abs(minIndice-Indices.r),0.001) + step(abs(minIndice-Indices.g),0.001) + step(abs(minIndice-Indices.b),0.001);
					float res = 1 - step(2,slopeDetection2) ;
					res = lerp(0,res,step(maxColComp,1 - _SlopeLengle));

					float3 color2 = i.color + res * slopeDetection;

					float grad3 = min(color2.r, min(color2.g, color2.b));
					float wireVal = smoothstep(max(_WireWidth,0),0,  grad3) ;

					float heightFade = smoothstep(_ModelBottom,_ModelTop,i.vertex.y);	

					float4 wire = fixed4(_WireColor.rgb ,saturate(wireVal * heightFade));

					//scan line
					float2 textureCoordinate = i.screenPosition.xy / i.screenPosition.w;
					float param = textureCoordinate.y;//i.vertex.y;
					float waveVal = step(0,sin(((param *_WaveFrequency + _WaveLength) + _Time.y * _WaveSpeed )));
					textureCoordinate.y +=  _Time.y * _WaveTexScrollSpeed ;
					float4 wave = waveVal * _WaveColor * tex2D(_WaveTex,textureCoordinate );
					wave.a = saturate(wave.a * (1-wireVal) * heightFade);
					wave.rgb *= (1-wireVal) ;

					//return wave;

					wire.rgb = wire.rgb + lerp(float3(0,0,0),float3(_WaveColor.rgb),step(0.001,waveVal));
					wire.rgb *= wire.a;

					return wire + wave * _WaveSwitch;

					//return lerp(wire,wave,step(0.001,waveVal));
					//return fixed4(step(2,slopeDetection2) ,1);
					//return fixed4(i.color.rgb ,1);
				}
			ENDCG
		}
	}
	Fallback "X_Shader/A_Default/VertexLit"
}
