Shader "Custom/GlowShader"
{
    Properties
    {
        [HDR]_BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _Opacity("Opacity", Range(0, 1)) = 0.5 
        _Strength("Strength", Range(0.1, 3)) = 1
        _BlendTex("Blend Texture", Range(0.0, 1)) = 0
        _Speed("Speed", Range(0, 10)) = 2.0 
        [MainTexture] _BaseMap("Base Map", 2D) = "black"
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            float _BeatTime;
            float4 _AudioBands;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS    : TEXCOORD1; // <- pass to fragment
                float3 viewDirWS   : TEXCOORD2;
                float3 positionWS  : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Opacity;
                float _Strength;
                float _Speed;
                float _BlendTex;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDirWS   = GetWorldSpaceViewDir(positionWS);
                OUT.positionWS = positionWS;
                return OUT;
            }

            static inline float  n21(float2 p) { return frac(sin(dot(p, float2(524.423, 123.34))) * 3228324.345); }

            static float  noise2(float2 n) {
                float2 d = float2(0.0, 1.0);
                float2 b = floor(n), f = frac(n);
                float n00 = n21(b);
                float n10 = n21(b + d.yx);
                float n01 = n21(b + d.xy);
                float n11 = n21(b + d.yy);
                float nx0 = lerp(n00, n10, f.x);
                float nx1 = lerp(n01, n11, f.x);
                return lerp(nx0, nx1, f.y);
            }

            static inline float3 bump3y(float3 x, float3 yoffset) {
                float3 y = 1.0 - x * x;
                return saturate(y - yoffset);
            }

            static float3 spectral_zucconi6(float x) {
                x = frac(x);
                const float3 c1 = float3(3.54585104, 2.93225262, 2.41593945);
                const float3 x1 = float3(0.69549072, 0.49228336, 0.27699880);
                const float3 y1 = float3(0.02312639, 0.15225084, 0.52607955);
                const float3 c2 = float3(3.90307140, 3.21182957, 3.96587128);
                const float3 x2 = float3(0.11748627, 0.86755042, 0.66077860);
                const float3 y2 = float3(0.84897130, 0.88445281, 0.73949448);
                return bump3y(c1 * (x - x1), y1) + bump3y(c2 * (x - x2), y2);
            }
            
            float3 pal(float x) {
                return .5 + .5*cos( PI*2.*(float3(1.0,1.0,1.0)*x+float3(0.0,0.3,0.5)));
            }

            // n: surface/perturbed normal (world/tangent—your choice), p: 2D coord for noise (e.g., UV * scale)
            static inline float3 iridesentColor(float3 n, float3 p)
            {
                float3 n2 = n;
                n2.xy += noise2(p.xy) * 0.5 - 0.025;    // subtle wobble
                n2 = normalize(n2);

                float height = atan2(n2.y, n2.x);    // [-pi, pi]
                float3 iri   = pal(height * 1.2 +.25*_Time.y) *
                            smoothstep(0.8, 0.2, abs(n2.z)) - 0.02;

                return iri; // linear RGB; apply gamma elsewhere if needed
            }


            #define SIN(x) (sin(x)*.5+.5)
            half4 frag(Varyings IN) : SV_Target
            {
                half3 n = normalize(IN.normalWS);
                half3 rd = normalize(IN.viewDirWS);

                float3 p = IN.positionWS; 
                half3 iri = iridesentColor(n, p);
                half3 col =  _BaseColor.rgb + lerp(.5, 1., 1.5*_AudioBands.x)*iri;
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

                // fresnel
                half fre = pow(1.0 - saturate(dot(n, rd)), 2.0);
                float alpha = fre*lerp(tex, fre, .5)* lerp(.1, 1., (SIN(_Time.y*_Speed + _BeatTime)));

 
                alpha = lerp(alpha, 1., tex.a*_BlendTex*.5);
                return half4(col*_Strength*(1.+tex.a*.5*_BlendTex), 1)*alpha;
            }
            ENDHLSL
        }
    }
}
