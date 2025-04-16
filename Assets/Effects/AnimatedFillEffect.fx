sampler2D uImage0 : register(s0);
float uTime;
float3 colorA;
float3 colorB;
float amplitude;

float4 AnimatedFillEffect(float2 coords : TEXCOORD0) : COLOR0
{
    float4 texColor = tex2D(uImage0, coords);
    if (texColor.a < 0.01)
        discard;
    
    float wave = sin((coords.x + uTime) * 10.0) * amplitude;
    float val = coords.y + wave;
    float3 finalColor = lerp(colorA, colorB, val);
    
    return float4(finalColor, texColor.a);
}

technique Technique1
{
    pass AnimatedFillPass
    {
        PixelShader = compile ps_2_0 AnimatedFillEffect();
    }
}