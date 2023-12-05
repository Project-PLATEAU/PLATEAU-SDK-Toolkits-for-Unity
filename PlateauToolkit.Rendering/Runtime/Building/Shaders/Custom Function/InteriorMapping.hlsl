#ifndef INTERIOR_MAPPING_INCLUDED
#define INTERIOR_MAPPING_INCLUDED

float4 Unity_SampleGradient_float(Gradient Gradient, float Time)
{
    float3 color = Gradient.colors[0].rgb;
    [unroll]
    for (int c = 1; c < 8; c++)
    {
        float colorPos = saturate((Time - Gradient.colors[c-1].w) / (Gradient.colors[c].w - Gradient.colors[c-1].w + 1e-6)) * step(c, Gradient.colorsLength-1);
        color = lerp(color, Gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), Gradient.type));
    }
# ifndef UNITY_COLORSPACE_GAMMA
    color = SRGBToLinear(color);
# endif
    float alpha = Gradient.alphas[0].x;
    [unroll]
    for (int a = 1; a < 8; a++)
    {
        float alphaPos = saturate((Time - Gradient.alphas[a-1].y) / ((Gradient.alphas[a].y - Gradient.alphas[a-1].y + 1e-6)) * step(a, Gradient.alphasLength-1) + 1e-8);
        alpha = lerp(alpha, Gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), Gradient.type));
    }
    return float4(color, alpha);
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
    )
{
    float3 rayDir = -viewDir;
    rayDir .z *= -1;
    float3 uvw = float3(frac(uv), -roomDepth + 1.0);

    float3 dist3 = (step(0.0, rayDir) - uvw) / (rayDir);
    float dist = min(min(dist3.x, dist3.y), dist3.z); 

    float3 rayHit = uvw + rayDir * dist;
    rayHit.z = (rayHit.z + roomDepth  - 1.0) / roomDepth ;
    float3 rayHitNormal = step(1.0 - 1e-3, abs(rayHit - 0.5) * 2.0);

    float3 finalRGB = float3(dist, dist, dist) * 0.5;

    float  tileX = 1.0/floor(roomAtlasColumns);
    float  tileY = 1.0/floor(roomAtlasRows);

    float RoomNumX = roomAtlasColumns/6.0;
    float RoomNumY = roomAtlasRows/3.0;

    float Columun = roomAtlasColumns/3.0;
    float Row = roomAtlasRows/3.0;

    float2 _Rooms = float2(RoomNumX, RoomNumY);
    float2 roomIndexUV = floor(uv);
    float2 co = roomIndexUV.x + roomIndexUV.y * (roomIndexUV.x + 1);
    float2 randA = frac(sin(co * float2(12.9898,78.233)) * 43758.5453);

    float2 randB = (randA -0.5) * 2;

    float2 randC = floor(randA  + 1.0 * (1-nightEmission));
    float nightBlendingRand = lerp(1, randC.x , nightEmission);

    // randomize the room
    float2 n = floor(randA * _Rooms.xy);
    roomIndexUV += n;

    float2 windowUV = {0,0};

    float distPlane = 0.01 / rayDir.z;
    float3 rayHitPlane = uvw + rayDir * distPlane;
    windowUV += rayHitPlane.xy + float2(2,2);
    windowUV *= float2(tileX, tileY);
    windowUV.x += n.x / RoomNumX;
    windowUV.y += n.y / RoomNumY;
    float4 windowColor = roomAtlasTex.Sample(Sampler, windowUV);
    float4 windowColor2 = roomAtlasTex.Sample(Sampler, windowUV + float2(1/Columun,0));
    windowColor = lerp(windowColor, windowColor2, nightBlendingRand);
    windowColor.a *= step(distPlane, dist);

    distPlane = 0.01 / rayDir.z;
    rayHitPlane = uvw + rayDir * distPlane;

    float2 roomsUV = {0,0};
    roomsUV += rayHitNormal.x * (rayHit.zy + float2(2 * step(0.5, rayHit.x), 1));
    roomsUV += rayHitNormal.y * (rayHit.xz + float2(1, 2 * step(0.5, rayHit.y)));
    roomsUV += rayHitNormal.z * (rayHit.xy + float2(1, 1));
    roomsUV *= float2(tileX, tileY);
    roomsUV.x += n.x / RoomNumX;
    roomsUV.y += n.y / RoomNumY;

    float4 roomColor = roomAtlasTex.Sample(Sampler, roomsUV);
    float4 roomColor2 = roomAtlasTex.Sample(Sampler, roomsUV + float2(1/Columun,0));
    roomColor = lerp(roomColor, roomColor2, nightBlendingRand);

    float randD= frac(sin(dot(roomIndexUV.xy, float2(12.9898,78.233))) * 43758.5453);
    float4 randRGB = Unity_SampleGradient_float(gradient, randD);  
 
    roomColor.rgb = saturate(roomColor.rgb) ;
    roomColor.a = 1.0;

    float4 finalColor = lerp(roomColor, windowColor, windowColor.a);

    Out = roomColor;
    Night = nightBlendingRand;
    Emissive = randRGB.rgb;
}

#endif



