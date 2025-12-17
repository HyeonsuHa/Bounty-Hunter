Shader "Custom/HybridLava_Opaque"
{
    Properties
    {
        _MainTex        ("Lava Texture", 2D) = "white" {}
        _TexTiling      ("Texture Tiling", Vector) = (1, 1, 0, 0)

        _Emission       ("Emission Strength", Range(0, 20)) = 8
        _FlowSpeed      ("Flow Speed (Y)", Range(0, 3)) = 1
        _Distortion     ("Distortion", Range(0, 1)) = 0.2
        _NoiseScale     ("Noise Scale", Range(1, 30)) = 10
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        ZWrite On
        Cull Back

        Pass
        {
            Name "LavaPass"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _TexTiling;
            float  _Emission;
            float  _FlowSpeed;
            float  _Distortion;
            float  _NoiseScale;

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(41.3, 89.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash(i);
                float b = hash(i + float2(1,0));
                float c = hash(i + float2(0,1));
                float d = hash(i + float2(1,1));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(a, b, u.x),
                            lerp(c, d, u.x), u.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;

                [unroll]
                for (int i = 0; i < 5; i++)
                {
                    v += noise(p) * a;
                    p *= 2.0;
                    a *= 0.5;
                }
                return v;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 baseUV = IN.uv;

                float t = _Time.y * _FlowSpeed;
                float2 flowUV = baseUV + float2(0, t);

                float2 noiseUV  = baseUV * _NoiseScale;
                float2 warp = float2(fbm(noiseUV + t), fbm(noiseUV - t));
                flowUV += warp * _Distortion;

                float2 texUV = flowUV * _TexTiling.xy;
                float4 texCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, texUV);

                float3 finalCol = texCol.rgb * _Emission;

                return float4(finalCol, 1.0);
            }
            ENDHLSL
        }
    }
}
