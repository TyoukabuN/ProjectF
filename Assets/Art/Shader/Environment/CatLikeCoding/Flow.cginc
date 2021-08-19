#if !defined(FLOW_INCLUDED)
#define FLOW_INCLUDED

float3 FlowUVW(float2 uv,float2 flowVector,float2 jump,float time, float flowB)
{
	float phaseOffset = flowB ? 0.5:0;
	float pct = frac(time + phaseOffset); //产生锯齿波 f(x) = frac(x)
	float3 uvw;
	//flowVector * pct 通过锯齿波来影响flowMap的采样
	//uv - flowVector * pct 应用flowMap的采样
	//添加offset来增加MainTex的相位(即有两个flow采样坐标差半个uv的MainTex采样),
	//下面称两份采样为相位,
	uvw.xy = uv - flowVector * pct + phaseOffset;
	//(time - pct) 获得的系一个f(x)=x去除了锯齿波的阶梯状函数,即去掉了f(x)上点的所有小数部分
	//jump在x∈(0,1)的时候可以改变每个三角波的初值,
	//
	//当jump等于1的时候uv就会+1 相当于没有改变到uv
	uvw.xy += (time - pct) * jump;
	//fade the texture color to black as it approach maximum distortion
	uvw.z = 1 - abs(1 - 2 * pct); //将锯齿波转换成三角波 f(x) = 1 - abs(1 - 2x)
	return uvw;
}

#endif //FLOW_INCLUDED