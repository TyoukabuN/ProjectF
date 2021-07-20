Shader "Tyous/Environment/Scanablc"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScanLineAppearCenter ("ScanLineAppearPoint", Vector) = (0,0,0,0)
        _ScanLineWidth ("ScanLineWidth", float) = 0.5
        _ScanLineColor ("ScanLineColor",Color)= (1,1,1,1)

        _ScaningRadius ("ScaningRadius",float)= 0.5
        _AutoScanMul ("AutoScanMul",float)= 0

        _ForwardGradient ("ForwardGradient",float)= 5
        _BackGradient ("BackGradient",float)= 5

        _ScanLineTestParam ("ScanLineTestParam", Vector) = (0,0,0,0)
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
            float4 _ScanLineColor;
            float _AutoScanMul;

            float _ScaningRadius;

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
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                float pct = CompoutScanLinePct(_ScaningRadius,i.wpos,_Time.y*_AutoScanMul);

                col = lerp(col,_ScanLineColor + col,pct);
                return col;
            }
            ENDCG
        }
    }
}
