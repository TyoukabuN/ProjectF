Shader "X_Shader/PostProcess/Blur"
{
	HLSLINCLUDE

		#include "../StdLib.hlsl"
		#include "../Colors.hlsl"
		#include "../Sampling.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

		float4 _MainTex_TexelSize;
		float4 _MainTex_ST;
		float4 _Parameter;

		#define SAMPLE_COUNT 8      //迭代次数  

		struct v2f_tap
		{
			float4 vertex : SV_POSITION;
			half2 uv20 : TEXCOORD0;
			half2 uv21 : TEXCOORD1;
			half2 uv22 : TEXCOORD2;
			half2 uv23 : TEXCOORD3;
		};	

		v2f_tap vert4Tap ( AttributesDefault v )
		{
			v2f_tap o;
			 
    		o.vertex = float4(v.vertex.xy, 0.0, 1.0);
    		float2 texcoord = TransformTriangleVertexToUV(v.vertex.xy);
        	o.uv20 = UnityStereoScreenSpaceUVAdjust(texcoord + _MainTex_TexelSize.xy, _MainTex_ST);
			o.uv21 = UnityStereoScreenSpaceUVAdjust(texcoord + _MainTex_TexelSize.xy * half2(-0.5h,-0.5h), _MainTex_ST);
			o.uv22 = UnityStereoScreenSpaceUVAdjust(texcoord + _MainTex_TexelSize.xy * half2(0.5h,-0.5h), _MainTex_ST);
			o.uv23 = UnityStereoScreenSpaceUVAdjust(texcoord + _MainTex_TexelSize.xy * half2(-0.5h,0.5h), _MainTex_ST);

			return o; 
		}	

		float4 fragDownsample ( v2f_tap i ) : SV_Target
		{				
			float4 color = tex2D (_MainTex, i.uv20);
			color += tex2D (_MainTex, i.uv21);
			color += tex2D (_MainTex, i.uv22);
			color += tex2D (_MainTex, i.uv23);
			return color / 4;
		}		

		inline float2 UnityStereoScreenSpaceUVAdjustInternal(float2 uv, float4 scaleAndOffset)
		{
			return uv.xy * scaleAndOffset.xy + scaleAndOffset.zw;
		}

		inline float4 UnityStereoScreenSpaceUVAdjustInternal(float4 uv, float4 scaleAndOffset)
		{
			return float4(UnityStereoScreenSpaceUVAdjustInternal(uv.xy, scaleAndOffset), UnityStereoScreenSpaceUVAdjustInternal(uv.zw, scaleAndOffset));
		}

		#define UnityStereoScreenSpaceUVAdjust(x, y) UnityStereoScreenSpaceUVAdjustInternal(x, y)

		static const half curve[7] = { 0.0205, 0.0855, 0.232, 0.324, 0.232, 0.0855, 0.0205 };  // gauss'ish blur weights

		static const half4 curve4[7] = { half4(0.0205,0.0205,0.0205,0), half4(0.0855,0.0855,0.0855,0), half4(0.232,0.232,0.232,0),
		half4(0.324,0.324,0.324,1), half4(0.232,0.232,0.232,0), half4(0.0855,0.0855,0.0855,0), half4(0.0205,0.0205,0.0205,0) };

		struct v2f_withBlurCoords8 
		{
			float4 vertex : SV_POSITION;
			half4 uv : TEXCOORD0;
			half2 offs : TEXCOORD1;
		};	
		
		struct v2f_withBlurCoordsSGX 
		{
			float4 vertex : SV_POSITION;
			half2 uv : TEXCOORD0;
			half4 offs[3] : TEXCOORD1;
		};

		v2f_withBlurCoords8 vertBlurHorizontal (AttributesDefault v)
		{
			v2f_withBlurCoords8 o;
    		o.vertex = float4(v.vertex.xy, 0.0, 1.0);

    		float2 texcoord = TransformTriangleVertexToUV(v.vertex.xy);
			o.uv = half4(texcoord,1,1);
			o.offs = _MainTex_TexelSize.xy * half2(1.0, 0.0) * _Parameter.x;

			return o; 
		}
		
		v2f_withBlurCoords8 vertBlurVertical (AttributesDefault v)
		{
			v2f_withBlurCoords8 o;
    		o.vertex = float4(v.vertex.xy, 0.0, 1.0);

    		float2 texcoord = TransformTriangleVertexToUV(v.vertex.xy);
			o.uv = half4(texcoord,1,1);
			o.offs = _MainTex_TexelSize.xy * half2(0.0, 1.0) * _Parameter.x;
			 
			return o; 
		}	

		half4 fragBlur8 ( v2f_withBlurCoords8 i ) : SV_Target
		{
			half2 uv = i.uv.xy; 
			half2 netFilterWidth = i.offs;  
			half2 coords = uv - netFilterWidth * 3.0;  
			
			half4 color = 0;
  			for( int l = 0; l < 7; l++ )  
  			{   
				half4 tap = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(coords, _MainTex_ST));
				color += tap * curve4[l];
				coords += netFilterWidth;
  			}
			return color;
		}



	ENDHLSL



	SubShader
	{
        Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

				#pragma vertex vert4Tap
				#pragma fragment fragDownsample

			ENDHLSL
		}

		Pass
		{
			HLSLPROGRAM

				#pragma vertex vertBlurVertical
				#pragma fragment fragBlur8 

			ENDHLSL
		}

		Pass
		{
			HLSLPROGRAM

				#pragma vertex vertBlurHorizontal
				#pragma fragment fragBlur8

			ENDHLSL
		}
	}

}
