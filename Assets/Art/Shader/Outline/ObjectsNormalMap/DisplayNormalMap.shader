
Shader "TyoukabuN/DisplayNormalMap"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _OutlineColor("OutlineColor",Color) = (0,0,0,1)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work

            #include "UnityCG.cginc"
            #include "Assets/Art/Shader/Base/TyousShaderUtility.cginc"
            #include "Assets/Art/Shader/Outline/OutlineEffectBase.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _OutlineColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = GetScreenSpaceTexcood(i.pos);
                float3 normal = SamplingDepthNormalsTexture_Normal(uv);
                //①基于深度的描边
                float depth = GetEdge_DepthDiff_RobertsCross(i.pos);
                depth = step(0.001, depth);
                float3 depthColor = fixed3(depth, depth, depth);
                //②基于发现差异的描边
                float edge = GetEdge_NormalDiff_RobertsCross(i.pos);
                float3 edgeColor = fixed3(edge, edge, edge);
                //混合①②两种描边
                //NOTICE:这种方法的Camera的Far-Near不能太小
                float3 outline = step(0.999, edgeColor) + depthColor;
                outline = saturate(outline);

                float4 color = tex2D(_MainTex, i.uv.xy);

                float3 outlineColor = lerp(color.rgb, _OutlineColor.rgb, _OutlineColor.a);
                return fixed4(lerp(color.rgb, outlineColor, step(0.001, outline)),1);
            }
            ENDCG
        }
    }
}
