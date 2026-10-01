Shader "Custom/PixelSpriteOutline"
{
    Properties
    {
        [PerRendererData]
        _MainTex ("Sprite Texture", 2D) = "white" {}

        [PerRendererData]
        _Color ("Sprite Color", Color) = (1,1,1,1)

        _OutlineColor ("Outline Color", Color) = (1,1,1,1)

        _OutlinePixels ("Outline Pixels", Range(1,4)) = 1

        _OutlineSize ("Outline Size", Range(0.001,0.1)) = 0.01

        _AlphaThreshold ("Alpha Threshold", Range(0,1)) = 0.01
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;
            float4 _Color;
            float4 _OutlineColor;

            float _OutlinePixels;
            float _OutlineSize;
            float _AlphaThreshold;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float2 expandDirection;

                expandDirection.x =
                    IN.positionOS.x >= 0.0
                    ? 1.0
                    : -1.0;

                expandDirection.y =
                    IN.positionOS.y >= 0.0
                    ? 1.0
                    : -1.0;

                float3 position =
                    IN.positionOS;

                position.xy +=
                    expandDirection *
                    _OutlineSize;

                OUT.positionHCS =
                    TransformObjectToHClip(position);

                OUT.color =
                    IN.color *
                    _Color;

                OUT.uv =
                    IN.uv;

                return OUT;
            }

            half SampleAlpha(float2 uv)
            {
                return
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        uv
                    ).a;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 sprite =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        IN.uv
                    );

                sprite *= IN.color;

                // 현재 위치가 실제 캐릭터인 경우
                if (sprite.a > _AlphaThreshold)
                {
                    return sprite;
                }

                float2 texel =
                    _MainTex_TexelSize.xy *
                    _OutlinePixels;

                half surroundingAlpha = 0.0;

                surroundingAlpha =
                    max(
                        surroundingAlpha,
                        SampleAlpha(
                            IN.uv +
                            float2(texel.x, 0)
                        )
                    );

                surroundingAlpha =
                    max(
                        surroundingAlpha,
                        SampleAlpha(
                            IN.uv -
                            float2(texel.x, 0)
                        )
                    );

                surroundingAlpha =
                    max(
                        surroundingAlpha,
                        SampleAlpha(
                            IN.uv +
                            float2(0, texel.y)
                        )
                    );

                surroundingAlpha =
                    max(
                        surroundingAlpha,
                        SampleAlpha(
                            IN.uv -
                            float2(0, texel.y)
                        )
                    );

                surroundingAlpha =
                    max(
                        surroundingAlpha,
                        SampleAlpha(
                            IN.uv +
                            float2(texel.x, texel.y)
                        )
                    );

                surroundingAlpha =
                    max(
                        surroundingAlpha,
                        SampleAlpha(
                            IN.uv +
                            float2(-texel.x, texel.y)
                        )
                    );

                surroundingAlpha =
                    max(
                        surroundingAlpha,
                        SampleAlpha(
                            IN.uv +
                            float2(texel.x, -texel.y)
                        )
                    );

                surroundingAlpha =
                    max(
                        surroundingAlpha,
                        SampleAlpha(
                            IN.uv -
                            float2(texel.x, texel.y)
                        )
                    );

                if (surroundingAlpha >
                    _AlphaThreshold)
                {
                    half4 outline =
                        _OutlineColor;

                    outline.a =
                        surroundingAlpha *
                        _OutlineColor.a;

                    return outline;
                }

                return 0;
            }

            ENDHLSL
        }
    }
}