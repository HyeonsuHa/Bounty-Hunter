Shader "Custom/GridOverlayURP"
{
    Properties
    {
        _BaseColor ("Base Color (with Alpha)", Color) = (0,0,0,0.08)
        _LineColor ("Line Color (with Alpha)", Color) = (1,1,1,0.35)

        _CellSize ("Cell Size (World)", Vector) = (1,1,1,0)
        _Origin   ("Grid Origin (World)", Vector) = (0,0,0,0)

        _LineWidth ("Line Width (World)", Float) = 0.04
        _MajorLineEvery ("Major Line Every N Cells", Float) = 5
        _MajorLineMul ("Major Line Intensity Mul", Float) = 1.6
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }

        Pass
        {
            Name "GridOverlay"
            Tags { "LightMode"="UniversalForward" }

            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos    : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _LineColor;
                float4 _CellSize;
                float4 _Origin;
                float  _LineWidth;
                float  _MajorLineEvery;
                float  _MajorLineMul;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.worldPos = worldPos;
                return OUT;
            }

            float GridLineMask(float coord, float cell, float lineWidth)
            {
                float u = coord / max(cell, 1e-6);
                float f = frac(u);
                float d = min(f, 1.0 - f);

                float w = lineWidth / max(cell, 1e-6);
                return 1.0 - smoothstep(w * 0.5, w, d);
            }

            float MajorLineMask(float coord, float cell, float majorEvery, float lineWidth)
            {
                float majorCell = max(cell * max(majorEvery, 1.0), 1e-6);
                return GridLineMask(coord, majorCell, lineWidth);
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float3 wp = IN.worldPos - _Origin.xyz;

                float cellX = max(_CellSize.x, 1e-6);
                float cellZ = max(_CellSize.z, 1e-6);

                float minorX = GridLineMask(wp.x, cellX, _LineWidth);
                float minorZ = GridLineMask(wp.z, cellZ, _LineWidth);
                float minorMask = max(minorX, minorZ);

                float majorX = MajorLineMask(wp.x, cellX, _MajorLineEvery, _LineWidth);
                float majorZ = MajorLineMask(wp.z, cellZ, _MajorLineEvery, _LineWidth);
                float majorMask = max(majorX, majorZ);

                float gridMask = max(minorMask, saturate(majorMask * _MajorLineMul));

                float4 col = _BaseColor;
                col.rgb = lerp(_BaseColor.rgb, _LineColor.rgb, gridMask);
                col.a   = lerp(_BaseColor.a,  _LineColor.a,  gridMask);

                return col;
            }
            ENDHLSL
        }
    }
}
