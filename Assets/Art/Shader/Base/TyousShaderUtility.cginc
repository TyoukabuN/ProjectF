#ifndef TYOUS_SHADER_UTILITY
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11
#define TYOUS_SHADER_UTILITY

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

float3 SamplingDepthNormalsTexture_Normal(float2 uv, float2 uvOffset)
{
	float4 enc = tex2D(_CameraDepthNormalsTexture, uv + uvOffset);
	return DecodeViewNormalStereo(enc);
}
float3 SamplingDepthNormalsTexture_Normal(float2 uv)
{
	return SamplingDepthNormalsTexture_Normal(uv, float2(0, 0));
}
//
float SamplingDepthNormalsTexture_Depth(float2 uv, float2 uvOffset)
{
	float4 enc = tex2D(_CameraDepthNormalsTexture, uv + uvOffset);
	return DecodeFloatRG(enc.zw);
}
float3 SamplingDepthNormalsTexture_Depth(float2 uv)
{
	return SamplingDepthNormalsTexture_Depth(uv, float2(0, 0));
}
//
float SamplingDepthNormalsTexture(float2 uv, float2 uvOffset, out float depth, out float3 normal)
{
	float4 enc = tex2D(_CameraDepthNormalsTexture, uv + uvOffset);
	DecodeDepthNormal(enc, depth, normal);
}
float SamplingDepthNormalsTexture(float2 uv, out float depth, out float3 normal)
{
	SamplingDepthNormalsTexture(uv, float2(0, 0), depth, normal);
}

#endif //TYOUS_SHADER_UTILITY