Shader "Tyouka/Env/Dithering"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DitherTex ("Dither Tex",2D) = "white" {}

        _MaxDistance ("_MaxDistance",float) = 0
        _MinDistance ("_MinDistance",float) = 0

    }

    SubShader
    {
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float3 viewPos : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _DitherTex;
            float4 _DitherTex_ST;
            float4 _DitherTex_TexelSize;
            float _MaxDistance;
            float _MinDistance;

            v2f vert(appdata_base v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
                o.screenPos = ComputeScreenPos(v.vertex);
                o.viewPos = UnityObjectToViewPos(v.vertex.xyz);
                return o;
            }

            fixed4 frag(v2f i):SV_Target
            {
                float4 texColor = tex2D(_MainTex, i.uv);

                float2 screenPos = i.screenPos.xy/i.screenPos.w;
                float2 dttherCoordinate = screenPos * _ScreenParams.xy * _DitherTex_TexelSize.xy;
                float ditherValue = tex2D(_DitherTex,dttherCoordinate * _DitherTex_ST.xy);

                float relDistance =  - i.viewPos.z;

                _MinDistance = min(_MinDistance,_MaxDistance);
                relDistance = relDistance - _MinDistance;
                relDistance = relDistance / (_MaxDistance - _MinDistance);
                relDistance -= ditherValue;
                clip(relDistance - ditherValue);

                float4 ditherValueCol = float4(ditherValue,ditherValue,ditherValue,1);
                return texColor;
            }

            ENDCG
        }

       
    }
}
