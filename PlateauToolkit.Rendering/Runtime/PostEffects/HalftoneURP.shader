Shader "Hidden/HalftoneURP" {
    SubShader {
        Tags {"RenderType"="Opaque" }
        Pass {
            HLSLPROGRAM
            #pragma shader_feature __ UNITY_PIPELINE_URP
            #if UNITY_PIPELINE_URP
                #pragma vertex vert 
                #pragma fragment frag
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
                TEXTURE2D_X(_halftoneBuffer); 
                SAMPLER(sampler_halftoneBuffer);
                float _Size;
                float _Range;
                float _UseColor;
                
                struct appdata_t 
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float4 screenPos : TEXCOORD2;
                };
                
                struct v2f 
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    float4 screenPos : TEXCOORD2;
                };

                v2f vert (appdata_t v) {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(input);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                    VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                    o.vertex = vertexInput.positionCS;
                    o.screenPos = ComputeScreenPos(o.vertex);
                    o.uv = v.uv;

                    return o;
                }
                
                float4 frag (v2f i) : SV_Target {
                    
                    float aspectRatio = _ScreenParams.x / _ScreenParams.y;
                    float2 uvScaled = i.uv * (100.0 / _Size);

                    float4 col = SAMPLE_TEXTURE2D(_halftoneBuffer, sampler_halftoneBuffer, i.uv);
                    float luminance = saturate(dot(col.rgb, float3(0.3, 0.59, 0.11)));
                    luminance = smoothstep(0, _Range, luminance);

                    uvScaled.x *= aspectRatio;
                    float2 uvFrac = frac(uvScaled);

                    float d = length(uvFrac - 0.5) * 2.0;
                    d = step(d, luminance);

                    return lerp(d * luminance, d * col, _UseColor); 
                }
            #else
                struct appdata_t { };
                struct v2f { };

                v2f vert (appdata_t v)
                {
                    v2f o;
                    return o;
                }

                float4 frag (v2f i) : SV_Target
                {
                    return float4(1.0, 0.0, 0.0, 1.0); // Returns red color
                }
            #endif
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
