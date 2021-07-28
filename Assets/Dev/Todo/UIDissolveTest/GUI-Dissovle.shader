Shader "X_Shader/G_GUI/Dissovle"
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
		_DissolvePower ("DissolvePower", Float) = 0
		_DissolvEdgeColor("Dissolv EdgeColor",Color) = (0.91,0.25,0.05,1.0)
 		_EdgeColor("edge color",Color) = (0.09,0.09,0.09,1)//边缘颜色

		_ColorMask ("Color Mask", Float) = 15

		_DissolveFactor1("dissolve factor1",range(0,1)) = 0.377//溶解因子1,大于这个因子就向溶解色过渡
        _DissolveFactor2("dissolve factor2",range(0,1)) = 0.595//溶解因子2,大于这个因子就向边缘色过渡

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

		_DissolvePreview("dissolve Preview",Range(0,1)) = 0
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
		//ColorMask [_ColorMask]

		Pass
		{
			Name "Default"
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

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

			fixed3 _DissolvEdgeColor;
			sampler2D _DissolveTex;
			float4 _DissolveTex_ST;
			float _DissolveFactor1;
            float _DissolveFactor2;
			float _DissolvePower;
			float4 _EdgeColor;
			float4 _XTime;
			float _DissolvePreview;

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
				float param = lerp(_XTime.y - _DissolvePower,_DissolvePreview,step(0.01,_DissolvePreview));
				float factor = lerp(0,1,param);
				// fixed v = (f + _DissolvePower) - 1;
				// if(factor > dissolve_c_r)
                // {
                //     discard;
                // }
				clip(dissolve_c_r - factor);//优化代码
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif
				float rate = factor / dissolve_c_r;

				rate = step(_DissolveFactor1,rate);
				rate = rate * factor / dissolve_c_r;
  				color.rgb = lerp(color.rgb,_DissolvEdgeColor.rgb,rate);//优化代码

				rate = step(_DissolveFactor2,rate);
				rate = rate * factor / dissolve_c_r;
				color.rgb = lerp(color.rgb,_EdgeColor.rgb,rate);//优化代码

				
				// if(rate > _DissolveFactor1) 
                // {
                //     //向溶解色过渡
                //     color.rgb = lerp(color.rgb,_DissolvEdgeColor.rgb,rate);
                //   if(rate>_DissolveFactor2)
                //   {
                //     //向边缘色过渡
                //     color.rgb = lerp(color.rgb, _EdgeColor.rgb,rate);
                // 	 }
                // }
				
				return color;
			}

		ENDCG
		}
	}
}
