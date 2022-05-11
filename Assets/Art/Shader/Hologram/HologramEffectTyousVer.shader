Shader "X_Shader/C_Charactar/Ef/HologramReplicate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FresnelColor ("Fresnel Color",Color) = (1,1,1,1)
        _FresnelPower ("Fresnel Power",float) = 1
        _FresnelDotAdditon("Fresnel Dot Additon",float) = 2.2
        _TextureMultiplier ("Texture Multiplier",float) = 0.9
        _TextureAdditon ("Texture Additon",float) = 0.6
        _FresnelMultiplier ("Fresnel Multiplier",float) = 1.51
        _FlashFraquency("闪烁频率",Range(0,1)) = 1
        _FlashPower("闪烁强度",Range(0,1)) = 1
        //view space scanline
        //_WaveTex("扫描线纹理",2D) = "white" {}
        //_WaveTexScrollSpeed("扫描线纹理滚动速度",float) = 1
        _WaveTex("扫描线纹理", 2D) = "white" {}
        _WaveColor("扫描线颜色",Color) = (1,1,1,1)
        //[KeywordEnum(Off,On)]_WaveSwitch("扫描线开关",float) = 1
        _WaveFrequency("扫描线的频率",float) = 4.76
        //_WaveLength("扫描线的波长",float) = 1
        _WaveSpeed("扫描线的速度",float) = 1
        _WaveSinParam("Wave Sin Param",Vector) = (10,6.5,0.3,0.06)
        _Test("test",Vector) = (1,1,1,1)

        
        //Stencil
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
    }
    SubShader
    {
        Tags {"RenderType"="Opaque" "Queue" = "Transparent"}
        LOD 100
        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
        }
        Pass
        {
            ZWrite On
            ColorMask 0

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Test;
            float4 _WaveSinParam;

            inline float VertexWave(float x, float t)
            {
                float vertexAddition = fmod(x + t * _WaveSinParam.y, _WaveSinParam.x);
                vertexAddition = smoothstep(0, _WaveSinParam.z, vertexAddition);
                vertexAddition = lerp(0, vertexAddition, step(vertexAddition, 0.9999));
                return vertexAddition * _WaveSinParam.w;
            }

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            v2f vert(appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.pos.x += VertexWave(o.pos.x, _Time.y);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }
            float4 frag(v2f i) : SV_Target
            {
                return fixed4(1, 1, 1, 1);
            }
            ENDCG

        }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                float3 wpos : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                float3 lightDir : TEXCOORD3;
                float4 screenPosition : TEXCOORD4;
                float3 vs_TEXCOORD5 : TEXCOORD5;
                float4 vertex : TEXCOORD6;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _FresnelColor;
            float _FresnelPower;
            float _FresnelMultiplier;
            float _FresnelDotAdditon;
            float _TextureMultiplier;
            float _TextureAdditon;
            float _LineModf;

            //view space scanline
            float4 _WaveColor;
            sampler2D _WaveTex;
            //uniform float4 _WaveTex_ST;
            float _WaveFrequency;
            float _FlashFraquency;
            float _FlashPower;
            float4 _WaveSinParam;
            float4 _Test;
            //uniform float _WaveLength;
            //uniform float _WireWidth;
            //uniform float _SlopeLengle;
            //uniform float _ModelTop;
            //uniform float _ModelBottom;
            uniform float _WaveSpeed;
            //uniform float _WaveTexScrollSpeed;
            //uniform float _WaveSwitch;

            inline float2 Pow4(float2 x)
            {
                return x * x * x * x;
            }

            inline float VertexWave(float x, float t)
            {
                float vertexAddition = fmod(x + t * _WaveSinParam.y, _WaveSinParam.x);
                vertexAddition = smoothstep(0, _WaveSinParam.z, vertexAddition);
                vertexAddition = lerp(0, vertexAddition, step(vertexAddition, 0.9999));
                return vertexAddition * _WaveSinParam.w;
            }

            v2f vert (appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.vertex = v.vertex;
                o.pos.x += VertexWave(o.pos.x, _Time.y);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.wpos = mul(unity_ObjectToWorld, v.vertex);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                o.lightDir = normalize(WorldSpaceLightDir(v.vertex));
                o.screenPosition = ComputeScreenPos(o.pos);
                o.vs_TEXCOORD5.xyz = unity_ObjectToWorld[2].xyz * v.normal.zzz;

                return o;
            }




            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 normal = normalize(i.normal);
                fixed3 viewDir = normalize(i.viewDir);
                fixed3 lightDir = normalize(i.lightDir);
                //fixed frensel = pow(1 - dot(norma l,viewDir),)

                float3 reflDir = reflect(viewDir, normal);
                 
                half nl = saturate(dot(normal, lightDir));
                half nv = saturate(dot(normal, viewDir)); 

                _FlashFraquency = max(_FlashFraquency, 0.001);
                //return float4(subtractor, subtractor, subtractor, 1);
                float2 rlPow4AndFresnelTerm = pow(float2(dot(reflDir, lightDir), (1 - nv) * _FresnelDotAdditon), _FresnelPower);
                float fresnelTerm = rlPow4AndFresnelTerm.y;
                //
                

                //scanline
                float x = i.vertex.y - _Time.x * _WaveSpeed;
                float2 texcoord = abs(sin(_WaveFrequency * x));
                texcoord = asin(texcoord) / UNITY_PI * 2.0;
                texcoord.x = 0.5;
                float4 scanline = tex2D(_WaveTex, texcoord);
                scanline.xyz *= _WaveColor.rgb;
                scanline *= _WaveColor.a;
                //texture
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed Luminance = tex.b + _TextureAdditon;
                tex.a = Luminance * _TextureMultiplier * _FresnelColor.a;
                tex.rgb = Luminance * _FresnelColor.rgb;
                tex = clamp(tex, 0, 1);
                fixed4 transparent = fixed4(0, 0, 0, 0);
                tex = tex * lerp(tex, transparent, fresnelTerm);
                //outline
                fixed4 outline = fresnelTerm * _FresnelMultiplier * _FresnelColor;
                outline.rgb *= _FresnelColor.rgb ;
                float f = abs(sin(400000 / (40000 * (1 - _FlashFraquency)) * _Time.y)) * max(sin(200 * _FlashFraquency * _Time.y), 0) * _FlashPower;
                outline.rgb += outline.rgb * f;


                fixed4 col = (tex + outline + scanline);
                col.rgb *= _FresnelColor.rgb;

                return  col;
            }
            ENDCG
        }
    }
}
