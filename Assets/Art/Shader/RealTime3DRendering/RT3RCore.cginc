#ifndef RT3RCORE_INCLUDED
#define RT3RCORE_INCLUDED

#include "UnityCG.cginc"

//fog
float4 _FogColor; 
float _FogStart;
float _FogRange;
//viewDirection:not normalize one, means the distance between camera to vector in worldSpace
float get_fog_amount(float3 viewDirection,float fogStart,float fogRange)
{
    return saturate((length(viewDirection) - fogStart)/fogRange);
}
#define APPLTY_FOG(color) color.rgb = lerp(color.rgb,_FogColor.rgb * _FogColor.a,get_fog_amount(UnityWorldSpaceViewDir(i.worldPos),_FogStart,_FogRange));
//fog

struct LightContributionData
{
    float4 albedo;
    float3 normal;
    float3 viewDirection;
    float4 lightColor;
    float4 lightDirection;
    float4 specularColor;
    float specularPower;
    float gloss;
};

//Common
#define DECLARE_WORLD_SPACE_POSITION(idx) float3 worldPos : TEXCOORD##idx;
#define ASSIGN_WORLD_SPACE_POSITION(o) o.worldPos = mul(UNITY_MATRIX_M,v.vertex);
//for v2f struct
#define DECLARE_LIGHT_BASE_FIELD_VERT float3 normal :NORMAL;
#define DECLARE_LIGHT_BASE_FIELD(idx) float3 normal :NORMAL;  float3 worldPos : TEXCOORD##idx;
//for vertex shader
#define ASSIGH_LIGHT_BASE_FIELD(o) o.normal = v.normal; o.worldPos = mul(UNITY_MATRIX_M,v.vertex);
//for fragment shader
#define LIGHT_ATTENUATION(lightDirection) lightDirection.a
#define APPLY_LIGHT_TO_COLOR(lightVec4,colorVec3) lightVec4.rgb * lightVec4.a * colorVec3

float3 GetLightContribution(LightContributionData i)
{
    float3 lightDirection = i.lightDirection.xyz;
    float Attenuation = i.lightDirection.w; // the w component of light Vector4 was open use for store the attenuation of light(point,spot etc...)
    //diffuse
    float n_dot_l = dot(i.normal,lightDirection); //Lambert's cos law
    float in_light_side = step(0,n_dot_l);
    float3 diffuse = APPLY_LIGHT_TO_COLOR(i.lightColor,i.albedo.rgb * n_dot_l) * Attenuation * in_light_side;
    //specular
    float3 halfVec3 = normalize(i.viewDirection + lightDirection); //blinn-phong
    float n_dot_h = dot(i.normal,halfVec3);//blinn-phong
    float specularCoeffecient = min(pow(saturate(n_dot_h),i.specularPower),i.albedo.a);// dot(N,H) ^ P
    float3 specular = APPLY_LIGHT_TO_COLOR(i.specularColor,specularCoeffecient) * Attenuation * i.gloss * in_light_side;
    //combine
    return diffuse + specular  ;
}

#endif

