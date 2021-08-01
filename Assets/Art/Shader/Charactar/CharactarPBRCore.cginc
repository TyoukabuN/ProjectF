






#ifndef CHARACTARPBRCORE_INCLUDED
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
#define CHARACTARPBRCORE_INCLUDED

#include "UnityCG.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityInstancing.cginc"
#include "UnityStandardConfig.cginc"
// #include "UnityStandardInput.cginc"
#include "UnityPBSLighting.cginc"
#include "UnityStandardUtils.cginc"
#include "UnityGBuffer.cginc"
#include "UnityStandardBRDF.cginc"
#include "AutoLight.cginc"
#include "CharactarEffectCore.cginc"








//---------------------------------------
// Directional lightmaps & Parallax require tangent space too
#if (_NORMALMAP || DIRLIGHTMAP_COMBINED || _PARALLAXMAP)
    #define _TANGENT_TO_WORLD 1
#endif


half4       _Color;
half        _ColorPower;
half        _LightPower;
half4       _DiffuseScatteringColor;
float       _DiffuseScatteringExponent;
half        _DiffuseScatteringOffset;
half3       _DiffuseScatteringDir;

half        _Cutoff;

sampler2D   _MainTex;
float4      _MainTex_ST;

sampler2D   _BumpSMap;
half        _BumpScale;

sampler2D   _MetallicGlossMap;
half        _Metallic;
float       _Glossiness;

half        _Occlusion;

fixed4 _LowColor;
sampler2D _LowMatCap;
half _LowMatCapPower;

half4 _RimColor;
half4 _GlobalRimColor;
half4 _ReplaceColor;

half _Alphactl;
half4 _FadeParameter;
float3 _SpecOffset;
float _SpecRange;

half        _InputRendererParameter;
fixed       _EnabledCustomLight;
half3       _LightColor;
half3       _LightDir;
UNITY_DECLARE_TEXCUBE(_GlossyEnvMap);
half4       _GlossyEnvMap_HDR;
fixed       _LightCameraDir;

#if defined(_MULTICOLOR)

sampler2D _MultiColorMask;
fixed4 _MultiColorR;
fixed4 _MultiColorG;
fixed4 _MultiColorB;

#endif

float4 _HitRimParameter;


float4  _xSHAr;
float4  _xSHAg;
float4  _xSHAb;
float4  _xSHBr;
float4  _xSHBg;
float4  _xSHBb;
float4  _xSHC;

fixed _Frozen;
fixed _ElectricFlow;
fixed _Shining;
fixed _Saturation;


half4 BRDF_MY_Unity_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
    float3 normal, float3 viewDir,
    UnityLight light, UnityIndirect gi)
{
    float3 halfDir = Unity_SafeNormalize (float3(light.dir) + viewDir);

    half nl = saturate(dot(normal, light.dir));
    half nlr = saturate(dot(viewDir, -light.dir + normal));

    float nh = saturate(dot(normal, halfDir));
    half nv = saturate(dot(normal, viewDir));
    float lh = saturate(dot(light.dir, halfDir));

    // Specular term
    half perceptualRoughness = SmoothnessToPerceptualRoughness (smoothness);
    half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);

#if UNITY_BRDF_GGX

    // GGX Distribution multiplied by combined approximation of Visibility and Fresnel
    // See "Optimizing PBR for Mobile" from Siggraph 2015 moving mobile graphics course
    // https://community.arm.com/events/1155
    half a = roughness;
    float a2 = a*a;

    float d = nh * nh * (a2 - 1.f) + 1.00001f;
#ifdef UNITY_COLORSPACE_GAMMA
    // Tighter approximation for Gamma only rendering mode!
    // DVF = sqrt(DVF);
    // DVF = (a * sqrt(.25)) / (max(sqrt(0.1), lh)*sqrt(roughness + .5) * d);
    float specularTerm = a / (max(0.32f, lh) * (1.5f + roughness) * d);
#else
    float specularTerm = a2 / (max(0.1f, lh*lh) * (roughness + 0.5f) * (d * d) * 4);
#endif

    // on mobiles (where half actually means something) denominator have risk of overflow
    // clamp below was added specifically to "fix" that, but dx compiler (we convert bytecode to metal/gles)
    // sees that specularTerm have only non-negative terms, so it skips max(0,..) in clamp (leaving only min(100,...))
#if defined (SHADER_API_MOBILE)
    specularTerm = specularTerm - 1e-4f;
#endif

#else

    // Legacy
    half specularPower = PerceptualRoughnessToSpecPower(perceptualRoughness);
    // Modified with approximate Visibility function that takes roughness into account
    // Original ((n+1)*N.H^n) / (8*Pi * L.H^3) didn't take into account roughness
    // and produced extremely bright specular at grazing angles

    half invV = lh * lh * smoothness + perceptualRoughness * perceptualRoughness; // approx ModifiedKelemenVisibilityTerm(lh, perceptualRoughness);
    half invF = lh;

    half specularTerm = ((specularPower + 1) * pow (nh, specularPower)) / (8 * invV * invF + 1e-4h);

#ifdef UNITY_COLORSPACE_GAMMA
    specularTerm = sqrt(max(1e-4f, specularTerm));
#endif

#endif

#if defined (SHADER_API_MOBILE)
    specularTerm = clamp(specularTerm, 0.0, 100.0); // Prevent FP16 overflow on mobiles
#endif
#if defined(_SPECULARHIGHLIGHTS_OFF)
    specularTerm = 0.0;
#endif

    // surfaceReduction = Int D(NdotH) * NdotH * Id(NdotL>0) dH = 1/(realRoughness^2+1)

    // 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
    // 1-x^3*(0.6-0.08*x)   approximation for 1/(x^4+1)
#ifdef UNITY_COLORSPACE_GAMMA
    half surfaceReduction = 0.28;
#else
    half surfaceReduction = (0.6-0.08*perceptualRoughness);
#endif

    surfaceReduction = 1.0 - roughness*perceptualRoughness*surfaceReduction;

    half grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));
    // half3 color =  (diffColor + specularTerm * specColor) * light.color * nl + gi.diffuse * diffColor; 
    //                 + surfaceReduction * gi.specular * FresnelLerpFast (specColor , grazingTerm, nv);

	fixed4 lcolor = fixed4(1,1,1,1);

	float TranslucencyF = 0.5;
	half3 lightDir = light.dir + normal;
	half transVdotL = saturate(dot(viewDir, -lightDir));
	half3 translucency = lcolor.rgb * (transVdotL + gi.diffuse * 1) * TranslucencyF;
	translucency = half3(diffColor * translucency * TranslucencyF);



	half3 color = (diffColor + specularTerm * specColor) * light.color * nl + gi.diffuse * diffColor
                     + translucency
                     + surfaceReduction * gi.specular * specColor * grazingTerm;
    return half4(color, 1);
}



#define UNITY_BRDF_PBS BRDF_MY_Unity_PBS












struct VertexInput
{
    float4 vertex   : POSITION;
    half3 normal    : NORMAL;
    float2 uv0      : TEXCOORD0;
    float2 uv1      : TEXCOORD1;
#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
    float2 uv2      : TEXCOORD2;
#endif
#ifdef _TANGENT_TO_WORLD
    half4 tangent   : TANGENT;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};


float4 TexCoords(VertexInput v)
{
    float4 texcoord;
    texcoord.xy = TRANSFORM_TEX(v.uv0, _MainTex);
    return texcoord;
}


half3 Albedo(float4 texcoords)
{
	half3 tex = tex2D(_MainTex, texcoords.xy).rgb;

#if _MULTICOLOR
	half3 mask = tex2D(_MultiColorMask, texcoords.xy).rgb;
	fixed3 des = dot(tex.rgb, fixed3(0.3, 0.59, 0.11));
	fixed3 mcolor = fixed3(0, 0, 0); 
	mcolor += des * _MultiColorR.rgb * _MultiColorR.a * 10 *  mask.r;  
	mcolor += des * _MultiColorG.rgb * _MultiColorG.a * 10 *  mask.g;
	mcolor += des * _MultiColorB.rgb * _MultiColorB.a * 10 *  mask.b;
	mcolor += tex.rgb * (1 - min(mask.r + mask.g + mask.b,1));

    mcolor = lerp(mcolor,des * _ReplaceColor.rgb,_ReplaceColor.a);
#else
	fixed3 mcolor = dot(tex.rgb, fixed3(0.3, 0.59, 0.11));
    mcolor = lerp(tex.rgb,mcolor * _ReplaceColor.rgb,_ReplaceColor.a);
#endif
	half3 albedo = _Color.rgb * mcolor * _ColorPower;
    return albedo;
}


half AlphaFade(half ca)
{
    half curAlpha = ca * _Alphactl;

    half sAlpha = min(curAlpha,_FadeParameter.z);
    half toAlpha = min(curAlpha,_FadeParameter.w);
    half alpha = curAlpha;
    float time = _XTime.y;
    // float time = _HitRimParameter.w;
    fixed fade01 = saturate((_FadeParameter.y - time)/(_FadeParameter.y - _FadeParameter.x));
    alpha = lerp(sAlpha,toAlpha,fade01);
    return alpha;
}



half Alpha(float2 uv)
{
    return tex2D(_MainTex, uv).a * _Color.a * _Alphactl;
}

half2 MetallicGloss(float2 uv)
{
    half2 mg;
    mg = tex2D(_MetallicGlossMap, uv).rg;
    mg.r *= _Metallic;
    mg.g *= _Glossiness;
    return mg;
}

float4 Parallax (float4 texcoords, half3 viewDir)
{
#if !defined(_PARALLAXMAP) || (SHADER_TARGET < 30)
    // Disable parallax on pre-SM3.0 shader target models
    return texcoords;
#else
    half h = tex2D (_ParallaxMap, texcoords.xy).g;
    float2 offset = ParallaxOffset1Step (h, _Parallax, viewDir);
    return float4(texcoords.xy + offset, texcoords.zw + offset);
#endif

}

half Skin(float2 uv)
{
    return tex2D(_BumpSMap, uv).b;
}


half Occlusion(float2 uv)
{
#if (SHADER_TARGET < 30)
    // SM20: instruction count limitation
    // SM20: simpler occlusion
    return tex2D(_MetallicGlossMap, uv).b;
#else
    half occ = tex2D(_MetallicGlossMap, uv).b;
    return LerpOneTo (occ, _Occlusion);
#endif
}


//-------------------------------------------------------------------------------------

half3 XUnpackScaleNormal(half4 packednormal, half bumpScale)
{
    packednormal.x *= packednormal.w;
    half3 normal;
    normal.xy = (packednormal.xy * 2 - 1) * bumpScale;
    normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
    return normal;
    // return UnpackScaleNormal(packednormal,bumpScale);
}



//逐顶点法线归一化
half3 NormalizePerVertexNormal (float3 n)
{
    #if (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
        return normalize(n);
    #else
        return n; // will normalize per-pixel instead
    #endif
}

//逐像素法线归一化
float3 NormalizePerPixelNormal (float3 n)
{
    #if (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
        return n;
    #else
        return normalize(n);
    #endif
}

//法线
half3 NormalInTangentSpace(float4 texcoords)
{
    half3 normalTangent = XUnpackScaleNormal(tex2D (_BumpSMap, texcoords.xy), _BumpScale);
    return normalTangent;
}


//-------------------------------------------------------------------------------------
UnityLight MainLight ()
{
    UnityLight l;

    //#if _ONINPUTRENDERER
    //    l.color = _LightColor;
    //    l.dir   = _LightDir;

    //#else

        l.color = lerp(_LightColor0.rgb, _LightColor, _EnabledCustomLight);
        l.dir   = lerp(_WorldSpaceLightPos0.xyz, _LightDir, _EnabledCustomLight);
    //#endif

    return l;
}

UnityLight AdditiveLight (half3 lightDir, half atten)
{
    UnityLight l;

    l.color = _LightColor0.rgb;
    l.dir = lightDir;
    #ifndef USING_DIRECTIONAL_LIGHT
        l.dir = NormalizePerPixelNormal(l.dir);
    #endif

    // shadow the light
    l.color *= atten;
    return l;
}

UnityLight DummyLight ()
{
    UnityLight l;
    l.color = 0;
    l.dir = half3 (0,1,0);
    return l;
}

UnityIndirect ZeroIndirect ()
{
    UnityIndirect ind;
    ind.diffuse = 0;
    ind.specular = 0;
    return ind;
}

//-------------------------------------------------------------------------------------
// Common fragment setup


//----------------------------------------------- GI ------------------------------------
// normal should be normalized, w=1.0
half3 XSHEvalLinearL0L1 (half4 normal)
{
    half3 x;

    // Linear (L1) + constant (L0) polynomial terms
    x.r = dot(_xSHAr,normal);
    x.g = dot(_xSHAg,normal);
    x.b = dot(_xSHAb,normal);

    return x;
}

// normal should be normalized, w=1.0
half3 XSHEvalLinearL2 (half4 normal)
{
    half3 x1, x2;
    // 4 of the quadratic (L2) polynomials
    half4 vB = normal.xyzz * normal.yzzx;
    x1.r = dot(_xSHBr,vB);
    x1.g = dot(_xSHBg,vB);
    x1.b = dot(_xSHBb,vB);

    // Final (5th) quadratic (L2) polynomial
    half vC = normal.x*normal.x - normal.y*normal.y;
    x2 = _xSHC.rgb * vC;

    return x1 + x2;
}



inline half3 XShadeSH9 (half4 normal)
{
    // Linear + constant polynomial terms
    half3 res = XSHEvalLinearL0L1 (normal);

    // Quadratic polynomials
    res += XSHEvalLinearL2 (normal);

#   ifdef UNITY_COLORSPACE_GAMMA
        res = LinearToGammaSpace (res);
#   endif

    return res;
}


half3 XShadeSHPerVertex (half3 normal, half3 ambient)
{
    #ifdef UNITY_COLORSPACE_GAMMA
        ambient = GammaToLinearSpace (ambient);
    #endif
    ambient += XSHEvalLinearL2 (half4(normal, 1.0));     
    return ambient;
}

half3 XShadeSHPerPixel (half3 normal, half3 ambient, float3 worldPos)
{
    half3 ambient_contrib = 0.0;
    ambient_contrib = XSHEvalLinearL0L1 (half4(normal, 1.0));
    ambient = max(half3(0, 0, 0), ambient + ambient_contrib);     
    #ifdef UNITY_COLORSPACE_GAMMA
        ambient = LinearToGammaSpace (ambient);
    #endif
    return ambient;

    // return ShadeSHPerPixel(normal,ambient,worldPos);
}



// deprecated
half3 WorldNormal(half4 tan2world[3])
{
    return normalize(tan2world[2].xyz);
}


float3 PerPixelWorldNormal(float4 i_tex, float4 tangentToWorld[3])
{
#ifdef _NORMALMAP
    half3 tangent = tangentToWorld[0].xyz;
    half3 binormal = tangentToWorld[1].xyz;
    half3 normal = tangentToWorld[2].xyz;

    #if UNITY_TANGENT_ORTHONORMALIZE
        normal = NormalizePerPixelNormal(normal);

        // ortho-normalize Tangent
        tangent = normalize (tangent - normal * dot(tangent, normal));

        // recalculate Binormal
        half3 newB = cross(normal, tangent);
        binormal = newB * sign (dot (newB, binormal));
    #endif

    half3 normalTangent = NormalInTangentSpace(i_tex);
    float3 normalWorld = NormalizePerPixelNormal(tangent * normalTangent.x + binormal * normalTangent.y + normal * normalTangent.z); // @TODO: see if we can squeeze this normalize on SM2.0 as well
#else
    float3 normalWorld = normalize(tangentToWorld[2].xyz);
#endif
    return normalWorld;
}

#ifdef _PARALLAXMAP
    #define IN_VIEWDIR4PARALLAX(i) NormalizePerPixelNormal(half3(i.tangentToWorldAndPackedData[0].w,i.tangentToWorldAndPackedData[1].w,i.tangentToWorldAndPackedData[2].w))
    #define IN_VIEWDIR4PARALLAX_FWDADD(i) NormalizePerPixelNormal(i.viewDirForParallax.xyz)
#else
    #define IN_VIEWDIR4PARALLAX(i) half3(0,0,0)
    #define IN_VIEWDIR4PARALLAX_FWDADD(i) half3(0,0,0)
#endif

#if UNITY_REQUIRE_FRAG_WORLDPOS
    #if UNITY_PACK_WORLDPOS_WITH_TANGENT
        #define IN_WORLDPOS(i) half3(i.tangentToWorldAndPackedData[0].w,i.tangentToWorldAndPackedData[1].w,i.tangentToWorldAndPackedData[2].w)
    #else
        #define IN_WORLDPOS(i) i.posWorld
    #endif
    #define IN_WORLDPOS_FWDADD(i) i.posWorld
#else
    #define IN_WORLDPOS(i) half3(0,0,0)
    #define IN_WORLDPOS_FWDADD(i) half3(0,0,0)
#endif

#define IN_LIGHTDIR_FWDADD(i) half3(i.tangentToWorldAndLightDir[0].w, i.tangentToWorldAndLightDir[1].w, i.tangentToWorldAndLightDir[2].w)


#define FRAGMENT_SETUP(x) FragmentCommonData x = \
    FragmentSetup(i.tex, i.eyeVec, IN_VIEWDIR4PARALLAX(i), i.tangentToWorldAndPackedData, IN_WORLDPOS(i));

#define FRAGMENT_SETUP_FWDADD(x) FragmentCommonData x = \
    FragmentSetup(i.tex, i.eyeVec, IN_VIEWDIR4PARALLAX_FWDADD(i), i.tangentToWorldAndLightDir, IN_WORLDPOS_FWDADD(i));


struct FragmentCommonData
{
    half3 diffColor, specColor;
    // Note: smoothness & oneMinusReflectivity for optimization purposes, mostly for DX9 SM2.0 level.
    // Most of the math is being done on these (1-x) values, and that saves a few precious ALU slots.
    half oneMinusReflectivity, smoothness;
    float3 normalWorld;
    float3 eyeVec;
    half alpha;
    float3 posWorld;

#if UNITY_STANDARD_SIMPLE
    half3 reflUVW;
#endif

#if UNITY_STANDARD_SIMPLE
    half3 tangentSpaceNormal;
#endif
};

#ifndef UNITY_SETUP_BRDF_INPUT
    #define UNITY_SETUP_BRDF_INPUT MetallicSetup
#endif


inline FragmentCommonData MetallicSetup (float4 i_tex)
{
    half2 metallicGloss = MetallicGloss(i_tex.xy);
    half metallic = metallicGloss.x;
    half smoothness = metallicGloss.y; // this is 1 minus the square root of real roughness m.

    half oneMinusReflectivity;
    half3 specColor;
    half3 diffColor = DiffuseAndSpecularFromMetallic (Albedo(i_tex), metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

    FragmentCommonData o = (FragmentCommonData)0;
    o.diffColor = diffColor;
    o.specColor = specColor;
    o.oneMinusReflectivity = oneMinusReflectivity;
    o.smoothness = smoothness;
    return o;
}

// parallax transformed texcoord is used to sample occlusion
inline FragmentCommonData FragmentSetup (inout float4 i_tex, float3 i_eyeVec, half3 i_viewDirForParallax, float4 tangentToWorld[3], float3 i_posWorld)
{
    i_tex = Parallax(i_tex, i_viewDirForParallax);

    half alpha = Alpha(i_tex.xy);
    #if defined(_ALPHATEST_ON)
        clip (alpha - _Cutoff);
    #endif

    FragmentCommonData o = UNITY_SETUP_BRDF_INPUT (i_tex);


    o.normalWorld = PerPixelWorldNormal(i_tex, tangentToWorld);
    o.eyeVec = NormalizePerPixelNormal(i_eyeVec);
    o.posWorld = i_posWorld;
    // NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
    o.diffColor = PreMultiplyAlpha (o.diffColor, alpha, o.oneMinusReflectivity, /*out*/ o.alpha);

    //DiffuseScattering
    half scatterNdotV = (dot(o.normalWorld, o.eyeVec + _DiffuseScatteringDir) + _DiffuseScatteringOffset);
    half scatter = exp2(-scatterNdotV * scatterNdotV*_DiffuseScatteringExponent);
    scatter *= scatter;
    o.diffColor += lerp(o.diffColor*_DiffuseScatteringColor.rgb, _DiffuseScatteringColor.rgb, _DiffuseScatteringColor.a)*scatter * 4;



    return o;
}

inline UnityGI FragmentGI (FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light, bool reflections)
{
    UnityGIInput d;
    d.light = light;
    d.worldPos = s.posWorld;
    d.worldViewDir = -s.eyeVec;
    d.atten = atten;
    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
        d.ambient = 0;
        d.lightmapUV = i_ambientOrLightmapUV;
    #else
        d.ambient = i_ambientOrLightmapUV.rgb;
        d.lightmapUV = 0;
    #endif

    d.probeHDR[0] = unity_SpecCube0_HDR;
    d.probeHDR[1] = unity_SpecCube1_HDR;
    #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
      d.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
    #endif
    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
      d.boxMax[0] = unity_SpecCube0_BoxMax;
      d.probePosition[0] = unity_SpecCube0_ProbePosition;
      d.boxMax[1] = unity_SpecCube1_BoxMax;
      d.boxMin[1] = unity_SpecCube1_BoxMin;
      d.probePosition[1] = unity_SpecCube1_ProbePosition;
    #endif

    UnityGI gi;
    if(reflections)
    {
        Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.smoothness, -s.eyeVec, s.normalWorld, s.specColor);
        // Replace the reflUVW if it has been compute in Vertex shader. Note: the compiler will optimize the calcul in UnityGlossyEnvironmentSetup itself
        #if UNITY_STANDARD_SIMPLE
            g.reflUVW = s.reflUVW;
        #endif

        gi = UnityGlobalIllumination (d, occlusion, s.normalWorld, g);
    }
    else
    {
        gi = UnityGlobalIllumination (d, occlusion, s.normalWorld);
    }


    return gi;
}


inline half3 XUnityGI_IndirectSpecular(UnityGIInput data, half occlusion, Unity_GlossyEnvironmentData glossIn)
{
    half3 specular;

    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
        // we will tweak reflUVW in glossIn directly (as we pass it to Unity_GlossyEnvironment twice for probe0 and probe1), so keep original to pass into BoxProjectedCubemapDirection
        half3 originalReflUVW = glossIn.reflUVW;
        glossIn.reflUVW = BoxProjectedCubemapDirection (originalReflUVW, data.worldPos, data.probePosition[0], data.boxMin[0], data.boxMax[0]);
    #endif

    #ifdef _GLOSSYREFLECTIONS_OFF
        specular = unity_IndirectSpecColor.rgb;
    #else
        half3 env0 = Unity_GlossyEnvironment (UNITY_PASS_TEXCUBE(_GlossyEnvMap), _GlossyEnvMap_HDR, glossIn);
        specular = env0;
    #endif

    return specular * occlusion;
}

inline UnityGI XUnityGlobalIllumination (UnityGIInput data, half occlusion, half3 normalWorld, Unity_GlossyEnvironmentData glossIn)
{
    UnityGI o_gi = UnityGI_Base(data, occlusion, normalWorld);
    o_gi.indirect.diffuse = data.ambient * occlusion;
    o_gi.indirect.specular = XUnityGI_IndirectSpecular(data, occlusion, glossIn);
    return o_gi;
}

inline UnityGI XUnityGlobalIllumination (UnityGIInput data, half occlusion, half3 normalWorld)
{
    return UnityGI_Base(data, occlusion, normalWorld);
}




inline UnityGI XFragmentGI (FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light, bool reflections)
{
    UnityGIInput d;
    d.light = light;
    d.worldPos = s.posWorld;
    d.worldViewDir = -s.eyeVec;
    d.atten = atten;
    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
        d.ambient = 0;
        d.lightmapUV = i_ambientOrLightmapUV;
    #else
        d.ambient = i_ambientOrLightmapUV.rgb;
        d.lightmapUV = 0;
    #endif

    d.probeHDR[0] = unity_SpecCube0_HDR;
    d.probeHDR[1] = unity_SpecCube1_HDR;
    #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
      d.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
    #endif
    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
      d.boxMax[0] = unity_SpecCube0_BoxMax;
      d.probePosition[0] = unity_SpecCube0_ProbePosition;
      d.boxMax[1] = unity_SpecCube1_BoxMax;
      d.boxMin[1] = unity_SpecCube1_BoxMin;
      d.probePosition[1] = unity_SpecCube1_ProbePosition;
    #endif

    UnityGI gi;
    if(reflections)
    {
        Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.smoothness, -s.eyeVec, s.normalWorld, s.specColor);
        // Replace the reflUVW if it has been compute in Vertex shader. Note: the compiler will optimize the calcul in UnityGlossyEnvironmentSetup itself
        #if UNITY_STANDARD_SIMPLE
            g.reflUVW = s.reflUVW;
        #endif
        gi = XUnityGlobalIllumination (d, occlusion, s.normalWorld, g);
    }
    else
    {
        gi = XUnityGlobalIllumination (d, occlusion, s.normalWorld);
    }
    return gi;
}


inline UnityGI FragmentGI (FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light)
{
    #if _ONINPUTRENDERER
    return XFragmentGI(s, occlusion, i_ambientOrLightmapUV, atten, light, true);
    #else
    return FragmentGI(s, occlusion, i_ambientOrLightmapUV, atten, light, true);
    #endif
}






//-------------------------------------------------------------------------------------
half4 OutputForward (half4 output, half alphaFromSurface)
{
    #if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
        output.a = alphaFromSurface;
    #else
        UNITY_OPAQUE_ALPHA(output.a);
    #endif
    return output;
}

inline half4 VertexGIForward(VertexInput v, float3 posWorld, half3 normalWorld)
{
    half4 ambientOrLightmapUV = 0;
    // Static lightmaps
    #ifdef LIGHTMAP_ON
        ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
        ambientOrLightmapUV.zw = 0;
    // Sample light probe for Dynamic objects only (no static or dynamic lightmaps)
    #elif UNITY_SHOULD_SAMPLE_SH
        #ifdef VERTEXLIGHT_ON
            // Approximated illumination from non-important point lights
            ambientOrLightmapUV.rgb = Shade4PointLights (
                unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                unity_4LightAtten0, posWorld, normalWorld);
        #endif

        #if _ONINPUTRENDERER
            ambientOrLightmapUV.rgb = XShadeSH9(float4(normalWorld,1.0));
        #else
            ambientOrLightmapUV.rgb = ShadeSHPerVertex (normalWorld, ambientOrLightmapUV.rgb);
        #endif

    #endif

    #ifdef DYNAMICLIGHTMAP_ON
        ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    #endif

    return ambientOrLightmapUV;
}

inline fixed4 XSaturation(fixed4 color)
{
    //saturation饱和度：首先根据公式计算同等亮度情况下饱和度最低的值：
	fixed gray = 0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
	fixed3 grayColor = fixed3(gray, gray, gray);
	//根据Saturation在饱和度最低的图像和原图之间差值
	color.rgb = lerp(grayColor, color.rgb, _Saturation);

    return color;
}

// ------------------------------------------------------------------
//  Base forward pass (directional light, emission, lightmaps, ...)
#ifndef UNITY_POSITION
#define UNITY_POSITION(pos) float4 pos : SV_POSITION
#endif

struct VertexOutputForwardBase
{
    UNITY_POSITION(pos);
    float4 tex                            : TEXCOORD0;
    float3 eyeVec                         : TEXCOORD1;
    float4 tangentToWorldAndPackedData[3] : TEXCOORD2;    // [3x3:tangentToWorld | 1x3:viewDirForParallax or worldPos]
    half4 ambientOrLightmapUV             : TEXCOORD5;    // SH or Lightmap UV
    UNITY_SHADOW_COORDS(6)
    UNITY_FOG_COORDS(7)

    // next ones would not fit into SM2.0 limits, but they are always for SM3.0+
    #if UNITY_REQUIRE_FRAG_WORLDPOS && !UNITY_PACK_WORLDPOS_WITH_TANGENT
        float3 posWorld                 : TEXCOORD8;
    #endif

    half4 rimColor                      : COLOR;
    float3 unscaleNormal                       : NORMAL;
    #ifdef _USE_CHARACTER_EFFECT
        fixed4 colorFrozen : COLOR1; 
        fixed4 worldPos : TEXCOORD9;
        fixed fadey : TEXCOORD10;
        float4 vertex : TEXCOORD11;
    #endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutputForwardBase vertForwardBase (VertexInput v)
{
    UNITY_SETUP_INSTANCE_ID(v);
    VertexOutputForwardBase o;
    UNITY_INITIALIZE_OUTPUT(VertexOutputForwardBase, o);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    #if defined(_USE_CHARACTER_EFFECT)
        appdata_base a_bv;
        UNITY_INITIALIZE_OUTPUT(appdata_base,a_bv);
        a_bv.vertex = v.vertex;
        a_bv.texcoord = TexCoords(v);
        a_bv.normal = v.normal;
        o.vertex = v.vertex;
        o.unscaleNormal = normalize(v.normal); 
        forzenOutPut fv2f = forzenVert(a_bv);

        o.colorFrozen = fv2f.color; 
        o.worldPos = fv2f.worldPos;
        o.fadey = fv2f.fadey;
    #endif

    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
    #if UNITY_REQUIRE_FRAG_WORLDPOS
        #if UNITY_PACK_WORLDPOS_WITH_TANGENT
            o.tangentToWorldAndPackedData[0].w = posWorld.x;
            o.tangentToWorldAndPackedData[1].w = posWorld.y;
            o.tangentToWorldAndPackedData[2].w = posWorld.z;
        #else
            o.posWorld = posWorld.xyz;
        #endif
    #endif
    o.pos = UnityObjectToClipPos(v.vertex);

    o.tex = TexCoords(v);
    o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
    float3 normalWorld = UnityObjectToWorldNormal(v.normal);
    #ifdef _TANGENT_TO_WORLD
        float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

        float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
        o.tangentToWorldAndPackedData[0].xyz = tangentToWorld[0];
        o.tangentToWorldAndPackedData[1].xyz = tangentToWorld[1];
        o.tangentToWorldAndPackedData[2].xyz = tangentToWorld[2];
    #else
        o.tangentToWorldAndPackedData[0].xyz = 0;
        o.tangentToWorldAndPackedData[1].xyz = 0;
        o.tangentToWorldAndPackedData[2].xyz = normalWorld;
    #endif

    //We need this for shadow receving
    UNITY_TRANSFER_SHADOW(o, v.uv1);

    o.ambientOrLightmapUV = VertexGIForward(v, posWorld, normalWorld);

    #ifdef _PARALLAXMAP
        TANGENT_SPACE_ROTATION;
        half3 viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
        o.tangentToWorldAndPackedData[0].w = viewDirForParallax.x;
        o.tangentToWorldAndPackedData[1].w = viewDirForParallax.y;
        o.tangentToWorldAndPackedData[2].w = viewDirForParallax.z;
    #endif

    //x:stime y:etime
    float time = _XTime.y;
    // float time = _HitRimParameter.w;
    fixed hitRim = saturate((_HitRimParameter.y - time)/(_HitRimParameter.y - _HitRimParameter.x));
    half   rimPower = lerp(_RimColor.a,_GlobalRimColor.a,ceil(_GlobalRimColor.a));
    fixed3 rimColor = lerp(_RimColor.rgb * hitRim,_GlobalRimColor.rgb,ceil(_GlobalRimColor.a));
    rimColor.rgb = lerp(fixed3(0,0,0),rimColor,ceil(rimPower));
    o.rimColor.rgb = saturate(pow(1.0-max(0,dot(normalWorld, -normalize(o.eyeVec))),rimPower * 10) * rimColor.rgb);

    UNITY_TRANSFER_FOG(o,o.pos);
    return o;
}




half4 fragForwardBaseInternal (VertexOutputForwardBase i)
{
    UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

    FRAGMENT_SETUP(s)

    #ifdef _USE_CHARACTER_EFFECT
        float fadeLine = ModeFade(i.tangentToWorldAndPackedData[0].w,i.tangentToWorldAndPackedData[1].w);
        clip(fadeLine-0.05);
    #endif
    
    s.diffColor.rgb += i.rimColor;

    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    UnityLight mainLight = MainLight ();

    mainLight.dir = lerp(mainLight.dir, normalize(UnityWorldSpaceViewDir( s.posWorld )),_LightCameraDir);

    UNITY_LIGHT_ATTENUATION(atten, i, s.posWorld);

    half occlusion = Occlusion(i.tex.xy);

    UnityGI gi = FragmentGI (s, occlusion, i.ambientOrLightmapUV, atten, mainLight);
    gi.light.color *= _LightPower * Skin(i.tex.xy);

    half4 c = UNITY_BRDF_PBS (s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect);

    UNITY_APPLY_FOG(i.fogCoord, c.rgb);

    c = OutputForward (c, s.alpha);

    c.a = AlphaFade(c.a );
    


    #if defined(_USE_CHARACTER_EFFECT)
        forzenStruct fst;
        fst.inputColor = c;
        fst.uv = i.tex.xy;
        fst.color = i.colorFrozen;
        fst.worldPos = i.worldPos;
        fst.fadey = i.fadey;
        fixed4 frozenColor = forzenFrag(fst);
        c = lerp(c,frozenColor, _Frozen);

        ElectricFlowInput einput;
        einput.texcoord = i.tex.xy;
        einput.inputColor = c;
        fixed4 electricColor = ElectricFlowFrag(einput);
        c = lerp(c,electricColor,_ElectricFlow);

        ShiningInputStruct sinput;
        sinput.inputColor = c;
        sinput.vertex = i.vertex;
        sinput.unscaleNormal = i.unscaleNormal;
        c = lerp(c,ShiningFrag(sinput),_Shining);
    #endif

    #ifdef _USE_CHARACTER_EFFECT
        c.a *= fadeLine;
    #endif
    c = XSaturation(c);

    return c;
}

half4 fragForwardBase (VertexOutputForwardBase i) : SV_Target   // backward compatibility (this used to be the fragment entry function)
{
    return fragForwardBaseInternal(i);
}




//------------------------------------------fast spce

struct VertexOutputSpce
{
    float2 uv : TEXCOORD0;
    UNITY_FOG_COORDS(1)
    float2 cap  : TEXCOORD2;
    float4 pos : SV_POSITION;
	fixed3 rimColor : COLOR0;
    #ifdef _USE_CHARACTER_EFFECT
        fixed4 worldPos : TEXCOORD3;
        fixed4 colorFrozen : COLOR1; 
        fixed fadey : TEXCOORD4;
        float4 vertex : TEXCOORD5;
        float3 unscaleNormal : NORMAL;
    #endif
};

VertexOutputSpce vertFastSpce (appdata_base v)
{
    VertexOutputSpce o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
    float3 worldNorm = UnityObjectToWorldNormal(v.normal);
	
	float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
	

     float time = _XTime.y;
     fixed hitRim = saturate((_HitRimParameter.y - time)/(_HitRimParameter.y - _HitRimParameter.x));

	float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);
	half   rimPower = lerp(_RimColor.a,_GlobalRimColor.a,ceil(_GlobalRimColor.a));
    fixed3 rimColor = lerp(_RimColor.rgb * hitRim,_GlobalRimColor.rgb,ceil(_GlobalRimColor.a));

    rimColor = lerp(fixed3(0,0,0),rimColor,ceil(rimPower));

	o.rimColor = pow(1.0-max(0,dot(worldNorm, viewDir)),rimPower * 10) * rimColor;
	
	
    worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
    o.cap.xy = worldNorm.xy * 0.5 + 0.5;

    #if defined(_USE_CHARACTER_EFFECT)
        forzenOutPut fv2f = forzenVert(v);
        o.colorFrozen = fv2f.color; 
        o.worldPos = fv2f.worldPos;
        o.fadey = fv2f.fadey;
        o.vertex = v.vertex;
        o.unscaleNormal = normalize(v.normal); 
    #endif

    UNITY_TRANSFER_FOG(o,o.pos);

    return o;
}
        
fixed4 fragFastSpce (VertexOutputSpce i) : SV_Target
{
	fixed4 col = tex2D(_MainTex, i.uv);
    #if defined(_ALPHATEST_ON)
        clip (col.a - _Cutoff);
    #endif

    #ifdef _USE_CHARACTER_EFFECT
        float3 worldPos = i.worldPos;
        float fadeLine = ModeFade(worldPos.x,worldPos.y);
        clip(fadeLine-0.05);
    #endif


    #if _MULTICOLOR
        half3 mask = tex2D(_MultiColorMask, i.uv).rgb;
        fixed3 des = dot(col.rgb, fixed3(0.3, 0.59, 0.11));
        fixed3 mcolor = fixed3(0, 0, 0); 
        mcolor += des * _MultiColorR.rgb * _MultiColorR.a * 10 *  mask.r;  
        mcolor += des * _MultiColorG.rgb * _MultiColorG.a * 10 *  mask.g;
        mcolor += des * _MultiColorB.rgb * _MultiColorB.a * 10 *  mask.b;
        mcolor += col.rgb * (1 - min(mask.r + mask.g + mask.b,1));
    #else
        fixed3 mcolor = col;
    #endif

    col.rgb = mcolor;
    col.rgb = tex2D(_LowMatCap, i.cap) * col * _LowColor * _LowMatCapPower;
	col.rgb += i.rimColor.rgb;
	col.a = col.a * _LowColor.a;

    col.a = AlphaFade(col.a);

    #if defined(_USE_CHARACTER_EFFECT)
        forzenStruct fst;
        fst.inputColor = col;
        fst.uv = i.uv;
        fst.color = i.colorFrozen;
        fst.worldPos = i.worldPos;
        fst.fadey = i.fadey;
        fixed4 frozenColor = forzenFrag(fst);
        col = lerp(col,frozenColor, _Frozen);

        ElectricFlowInput einput;
        einput.texcoord = i.uv;
        einput.inputColor = col;
        fixed4 electricColor = ElectricFlowFrag(einput);
        col = lerp(col,electricColor,_ElectricFlow);

        ShiningInputStruct sinput;
        sinput.inputColor = col;
        sinput.vertex = i.vertex;
        sinput.unscaleNormal = i.unscaleNormal;
        col = lerp(col,ShiningFrag(sinput),_Shining);
    #endif

    UNITY_APPLY_FOG(i.fogCoord, col);

    #ifdef _USE_CHARACTER_EFFECT
        col.a *= fadeLine;
    #endif
    col = XSaturation(col);
    
    return col;
}


#endif // CHARACTARPBRCORE_INCLUDED
