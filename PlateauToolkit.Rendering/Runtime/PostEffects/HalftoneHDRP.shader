Shader "Hidden/HalftoneHDRP"
{
    HLSLINCLUDE
    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
    #pragma shader_feature __ UNITY_PIPELINE_HDRP
    #if UNITY_PIPELINE_HDRP
        #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

        // The PositionInputs struct allow you to retrieve a lot of useful information for your fullScreenShader:
        // struct PositionInputs
        // {
            //     float3 positionWS;  // World space position (could be camera-relative)
            //     float2 positionNDC; // Normalized screen coordinates within the viewport    : [0, 1) (with the half-pixel offset)
            //     uint2  positionSS;  // Screen space pixel coordinates                       : [0, NumPixels)
            //     uint2  tileCoord;   // Screen tile coordinates                              : [0, NumTiles)
            //     float  deviceDepth; // Depth from the depth buffer                          : [0, 1] (typically reversed)
            //     float  linearDepth; // View space Z coordinate                              : [Near, Far]
        // };

        // To sample custom buffers, you have access to these functions:
        // But be careful, on most platforms you can't sample to the bound color buffer. It means that you
        // can't use the SampleCustomColor when the pass color buffer is set to custom (and same for camera the buffer).
        // float4 CustomPassSampleCustomColor(float2 uv);
        // float4 CustomPassLoadCustomColor(uint2 pixelCoords);
        // float LoadCustomDepth(uint2 pixelCoords);
        // float SampleCustomDepth(float2 uv);

        // There are also a lot of utility function you can use inside Common.hlsl and Color.hlsl,
        // you can check them out in the source code of the core SRP package.
        TEXTURE2D_X(_halftoneBuffer); 
        SAMPLER(sampler_halftoneBuffer);
        float _Size;
        float _Range;
        float _UseColor;

        float2 ClampUVs(float2 uv)
        {
            uv = clamp(uv, 0, _RTHandleScale.xy - _ScreenSize.zw * 2); // clamp UV to 1 pixel to avoid bleeding
            return uv;
        }

        float4 Frag(Varyings varyings) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
            float depth = LoadCameraDepth(varyings.positionCS.xy);
            PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
            float3 viewDirection = GetWorldSpaceNormalizeViewDir(posInput.positionWS);
            float2 uv = ClampUVs(posInput.positionNDC.xy * _RTHandleScale.xy);
            float2 uvScaled = uv * (100.0 / _Size);
            float aspectRatio = _ScreenSize.x / _ScreenSize.y;

            float4 col = SAMPLE_TEXTURE2D_X(_halftoneBuffer, sampler_halftoneBuffer, uv);
            float luminance = saturate(dot(col.rgb, float3(0.3, 0.59, 0.11)));
            luminance = smoothstep(0, _Range * 2, luminance * 0.5 + 0.5);

            uvScaled.x *= aspectRatio;
            float2 uvFrac = frac(uvScaled);

            float d = length(uvFrac - 0.5) * 2.0;
            d = step(d, luminance);
            float4 c = lerp(d * luminance, d * col, _UseColor);
            c.a = 1;

            return c; 
        }
    #else 
        struct Appdata { };
        struct Varyings { };

        Varyings Vert(Appdata v) : SV_Position
        {
            Varyings o;
            return o;
        }

        float4 Frag(Varyings varyings) : SV_Target
        {
            return float4(1.0, 0.0, 0.0, 1.0);
        }
    #endif
    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "Custom Pass 0"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
            #pragma fragment Frag
            #pragma vertex Vert 
            ENDHLSL
        }
    }
    Fallback Off
}