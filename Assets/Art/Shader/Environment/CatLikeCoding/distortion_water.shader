Shader "Custom/catlikecodeing_distortion_water"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        [NoScaleOffset] _FlowMap ("Flow (RG,A noise)",2D) = "black" {}
        _UJump ("U jump per phase", Range(-0.25, 0.25)) = 0.25
		_VJump ("V jump per phase", Range(-0.25, 0.25)) = 0.25
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _TimeScale ("Time Scale",float) = 1

        PowerA ("Phase A Power",Range(0,1)) = 1
        PowerB ("Phase B Power",Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #include "Flow.cginc"

        sampler2D _MainTex,_FlowMap;
        float _UJump, _VJump;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        half PowerA;
        half PowerB;
        half _TimeScale;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 flowVector = tex2D(_FlowMap,IN.uv_MainTex).rg * 2 - 1;
            float noise = tex2D(_FlowMap,IN.uv_MainTex).a;
            float time = noise + _Time.y * _TimeScale;
            float2 jump = float2(_UJump, _VJump);

            float3 uvwA = FlowUVW(IN.uv_MainTex,flowVector,jump,time,false);
            float3 uvwB = FlowUVW(IN.uv_MainTex,flowVector,jump,time,true);

            fixed4 texA = tex2D (_MainTex,uvwA.xy)* uvwA.z * PowerA ;
            fixed4 texB = tex2D (_MainTex,uvwB.xy)* uvwB.z * PowerB ;

            fixed4 c = (texA + texB) * _Color;

            o.Albedo = c.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
