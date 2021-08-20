#if !defined(FLOW_INCLUDED)
#define FLOW_INCLUDED

float2 RadialShear_float(float2 UV, float2 Center, float2 Strength, float2 Offset)
{
    float2 delta = UV - Center;
    float delta2 = dot(delta.xy, delta.xy);
    float2 delta_offset = delta2 * Strength;
	//-delta.x  xy的符号是为了逆时针旋转
	//-delta.y, delta.x 则为逆时针
	return UV + float2(-delta.y, delta.x) * delta_offset + Offset;
}

float3 FlowUVW(float2 uv,float2 flowVector,float2 jump,float flowOffect,float tiling,float time, float flowB)
{
	float phaseOffset = flowB ? 0.5:0;
	//time加了noise,这是单个采样根据锯齿波交替的时发生的图样的变化的由来
	float pct = frac(time + phaseOffset); //产生锯齿波 f(x) = frac(x),产生阶段(一个pct 0👉1的周期)变化
	float3 uvw;
	//flowVector * pct 通过锯齿波来影响flowMap的采样
	//uv - flowVector * pct 应用flowMap的采样
	//添加offset来增加每个相位的MainTex的偏移(即有两个flow采样坐标差半个uv的MainTex采样),
	uvw.xy = uv - flowVector * (pct + flowOffect);
	uvw.xy *= tiling;
	uvw.xy += phaseOffset;
	//(time - pct) 获得的系一个f(x)=x去除了锯齿波的阶梯状函数,即去掉了f(x)上点的所有小数部分
	//jump在x∈(0,1)的时候可以改变每个三角波的初值,
	//相位变化的时候，采样用的uv都会有点变化
	//当jump等于1的时候uv就会+1 相当于没有改变到uv
	uvw.xy += (time - pct) * jump;
	//fade the texture color to black as it approach maximum distortion
	uvw.z = 1 - abs(1 - 2 * pct); //将锯齿波转换成三角波 f(x) = 1 - abs(1 - 2x)
	return uvw;
}

#endif //FLOW_INCLUDED