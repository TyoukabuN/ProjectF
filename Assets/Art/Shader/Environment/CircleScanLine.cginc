#ifndef CIRCLESCANLINE_INCLUDED
#define CIRCLESCANLINE_INCLUDED

#include "UnityCG.cginc"

float4 _ScanLineAppearCenter;
float _ScanLineWidth;

float _ForwardGradient;
float _BackGradient;

float4 _ScanLineTestParam;

float CompoutScanLinePct(float scaningRadius,float3 wpos,float addtion)
{
	wpos.y = 0;
	float3 vec3 = wpos - _ScanLineAppearCenter.xyz;
	vec3 += normalize(vec3) * _ScanLineTestParam.y;

	float halfW = _ScanLineWidth * 0.5;

	_ForwardGradient =  max(_ForwardGradient,0.0001);
	_BackGradient =  max(_BackGradient,0.0001);

	float edge2 = scaningRadius - halfW;
	float edge1 = edge2 - _BackGradient;
	//float edge3 = scaningRadius + halfW;
	//float edge4 = edge3 + _ForwardGradient;

	addtion *= -1;
	//minues part
	//special process because gradient part will inverse when addtion is negative
	float integral = 0;
	float fractional = modf(abs(addtion)/scaningRadius,integral);
	integral += 1;
	addtion = lerp(addtion + scaningRadius * integral,addtion,step(0.00001,addtion));

	//anthor approach
	//float radius = (abs(length(vec3) + _ScanLineTestParamX) )%(edge4);
	//float pct = smoothstep(edge1,edge2,radius) - smoothstep(edge3,edge4,radius);

	float radius = (abs(length(vec3) + addtion) )%(scaningRadius);
	float pct = smoothstep(edge1,edge2,radius) + smoothstep(halfW + _ForwardGradient,halfW,radius);

	return pct;
}

#endif //CIRCLESCANLINE_INCLUDED