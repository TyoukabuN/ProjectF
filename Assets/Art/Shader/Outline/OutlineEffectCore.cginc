#ifndef OUTLINE_EFFECT_CORE
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11
#define OUTLINE_EFFECT_CORE

#include "UnityCG.cginc"

//reference:https://alexanderameye.github.io/notes/rendering-outlines/

//outline common property
uniform float4 _OutlineColor;
uniform float _OutlineWidth;
uniform float _OutlineSoftness;
uniform float _OutlinePower;
uniform sampler2D _OutlinNormalMap;
uniform float4 _OutlinNormalMap_ST;

//Extrusion direction////////////////////////////////////////////////////
//Vertex position OS:ObjectSpace
float3 GetEdge_MoveVertex(float3 vertexOS,float width)
{
	return vertexOS + normalize(vertexOS) * width;
}
float4 GetEdge_MoveVertex(float4 vertexOS,float width)
{
	return float4(vertexOS.xyz + normalize(vertexOS.xyz) * width,vertexOS.w);
}
float3 GetEdge_MoveVertex(float3 vertexOS)
{
	return GetEdge_MoveVertex(vertexOS,_OutlineWidth);
}
float4 GetEdge_MoveVertex(float4 vertexOS)
{
	return GetEdge_MoveVertex(vertexOS,_OutlineWidth);
}
//pipeline function
float4 frag_outline(appdata_base v):SV_TARGET
{
	return _OutlineColor;
}
//moveVertex
struct v2f_outline_moveVertex {
	float4 pos:SV_POSITION;
};
v2f_outline_moveVertex vert_outline_moveVertex(appdata_base v)
{
	v2f_outline_moveVertex o;
	float4 vertex = GetEdge_MoveVertex(v.vertex,_OutlineWidth);
	o.pos = UnityObjectToClipPos(vertex);
	
	return o;
}
//moveVertex along normal
v2f_outline_moveVertex vert_outline_moveVertex_alongOSNormal(appdata_base v)
{
	v2f_outline_moveVertex o;
	float4 vertex = v.vertex;
	// float4 texcoord = float4(TRANSFORM_TEX(v.texcoord,_OutlinNormalMap),0,0);
	// float3 normal = UnpackNormal(tex2Dlod(_OutlinNormalMap,texcoord));
	float3 normal = v.normal.xyz;
	vertex.xyz += normal * _OutlineWidth;
	o.pos = UnityObjectToClipPos(vertex);
	return o;
}
//recommend!!!!!!!!!!
v2f_outline_moveVertex vert_outline_moveVertex_alongCSNormal(appdata_base v)
{
	v2f_outline_moveVertex o;
	float4 vertexCS = UnityObjectToClipPos(v.vertex);
	float3 normalCS = mul(UNITY_MATRIX_VP,UnityObjectToWorldNormal(v.normal));
	// vertexCS.xyz += normalCS.xyz * _OutlineWidth;
	vertexCS.xy += normalize(normalCS.xy)/ _ScreenParams.xy * vertexCS.w * max(0.01,_OutlineWidth) * 2;
	o.pos = vertexCS;
	return o;
}
v2f_outline_moveVertex vert_outline_moveVertex_alongCSNormalMap(appdata_base v)
{
	v2f_outline_moveVertex o;
	float4 vertexCS = UnityObjectToClipPos(v.vertex);
	float3 normalCS = mul(UNITY_MATRIX_VP,UnityObjectToWorldNormal(v.normal));
	vertexCS.xy += normalize(normalCS.xy)/ _ScreenParams.xy * vertexCS.w * max(0.01,_OutlineWidth) * 2;
	o.pos = vertexCS;
	return o;
}
float4 frag_outline_moveVertex_alongNormal(appdata_base v):SV_TARGET
{
	return float4(0,0,0,1);
}
//Rim////////////////////////////////////////////////////////////////////
//Rim effect outlines are simple but only work well on spherical objects.
uniform float4 _RimColor;
uniform fixed _RimReducer;
uniform fixed _RimPow;
//不能用于太几何的模型(例如立方体)
//因为原理是用的是菲涅尔公式(用到Dot(N,V))
float3 GetEdge_Rim(float3 normalWS, float3 viewDirWS)
{
	float edge1 = 1 - _OutlineWidth;
	float edge2 = edge1 + _OutlineSoftness;
	float fresnel = pow(1 - saturate(dot(normalWS, viewDirWS)), _OutlinePower);
	return lerp(1, smoothstep(edge1, edge2, fresnel), step(0, edge1));
}

//RobertsCross////////////////////////////////////////////////////////////////////
// static const int RobertsCrossX[4] = { 1,0,0,-1 };
// static const int RobertsCrossY[4] = { 0,1,-1,0 };
float GetEdge_RobertsCross(float3 samples[4])
{
	float3 horizontal = samples[0] * 1; // top left (factor +1)
	horizontal += samples[3] * -1; 		// bottom right (factor -1)

	float3 vertical = samples[2] * -1; 	// bottom left (factor -1)
	vertical += samples[1] * 1; 		// top right (factor +1)

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


#endif //OUTLINE_EFFECT_CORE