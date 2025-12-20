#ifndef TOON_LIGHTING_INCLUDED
#define TOON_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

void Toon shader_float(
    float3 PositionWS,
    float3 NormalWS,
    out float3 LightColor
)
{
    LightColor = float3(0,0,0);

    // Main Directional Light
    {
        Light mainLight = GetMainLight();
        float3 L = normalize(mainLight.direction);
        float NdotL = saturate(dot(NormalWS, L));

        LightColor += mainLight.color * NdotL;
    }

    // Additional Lights
    int lightCount = GetAdditionalLightsCount();

    for (int i = 0; i < lightCount; i++)
    {
        Light light = GetAdditionalLight(i, PositionWS);

        float3 L = normalize(light.position - PositionWS);
        float NdotL = saturate(dot(NormalWS, L));

        LightColor += light.color * light.distanceAttenuation * NdotL;
    }
}
#endif
