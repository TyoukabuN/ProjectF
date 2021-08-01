Shader "X_Shader/C_Charactar/PBR/StandardXray"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		_ColorPower("ColorPower",Range(1,3)) = 1.0
		_LightPower("LightPower",Range(0.1,1)) = 1
		_DiffuseScatteringColor("Diffuse scattering", Color) = (0,0,0,0)
		_DiffuseScatteringExponent("Diffuse scattering exponent", Range(2,20)) = 8
		_DiffuseScatteringOffset("Diffuse scattering offset", Range(-0.5,0.5)) = 0
		_DiffuseScatteringDir("DiffuseScattering dirction",Vector) = (0,0,0,0)

		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_BumpSMap("NormalS Map (B:LightPower)", 2D) = "bump" {}
		_BumpScale("Scale", Float) = 1.0

		_MetallicGlossMap("MetallicGloss Map(R:Metallic  G:Gloss G:AO)", 2D) = "white" {}
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Occlusion("Occlusion", Range(0,1)) = 1

        _LowColor("Color",Color) = (1,1,1,1)
        _LowMatCap("LowMatCap (RGB)", 2D) = "white" {}
        _LowMatCapPower("LowPower",Range(0.25,8.0)) = 2.0


        [Toggle(_ONINPUTRENDERER)] _InputRendererParameter ("_InputRendererParameter", Float) = 0
        _EnabledCustomLight("EnabledCustomLight",Float) = 0.0
        _LightColor ("LightColor", Color) = (1,1,1,1)
        _LightDir ("LightDir", Vector) = (0.89,-0.24,0.37,0.08)
        [NoScaleOffset] _GlossyEnvMap("GlossyEnvMap", Cube) = "black" {}


        [KeywordEnum(High,Middle,Low)] _Quality("Quality", Float) = 0

        _Saturation("Saturation", Range(-1,1)) = 1

        // SH lighting environment
        [HideInInspector] _xSHAr("_xSHAr", Vector) = (-0.0075,-0.015,0.009,0.16)
        [HideInInspector] _xSHAg("_xSHAg", Vector) = (-0.012,0.028,0.015,0.2)
        [HideInInspector] _xSHAb("_xSHAb", Vector) = (-0.023,0.11,0.027,0.26)
        [HideInInspector] _xSHBr("_xSHBr", Vector) = (-0.0056,0.0066,0.031,-0.021)
        [HideInInspector] _xSHBg("_xSHAr", Vector) = (-0.0094,0.011,0.045,-0.032)
        [HideInInspector] _xSHBb("_xSHBg", Vector) = (-0.018,0.021,0.057,-0.047)
        [HideInInspector] _xSHC ("_xSHC", Vector)  = (0.027,0.039,0.047,1)

		 // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 300
		UsePass "X_Shader/C_Charactar/Xray/XRAY"
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 3.0

            #pragma multi_compile _ _QUALITY_HIGH _QUALITY_MIDDLE _QUALITY_LOW
            #pragma multi_compile _ _ONINPUTRENDERER



            #if _QUALITY_HIGH
                #define _NORMALMAP 1
            #elif _QUALITY_MIDDLE
                #define _NORMALMAP 0
            #else
                #define _NORMALMAP 0
            #endif


            #pragma multi_compile  _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            // #pragma shader_feature _ _TONEMAPPINGENABLED_ON


        	#pragma skip_variants SHADOWS_SHADOWMASK DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING VERTEXLIGHT_ON FOG_EXP FOG_EXP2 POINT SPOT

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #pragma vertex vertBase
            #pragma fragment fragBase
            #include "CharactarPBRCore.cginc"

            #ifndef _QUALITY_LOW
		          VertexOutputForwardBase vertBase (VertexInput v) { return vertForwardBase(v); }
		          half4 fragBase (VertexOutputForwardBase i) : SV_Target { return fragForwardBaseInternal(i); }
            #else
                VertexOutputSpce vertBase(appdata_base v){ return vertFastSpce(v); }
                fixed4 fragBase(VertexOutputSpce i):SV_Target{ return fragFastSpce(i); }
            #endif
            ENDCG
        }

        // ------------------------------------------------------------------
        // Additive forward pass (one light per pass)
        // Pass
        // {

        //     Name "FORWARD_DELTA"
        //     Tags { "LightMode" = "ForwardAdd" }
        //     Blend [_SrcBlend] One
        //     Fog { Color (0,0,0,0) } // in additive pass fog should be black
        //     ZWrite Off
        //     ZTest LEqual

        //     CGPROGRAM
        //     #pragma target 3.0

        //     #pragma shader_feature _NORMALMAP
        //     #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON

        //     #pragma multi_compile_fwdadd_fullshadows
        //     #pragma multi_compile_fog

        //     #pragma vertex vertAdd
        //     #pragma fragment fragAdd
        //     #include "CharactarPBRCore.cginc"

        //     VertexOutputForwardAdd vertAdd (VertexInput v) { return vertForwardAdd(v); }
        //     half4 fragAdd (VertexOutputForwardAdd i) : SV_Target { return fragForwardAddInternal(i); }
        //     ENDCG
        // }

        // ------------------------------------------------------------------
        //  Shadow rendering pass
	    Pass 
	    {
	        Name "ShadowCaster"
	        Tags { "LightMode" = "ShadowCaster" }

	        ZWrite On ZTest LEqual

	        CGPROGRAM
            #pragma target 3.0

            // -------------------------------------
            #pragma skip_variants SHADOWS_CUBE FOG_EXP FOG_EXP2 POINT SPOT
            #pragma multi_compile _ _ALPHATEST_ON _ALPHABLEND_ON
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"
	       ENDCG
	    }
	}

	FallBack "Mobile/VertexLit"
    CustomEditor "CharactarPBRShaderGUI"
}
