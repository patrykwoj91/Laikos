//------- Constants --------
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;
float3 xLightDirection;
float xAmbient;
bool xEnableLighting;

//------- Texture Samplers --------

Texture xTexture;
sampler TextureSampler = sampler_state
{
    texture = <xTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};

Texture xTexture0;
sampler TextureSampler0 = sampler_state
{
    texture = <xTexture0> ;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

Texture xTexture1;
sampler TextureSampler1 = sampler_state
{
    texture = <xTexture1>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

Texture xTexture2;
sampler TextureSampler2 = sampler_state
{
    texture = <xTexture2>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};

Texture xTexture3;
sampler TextureSampler3 = sampler_state
{
    texture = <xTexture3>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};

//------- Technique: Multitextured ---

struct MTVertexToPixel
{
    float4 Position         : POSITION;    
    float4 Color            : COLOR0;
    float3 Normal            : TEXCOORD0;
    float2 TextureCoords    : TEXCOORD1;
    float4 LightDirection    : TEXCOORD2;
    float4 TextureWeights    : TEXCOORD3;
	float Depth			     : TEXCOORD4;
};

MTVertexToPixel MultiTexturedVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0, float4 inTexWeights: TEXCOORD1)
{    
    MTVertexToPixel Output = (MTVertexToPixel)0;
    float4x4 preViewProjection = mul (xView, xProjection);
    float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
    Output.Position = mul(inPos, preWorldViewProjection);
    Output.Normal = mul(normalize(inNormal), xWorld);
    Output.TextureCoords = inTexCoords;
    Output.LightDirection.xyz = -xLightDirection;
    Output.LightDirection.w = 1;    
    Output.TextureWeights = inTexWeights;
    Output.Depth = Output.Position.z/Output.Position.w;
    return Output;    
}

struct MTPixelToFrame
{
    float4 Color : COLOR0;
};

MTPixelToFrame MultiTexturedPS(MTVertexToPixel PSIn)
{
    MTPixelToFrame Output = (MTPixelToFrame)0;        
    
    float lightingFactor = 1;
    if (xEnableLighting)
        lightingFactor = saturate(saturate(dot(PSIn.Normal, PSIn.LightDirection)) + xAmbient);

     float blendDistance = 0.99f;
     float blendWidth = 0.005f;
     float blendFactor = clamp((PSIn.Depth-blendDistance)/blendWidth, 0, 1);
         
     float4 farColor;
     farColor = tex2D(TextureSampler0, PSIn.TextureCoords)*PSIn.TextureWeights.x;
     farColor += tex2D(TextureSampler1, PSIn.TextureCoords)*PSIn.TextureWeights.y;
     farColor += tex2D(TextureSampler2, PSIn.TextureCoords)*PSIn.TextureWeights.z;
     farColor += tex2D(TextureSampler3, PSIn.TextureCoords)*PSIn.TextureWeights.w;
     
     float4 nearColor;
     float2 nearTextureCoords = PSIn.TextureCoords*3;
     nearColor = tex2D(TextureSampler0, nearTextureCoords)*PSIn.TextureWeights.x;
     nearColor += tex2D(TextureSampler1, nearTextureCoords)*PSIn.TextureWeights.y;
     nearColor += tex2D(TextureSampler2, nearTextureCoords)*PSIn.TextureWeights.z;
     nearColor += tex2D(TextureSampler3, nearTextureCoords)*PSIn.TextureWeights.w;
 
     Output.Color = lerp(nearColor, farColor, blendFactor);
     Output.Color *= lightingFactor;


    return Output;
}

//------- Technique: Textured --------

struct TexVertexToPixel
{
    float4 Position       : POSITION;    
    float4 Color        : COLOR0;
    float LightingFactor: TEXCOORD0;
    float2 TextureCoords: TEXCOORD1;
};

struct TexPixelToFrame
{
    float4 Color : COLOR0;
};

TexVertexToPixel TexturedVS( float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0)
{    
    TexVertexToPixel Output = (TexVertexToPixel)0;
    float4x4 preViewProjection = mul (xView, xProjection);
    float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
    
    Output.Position = mul(inPos, preWorldViewProjection);    
    Output.TextureCoords = inTexCoords;
    
    float3 Normal = normalize(mul(normalize(inNormal), xWorld));    
    Output.LightingFactor = 1;
    if (xEnableLighting)
        Output.LightingFactor = saturate(dot(Normal, -xLightDirection));
    
    return Output;    
}

TexPixelToFrame TexturedPS(TexVertexToPixel PSIn)
{
    TexPixelToFrame Output = (TexPixelToFrame)0;        
    
    Output.Color = tex2D(TextureSampler, PSIn.TextureCoords);
    Output.Color.rgb *= saturate(PSIn.LightingFactor + xAmbient);

    return Output;
}

// --------- List of Techniques ---------

technique Textured
{
    pass Pass0
    {  
        VertexShader = compile vs_2_0 TexturedVS();
        PixelShader  = compile ps_2_0 TexturedPS();
    }
}

technique MultiTextured
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 MultiTexturedVS();
        PixelShader = compile ps_2_0 MultiTexturedPS();
    }
}