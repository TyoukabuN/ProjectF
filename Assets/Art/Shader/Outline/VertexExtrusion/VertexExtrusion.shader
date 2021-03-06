Shader "TyoukabuN/VertexExtrusion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _OutlineColor("_OutlineColor",Color) = (0,0,0,1)
        _OutlineWidth("_OutlineWidth",float) = 0.02
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Cull front
            
            CGPROGRAM
            #pragma vertex vert_outline_moveVertex
            #pragma fragment frag_outline

            #include "UnityCG.cginc"
            #include "Assets/Art/Shader/Base/TyousShaderUtility.cginc"
            #include "Assets/Art/Shader/Outline/OutlineEffectCore.cginc"

            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Art/Shader/Base/TyousShaderUtility.cginc"
            #include "Assets/Art/Shader/Outline/OutlineEffectCore.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
