#ifndef CHARACTAR_EFFECT_CORE
#define CHARACTAR_EFFECT_CORE

#include "UnityCG.cginc"

struct forzenOutPut { 
    fixed4 color : COLOR; 
    fixed4 worldPos : TEXCOORD1;
    fixed fadey : TEXCOORD2;
    fixed4 inputColor : COLOR1; 
};

struct forzenStruct { 
    half2 uv : TEXCOORD0; 
    fixed4 color : COLOR; 
    fixed4 worldPos : TEXCOORD1;
    fixed fadey : TEXCOORD2;
    fixed4 inputColor : COLOR1; 
}; 


uniform fixed4 _FrozenRimColor; 
uniform fixed _Rampage; 
uniform fixed _Frezz; 
half4 _FrozenParameter;
float4 _XTime;
uniform sampler2D _FrozenRandomTex; 
fixed4 _FrezzeColor;

forzenOutPut forzenVert(appdata_base v) { 
    forzenOutPut o; 
    o.worldPos = mul(UNITY_MATRIX_M,v.vertex); 
    fixed3 viewDir = normalize(ObjSpaceViewDir(v.vertex)); 
    fixed dotProduct = 1 - dot(v.normal, viewDir); 
    o.color = smoothstep(0, 1, dotProduct);     
    o.color *= _FrozenRimColor; 
    float3 normal = mul(SCALED_NORMAL, (float3x3)unity_WorldToObject); 
    dotProduct = dot(normal, fixed3(0, 1, 0)) / 2; 
  	dotProduct = clamp(dotProduct,0,1);
    o.color.rgb += dotProduct.xxx * 2; 
    float time = _XTime.y;
    fixed sign = saturate((_FrozenParameter.y - time)/(_FrozenParameter.y - _FrozenParameter.x));
    o.fadey = lerp(_FrozenParameter.z,_FrozenParameter.w,sign);
    return o; 
} 

fixed4 forzenFrag(forzenStruct i){ 
    fixed sign = step(i.fadey,i.worldPos.y);
    float ClipTex = tex2D (_FrozenRandomTex, i.uv).r ; 
    float ClipAmount = (_Frezz - ClipTex) / 2 + 0.5; 
	ClipAmount = clamp(ClipAmount,0,1);
	fixed4 texcol;
    texcol = i.color; 
    texcol = texcol * ClipAmount +  (1 - ClipAmount); 
    texcol *= _FrezzeColor;
    return lerp(i.inputColor,lerp(i.inputColor,texcol,texcol.a), step(sign,0.9));
} 

struct ElectricFlowInput 
{
	float2 texcoord : TEXCOORD0;
    fixed4 inputColor  : COLOR;
};

sampler2D _ElectricFlowFlowTex1;
float4 _ElectricFlowFlowTex1_ST;
sampler2D _ElectricFlowFlowTex2;
float4 _ElectricFlowFlowTex2_ST;

half4  _ElectricFlowColor;
fixed  _ElectricFlowElectricFlowColorStrength;
fixed3 _ElectricFlowFlowSpeed;
fixed3 _ElectricFlowFlowSpeed1;
fixed4 _ElectricFlowTimeEditor;
fixed _ElectricFlowPow;
fixed _ElectricFlowWidth;
fixed _ElectricFlowColorStrength;

half remap(half x, half t1, half t2, half s1, half s2)
{
	return (x - t1) / (t2 - t1) * (s2 - s1) + s1;
}

float Unity_Rectangle_float(float2 UV, float Width, float Height)
{
	float2 d = abs(UV * 2 - 1) - float2(Width, Height);
	d = 1 - d / fwidth(d);
	float Out = saturate(min(d.x, d.y));
	return Out;
}

fixed4 ElectricFlowFrag (ElectricFlowInput i) 
{
	float4 tTime = _Time + _ElectricFlowTimeEditor;
	float2 uvPos = (i.texcoord + (tTime.g * _ElectricFlowFlowSpeed.rgb) * fixed2(1,1));
	float2 uvPos1 = (i.texcoord + (tTime.g * _ElectricFlowFlowSpeed1.rgb) * fixed2(1,1));
	fixed4 Noise = tex2D(_ElectricFlowFlowTex1,TRANSFORM_TEX(uvPos, _ElectricFlowFlowTex1)) + tex2D(_ElectricFlowFlowTex2,TRANSFORM_TEX(uvPos1, _ElectricFlowFlowTex2));
	fixed4 N = pow(Noise,_ElectricFlowPow);
	N.r = remap(N.r,0,1,-10,10);
	N.g = remap(N.g,0,1,-10,10);
	clamp(N,0,1);
	N.rgb = 1 - N.rgb;

	float r = Unity_Rectangle_float(N.rg,_ElectricFlowWidth,_ElectricFlowWidth);
	fixed4 targetColor = r * _ElectricFlowColor * _ElectricFlowColorStrength;	
	return lerp(i.inputColor,targetColor,targetColor.a);
}

struct ShiningInputStruct { 
    float4 vertex : TEXCOORD0; 
    float3 unscaleNormal : NORMAL;
    fixed4 inputColor : COLOR; 
}; 

uniform fixed4 _ShiningRimColor; 
uniform fixed _ShiningStrength; 
uniform fixed _ShiningUpIntensity;
uniform fixed4 _ShiningUpColor;

fixed4 ShiningFrag(ShiningInputStruct i) : COLOR { 
    fixed3 viewDir = normalize(ObjSpaceViewDir(i.vertex)); 
    fixed dotProduct = 1 - dot(i.unscaleNormal, viewDir);
    fixed4 col; 
    dotProduct = pow(dotProduct,_ShiningStrength);
    col = smoothstep(0, 1, dotProduct);    
    col *= _ShiningRimColor; 
   
    dotProduct = dot(mul(i.unscaleNormal*1.0, (float3x3)unity_WorldToObject), fixed3(0, 1, 0)) / _ShiningUpIntensity; 
  	dotProduct = clamp(dotProduct,0,1);
    col.rgb += dotProduct.xxx *  _ShiningUpColor; 
    return lerp(i.inputColor,col,col.a);
} 

uniform fixed4 _ModelFadeParam;
uniform fixed4 _ModelFadeWaveParam;
uniform fixed _ModelFadePreview;


float GetModelFadeSinWave (float param1,float param2)
{
    float sinWave = sin(param1*3.14159*param2);
    sinWave = (sinWave + 1) * 0.5 * _ModelFadeWaveParam.y * abs(_ModelFadeParam.z) ;
    return sinWave;
}

float ModeFade (float posWorldX,float posWorldY)
{
    float time = _XTime.y; 
     float modelFadeProgress  = saturate((_ModelFadeParam.y - time)/(_ModelFadeParam.y - _ModelFadeParam.x));
    //  float modelFadeProgress  = _ModelFadeParam.x;//for Test
         modelFadeProgress = lerp(modelFadeProgress,_ModelFadePreview,step(0.001,_ModelFadePreview));

    float edge = modelFadeProgress * abs(_ModelFadeParam.z);///_ModelFadeParam.z;

    float sinWave = GetModelFadeSinWave(posWorldX,_ModelFadeWaveParam.x);

    sinWave += GetModelFadeSinWave(posWorldX,_ModelFadeWaveParam.z)*0.5;
    float edgeTop = (modelFadeProgress + max(_ModelFadeWaveParam.w + sinWave,0)) * abs(_ModelFadeParam.z);///_ModelFadeParam.z;

    float smoothstepMin = lerp(edgeTop,edge,step(0,_ModelFadeParam.z));
    float smoothstepMax = lerp(edge,edgeTop,step(0,_ModelFadeParam.z));

    float fadeLine = smoothstep(smoothstepMin,smoothstepMax,posWorldY - _ModelFadeParam.w + 0.08);

    fadeLine = lerp(fadeLine,1,step(abs(_ModelFadeParam.z),0.1));

    return fadeLine;
}



#endif // CHARACTAR_EFFECT_CORE
