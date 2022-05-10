Shader "Custom/UVChecker/UVCheckerUnLit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Cull off
        pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex:POSITION;
                float2 texcoord:TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex :SV_POSITION;
                float2 uv : TEXCOORD0;
            }

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
            }

            fixed4 frag(v2f i):SV_TARGET
            {
                fixed4 tex = tex2D(_MainTex,i.uv);
                return tex;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
