#if !defined(FLOW_INCLUDED)
#define FLOW_INCLUDED

float2 FlowUV(float2 uv,float2 flowVector,float timeStep)
{
	return uv - flowVector * timeStep;
}

#endif //FLOW_INCLUDED