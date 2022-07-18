Shader "Unlit/RT3R-NormalMapping"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BumpTex ("Bump",2D) = "white"{}
        _BumpPower("_Bump Power",float) = 1
        _DisplacementMap ("Displacement Map",2D)= "white"{}
        _DisplacementScale("_Displacement Scale",float) = 20
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
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "AutoLight.cginc"
            #include "RT3RCore.cginc"

            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal:NORMAL;
                float3 tangent:TEXCOORD1;
                float3 binormal:TEXCOORD2;
                DECLARE_WORLD_SPACE_POSITION(3)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;            
            sampler2D _BumpTex;
            float4 _BumpTex_ST;            
            sampler2D _DisplacementMap;
            float4 _DisplacementMap_ST;
            float _DisplacementScale;
            sampler2D _GlossTex;
            float4 _AmbientColor;
            float4 _SpecularColor; 
            float _SpecularPower;
            float _BumpPower;

            v2f vert (a2v v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                float displacement = tex2Dlod(_DisplacementMap,float4(o.uv,0,0));
                v.vertex.xyz += v.normal * _DisplacementScale * (displacement - 1);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.tangent = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.binormal = normalize(cross(o.normal, o.tangent) * v.tangent.w);
                ASSIGN_WORLD_SPACE_POSITION(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 albedo = tex2D(_MainTex, i.uv);
                float gloss = tex2D(_GlossTex, i.uv).r;
                fixed3 viewDirection = UnityWorldSpaceViewDir(i.worldPos);
                fixed3 lightDirection = normalize(UnityWorldSpaceLightDir(i.worldPos));
                //
                float3x3 tbn = float3x3(i.tangent,i.binormal,i.normal);
                float3 normal = UnpackNormal(tex2D(_BumpTex,i.uv));
                normal = mul(tbn,normal) * _BumpPower;
                //
                LightContributionData lightContributionData;
                lightContributionData.albedo = albedo;
                lightContributionData.normal = normal;
                lightContributionData.viewDirection = normalize(viewDirection);
                lightContributionData.lightColor = _LightColor0;
                lightContributionData.lightDirection = float4(lightDirection,1);
                lightContributionData.specularColor = _SpecularColor;
                lightContributionData.specularPower = _SpecularPower;
                lightContributionData.gloss = 1;

                float4 finalColor = 1.0f;
                finalColor.rgb =  APPLY_LIGHT_TO_COLOR(_AmbientColor,albedo) + GetLightContribution(lightContributionData);
                return finalColor;
            }
            ENDCG
        }
    }
}
