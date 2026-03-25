#ifndef BICUBIC_TEXTURE_SAMPLING_INCLUDED
#define BICUBIC_TEXTURE_SAMPLING_INCLUDED

// Helper to calculate the 4 weights and coordinates for bicubic fetch
void GetBicubicCoords(float2 uv, float2 texelSize, out float2 uv00, out float2 uv10, out float2 uv01, out float2 uv11, out float2 g0, out float2 g1)
{
    float2 textureSize = 1.0 / texelSize;
    float2 pixelCoordinates = uv * textureSize - 0.5;

    float2 fxy = frac(pixelCoordinates);
    pixelCoordinates -= fxy;

    // Cubic weights
    float2 one_frac = 1.0 - fxy;
    float2 w0 = 1.0/6.0 * one_frac * one_frac * one_frac;
    float2 w1 = 2.0/3.0 - 0.5 * fxy * fxy * (2.0 - fxy);
    float2 w2 = 2.0/3.0 - 0.5 * one_frac * one_frac * (2.0 - one_frac);
    float2 w3 = 1.0/6.0 * fxy * fxy * fxy;

    g0 = w0 + w1;
    g1 = w2 + w3;
    
    // Calculate sample coordinates
    float2 h0 = (w1 / g0) - 0.5 + pixelCoordinates;
    float2 h1 = (w3 / g1) + 1.5 + pixelCoordinates;

    uv00 = float2(h0.x, h0.y) * texelSize;
    uv10 = float2(h1.x, h0.y) * texelSize;
    uv01 = float2(h0.x, h1.y) * texelSize;
    uv11 = float2(h1.x, h1.y) * texelSize;
}

// ------------------------------------------------------------------
// 1. DEPTH SAMPLER (Float)
// ------------------------------------------------------------------
void SampleDepthBicubic_float(float2 UV, float2 TexelSize, out float Depth)
{
    float2 uv00, uv10, uv01, uv11, g0, g1;
    GetBicubicCoords(UV, TexelSize, uv00, uv10, uv01, uv11, g0, g1);

    // Fetch 4 Linear Depth Samples
    // Note: We use the macro to safely sample the Camera Depth Texture
    float d00 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(uv00), _ZBufferParams);
    float d10 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(uv10), _ZBufferParams);
    float d01 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(uv01), _ZBufferParams);
    float d11 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(uv11), _ZBufferParams);

    // Interpolate
    float depthY0 = (g0.x * d00) + (g1.x * d10);
    float depthY1 = (g0.x * d01) + (g1.x * d11);
    
    Depth = (g0.y * depthY0) + (g1.y * depthY1) / ((g0.x + g1.x) * (g0.y + g1.y));
}

// ------------------------------------------------------------------
// 2. NORMAL SAMPLER (Vector3)
// ------------------------------------------------------------------
// Note: Requires _CameraNormalsTexture to be available in URP settings
//TEXTURE2D(_CameraNormalsTexture);
//SAMPLER(sampler_CameraNormalsTexture);

void SampleNormalBicubic_float(float2 UV, float2 TexelSize, out float3 Normal)
{
    float2 uv00, uv10, uv01, uv11, g0, g1;
    GetBicubicCoords(UV, TexelSize, uv00, uv10, uv01, uv11, g0, g1);

    // Fetch 4 Normal Samples
    float3 n00 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv00).xyz;
    float3 n10 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv10).xyz;
    float3 n01 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv01).xyz;
    float3 n11 = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv11).xyz;

    // Interpolate (Vector math)
    float3 normY0 = (g0.x * n00) + (g1.x * n10);
    float3 normY1 = (g0.x * n01) + (g1.x * n11);
    
    Normal = (g0.y * normY0) + (g1.y * normY1) / ((g0.x + g1.x) * (g0.y + g1.y));
}

#endif