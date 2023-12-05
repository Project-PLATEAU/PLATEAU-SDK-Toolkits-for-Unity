Shader "Skybox/URPPhysicallyBasedSky"
{
    Properties
    {
        _Brightness ("Exposure", range(0, 30)) = 20 
        _Samples ("Scattering Samples", Range(2, 64)) = 16
        _CloudLightmapA("Cloud LightmapA", 2D) = "white" {}
        _CloudLightmapB("Cloud LightmapB", 2D) = "white" {}
        _CloudScrollSpeed("Cloud Scroll Speed", Range(-1, 1)) = 0.01
        _CloudCylinderCenter("Cloud Cylinder Center", Vector) = (0, 0, 0)
        _CloudCylinderBottom("Cloud Cylinde rBottom", float) = -50
        _CloudCylinderTop("Cloud Cylinder Top", float) = 50
        _PanoramaTex("Panorama Texture", 2D) = "white" {}
        _GloundHeight("Glound Height", float) = 0
        _SpaceFadeDistance("Space Fade Distance", float) = 30000
        _HightFogStart("Hight Fog Start", float) = 0
        _HightFogEnd("Hight Fog End", float) = 50000

        [HDR]_MoonEmissionColor ("Moon Emission Color", Color) = (1, 1, 1, 1)
        _MoonTexture ("Moon Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off
        Tags { "Queue" = "Background" "RenderType" = "Background" "RenderPipeline" = "UniversalPipeline" "PreviewType" = "Skybox" }


        Pass
        {
            PackageRequirements
            {
                "com.unity.render-pipelines.universal": "10.0"
            }
            HLSLPROGRAM
            #pragma vertex vert 
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION; // clipspace
                float3 worldPos : TEXCOORD1;
            };

            float _Brightness;
            float _SkyMultiplier;
            float _Cloud;
            uint _Samples;

            sampler2D _CloudLightmapA;
            sampler2D _CloudLightmapB;
            float4 _CloudLightmapA_ST;
            float _CloudScrollSpeed;
            float3 _CloudCylinderCenter;
            float _CloudCylinderBottom;
            float _CloudCylinderTop;
            sampler2D _PanoramaTex;
            float _IsNight;

            float _CloudIntensity;
            float _SpaceFadeDistance;
            float _GloundHeight;

            float _HightFogStart;
            float _HightFogEnd;
            float4 _FogColor;
            float _FogDistance;

            float4 _MoonEmissionColor;
            sampler2D _MoonTexture;
            float3 _MoonLightRight;
            float3 _MoonLightUp;
            float3 _MoonLightDirection;
            float _MoonScale;
            float _MoonPhase;

            static const float EARTH_RADIUS = 6371000;
            static const float3 EARTH_CENTER = float3(0, -6371000, 0);
            static const float ATMO_HEIGHT = 100000;
            static const float MIE = 0.000003996;
            static const float3 RAYLEIGH = float3(0.000005802, 0.000013558, 0.0000331);
            static const float3 OZONE = float3(0.00000065, 0.000001881, 0.000000085);

            float2 EarthIntersect (float3 rayPos, float3 rayDir, float3 center, float radius)
            {
                rayPos -= center;
                float a = dot(rayDir, rayDir);
                float b = 2.0 * dot(rayPos, rayDir);
                float c = dot(rayPos, rayPos) - (radius * radius);
                float d = b * b - 4 * a * c;
                if (d < 0)
                {
                    return -1;
                }
                else
                {
                    d = sqrt(d);
                    return float2(-b - d, -b + d) / (2 * a);
                }
            }

            float3 Density (float h)
            {
                float rayleigh = exp(-max(0, h / (ATMO_HEIGHT * 0.08)));
                float mie = exp(-max(0, h / (ATMO_HEIGHT * 0.012)));
                float ozone = max(0, 1 - abs(h - 25000.0) / 15000.0);

                return float3(rayleigh, mie, ozone);
            }

            float3 ViewDepth (float3 rayPos, float3 rayDir)
            {
                float2 intersection = EarthIntersect(rayPos, rayDir, EARTH_CENTER, EARTH_RADIUS + ATMO_HEIGHT);
                float  rayDist    = intersection.y;
                float  stepSize     = rayDist / (_Samples / 8);
                float3 vDepth = 0;

                for (int i = 0; i < (_Samples / 8); i++)
                {
                    float3 pos = rayPos + rayDir * (i + 0.5) * stepSize;
                    float  h   = distance(pos, EARTH_CENTER) - EARTH_RADIUS;
                    vDepth += Density(h) * stepSize;
                }

                return vDepth;
            }

            float3 Scatter (float3 vDepth)
            {
                return exp(-(vDepth.x * RAYLEIGH + vDepth.y * MIE * 1.1 + vDepth.z * OZONE)); 
            }

            float3 RayMarch (float3 rayPos, float3 rayDir, float rayDist)
            {
                Light light = GetMainLight();
                float rayHeight = distance(rayPos, EARTH_CENTER) - EARTH_RADIUS;
                float sampleDistributionExponent = 1 + saturate(1 - rayHeight / ATMO_HEIGHT) * 8;
                float2 intersection = EarthIntersect(rayPos, rayDir, EARTH_CENTER, EARTH_RADIUS + ATMO_HEIGHT);

                rayDist = min(rayDist, intersection.y);

                if (intersection.x > 0)
                {
                    rayPos += rayDir * intersection.x;
                    rayDist -= intersection.x;
                }
                
                float RdotL = dot(rayDir, light.direction);
                float3 vDepth = 0;
                float3 rayleigh = 0;
                float3 mie = 0;
                float rayDelta = 0;

                for (int i = 0; i < _Samples; i++)
                {
                    float  rayLen = pow(abs((float)i / _Samples), sampleDistributionExponent) * rayDist;
                    float  stepSize = (rayLen - rayDelta);

                    float3 pos = rayPos + rayDir * rayLen;
                    float  h   = distance(pos, EARTH_CENTER) - EARTH_RADIUS; 
                    float3 d  = Density(h);
                    vDepth += d * stepSize;
                    float3 vTransmit = Scatter(vDepth);
                    float3 vDepthLight  = ViewDepth(pos, light.direction);
                    float3 lTransmit = Scatter(vDepthLight);
                    float pRayleigh = 3 * (1 + RdotL*RdotL) / (16 * 3.1415);
                    float k = 1.55 * 0.85 - 0.55 * 0.85 * 0.85 *0.85;
                    float kRdotL = k*RdotL;
                    float pMie = (1 - k*k) / ((4 * 3.1415) * (1-kRdotL) * (1-kRdotL));

                    rayleigh += vTransmit * lTransmit * pRayleigh * d.x * stepSize;
                    mie      += vTransmit * lTransmit * pMie * d.y * stepSize;
                    rayDelta = rayLen;
                }

                float3 multiplier = (_Brightness * _SkyMultiplier);
                multiplier *= light.color;
                return (rayleigh * RAYLEIGH + mie * MIE) * multiplier; 
            }

            float2 SphericalMapping(float3 viewDir)
            {
                float lon = atan2(viewDir.z, viewDir.x);
                float lat = acos(viewDir.y);
                float invPi = 1.0 / 3.14159265;
                float u = lon * invPi * 0.5 + 0.5;
                float v = lat * invPi;
                return float2(u, -v);
            }

            float pseudoRandom(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
            }

            float2 ComputeCylindricalUV(float3 worldPos, float2 tiling, float2 offset)
            {
                float3 centeredPosition = worldPos - float3(_CloudCylinderCenter.x, _WorldSpaceCameraPos.y, _CloudCylinderCenter.z);
                float angle = atan2(centeredPosition.z, centeredPosition.x) / (2.0 * 3.14159265) + 0.5;

                float currentDistance = length(centeredPosition - _WorldSpaceCameraPos);
                float _InitialCameraDistance = 100;
                float distanceRatio = _InitialCameraDistance / currentDistance;

                float height = ((centeredPosition.y + _WorldSpaceCameraPos.y - _CloudCylinderBottom) / (_CloudCylinderTop - _CloudCylinderBottom)) * distanceRatio;

                float2 uv = float2(angle, height);

                uv = uv * tiling + offset;
                return uv;
            }


            v2f vert (appdata v)        
            {
                v2f o;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                o.vertex = vertexInput.positionCS;
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half ComputeLightMap(half4 cloudLightmapA, half4 cloudLightmapB)
            {
                Light light = GetMainLight();

                half frontMap = cloudLightmapA.b;
                half backMap = cloudLightmapB.b * 0.5;

                half rightMap = cloudLightmapA.r;
                half leftMap = cloudLightmapB.r;

                half topMap = cloudLightmapA.g;
                half bottomMap = cloudLightmapB.g;

                half hMap = (light.direction.x > 0.0h) ? rightMap : leftMap;
                half vMap = (light.direction.y > 0.0h) ? topMap : bottomMap;
                half dMap = (light.direction.z > 0.0h) ? frontMap : backMap;
                half lightMap = hMap * light.direction.x * light.direction.x + vMap * light.direction.y * light.direction.y + dMap * light.direction.z * light.direction.z;
                return lightMap;
            }

            void ConstructMatrixFromVectors(float3 right, float3 up, float3 forward, out float4x4 resultMatrix) 
            {
                resultMatrix[0] = float4(right, 0.0);
                resultMatrix[1] = float4(up, 0.0);
                resultMatrix[2] = float4(forward, 0.0);
                resultMatrix[3] = float4(0, 0, 0, 1);
            }

            void ConstructMatrix3x3FromVectors(float3 right, float3 up, float3 forward, out float3x3 resultMatrix) 
            {
                resultMatrix[0] = right;
                resultMatrix[1] = up;
                resultMatrix[2] = forward;
            }

            float3 MultiplyMatrix3x3WithVector(float3 row1, float3 row2, float3 row3, float3 vec) 
            {
                return float3(
                    dot(row1, vec),
                    dot(row2, vec),
                    dot(row3, vec)
                );
            }

            float3 RotateAroundAxis(float3 v, float3 axis, float angle)
            {
                float cosAngle = cos(angle);
                float sinAngle = sin(angle);

                float3x3 rotationMatrix = float3x3(
                    cosAngle + axis.x * axis.x * (1.0 - cosAngle),
                    axis.x * axis.y * (1.0 - cosAngle) - axis.z * sinAngle,
                    axis.x * axis.z * (1.0 - cosAngle) + axis.y * sinAngle,

                    axis.y * axis.x * (1.0 - cosAngle) + axis.z * sinAngle,
                    cosAngle + axis.y * axis.y * (1.0 - cosAngle),
                    axis.y * axis.z * (1.0 - cosAngle) - axis.x * sinAngle,

                    axis.z * axis.x * (1.0 - cosAngle) - axis.y * sinAngle,
                    axis.z * axis.y * (1.0 - cosAngle) + axis.x * sinAngle,
                    cosAngle + axis.z * axis.z * (1.0 - cosAngle)
                );

                return mul(rotationMatrix, v);
            }

            float4 frag (v2f i) : SV_Target
            { 
                float3 scattering = 0;
                float3 rayPos = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.worldPos);
                float scaleFactor = 100000 / max(_ProjectionParams.z, 0.00001);
                float rayDist = distance(rayPos, i.worldPos) * scaleFactor;
                
                //Staring Sky
                float2 panoramaUV = SphericalMapping(rayDir);
                float3 panoramaColor = tex2D(_PanoramaTex, panoramaUV).rgb;

                scattering = RayMarch(rayPos, rayDir, rayDist);

                // Compute cylindrical UV coordinates
                float2 cylindricalUV = ComputeCylindricalUV(i.worldPos, _CloudLightmapA_ST.xy, _CloudLightmapA_ST.zw);
                cylindricalUV.x += _CloudScrollSpeed * _Time.y;

                float2 cloudUV = cylindricalUV;

                cloudUV = TRANSFORM_TEX(cloudUV, _CloudLightmapA);
                half4 cloudLightmapA = tex2D(_CloudLightmapA, cloudUV);
                half4 cloudLightmapB = tex2D(_CloudLightmapB, cloudUV);

                half3 cloudColor = ComputeLightMap(cloudLightmapA, cloudLightmapB) * _CloudIntensity;
                half cloudAlpha = cloudLightmapA.a * _CloudIntensity;

                float3 pixelWorldPosition = rayPos + rayDir * rayDist;  // Using the ray marching approach

                // Subtract this distance from the radius of the earth to get the altitude from the surface
                float altitude = length(pixelWorldPosition - EARTH_CENTER) - EARTH_RADIUS;

                // Calculate the camera's distance from the earth
                float cameraToEarthDistance = length(_WorldSpaceCameraPos - EARTH_CENTER);

                float spaceFadeFactor = smoothstep(EARTH_RADIUS + _SpaceFadeDistance, EARTH_RADIUS, cameraToEarthDistance);
                float3 starlySky = lerp(float3(0, 0, 0), panoramaColor, _IsNight); 
                scattering += saturate(lerp(panoramaColor, starlySky, spaceFadeFactor));

                cloudAlpha *= spaceFadeFactor;

                // Fade out clouds when below ground level
                float fadeAlpha = smoothstep(-_GloundHeight, 0, altitude);
                cloudAlpha *= fadeAlpha;
                cloudColor *= fadeAlpha;
 
                float3 dayTimeSky = (scattering + cloudColor); // addition and composition
                float3 nightTimeSky = lerp(scattering, float3(0,0,0), saturate(pow(cloudColor.r, 0.5))); // alpha synthesis

                // Interpolate between additive composition and alpha composition based on _IsNight value
                scattering =lerp(dayTimeSky, nightTimeSky, _IsNight);

                float3 monochrome = dot(scattering, float3(0.299, 0.587, 0.114));
                scattering = lerp(scattering, monochrome, _Cloud);

                //Moon
                float3 moonLightDirection = normalize(_MoonLightDirection);
                float3 moonLightRight = normalize(_MoonLightRight);
                float3 moonLightUp = normalize(_MoonLightUp);

                float3x3 constructedMatrix3x3;
                ConstructMatrix3x3FromVectors(moonLightRight, moonLightUp, moonLightDirection, constructedMatrix3x3);

                float3 worldSpaceViewDirection = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 viewDirectionInMoonSpace = MultiplyMatrix3x3WithVector(constructedMatrix3x3[0], constructedMatrix3x3[1], constructedMatrix3x3[2], worldSpaceViewDirection);

                float2 scalingVector = float2(0.5, -0.5);
                float moonSize = _MoonScale;
                float clampedMoonSize = clamp(moonSize, 0.001, 1);
                float inverseClampedMoonSize = 1 / clampedMoonSize;

                float2 moonScaledVector = scalingVector * float2(inverseClampedMoonSize, inverseClampedMoonSize);
                float2 moonOffsetVector = float2(0.5, 0.5);
                float2 moonScaledUV = viewDirectionInMoonSpace.xy * moonScaledVector;
                float2 moonTransformedUV = moonScaledUV + moonOffsetVector;

                float4 moonSample = tex2D(_MoonTexture, moonTransformedUV);
                float moonViewDotDirection = dot(moonLightDirection, worldSpaceViewDirection);
                float moonViewAngle = acos(moonViewDotDirection);
                float moonVisibility = step(moonViewAngle, moonSize);

                float moonSizeScaled = moonSize * 2;
                float negMoonSizeScaled = -moonSizeScaled;
                float moonCurrentPhase = _MoonPhase;
                float moonRotationAngle = lerp(negMoonSizeScaled, moonSizeScaled, moonCurrentPhase);

                float3 moonRotatedDirection = RotateAroundAxis(moonLightDirection, moonLightUp, moonRotationAngle);
                float moonRotatedViewDotDirection = dot(moonRotatedDirection, worldSpaceViewDirection);
                float moonRotatedViewAngle = acos(moonRotatedViewDotDirection);

                float moonRotatedVisibility = step(moonRotatedViewAngle, moonSize);
                float moonVisibilityDifference = moonVisibility - moonRotatedVisibility;
                float moonFinalVisibility = saturate(moonVisibilityDifference) * _IsNight;

                // Compute the luminance of the cloud color
                float cloudLuminance = dot(cloudColor, float3(0.299, 0.587, 0.114)) * 4.0; 
                moonSample.rgb *= (1.0 - saturate(cloudLuminance));

                scattering += moonFinalVisibility * moonSample * _MoonEmissionColor;

                float belowStart = step(altitude, _HightFogStart);
                float altitudeFogFactor = (altitude - _HightFogEnd) / (_HightFogStart - _HightFogEnd);
                altitudeFogFactor = saturate(altitudeFogFactor) * (1.0 - belowStart) + belowStart;

                // Adjust height fog effects using both altitudeFogFactor and spaceFadeFactor
                float combinedFogFactor = altitudeFogFactor * spaceFadeFactor * _FogColor.a;
                combinedFogFactor *= (1.0 - _IsNight); // Reduce fog factor if it's night
                scattering = lerp(scattering, _FogColor, combinedFogFactor);

                return float4(scattering, 1);
            }
            ENDHLSL
        }
    }
}