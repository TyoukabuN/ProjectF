Shader "Custom/catlikecodeing_distortion_water"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset] _FlowMap ("Flow (RG,A noise)",2D) = "black" {}
        [NoScaleOffset] _NormalMap ("Normals",2D)  = "white" {}
        _UJump ("U jump per phase", Range(-0.25, 0.25)) = 0.25
		_VJump ("V jump per phase", Range(-0.25, 0.25)) = 0.25
        //Tweak value
        _Tiling("Tiling",Float) = 1
        _NormalScale ("NormalScale",Float) = 1
        _Speed("_Speed",Float) = 1
        _FlowStrength("_FlowStrength",Float) = 1
        _FlowOffect ("FlowOffect",Range(-1,1)) = 1

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        PowerA ("Phase A Power",Range(0,1)) = 1
        PowerB ("Phase B Power",Range(0,1)) = 1

        _RadialShear_Center ("RadialShear Center",Vector) = (0.5,0.5,0,0)
        _RadialShear_Strength ("RadialShear Strength",Vector) = (1,1,0,0)
        _RadialShear_Offset ("RadialShear Offset",Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags {"RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #include "UnityCG.cginc"
        #include "Flow.cginc"
        #include "TyousShaderUtilities.cginc"

        struct Input
        {
            float2 uv_MainTex: TEXCOORD0;
        };

        sampler2D _MainTex,_FlowMap,_NormalMap;
        float _UJump, _VJump,_Tiling,_Speed,_FlowStrength,_FlowOffect;
        float _Glossiness;
        float _Metallic;
        fixed4 _Color;
        float PowerA;
        float PowerB;
        float _NormalScale;

        float2 _RadialShear_Center;
        float2 _RadialShear_Strength;
        float2 _RadialShear_Offset;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 flowVector = tex2D(_FlowMap,IN.uv_MainTex).rg * 2 - 1;
            flowVector *= _FlowStrength;
            float noise = tex2D(_FlowMap,IN.uv_MainTex).a;
            float time = noise + _Time.y * _Speed;
            float2 jump = float2(_UJump, _VJump);
            IN.uv_MainTex = RadialShear_float(IN.uv_MainTex,_RadialShear_Center,_RadialShear_Strength,_RadialShear_Offset);
            //A B两份有阶段变化的采样
            float3 uvwA = FlowUVW(IN.uv_MainTex,flowVector,jump,_FlowOffect,_Tiling,time,false);
            float3 uvwB = FlowUVW(IN.uv_MainTex,flowVector,jump,_FlowOffect,_Tiling,time,true);

            float3 normalA = UnpackNormal(tex2D(_NormalMap,uvwA.xy)) * uvwA.z;
            float3 normalB = UnpackNormal(tex2D(_NormalMap,uvwB.xy)) * uvwB.z;

            fixed4 texA = tex2D (_MainTex,uvwA.xy)* uvwA.z * PowerA ;
            fixed4 texB = tex2D (_MainTex,uvwB.xy)* uvwB.z * PowerB ;

            fixed4 c = (texA + texB) * _Color;
            o.Normal = normalize(normalA + normalB);
            o.Albedo = c;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            
        }
        ENDCG
    }
    FallBack "Diffuse"
}
