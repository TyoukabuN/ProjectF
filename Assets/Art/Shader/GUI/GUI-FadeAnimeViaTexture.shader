Shader "X_Shader/G_GUI/FadeAnimeViaTexture"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil   Mask", Float) = 255

		_DissolveTex ("DissolveTex", 2D) = "white" {}

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

		_Preview("Preview",Range(0,1)) = 0

		_FadeParam("Fade Param",Vector) = (0,0,0,0)

	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Name "Default"
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
			#include "Easing.cginc"


			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
			};
			
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;

			sampler2D _DissolveTex;
			float4 _DissolveTex_ST;
			float4 _FadeParam;
			float4 _XTime;
			float _Preview;
			float _SpeedFactor;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;
				
				OUT.color = IN.color * _Color;

				OUT.texcoord2 = TRANSFORM_TEX(IN.texcoord1,_DissolveTex);
				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				fixed dissolve_c_r = tex2D(_DissolveTex,IN.texcoord2).r;

				fixed param = saturate((_XTime.y-_FadeParam.x)/(_FadeParam.y-_FadeParam.x));
				
				param = lerp(param,_Preview,step(0.0001,_Preview));
				//return fixed4(IN.texcoord.x,0,IN.texcoord.y,1); // test script
				//return fixed4(dissolve_c_r,dissolve_c_r,dissolve_c_r,1); // test script

				param = EaseOutCubic(param);

				float factor = _FadeParam.z > 0 ? 1 - param :param;
				//return fixed4(factor,factor,factor,1); // test script

				clip(dissolve_c_r - factor);
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif
				float rate = factor / dissolve_c_r;
				rate = 1 - rate;
				color.a *= _FadeParam.w > 0 ? rate : 1;

				color.a = rate >= 1? color.a * rate : color.a;
				
				return color;
			}

		ENDCG
		}
	}
}
