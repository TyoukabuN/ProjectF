Shader "X_Shader/PostProcess/ChromaKey"
{
	HLSLINCLUDE

		#include "../StdLib.hlsl"
		#include "../Colors.hlsl"
		#include "../Sampling.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

        float4 _MainTex_TexelSize;//("KeyColor", Color) = (0, 1, 0, 0)
        float4 _KeyColor;//("KeyColor", Color) = (0, 1, 0, 0)
        float4 _TintColor;//("TintColor", Color) = (1, 1, 1, 1)
        float _ColorCutoff;//("Cutoff", Range(0, 1)) = 0.2
        float _ColorFeathering;//("ColorFeathering", Range(0, 1)) = 0.33
        float _MaskFeathering;//("MaskFeathering", Range(0, 1)) = 1
        float _Sharpening;//("Sharpening", Range(0, 1)) = 0.5
        float _Despill;//("DespillStrength", Range(0, 1)) = 1
        float _DespillLuminanceAdd;;//("DespillLuminanceAdd", Range(0, 1)) = 0.2
        float rgb2y(float3 c)
        {
            return (0.299 * c.r + 0.587 * c.g + 0.114 * c.b);
        }
        
        float rgb2cb(float3 c)
        {
            return (0.5 + -0.168736 * c.r - 0.331264 * c.g + 0.5 * c.b);
        }

        float rgb2cr(float3 c)
        {
            return (0.5 + 0.5 * c.r - 0.418688 * c.g - 0.081312 * c.b);
        }
        float colorclose(float Cb_p, float Cr_p, float Cb_key, float Cr_key, float tola, float tolb)
        {
            float temp = (Cb_key - Cb_p) * (Cb_key - Cb_p) + (Cr_key - Cr_p) * (Cr_key - Cr_p);
            float tola2 = tola * tola;
            float tolb2 = tolb * tolb;
            if (temp < tola2) return (0);
            if (temp < tolb2) return (temp - tola2) / (tolb2 - tola2);
            return (1);
        }

        float maskedTex2D(sampler2D tex, float2 uv)
        {
            float4 color = tex2D(tex, uv);

            // Chroma key to CYK conversion
            float key_cb = rgb2cb(float3(0, 1, 0));
            float key_cr = rgb2cr(float3(0, 1, 0));
            float pix_cb = rgb2cb(color.rgb);
            float pix_cr = rgb2cr(color.rgb);

            return colorclose(pix_cb, pix_cr, key_cb, key_cr, _ColorCutoff, _ColorFeathering);
        }
        half4 ChromaKey(VaryingsDefault i) : SV_Target
        {
            // Get pixel width
            float2 pixelWidth = float2(1.0 / _MainTex_TexelSize.z, 0);
            float2 pixelHeight = float2(0, 1.0 / _MainTex_TexelSize.w);


            //float2 uv = uv.xy;
            //half4 grab = GRABXYPIXEL(0,0);

            // Unmodified MainTex
            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            //return float4(1,1,1,1);

            // Unfeathered mask
            float mask = maskedTex2D(_MainTex, i.texcoord);

            // Feathering & smoothing
            float c = mask;
            float r = maskedTex2D(_MainTex, i.texcoord + pixelWidth);
            float l = maskedTex2D(_MainTex, i.texcoord - pixelWidth);
            float d = maskedTex2D(_MainTex, i.texcoord + pixelHeight);
            float u = maskedTex2D(_MainTex, i.texcoord - pixelHeight);
            float rd = maskedTex2D(_MainTex, i.texcoord + pixelWidth + pixelHeight) * .707;
            float dl = maskedTex2D(_MainTex, i.texcoord - pixelWidth + pixelHeight) * .707;
            float lu = maskedTex2D(_MainTex, i.texcoord - pixelHeight - pixelWidth) * .707;
            float ur = maskedTex2D(_MainTex, i.texcoord + pixelWidth - pixelHeight) * .707;
            float blurContribution = (r + l + d + u + rd + dl + lu + ur + c) * 0.12774655;
            float smoothedMask = smoothstep(_Sharpening, 1, lerp(c, blurContribution, _MaskFeathering));
            float4 result = color * smoothedMask;

            // Despill
            float v = (2 * result.b + result.r) / 4;
            if (result.g > v) result.g = lerp(result.g, v, _Despill);
            float4 dif = (color - result);
            float desaturatedDif = rgb2y(dif.xyz);
            result += lerp(0, desaturatedDif, _DespillLuminanceAdd);

            return float4(result.xyz, smoothedMask);// 
        }

	ENDHLSL



	SubShader
	{
        Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

				#pragma vertex VertDefault  
				#pragma fragment ChromaKey  

			ENDHLSL
		}
	}

}
