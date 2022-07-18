Shader "Unlit/RT3R-Blend"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FogColor("Fog Color",Color) = (0.5,0.5,0.5,0.5)
        _FogStart("Fog Start",float) = 10
        _FogRange("Fog Range",float) = 20
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" 
        "LightMode"="ForwardBase"
        "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "RT3RCore.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                DECLARE_LIGHT_BASE_FIELD_VERT
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                DECLARE_LIGHT_BASE_FIELD(1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                ASSIGH_LIGHT_BASE_FIELD(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                APPLTY_FOG(col)
                return col;
            }
            ENDCG
        }
    }
}
