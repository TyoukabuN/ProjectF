Shader "tyou/ObjCoverWater"
{
    Properties
    {
        _MainTex ("Texture",2D) = "white"{}
        _Color ("Color",color) = (1,1,1,1)
        _Value ("_Value",vector) = (1,1,1,1)
    }

    SubShader
    {

        pass{
                    Tags{"RenderType" = "Opaque" 
            "Queue" = "Geometry" 
            "LightMode" = "ForwardBase"
         }
            CGPROGRAM
            #include "UnityCG.cginc"
			#include "Lighting.cginc"
            #pragma vertex vert
            #pragma fragment frag
            struct a2v{
                float4 vertex : POSITION;
                float3 normal :NORMAL;
                half2 uv : TEXCOORD0;
            };
            struct v2f{
                float4 pos:SV_POSITION;
                half2 uv:TEXCOORD0;
                float3 normal:TEXCOORD1;
                float3 viewDir : TEXCOORD2;
				float3 lightDir	:TEXCOORD3;
            };
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _Value;
            v2f vert(a2v v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.pos.z -= o.pos.z*0.05;
                o.pos.x += sin(o.pos.x + _Time.y )*_Value.y;
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                // o.wpos = mul(unity_ObjectToWorld,v.vertex);
				o.viewDir = UnityWorldSpaceViewDir(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.lightDir = UnityWorldSpaceLightDir(v.vertex);
 
                return o;
            }
            float4 frag(v2f i):SV_TARGET
            {
                float3 lightDir = normalize(i.lightDir);
                float3 normal = normalize(i.normal);

                float4 col = tex2D(_MainTex,i.uv) * _Color;
                float3 diffuse = col.rgb * saturate(dot(normal,lightDir));
                return float4(diffuse,1);
            }
            ENDCG

        }

        pass{
                    
            Offset 0,1
            CGPROGRAM
            #include "UnityCG.cginc"
			#include "Lighting.cginc"
            #pragma vertex vert
            #pragma fragment frag
            struct a2v{
                float4 vertex : POSITION;
                float3 normal :NORMAL;
                half2 uv : TEXCOORD0;
            };
            struct v2f{
                float4 pos:SV_POSITION;
                half2 uv:TEXCOORD0;
                float3 normal:TEXCOORD1;
                float3 viewDir : TEXCOORD2;
				float3 lightDir	:TEXCOORD3;
            };
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            v2f vert(a2v v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                // o.wpos = mul(unity_ObjectToWorld,v.vertex);
				o.viewDir = UnityWorldSpaceViewDir(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.lightDir = UnityWorldSpaceLightDir(v.vertex);
 
                return o;
            }
            float4 frag(v2f i):SV_TARGET
            {
                float3 lightDir = normalize(i.lightDir);
                float3 normal = normalize(i.normal);

                float4 col = tex2D(_MainTex,i.uv) * _Color;
                float3 diffuse = col.rgb * saturate(dot(normal,lightDir));
                return float4(diffuse,1);
            }
            ENDCG

        }

        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}