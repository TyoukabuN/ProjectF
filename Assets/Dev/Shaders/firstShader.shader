// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/firstShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color",Color) = (1.0,1.0,1.0,1.0)
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            fixed4 _Color;
            sampler2D _MainTex;
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
           
            float4 vert(float4 pos : POSITION):POSITION
            {
                return UnityObjectToClipPos(pos);
            }

            fixed4 frag(float2 uv:TEXCOORD0):COLOR
            {   
                return _Color;
                //return tex2D(_MainTex,uv);
            }
            //fixed4 frag (v2f i) : SV_Target
            //{
            //    // sample the texture
            //    fixed4 col = tex2D(_MainTex, i.uv);
            //    // apply fog
            //    UNITY_APPLY_FOG(i.fogCoord, col);
            //    return col;
            //}
            ENDCG
        }
   
    }
}
