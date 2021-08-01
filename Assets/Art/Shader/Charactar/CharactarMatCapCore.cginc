#ifndef CHARACTAR_MATCAP_CORE
#define CHARACTAR_MATCAP_CORE

#include "UnityCG.cginc"
#include "CharactarEffectCore.cginc"

struct VertexOutput
{
    float4 pos  : SV_POSITION;
    float2 uv   : TEXCOORD0;
    float2 uv_bump : TEXCOORD1;
    float3 c0 : TEXCOORD2;
    float3 c1 : TEXCOORD3;
    UNITY_FOG_COORDS(4)
};

           
uniform sampler2D _MainTex;
uniform float4 _MainTex_ST;
uniform sampler2D _ColorMaskTex;
uniform sampler2D _BumpMap;
uniform float4 _BumpMap_ST;
uniform sampler2D _MaskMap;
uniform sampler2D _MatCap;
uniform sampler2D _MatCap2;
uniform fixed4 _Color;
uniform fixed4 _MaskColor;
uniform half _ColorPower;
uniform half _Saturate;
uniform fixed4 _EmissionColor;
uniform half _EmissionPower;
uniform fixed4 _MatCapColor;
uniform fixed4 _MatCap2Color;
uniform half _MatCapPower;
uniform half _MatCap2Power;
uniform half _Cutoff;
uniform half4 _ReplaceColor;

fixed _Frozen;
fixed _ElectricFlow;


VertexOutput vertMatCapFull (appdata_tan v)
{
    VertexOutput o;
    o.pos = UnityObjectToClipPos (v.vertex);
    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
    o.uv_bump = TRANSFORM_TEX(v.texcoord,_BumpMap);

    v.normal = normalize(v.normal);
    v.tangent = normalize(v.tangent);
    TANGENT_SPACE_ROTATION;
    o.c0 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[0].xyz));
    o.c1 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[1].xyz));
    UNITY_TRANSFER_FOG(o, o.pos);

    return o;
}

fixed4 fragMatCapFull  (VertexOutput i) : COLOR
{
    fixed4 tex = tex2D(_MainTex, i.uv) * _Color;

    fixed4 cmaskTex = tex2D(_ColorMaskTex, i.uv);

    fixed4 c = dot(tex,fixed3(0.3,0.59,0.11));
    
    fixed4 maskColor = lerp(c,c * _MaskColor * _ColorPower,_Saturate);

    tex = lerp(tex,maskColor,cmaskTex.r);

    fixed4 maskTex = tex2D(_MaskMap, i.uv);

    fixed3 normals = UnpackNormal(tex2D(_BumpMap, i.uv_bump));
    
    float2 capCoord = float2(dot(i.c0, normals), dot(i.c1, normals)) * 0.5 + 0.5;

    fixed4 capTex = tex2D(_MatCap, capCoord);

    fixed4 cap2Tex = tex2D(_MatCap2, capCoord);

    fixed4 matColor  = tex * capTex * _MatCapColor * _MatCapPower * maskTex.r ;
    
    fixed4 matColor2 = tex * cap2Tex * _MatCap2Color * _MatCap2Power * maskTex.g;

    fixed4 emissColor = _EmissionColor * _EmissionPower * maskTex.b;

    fixed4 color = (matColor + matColor2 + emissColor);
    color.a = tex.a * _Color.a;

    UNITY_APPLY_FOG(i.fogCoord, color);

    return color;
}


fixed4 fragMatCapReflectMask (VertexOutput i) : COLOR
{
    fixed4 tex = tex2D(_MainTex, i.uv) * _Color;

    fixed4 maskTex = tex2D(_MaskMap, i.uv);

    fixed3 normals = UnpackNormal(tex2D(_BumpMap, i.uv_bump));
    
    float2 capCoord = float2(dot(i.c0, normals), dot(i.c1, normals)) * 0.5 + 0.5;

    fixed4 capTex = tex2D(_MatCap, capCoord);

    fixed4 cap2Tex = tex2D(_MatCap2, capCoord);

    fixed4 matColor  = tex * capTex * _MatCapColor * _MatCapPower * maskTex.r ;
    
    fixed4 matColor2 = tex * cap2Tex * _MatCap2Color * _MatCap2Power * maskTex.g;

    fixed4 emissColor = _EmissionColor * _EmissionPower * maskTex.b;

    fixed4 color = (matColor + matColor2 + emissColor);
    color.a = tex.a * _Color.a;

    UNITY_APPLY_FOG(i.fogCoord, color);

    return color;
}

//-----------------------------------------------------------------------------------light----------------------------------------------------------

struct VertexOutputLit
{
    float2 uv : TEXCOORD0;
    UNITY_FOG_COORDS(1)
    float4 vertex : SV_POSITION;
    float4 posWorld : TEXCOORD2;
    float3 normalDir : TEXCOORD3;
    float3 tangentDir : TEXCOORD4;
    float3 bitangentDir : TEXCOORD5;

    float3 c0 : TEXCOORD6;
    float3 c1 : TEXCOORD7;
};



sampler2D _ReflectTex;

fixed4 _ReflectColor;
fixed4 _LightColor;
fixed4 _SpecColor;
half4 _LightDir;
half _Shininess;



VertexOutputLit vertMatCapLit (appdata_tan v)
{
    VertexOutputLit o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

    o.normalDir = UnityObjectToWorldNormal(v.normal);
    o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
    o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
    o.posWorld = mul(unity_ObjectToWorld, v.vertex);

    v.normal = normalize(v.normal);
    v.tangent = normalize(v.tangent);
    TANGENT_SPACE_ROTATION;
    o.c0 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[0].xyz));
    o.c1 = mul(rotation, normalize(UNITY_MATRIX_IT_MV[1].xyz));
    UNITY_TRANSFER_FOG(o,o.vertex);
    return o;
}


fixed4 fragMatCapLit (VertexOutputLit i) : SV_Target
{
    float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
    float3 normalLocal = UnpackNormal(tex2D(_BumpMap, i.uv));
    float3 normalDir = normalize(mul( normalLocal, tangentTransform ));
    float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);

    fixed4 col = tex2D(_MainTex, i.uv);
    fixed4 mask = tex2D(_MaskMap, i.uv);


    // float3 worldNorm = mul((float3x3)UNITY_MATRIX_V, normalDir);
    // float2 capCoord = ((worldNorm.xy * 0.5 + 0.5)*2.0) - 1.0;
    float2 capCoord = float2(dot(i.c0,normalLocal), dot(i.c1, normalLocal)) * 0.5 + 0.5;

    fixed4 reflTex = tex2D(_ReflectTex, capCoord);

    fixed4 cap2Tex = tex2D(_MatCap2, capCoord);

    float3 lightDir = viewDir;// + _LightDir.xyz;

    fixed4 lightColor = _LightColor;
    lightColor.rgb *= _LightColor.a * 8;

    half3 h = normalize (lightDir + viewDir);
    fixed diff = max (0, dot (normalDir, lightDir));
    float nh = max (0, dot (normalDir, h));
    float spec = pow (nh, _Shininess*128.0);
    
    fixed3 diffColor = col.rgb * _Color.rgb;


    fixed3 specular = diffColor * lightColor.rgb * diff + spec * _SpecColor.rgb * (_SpecColor.a * 8) * mask.r;
    fixed4 c;
    c.rgb = diffColor;
    c.rgb += specular.rgb;

    c.rgb = lerp(c.rgb,c.rgb  + ( reflTex.rgb * col.rgb * _ReflectColor.rgb * (_ReflectColor.a * 8)),mask.g);

    c.rgb += col.rgb * cap2Tex * _MatCap2Color.rgb * (_MatCap2Color.a * 8);

    fixed4 emissColor = _EmissionColor * mask.b;
    c.rgb += emissColor.rgb *  col.rgb;

    c.a = col.a * _Color.a;


    UNITY_APPLY_FOG(i.fogCoord, c);
    return c;
}


//--------------------------------------------------------------------------------- simple ----------------------------------------------------------


struct v2f
{
    float4 pos  : SV_POSITION;
    float2 uv   : TEXCOORD0;
    float2 cap  : TEXCOORD1;
    UNITY_FOG_COORDS(2)
    fixed3 rimColor  : COLOR;
    fixed4 colorFrozen : COLOR1; 
    fixed4 worldPos : TEXCOORD3;
    fixed fadey : TEXCOORD4;
};


fixed _Alphactl;
float4 _HitRimParameter;
half4  _RimColor;
half4 _FadeParameter;

half AlphaFade(half ca)
{
    half curAlpha = ca;

    half sAlpha = min(curAlpha,_FadeParameter.z);
    half toAlpha = min(curAlpha,_FadeParameter.w);
    half alpha = curAlpha;
    float time = _XTime.y;
    // float time = _HitRimParameter.w;
    fixed fade01 = saturate((_FadeParameter.y - time)/(_FadeParameter.y - _FadeParameter.x));
    alpha = lerp(sAlpha,toAlpha,fade01);
    return alpha;
}


v2f vertMatCapSimple (appdata_base v)
{
    v2f o;
    o.pos = UnityObjectToClipPos (v.vertex);
    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
    half2 capCoord;
    
    float3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
    worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
    o.cap.xy = worldNorm.xy * 0.5 + 0.5;
    
    float3 worldPos = mul(unity_ObjectToWorld, v.vertex);

    float time = _XTime.y;
    fixed hitRim = saturate((_HitRimParameter.y - time)/(_HitRimParameter.y - _HitRimParameter.x));
    float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);
    half   rimPower = _RimColor.a;
    fixed3 rimColor = _RimColor.rgb * hitRim;
    rimColor = lerp(fixed3(0,0,0),rimColor,ceil(rimPower));

    o.rimColor = pow(1.0-max(0,dot(worldNorm, viewDir)),rimPower * 10) * rimColor;

    #if defined(_USE_CHARACTER_EFFECT)
        forzenOutPut fv2f = forzenVert(v);
        o.colorFrozen = fv2f.color; 
        o.worldPos = fv2f.worldPos;
        o.fadey = fv2f.fadey;
    #endif
    UNITY_TRANSFER_FOG(o, o.pos);

    return o;
}


fixed4 fragMatCapSimple (v2f i) : COLOR
{
    fixed4 tex = tex2D(_MainTex, i.uv);

    #if defined(_ALPHATEST_ON)
        clip(tex.a - _Cutoff);
    #endif

    tex.rgb = lerp(tex.rgb,_ReplaceColor.rgb *dot(tex.rgb, fixed3(0.3, 0.59, 0.11)),_ReplaceColor.a);  
    
    fixed4 mc = tex2D(_MatCap, i.cap) * tex * _Color * _MatCapPower;
    mc.rgb += i.rimColor;

    mc.a = clamp(tex.a * _Color.a * _Alphactl,0,1);
    UNITY_APPLY_FOG(i.fogCoord, mc);

    mc.a = AlphaFade(mc.a);


    #if defined(_USE_CHARACTER_EFFECT)
        forzenStruct fst;
        fst.inputColor = mc;
        fst.uv = i.uv;
        fst.color = i.colorFrozen;
        fst.worldPos = i.worldPos;
        fst.fadey = i.fadey;
        fixed4 frozenColor = forzenFrag(fst);
        mc = lerp(mc,frozenColor, _Frozen); 

        ElectricFlowInput einput;
        einput.texcoord = i.uv;
        einput.inputColor = mc;
        fixed4 electricColor = ElectricFlowFrag(einput);
        mc = lerp(mc,electricColor,_ElectricFlow);
    #endif

    return mc;
}

#endif // CHARACTAR_MATCAP_CORE
