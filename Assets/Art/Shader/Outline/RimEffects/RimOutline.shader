Shader "TyoukabuN/RimOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Fresnel("_Fresnel",Vector) = (1,5,0.01,0)
        _FresnelColor("_FresnelColor",Color) = (1,1,1,1)

        _OutlineWidth("_OutlineWidth",float) = 1
        _OutlineSoftness("_OutlineSoftness",float) = 0.05
        _OutlinePower("_OutlinePower",float) = 5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Art/Shader/Base/TyousShaderUtility.cginc"
            #include "Assets/Art/Shader/Outline/OutlineEffectBase.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                WORLD_POSITION(1, float3)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Fresnel;
            float4 _FresnelColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = v.normal;
                o.wpos = mul(UNITY_MATRIX_M, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 N = normalize(i.normal);
                float3 L = normalize(_WorldSpaceLightPos0 - i.wpos);
                float3 V = normalize(_WorldSpaceCameraPos - i.wpos);

                fixed4 col = tex2D(_MainTex, i.uv);

                float fresnel = GetEdge_Rim(N,V);
                float3 fresnelColor = fresnel * _FresnelColor;
                col.rgb = lerp(col, fresnelColor, fresnel);

                return col;
            }
            ENDCG
        }
    }
}
