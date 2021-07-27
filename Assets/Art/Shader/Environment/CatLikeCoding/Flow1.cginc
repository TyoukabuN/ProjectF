#if !defined(FLOW1_INCLUDED)
#define FLOW1_INCLUDED

float3 FlowUVW(float2 uv,float2 flowVector,float2 jump,float time, float flowB)
{
	float phaseOffset = flowB ? 0.5:0;
	float pct = frac(time + phaseOffset);
	float3 uvw;
	uvw.xy = uv - flowVector * 0 + phaseOffset;
	uvw.xy += (time - pct) * jump;
	//fade the texture color to black as it approach maximum distortion
	uvw.z = 1;
	return uvw;
}

#endif //FLOW1_INCLUDED