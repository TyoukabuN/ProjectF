#if !defined(FLOW_INCLUDED)
#define FLOW_INCLUDED

float3 FlowUVW(float2 uv,float2 flowVector,float2 jump,float time, float flowB)
{
	float phaseOffset = flowB ? 0.5:0;
	float pct = frac(time + phaseOffset);
	float3 uvw;
	uvw.xy = uv - flowVector * pct + phaseOffset;
	uvw.xy += (time - pct) * jump;
	//fade the texture color to black as it approach maximum distortion
	uvw.z = 1 - abs(1 - 2 * pct);
	return uvw;
}

#endif //FLOW_INCLUDED