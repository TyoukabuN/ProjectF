Shader "TyoukabuN/Test/ParamTex"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 colorParam : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

             sampler2D _ParamTexture;
             float4 _ParamTexture_ST;
             float4 _ParamTexture_TexelSize;


            inline float2 GetParamTexcoord(float2 index)
            {
                float unitX = _ParamTexture_TexelSize.x;
                float unitY = _ParamTexture_TexelSize.y;
                index *= _ParamTexture_TexelSize.xy;
                index += _ParamTexture_TexelSize.xy * 0.5;

                return index;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float2 index = float2(1,1);
                o.colorParam = tex2Dlod(_ParamTexture, float4(GetParamTexcoord(index),0,0));

                return o;
            } 

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(i.colorParam);
            }
            ENDCG
        }
    }
}
