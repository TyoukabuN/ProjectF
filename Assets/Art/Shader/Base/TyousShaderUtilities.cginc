#if !defined(TYOUS_SHADER_UTILITIES_INCLUDED)
#define TYOUS_SHADER_UTILITIES_INCLUDED

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

#endif //TYOUS_SHADER_UTILITIES_INCLUDED