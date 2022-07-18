Shader "Unlit/RT3R-Fog"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlossTex("GlossTex",2D) = "white" {}
        _AmbientColor("Ambient Color",Color) = (1,1,1,1)
        _SpecularColor("Specular Color",Color) = (1,1,1,1)
        _SpecularPower("Specular Power",float) = 50
        _FogColor("Fog Color",Color) = (0.5,0.5,0.5,0.5)
        _FogStart("Fog Start",float) = 10
        _FogRange("Fog Range",float) = 20
    }
    SubShader
    {
        Name "FORWARD"
        Tags { "LightMode"="ForwardBase" "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "AutoLight.cginc"
            #include "RT3RCore.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                DECLARE_LIGHT_BASE_FIELD(2)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _GlossTex;
            float4 _AmbientColor;
            float4 _SpecularColor; 
            float _SpecularPower;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                ASSIGH_LIGHT_BASE_FIELD(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 albedo = tex2D(_MainTex, i.uv);
                float gloss = tex2D(_GlossTex, i.uv).r;
                fixed3 viewDirection = UnityWorldSpaceViewDir(i.worldPos);
                fixed3 lightDirection = normalize(UnityWorldSpaceLightDir(i.worldPos));
                //
                LightContributionData lightContributionData;
                lightContributionData.albedo = albedo;
                lightContributionData.normal = UnityObjectToWorldNormal(i.normal);
                lightContributionData.viewDirection = normalize(viewDirection);
                lightContributionData.lightColor = _LightColor0;
                lightContributionData.lightDirection = float4(lightDirection,1);
                lightContributionData.specularColor = _SpecularColor;
                lightContributionData.specularPower = _SpecularPower;
                lightContributionData.gloss = gloss;

                float4 finalColor = 1.0f;
                finalColor.rgb =  APPLY_LIGHT_TO_COLOR(_AmbientColor,albedo) + GetLightContribution(lightContributionData);
                float fogAmount = get_fog_amount(viewDirection,_FogStart,_FogRange);
                finalColor.rgb = lerp(finalColor.rgb,_FogColor.rgb * _FogColor.a,fogAmount);
                return finalColor;
            }
            ENDCG
        }
    }
}
