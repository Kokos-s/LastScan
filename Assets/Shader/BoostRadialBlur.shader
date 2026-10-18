Shader "Custom/BoostRadialBlur"
{
    Properties
    {
        _Strength ("Blur Strength", Range(0, 0.15)) = 0.05
        _InnerRadius ("Clear Center Radius", Range(0, 0.9)) = 0.35
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "BoostRadialBlur"

            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Strength;
                float _InnerRadius;
            CBUFFER_END

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;

                half4 original = SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp, uv
                );

                // Не применяем эффект к ортографической миникарте.
                if (unity_OrthoParams.w > 0.5 || _Strength <= 0.00001)
                    return original;

                float2 direction = uv - float2(0.5, 0.5);
                float radius = length(direction) * 2.0;

                float edgeMask = smoothstep(
                    _InnerRadius, 1.0, radius
                );

                float amount = _Strength * edgeMask;

                half4 color = original;

                [unroll]
                for (int i = 1; i < 8; i++)
                {
                    float progress = (float)i / 7.0;
                    float2 sampleUV =
                        uv - direction * amount * progress;

                    color += SAMPLE_TEXTURE2D_X(
                        _BlitTexture, sampler_LinearClamp, sampleUV
                    );
                }

                return color / 8.0;
            }

            ENDHLSL
        }
    }
}