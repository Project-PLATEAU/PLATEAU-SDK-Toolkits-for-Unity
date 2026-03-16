#define EPS_SMALL   1e-4
#define EPS_RAY     1e-3

inline float  safe_div(float n, float d)  { return n / max(abs(d), EPS_SMALL); }
inline float3 safe_ray(float3 v)          { return sign(v) * max(abs(v), EPS_RAY); }

float4 Unity_SampleGradient_float(Gradient G, float t)
{
    float3 color = G.colors[0].rgb;

    [unroll]
    for (int c = 1; c < 8; c++)
    {
        float denom   = max(G.colors[c].w - G.colors[c-1].w, EPS_SMALL);
        float colorPos = saturate( safe_div(t - G.colors[c-1].w, denom) ) * step(c, G.colorsLength - 1);
        color = lerp(color, G.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), G.type));
    }
#ifndef UNITY_COLORSPACE_GAMMA
    color = SRGBToLinear(color);
#endif

    float alpha = G.alphas[0].x;
    [unroll]
    for (int a = 1; a < 8; a++)
    {
        float denom   = max(G.alphas[a].y - G.alphas[a-1].y, EPS_SMALL);
        float alphaPos = saturate( safe_div(t - G.alphas[a-1].y, denom) ) * step(a, G.alphasLength - 1);
        alpha = lerp(alpha, G.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), G.type));
    }
    return float4(color, alpha);
}

inline float hash12(float2 p) {
    p = frac(p * float2(0.1031, 0.1030));
    p += dot(p, p.yx + 33.33);
    return frac((p.x + p.y) * p.x);
}

void InteriorMapping_float(
    float2 uv, 
    float3 viewDir,
    UnityTexture2D roomAtlasTex, 
    UnitySamplerState Sampler,
    float roomDepth, 
    float roomAtlasRows, 
    float roomAtlasColumns, 
    float nightEmission, 
    Gradient gradient,
    out float4 Out, 
    out float Night, 
    out float3 Emissive
){
    float3 rayDir = -viewDir;
    rayDir.z *= -1;
    rayDir = safe_ray(rayDir);

    float3 uvw = float3(frac(uv), -roomDepth + 1.0);

    float3 dist3 = (step(0.0, rayDir) - uvw) / rayDir;
    float  dist  = min(min(dist3.x, dist3.y), dist3.z);

    float3 rayHit = uvw + rayDir * dist;
    rayHit.z = safe_div(rayHit.z + roomDepth - 1.0, roomDepth);

    float3 rayHitNormal = step(1.0 - 1e-3, abs(rayHit - 0.5) * 2.0);

    float cols = max(floor(roomAtlasColumns + 0.5), 1.0);
    float rows = max(floor(roomAtlasRows    + 0.5), 1.0);
    float  tileX = safe_div(1.0, cols);
    float  tileY = safe_div(1.0, rows);

    float RoomNumX = safe_div(cols, 6.0);
    float RoomNumY = safe_div(rows, 3.0);
    float Columun  = safe_div(cols, 3.0);
    float Row      = safe_div(rows, 3.0);

    float2 roomIndexUV = floor(uv);
    float  randD       = hash12(roomIndexUV);
    float4 randRGB     = Unity_SampleGradient_float(gradient, randD);

    float2 rA = float2(hash12(roomIndexUV + 11.0), hash12(roomIndexUV + 29.0));
    float2 rC = floor(rA + (1.0 - nightEmission));
    float  nightBlend = lerp(1.0, rC.x, saturate(nightEmission));
    Night = nightBlend;

    float2 n = floor(rA * float2(RoomNumX, RoomNumY));
    float2 windowUV = 0;

    float  distPlane   = safe_div(0.01, rayDir.z);
    float3 rayHitPlane = uvw + rayDir * distPlane;

    windowUV = (rayHitPlane.xy + float2(2,2)) * float2(tileX, tileY);
    windowUV += float2( safe_div(n.x, RoomNumX), safe_div(n.y, RoomNumY) );

    float4 windowColor  = roomAtlasTex.Sample(Sampler, windowUV);
    float4 windowColor2 = roomAtlasTex.Sample(Sampler, windowUV + float2(safe_div(1.0, Columun), 0));
    windowColor = lerp(windowColor, windowColor2, nightBlend);
    windowColor.a *= step(distPlane, dist);

    float2 roomsUV = 0;
    roomsUV += rayHitNormal.x * (rayHit.zy + float2(2 * step(0.5, rayHit.x), 1));
    roomsUV += rayHitNormal.y * (rayHit.xz + float2(1, 2 * step(0.5, rayHit.y)));
    roomsUV += rayHitNormal.z * (rayHit.xy + float2(1, 1));
    roomsUV  = roomsUV * float2(tileX, tileY) + float2( safe_div(n.x, RoomNumX), safe_div(n.y, RoomNumY) );

    float4 roomColor  = roomAtlasTex.Sample(Sampler, roomsUV);
    float4 roomColor2 = roomAtlasTex.Sample(Sampler, roomsUV + float2(safe_div(1.0, Columun), 0));
    roomColor = lerp(roomColor, roomColor2, nightBlend);
    roomColor.rgb = saturate(roomColor.rgb);
    roomColor.a   = 1.0;

    float4 finalColor = lerp(roomColor, windowColor, windowColor.a);
    Out      = finalColor;
    Emissive = randRGB.rgb;
}