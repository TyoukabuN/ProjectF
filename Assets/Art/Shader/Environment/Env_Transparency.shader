Shader "Tyouka/Env/Transparency"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FadeOutParam ("FadeOutParam",Vector) = (0,0,0,0)
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend ("SrcBlend",float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)]_DistBlend ("SrcBlend",float) = 10
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        Blend [_SrcBlend][_DistBlend]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "CircleScanLine.cginc"


            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 wpos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _FadeOutParam;
            float4 _Realtime;

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.wpos = mul(UNITY_MATRIX_M,v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float param  = 1 - saturate((_FadeOutParam.y - _Realtime.y)/(_FadeOutParam.y - _FadeOutParam.x));
                float targetAlpha = lerp(_FadeOutParam.z,_FadeOutParam.w,param);

                targetAlpha = lerp(1,targetAlpha,step(0.001,_FadeOutParam.x));

                fixed4 texColor = tex2D(_MainTex, i.uv); 

                float4 col = texColor;

                UNITY_APPLY_FOG(i.fogCoord, col);

                col.a *= targetAlpha;

                return col;
            }
            ENDCG
        }
    }
}
