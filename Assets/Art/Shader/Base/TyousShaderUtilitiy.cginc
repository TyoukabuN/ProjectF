#ifndef TYOUS_SHADER_UTILITIES_INCLUDED
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11
#define TYOUS_SHADER_UTILITIES_INCLUDED

#include "UnityCG.cginc"

//径向倾斜
float2 RadialShear_float(float2 UV, float2 Center, float2 Strength, float2 Offset)
{
    float2 delta = UV - Center;
    float delta2 = dot(delta.xy, delta.xy);
    float2 delta_offset = delta2 * Strength;
	//-delta.x  xy的符号是为了逆时针旋转
	//-delta.y, delta.x 则为逆时针
	return UV + float2(-delta.y, delta.x) * delta_offset + Offset;
}

//获取屏幕空间纹理坐标
float2 GetScreenSpaceTexcood(float4 clipPos)
{
	float2 uv = clipPos.xy / _ScreenParams.xy;
#if UNITY_UV_STARTS_AT_TOP
	uv.y = 1 - uv.y;
#endif
	return uv;
}

//深度图相关//////////////////////////////////////////////////////////////////////////////////

sampler2D _CameraDepthNormalsTexture;
float4 _CameraDepthNormalsTexture_ST;
float4 _CameraDepthNormalsTexture_TexelSize;

float3 SamplingDepthNormalsTexture_Normal(float2 uv,float2 uvOffset)
{
	float4 enc = tex2D(_CameraDepthNormalsTexture, uv + uvOffset);
	float depth;
	float3 normal;
	DecodeDepthNormal(enc, depth, normal);
	return normal;
}
float3 SamplingDepthNormalsTexture_Normal(float2 uv)
{
	return SamplingDepthNormalsTexture_Normal(uv,float2(0,0));
}
//
float SamplingDepthNormalsTexture_Depth(float2 uv, float2 uvOffset)
{
	float4 enc = tex2D(_CameraDepthNormalsTexture, uv + uvOffset);
	float depth;
	float3 normal;
	DecodeDepthNormal(enc, depth, normal);
	return depth;
}
float3 SamplingDepthNormalsTexture_Depth(float2 uv)
{
	return SamplingDepthNormalsTexture_Depth(uv, float2(0, 0));
}
//
float SamplingDepthNormalsTexture(float2 uv,float2 uvOffset,out float depth,out float3 normal)
{
	float4 enc = tex2D(_CameraDepthNormalsTexture, uv + uvOffset);
	DecodeDepthNormal(enc, depth, normal);
}
float SamplingDepthNormalsTexture(float2 uv, out float depth, out float3 normal)
{
	SamplingDepthNormalsTexture(uv, float2(0, 0), depth, normal);
}

//描边相关//////////////////////////////////////////////////////////////////////////////////

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

float GetEdge_RobertsCross(sampler2D _sampler,float2 uv,float2 pixelSize)
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
float GeteEdge_NormalDiff_RobertsCross(float4 clipPos)
{
	float2 uv = GetScreenSpaceTexcood(clipPos);
	float3 samples[4];
	float2 pixelSize = _ScreenParams.zw - 1;
	//float2 pixelSize = _CameraDepthNormalsTexture_ST.xy;???
	//horizontal
	samples[0] = SamplingDepthNormalsTexture_Normal(uv, float2(-pixelSize.x, 0));
	samples[3] = SamplingDepthNormalsTexture_Normal(uv, float2(pixelSize.x, 0));
	//vertical 
	samples[1] = SamplingDepthNormalsTexture_Normal(uv, float2(0,  pixelSize.y));
	samples[2] = SamplingDepthNormalsTexture_Normal(uv, float2(0, -pixelSize.y));

	return GetEdge_RobertsCross(samples);
}
float GeteEdge_DepthDiff_RobertsCross(float4 clipPos)
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


#endif //TYOUS_SHADER_UTILITIES_INCLUDED