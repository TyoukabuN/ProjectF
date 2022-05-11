Shader "TyoukabuN/Env/Unlit_Toon_Water"
{
	Properties{
		_Color ("Color",color) = (1,1,1,1)
		_EdgeColor("_EdgeColor",color) = (1,1,1,1)
		_BottomColor("_BottomColor",color) = (1,1,1,1)
		_DepthFactor("_DepthFactor",float) = 0.5
		_RampTex("_RampTex",2D) ="white"{}
		_NoiseTex("NoiseTex",2D) = "white"{}
		_RampColorIntensity ("RampColorIntensity",float) = 1
		_waveSpeed("waveSpeed",float) = 1
		_waveHight("waveHight",Range(0,0.3)) = 1
		_waveScale("waveScale",Vector) = (1,1,1,1)
	}
	SubShader{
		pass{
			Tags { "LightMode" = "ForwardBase" }
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#pragma enable_d3d11_debug_symbols
			#pragma vertex vert
			#pragma fragment frag

			struct v2f{
				float4 pos:SV_POSITION;
				float2 uv:TEXCOORD0;
				float4 screenPos:TEXCOORD1;
				float2 depth : TEXCOORD2;
			};
			float4 _Color;
			float4 _EdgeColor;
			float4 _BottomColor;
			float _DepthFactor;
			sampler2D _RampTex;
			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;
			float _RampColorIntensity;
			float _waveSpeed;
			float _waveHight;
			float2 _waveScale;

			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
			v2f vert(appdata_img v){
				v2f o;
				// v.vertex.y += sin(v.vertex.x * _waveScale + _waveSpeed * _Time.x) * _waveHight;
				float4 wpos = mul(unity_ObjectToWorld, v.vertex);

				float4 _sampler = tex2Dlod(_NoiseTex,float4(v.texcoord.xy * _NoiseTex_ST.xy + _NoiseTex_ST.zw,0,0));
				//wpos.y += sin(_Time.y + (v.texcoord.x + _sampler.x) * _waveScale.x) * _waveHight;
				//wpos.y += sin(_Time.y + (v.texcoord.y + _sampler.y) * _waveScale.y) * _waveHight;
				 wpos.x += sin(_sampler.x + _waveScale.x + _Time.x *  _waveSpeed)* _waveHight;
				 wpos.y += cos(_sampler.y + _waveScale.y + _Time.x *  _waveSpeed)* _waveHight;
				o.pos = mul(UNITY_MATRIX_VP, wpos);
				o.uv = v.texcoord;
				o.screenPos = ComputeScreenPos(o.pos);
				return o;
			}
			float4 frag(v2f i):SV_TARGET{
				float4 depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD(i.screenPos));
				
				//LinearEyeDepth(depth) 是在Project Space下的z 不是在ndc
				//i.screenPos.w P下的-z 在齐次空间下screenPos为（x,y,z,-z） 参考projection matrix的生成过程 
				//越深越大
				 //( LinearEyeDepth(depth) - i.screenPos.w)
				 //即表示浅的地方
				 //1 - (LinearEyeDepth(depth) - i.screenPos.w)
				 //加个factor控制这个浅的范围
				 //1 - _DepthFactor * (LinearEyeDepth(depth) - i.screenPos.w)
				 //限制大于0 不然越深反而越亮
				// float foamLine =  1 - saturate(_DepthFactor * (LinearEyeDepth(depth) - i.screenPos.w)) ;
				float foamLine =  1 - saturate(_DepthFactor * (LinearEyeDepth(depth) - i.screenPos.w)) ;

				float4 foamRamp = float4(tex2D(_RampTex,float2(foamLine,0.5)).rgb,1.0) * _RampColorIntensity;
				// float4 col = _Color + clamp(foamLine - _RampColorIntensity,0,1) * _EdgeColor;
				float4 col = _Color * foamRamp;
				return col;
			}
			ENDCG
		}
	}
}