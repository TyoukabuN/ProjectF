Shader "TyoukabuN/HoleTwisty"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HoleCenter("Hole Center",Vector) = (0.5,0.5,0,0)
        _HoleColor("Hole Color",Color) = (1,1,1,1)
        _Radius("Radius",float)=0.5
        _EdgeInner("Edge Inner",float)=0.4
        _EdgeOuter("Edge Outer",float)=0.5
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _HoleColor;
            float2 _HoleCenter;
            float _EdgeInner;
            float _EdgeOuter;
            float _Radius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float len = length(i.uv - _HoleCenter.xy);
                float value = smoothstep(_EdgeOuter, _EdgeInner, len);

                fixed2 dir = 0.5 - (i.uv.xy - _Radius);
                fixed4 col = tex2D(_MainTex, i.uv + _Radius * value * dir - 0.5 + _Time.xx);
                return lerp(col, _HoleColor,value);
            }
            ENDCG
        }
    }
}
