#ifndef OUTLINE_EFFECT_BASE
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11
#define OUTLINE_EFFECT_BASE

#include "UnityCG.cginc"

//Rim////////////////////////////////////////////////////////////////////
//Rim effect outlines are simple but only work well on spherical objects.
//不能用于太几何的模型(例如立方体)
//因为原理是用的是菲涅尔公式(用到Dot(N,V))
uniform float4 _RimColor;
uniform fixed _RimReducer;
uniform fixed _RimPow;

uniform float _OutlineWidth;
uniform float _OutlineSoftness;
uniform float _OutlinePower;
float3 GetEdge_Rim(float3 normalWS, float3 viewDirWS)
{
	float edge1 = 1 - _OutlineWidth;
	float edge2 = edge1 + _OutlineSoftness;
	float fresnel = pow(1 - saturate(dot(normalWS, viewDirWS)), _OutlinePower);
	return lerp(1, smoothstep(edge1, edge2, fresnel), step(0, edge1));
}

//RobertsCross////////////////////////////////////////////////////////////////////
static const int RobertsCrossX[4] = { 1,0,0,-1 };
static const int RobertsCrossY[4] = { 0,1,-1,0 };

//reference:https://alexanderameye.github.io/notes/rendering-outlines/
float GetEdge_RobertsCross(float3 samples[4])
{
	float3 horizontal = samples[0] * RobertsCrossX[0]; // top left (factor +1)
	horizontal += samples[3] * RobertsCrossX[3]; // bottom right (factor -1)

	float3 vertical = samples[2] * RobertsCrossY[2]; // bottom left (factor -1)
	vertical += samples[1] * RobertsCrossY[1]; // top right (factor +1)

	return sqrt(dot(horizontal, horizontal) + dot(vertical, vertical));
}

float GetEdge_RobertsCross(sampler2D _sampler, float2 uv, float2 pixelSize)
{
	float3 samples[4];
	//horizontal
	samples[0] = tex2D(_sampler, uv + float2(pixelSize.x, 0));
	samples[3] = tex2D(_sampler, uv + float2(-pixelSize.x, 0));
	//vertical 
	samples[1] = tex2D(_sampler, uv + float2(0, pixelSize.y));
	samples[2] = tex2D(_sampler, uv + float2(0, -pixelSize.y));

	return GetEdge_RobertsCross(samples);
}
float GetEdge_NormalDiff_RobertsCross(float4 clipPos)
{
	float2 uv = GetScreenSpaceTexcood(clipPos);
	float3 samples[4];
	float2 pixelSize = _ScreenParams.zw - 1;
	//float2 pixelSize = _CameraDepthNormalsTexture_ST.xy;???
	//horizontal
	samples[0] = SamplingDepthNormalsTexture_Normal(uv, float2(-pixelSize.x, 0));
	samples[3] = SamplingDepthNormalsTexture_Normal(uv, float2(pixelSize.x, 0));
	//vertical 
	samples[1] = SamplingDepthNormalsTexture_Normal(uv, float2(0, pixelSize.y));
	samples[2] = SamplingDepthNormalsTexture_Normal(uv, float2(0, -pixelSize.y));

	return GetEdge_RobertsCross(samples);
}
float GetEdge_DepthDiff_RobertsCross(float4 clipPos)
{
	float2 uv = GetScreenSpaceTexcood(clipPos);
	float3 samples[4];
	float2 pixelSize = _ScreenParams.zw - 1;
	//horizontal
	samples[0] = SamplingDepthNormalsTexture_Depth(uv, float2(-pixelSize.x, 0));
	samples[3] = SamplingDepthNormalsTexture_Depth(uv, float2(pixelSize.x, 0));
	//vertical 
	samples[1] = SamplingDepthNormalsTexture_Depth(uv, float2(0, pixelSize.y));
	samples[2] = SamplingDepthNormalsTexture_Depth(uv, float2(0, -pixelSize.y));

	return GetEdge_RobertsCross(samples);
}


#endif //OUTLINE_EFFECT_BASE