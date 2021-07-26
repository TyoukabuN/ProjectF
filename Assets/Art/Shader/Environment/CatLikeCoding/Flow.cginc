#if !defined(FLOW_INCLUDED)
#define FLOW_INCLUDED

float3 FlowUVW(float2 uv,float2 flowVector,float timeStep)
{
	float pct = frac(timeStep);
	float3 uvw;
	uvw.xy = uv - flowVector * pct ;
	//fade the texture color to black as it approach maximum distortion
	uvw.z = 1 - abs(1 - 2 * pct);
	return uvw;
}

#endif //FLOW_INCLUDED