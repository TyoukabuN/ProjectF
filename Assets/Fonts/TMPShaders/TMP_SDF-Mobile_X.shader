// Simplified SDF shader:
// - No Shading Option (bevel / bump / env map)
// - No Glow Option
// - Softness is applied on both side of the outline

Shader "TextMeshPro/Mobile/Distance Field X" {

Properties {

	_FaceColor			("Face Color", Color) = (1,1,1,1)
	_FaceDilate			("Face Dilate", Range(-1,1)) = 0

	_OutlineColor		("Outline Color", Color) = (0,0,0,1)
	_OutlineWidth		("Outline Thickness", Range(0,1)) = 1
	_OutlineWidthEX		("Outline Width",Range(0,1)) = 0.5
	_OutlineSoftness	("Outline Softness", Range(0,1)) = 0
 	_OutlineBrightness 	("Outline Brightness",Range(1,5)) = 1.5

	_UnderlayColor		("Border Color", Color) = (0,0,0,.5)
	_UnderlayOffsetX 	("Border OffsetX", Range(-2,2)) = 0
	_UnderlayOffsetY 	("Border OffsetY", Range(-2,2)) = 0
	_UnderlayDilate		("Border Dilate", Range(-2,2)) = 0 
	_UnderlaySoftness 	("Border Softness", Range(0,1)) = 0
	//_UnderlayDirection 	("Underlay Direction",Range(0,315)) = 0 
	//_UnderlayWidth		("Underlay Width",Range(0,1)) = 1  
	_WeightNormal		("Weight Normal", float) = 0  
	_WeightBold			("Weight Bold", float) = .5

	_ShaderFlags		("Flags", float) = 0
	_ScaleRatioA		("Scale RatioA", float) = 1
	_ScaleRatioB		("Scale RatioB", float) = 1
	_ScaleRatioC		("Scale RatioC", float) = 1
 
	_MainTex			("Font Atlas", 2D) = "white" {}
	_TextureWidth		("Texture Width", float) = 512
	_TextureHeight		("Texture Height", float) = 512
	_GradientScale		("Gradient Scale", float) = 5
	_ScaleX				("Scale X", float) = 1
	_ScaleY				("Scale Y", float) = 1
	_PerspectiveFilter	("Perspective Correction", Range(0, 1)) = 0.875
	_Sharpness			("Sharpness", Range(-1,1)) = 0

	_VertexOffsetX		("Vertex OffsetX", float) = 0
	_VertexOffsetY		("Vertex OffsetY", float) = 0

	_ClipRect			("Clip Rect", vector) = (-32767, -32767, 32767, 32767)
	_MaskSoftnessX		("Mask SoftnessX", float) = 0
	_MaskSoftnessY		("Mask SoftnessY", float) = 0
	
	_StencilComp		("Stencil Comparison", Float) = 8
	_Stencil			("Stencil ID", Float) = 0
	_StencilOp			("Stencil Operation", Float) = 0
	_StencilWriteMask	("Stencil Write Mask", Float) = 255
	_StencilReadMask	("Stencil Read Mask", Float) = 255
	
	_ColorMask			("Color Mask", Float) = 15
	[Toggle]_IsGray		("Gray",Float) = 0
}

SubShader {
	Tags 
	{
		"Queue"="Transparent"
		"IgnoreProjector"="True"
		"RenderType"="Transparent"
	}


	Stencil
	{
		Ref [_Stencil]
		Comp [_StencilComp]
		Pass [_StencilOp] 
		ReadMask [_StencilReadMask]
		WriteMask [_StencilWriteMask]
	}

	Cull [_CullMode]
	ZWrite Off
	Lighting Off
	Fog { Mode Off }
	ZTest [unity_GUIZTestMode]
	Blend One OneMinusSrcAlpha
	ColorMask [_ColorMask]

	Pass {
		CGPROGRAM
		#pragma vertex VertShader
		#pragma fragment PixShader
		// #pragma multi_compile __ OUTLINE_ON
		// #pragma multi_compile __ UNDERLAY_ON UNDERLAY_INNER

		#pragma multi_compile __ UNITY_UI_CLIP_RECT
		#pragma multi_compile __ UNITY_UI_ALPHACLIP

		#include "UnityCG.cginc"
		#include "UnityUI.cginc"
		#include "TMPro_Properties.cginc"

		struct vertex_t {
			UNITY_VERTEX_INPUT_INSTANCE_ID
			float4	vertex			: POSITION;
			float3	normal			: NORMAL;
			float4	tangent			: TANGENT;
			fixed4	color			: COLOR;
			float2	texcoord0		: TEXCOORD0;
			float2	texcoord1		: TEXCOORD1;

			float2	texcoord2		: TEXCOORD2;
			float2	texcoord3		: TEXCOORD3;
		};

		struct pixel_t {
			UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
			float4	vertex			: SV_POSITION;
			fixed4	faceColor		: COLOR;
			fixed4	outlineColor	: COLOR1;
			float4	texcoord0		: TEXCOORD0;			// Texture UV, Mask UV
			half4	param			: TEXCOORD1;			// Scale(x), BiasIn(y), BiasOut(z), Bias(w)
			half4	mask			: TEXCOORD2;			// Position in clip space(xy), Softness(zw)
			// #if (UNDERLAY_ON | UNDERLAY_INNER)
			float4	texcoord1		: TEXCOORD3;			// Texture UV, alpha, reserved
			half4	underlayParam	: TEXCOORD4;			// Scale(x), Bias(y)
			fixed4	underlayColor   : TEXCOORD5;
			fixed2  texcoord2	    : TEXCOORD6;
			float4  debug			: TEXCOORD7;
			// #endif
		}; 

		float _OutlineWidthEX; 
		//float _UnderlayDirection;
		//float _UnderlayWidth;
		float _OutlineBrightness;

        sampler2D _ParamTexture;
        float4 _ParamTexture_ST;
        float4 _ParamTexture_TexelSize;

        inline float2 GetParamTexcoord(float2 index)
        {
            float unitX = _ParamTexture_TexelSize.x;
            float unitY = _ParamTexture_TexelSize.y;
            index *= _ParamTexture_TexelSize.xy;
            index += _ParamTexture_TexelSize.xy * 0.5;

            return index;
        }

		//8个方向采样
		fixed SamplePixelAlpha(int index, pixel_t input, float length)
		{
		    const fixed sinArray[8] = { 0, 0.707,1,0.707,0,-0.707,-1,-0.707};
			const fixed cosArray[8] = {1,0.707,0,-0.707,-1,-0.707,0,0.707};
		    float2 pos = input.texcoord0 + float2(1/_TextureWidth,1/_TextureHeight) * float2(cosArray[index], sinArray[index]) * length;
			return tex2D(_MainTex, pos).w * 1;
		}

		fixed3 decodeRGB(float colorInt)
		{
			colorInt = int(colorInt);
			int rValue = colorInt / 256 / 256 ;
			int gValue = colorInt / 256 - rValue * 256;
			int bValue = colorInt - rValue * 256 * 256 - gValue * 256;
			return fixed3(rValue / 255.0, gValue / 255.0, bValue / 255.0);
		}

		fixed4 decodeRGB6bit(float colorInt)
		{
			colorInt = int(colorInt);
			int rValue = colorInt / 64 / 64 / 64;
			int gValue = colorInt / 64 / 64 - rValue * 64;
			int bValue = colorInt / 64 - rValue * 64 * 64 - gValue * 64;
			int aValue = colorInt - rValue * 64 * 64 * 64 - gValue * 64 * 64 - bValue * 64;
			return fixed4(rValue / 63.0, gValue / 63.0, bValue / 63.0,aValue / 63.0);
		}

		pixel_t VertShader(vertex_t input)
		{
			pixel_t output;

			UNITY_INITIALIZE_OUTPUT(pixel_t, output);
			UNITY_SETUP_INSTANCE_ID(input);
			UNITY_TRANSFER_INSTANCE_ID(input, output);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);


			//fixed4 outCol = fixed4(decodeRGB(input.tangent.r),1);
			//output.debug = outCol;
			fixed4 decodeR = decodeRGB6bit(input.texcoord3.r);
			fixed outlineSoftness = decodeR.r;
			fixed faceDilate = decodeR.g * 2 - 1;
			fixed outlineWidth = decodeR.b;
			fixed outlineWidthEX = decodeR.a;

			fixed4 decodeG = decodeRGB6bit(input.texcoord3.g);
			half outlineBrightness = decodeG.r * 4 + 1;
			fixed underlayOffsetX = decodeG.g * 4 - 2;
			fixed underlayOffsetY = decodeG.b * 4 - 2;
			fixed underlayDilate = decodeG.a * 4 - 2;

			fixed4 decodeW = decodeRGB6bit(input.tangent.w);
			fixed underlaySoftness = decodeW.r;
			fixed2 alpha = decodeW.gb;

			int outlineOrUnderlay = int(input.tangent.w) % 64;
			fixed isOutline = outlineOrUnderlay % 64 / 8;
			fixed isUnderlay = outlineOrUnderlay % 64 % 8;
			
			//ratio_A
            float weightNW = max(_WeightNormal, _WeightBold) / 4.0f;
            float t = max(1, weightNW + faceDilate + outlineWidth + outlineSoftness);
            float ScaleRatioA = (_GradientScale - 1.0) / (_GradientScale * t);

			//ratio_C
			float range = (weightNW + faceDilate) * (_GradientScale - 1.0);
			t = max(1, max(abs(underlayOffsetX), abs(underlayOffsetY)) + underlayDilate + underlaySoftness);
			float ScaleRatioC = max(0, _GradientScale - 1.0 - range) / (_GradientScale * t);

			float bold = step(input.texcoord1.y, 0);

			float4 vert = input.vertex;
			vert.x += _VertexOffsetX;
			vert.y += _VertexOffsetY;
			float4 vPosition = UnityObjectToClipPos(vert);

			float2 pixelSize = vPosition.w;
			pixelSize /= float2(_ScaleX, _ScaleY) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
			
			float scale = rsqrt(dot(pixelSize, pixelSize));
			scale *= abs(input.texcoord1.y) * _GradientScale * (_Sharpness + 1);
			if(UNITY_MATRIX_P[3][3] == 0) scale = lerp(abs(scale) * (1 - _PerspectiveFilter), scale, abs(dot(UnityObjectToWorldNormal(input.normal.xyz), normalize(WorldSpaceViewDir(vert)))));

			float weight = lerp(_WeightNormal, _WeightBold, bold) / 4.0;
			weight = (weight + faceDilate) * ScaleRatioA * 0.5;

			float layerScale = scale;

			scale /= 1 + (outlineSoftness * ScaleRatioA * scale);
			float bias = (0.5 - weight) * scale - 0.5;
			float outline = outlineWidth * ScaleRatioA * 0.5 * scale;

			float opacity = input.color.a;

			// #if (UNDERLAY_ON | UNDERLAY_INNER)
			if (isUnderlay > 0.1)
			{
				opacity = 1.0;
			}
			// #endif

			fixed4 faceColor = fixed4(input.color.rgb, opacity) * _FaceColor;
			faceColor.rgb *= faceColor.a;
		
			fixed4 outlineColor = fixed4(decodeRGB(input.texcoord2.r),alpha.r);
			outlineColor.a *= opacity;
			outlineColor.rgb *= outlineColor.a;
			outlineColor = lerp(faceColor, outlineColor, sqrt(min(1.0, (outline * 2))));

			// #if (UNDERLAY_ON | UNDERLAY_INNER)
			float2 layerOffset = float2(0, 0);
			float layerBias = 0;
			if (isUnderlay > 0.1)
			{	
				layerScale /= 1 + ((underlaySoftness * ScaleRatioC) * layerScale);
				layerBias = (.5 - weight) * layerScale - .5 - ((underlayDilate * ScaleRatioC) * .5 * layerScale);

				float x = -(underlayOffsetX * ScaleRatioC) * _GradientScale / _TextureWidth;
				float y = -(underlayOffsetY * ScaleRatioC) * _GradientScale / _TextureHeight;
				layerOffset = float2(x, y);
			}
			// #endif

			// Generate UV for the Masking Texture
			float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
			float2 maskUV = (vert.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);

			// Populate structure for pixel shader
			output.vertex = vPosition;
			output.faceColor = faceColor;
			output.outlineColor = outlineColor;
			output.texcoord0 = float4(input.texcoord0.x, input.texcoord0.y, maskUV.x, maskUV.y);
			output.param = half4(scale, bias - outline, bias + outline, bias);
			output.mask = half4(vert.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_MaskSoftnessX, _MaskSoftnessY) + pixelSize.xy));
			
			// #if (UNDERLAY_ON || UNDERLAY_INNER)
			if (isUnderlay > 0.1)
			{
				output.texcoord1 = float4(input.texcoord0 + layerOffset, input.color.a, 0);
				output.underlayColor = fixed4(decodeRGB(input.texcoord2.g),alpha.g);
			}
			output.underlayParam = half4(layerScale, layerBias,outlineWidthEX,outlineBrightness);
			output.texcoord2 = fixed2(isOutline, isUnderlay);

			//output.debug.xy = input.texcoord0 + input.texcoord1 + input.texcoord2 + input.texcoord3;
			// #endif

			return output;
		}
 
		// PIXEL SHADER

		fixed4 PixShader(pixel_t input) : SV_Target
		{
			float2 index = float2(0,0);
            fixed4 colorParam = tex2Dlod(_ParamTexture, float4(GetParamTexcoord(index),0,0));

			return fixed4(decodeRGB(colorParam.r),1);

			fixed isOutline = input.texcoord2.x;
			fixed isUnderlay = input.texcoord2.y;
			UNITY_SETUP_INSTANCE_ID(input);
			//return tex2D(_MainTex, input.texcoord0.xy);
			half d = tex2D(_MainTex, input.texcoord0.xy).a * input.param.x;

			half4 c = input.faceColor * 1.2 * saturate(d - input.param.w);
			c *= saturate(d - input.param.y);
			half4 val = half4(0,0,0,0);
			half d2;
			half4 c2;
			
			// #ifdef OUTLINE_ON  
			if (isOutline > 0.1)
			{
				c2 = input.outlineColor; 
				c2.rgb = lerp(input.outlineColor.rgb, half3(0.43,0.43,0.43), _IsGray);
				c2.rgb *= input.outlineColor.a;

				half sampleDistance = 1;  
				val.w += SamplePixelAlpha(0, input, sampleDistance); 
				val.w += SamplePixelAlpha(4, input, sampleDistance);
				d2 = val.w * input.param.x * input.underlayParam.z * 0.8;
				//c2 = input.faceColor * saturate(d2 - input.param.w );
				c2 *= saturate(d2 - input.param.y);
				c = (c2 * (1.0 - c.a)) + (c * c.a); 
				c.rgb *= input.underlayParam.w;
			}
				
				//c = (c2 * (1.0 - c.a)) + (c * c.a * 1.5); 
			// #endif 
			// #if UNDERLAY_ON
			if (isUnderlay > 0.1)
			{
				d = tex2D(_MainTex, input.texcoord1.xy).a * input.underlayParam.x;
				c += float4(input.underlayColor.rgb * input.underlayColor.a, input.underlayColor.a) * saturate(d - input.underlayParam.y) * (1 - c.a);
			}

			// #endif

			// #if UNDERLAY_INNER
			// half sd = saturate(d - input.param.z);
			// d = GetChannel(tex2D(_MainTex, input.texcoord1.xy)) * input.underlayParam.x;
			// c += float4(input.underlayColor.rgb * input.underlayColor.a, input.underlayColor.a) * (1 - saturate(d - input.underlayParam.y)) * sd * (1 - c.a);
			// #endif

			// Alternative implementation to UnityGet2DClipping with support for softness.
			#if UNITY_UI_CLIP_RECT
			half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(input.mask.xy)) * input.mask.zw);
			c *= m.x * m.y;
			#endif

			// #if (UNDERLAY_ON | UNDERLAY_INNER)
			if (isUnderlay > 0.1)
			{
				c *= input.texcoord1.z;
			}
			// #endif

			#if UNITY_UI_ALPHACLIP
			clip(c.a - 0.001);
			#endif
			
			return c;
		}
		ENDCG
	}
}

CustomEditor "TMPro.EditorUtilities.TMP_SDFShaderGUI_X"
}
