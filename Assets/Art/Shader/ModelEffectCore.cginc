#ifndef MODEL_EFFECT_CORE
#define MODEL_EFFECT_CORE

#include "UnityCG.cginc"

fixed4 _ModelFadeParam;
fixed4 _ModelFadeWaveParam;
fixed _ModelFadePreview;

inline float GetModelFadeSinWave (float param1,float param2,float param3)
{
    float sinWave = sin(param1*3.14159*param2);
    sinWave = (sinWave + 1) * 0.5 * param3 ;
    return sinWave;
}

float ModeFade (float posWorldX,float posWorldY)
{
    float time = _XTime.y; 
     float modelFadeProgress  = saturate((_ModelFadeParam.y - time)/(_ModelFadeParam.y - _ModelFadeParam.x));
     modelFadeProgress = lerp(modelFadeProgress,_ModelFadePreview,step(0.001,_ModelFadePreview));
    //  float modelFadeProgress  = _ModelFadeParam.x;//for Test
    float edge = modelFadeProgress * abs(_ModelFadeParam.z);///_ModelFadeParam.z;

    float sinWave = GetModelFadeSinWave(posWorldX,_ModelFadeWaveParam.x, _ModelFadeWaveParam.y );

    sinWave += GetModelFadeSinWave(posWorldX,_ModelFadeWaveParam.z,_ModelFadeWaveParam.w)*0.5;
    float edgeTop = (modelFadeProgress + max(0.01 + sinWave,0)) * abs(_ModelFadeParam.z);///_ModelFadeParam.z;

    float smoothstepMin = lerp(edgeTop,edge,step(0,_ModelFadeParam.z));
    float smoothstepMax = lerp(edge,edgeTop,step(0,_ModelFadeParam.z));

    float fadeLine = smoothstep(smoothstepMin,smoothstepMax,posWorldY - _ModelFadeParam.w + 0.08);

    fadeLine = lerp(fadeLine,1,step(abs(_ModelFadeParam.z),0.1));

    return fadeLine;
}



#endif // MODEL_EFFECT_CORE
