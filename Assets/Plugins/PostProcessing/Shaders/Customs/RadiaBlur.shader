Shader "X_Shader/PostProcess/RadiaBlur"
{
	HLSLINCLUDE

		#include "../StdLib.hlsl"
		#include "../Colors.hlsl"
		#include "../Sampling.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		TEXTURE2D_SAMPLER2D(_BlurTex, sampler_BlurTex);

		float _BlurFactor;  //模糊强度（0-0.05）  
		float _LerpFactor;  //插值的强度（0-1）  
		float4 _BlurCenter; //模糊中心点xy值（0-1）屏幕空间  
		float4 _MainTex_TexelSize;

		#define SAMPLE_COUNT 8      //迭代次数  

		float4 frag_blur(VaryingsDefault i):SV_Target
		{
			//模糊方向为模糊中点指向边缘（当前像素点），而越边缘该值越大，越模糊  
			float2 dir = i.texcoord - _BlurCenter.xy;
			float4 outColor = 0;
			//采样SAMPLE_COUNT次  
			for (int j = 0; j < SAMPLE_COUNT; ++j)
			{ 
				//计算采样uv值：正常uv值+从中间向边缘逐渐增加的采样距离  
				float2 uv = i.texcoord + _BlurFactor * dir * j;
				outColor += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
			}
			//取平均值  
			outColor /= SAMPLE_COUNT;
			return outColor; 
		}

		float4 frag_lerp(VaryingsDefault i) : SV_Target
		{
			float2 dir = i.texcoord - _BlurCenter.xy;
			float dis = length(dir);
			float4 oriTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
			float4 blurTex = SAMPLE_TEXTURE2D(_BlurTex, sampler_BlurTex, i.texcoord);
			//按照距离乘以插值系数在原图和模糊图之间差值  
			return lerp(oriTex, blurTex, _LerpFactor * dis);
		}

	ENDHLSL



	SubShader
	{
        Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

				#pragma fragmentoption ARB_precision_hint_fastest 
				#pragma vertex VertDefault  
				#pragma fragment frag_blur  

			ENDHLSL
		}

		Pass
		{
			HLSLPROGRAM

				#pragma fragmentoption ARB_precision_hint_fastest 
				#pragma vertex VertDefault  
				#pragma fragment frag_lerp  

			ENDHLSL
		}
	}

}
