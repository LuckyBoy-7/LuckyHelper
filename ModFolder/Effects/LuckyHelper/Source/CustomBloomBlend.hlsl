// fxc /T fx_2_0 /E main /Fo "CustomBloomBlend.cso" "CustomBloomBlend.hlsl"
// 神秘的是一开始用 fx_2_0 报错, 搞得我以为用不了(
#include "Common.fxh"

DECLARE_TEXTURE(SrcTexture, 0);
DECLARE_TEXTURE(DestTexture, 1);

struct PS_INPUT
{
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

float4 PSMain(PS_INPUT input) : SV_TARGET0
{
    float4 srcColor = SAMPLE_TEXTURE(SrcTexture, input.TexCoord) * input.Color;
    float4 destColor = SAMPLE_TEXTURE(DestTexture, input.TexCoord);
    // 把预乘的 color 夺回来ε=( o｀ω′)ノ
    if (destColor.a != 0)
        destColor.rgb /= destColor.a;
    return float4((srcColor * destColor).rgb, destColor.a);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 PSMain();
    }
}
