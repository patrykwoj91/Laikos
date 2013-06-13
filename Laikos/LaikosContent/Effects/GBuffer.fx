//Vertex Shader Constants
#define MAX_BONES 60
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x3 Bones[MAX_BONES];
float specularIntensity = 0.8f;
float specularPower = 0.5f;
bool isSkinned = false;
bool xClip;
float4 xClipPlane;

texture Texture;
texture NormalMap;
texture SpecularMap;
texture xTexture;
texture xTexture0;
texture xTexture1;
texture xTexture2;
texture xTexture3;

sampler diffuseSampler = sampler_state
{
	texture = <Texture>;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};

//NormalMap Sampler
sampler normalSampler = sampler_state
{
	texture = <NormalMap>;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};

//SpecularMap Sampler
sampler specularSampler = sampler_state
{
	texture = <SpecularMap>;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};


sampler TextureSampler = sampler_state
{
    texture = <xTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};


sampler TextureSampler0 = sampler_state
{
    texture = <xTexture0> ;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};


sampler TextureSampler1 = sampler_state
{
    texture = <xTexture1>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};


sampler TextureSampler2 = sampler_state
{
    texture = <xTexture2>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};


sampler TextureSampler3 = sampler_state
{
    texture = <xTexture3>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};

//Vertex Input Structure
struct VSI
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 UV : TEXCOORD0;
	float3 Tangent : TANGENT0;
	float3 BiTangent : BINORMAL0;
	int4 BoneIndices : BLENDINDICES0;
	float4 BoneWeights : BLENDWEIGHT0;
};

//Vertex Output Structure
struct VSO
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float2 Depth : TEXCOORD1;
	float3x3 TBN : TEXCOORD2;
};

void Skin(inout VSI vin, uniform int boneCount)
{
    float4x3 skinning = 0;

    [unroll]
    for (int i = 0; i < boneCount; i++)
    {
        skinning += Bones[vin.BoneIndices[i]] * vin.BoneWeights[i];
    }

    vin.Position.xyz = mul(vin.Position, skinning);
    vin.Normal = mul(vin.Normal, (float3x3)skinning);
}

//Vertex Shader
VSO VS(VSI input)
{
	//Initialize Output
	VSO output;

	Skin(input, 1);

	//Transform Position
	float4 worldPosition = mul(float4(input.Position.xyz, 1), World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	
	//Pass Depth
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;


	//Build TBN Matrix
	output.TBN[0] = mul(input.Tangent, World);
	output.TBN[1] = mul(input.BiTangent, World);
	output.TBN[2] = mul(input.Normal, World);

	//Pass UV
	output.UV = input.UV;

	//Return Output
	return output;
}

//Vertex Shader
VSO VSNoSkin(VSI input)
{
	//Initialize Output
	VSO output;

	//Skin(input, 1);

	//Transform Position
	float4 worldPosition = mul(float4(input.Position.xyz, 1), World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	
	//Pass Depth
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;


	//Build TBN Matrix
	output.TBN[0] = mul(input.Tangent, World);
	output.TBN[1] = mul(input.BiTangent, World);
	output.TBN[2] = mul(input.Normal, World);

	//Pass UV
	output.UV = input.UV;

	//Return Output
	return output;
}

//Pixel Output Structure
struct PSO
{
	half4 Color : COLOR0;
	half4 Normals : COLOR1;
	half4 Depth : COLOR2;
};

//Pixel Shader
PSO PS(VSO input)
{
	//Initialize Output
	PSO output = (PSO)0;

	//Pass Albedo from Texture
	output.Color = tex2D(diffuseSampler, input.UV);
	float4 specularAttributes = tex2D(specularSampler, input.UV);
	output.Color.a = specularAttributes.r;

	float3 normalFromMap = tex2D(normalSampler, input.UV);
	normalFromMap = 2.0f * normalFromMap - 1.0f;
	normalFromMap = mul(normalFromMap, input.TBN);
	normalFromMap = normalize(normalFromMap);
	output.Normals.rgb = 0.5f * (normalFromMap + 1.0f);
	output.Normals.a = specularAttributes.a;

	output.Depth = input.Depth.x / input.Depth.y;

	//Return Output
	return output;
}

//------- Technique: Multitextured ---

struct MTVertexToPixel
{
    float4 Position          : POSITION;    
    float4 Color             : COLOR0;
    float3 Normal            : TEXCOORD0;
    float2 TextureCoords     : TEXCOORD1;
    float4 TextureWeights    : TEXCOORD2;
	float2 Depth			 : TEXCOORD3;
	float4 ClipDistances	:TEXCOORD4;
};

struct MTVertexIN
{
	float4 Position		: POSITION;
	float3 Normal		: NORMAL;
	float2 TexCoords    : TEXCOORD0;
	float4 TexWeights   : TEXCOORD1;
};

MTVertexToPixel MultiTexturedVS(MTVertexIN input)
{    
    MTVertexToPixel output = (MTVertexToPixel)0;
    float4 worldPosition = mul(float4(input.Position.xyz,1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.Normal = mul(normalize(input.Normal), World);

    output.TextureCoords = input.TexCoords;  

    output.TextureWeights = input.TexWeights;

    output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;

	if(xClip)
	{
		output.ClipDistances=dot(input.Position, xClipPlane);
	}

    return output;    
}

struct MTPixelToFrame
{
    half4 Color  : COLOR0;
	half4 Normal : COLOR1;
	half4 Depth  : COLOR2;
};



PSO MultiTexturedPS(MTVertexToPixel PSIn)
{
     PSO Output = (PSO)0;

	 clip(PSIn.ClipDistances);

	 float4 specularAttributes = tex2D(specularSampler, PSIn.TextureCoords);
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
	 Output.Color.a = 0/*specularAttributes.r*/;

	 Output.Normals.rgb = PSIn.Normal / 3.0f+0.5f;
	 Output.Normals.a = 1 /*specularAttributes.a*/;

	 Output.Depth = PSIn.Depth.x / PSIn.Depth.y;

     return Output;
}

//------- Technique: SkyDome --------
 struct SDVertexToPixel
 {    
     float4 Position         : POSITION;
     float2 TextureCoords    : TEXCOORD0;
     float4 ObjectPosition    : TEXCOORD1;
	 float4 ClipDistances		:TEXCOORD2;
 };
 
 struct SDPixelToFrame
 {
    half4 Color  : COLOR0;
	half4 Normal : COLOR1;
	half4 Depth  : COLOR2;
 };
 
 SDVertexToPixel SkyDomeVS( float4 inPos : POSITION, float2 inTexCoords: TEXCOORD0)
 {    
     SDVertexToPixel Output = (SDVertexToPixel)0;

     float4x4 preViewProjection = mul (View, Projection);
     float4x4 preWorldViewProjection = mul (World, preViewProjection);
	
     Output.Position = mul(inPos, preWorldViewProjection);
     Output.TextureCoords = inTexCoords;
     Output.ObjectPosition = inPos;
     
	 if(xClip)
	{
		Output.ClipDistances=dot(inPos,xClipPlane);
	}

     return Output;    
 }
 
 SDPixelToFrame SkyDomePS(SDVertexToPixel PSIn)
 {
     SDPixelToFrame Output = (SDPixelToFrame)0;        

	 clip(PSIn.ClipDistances);

     float4 topColor = float4(0.3f, 0.3f, 0.8f, 1);    
     float4 bottomColor = 1;    
     
     float4 baseColor = lerp(bottomColor, topColor, saturate((PSIn.ObjectPosition.y)/0.4f));
     float4 cloudValue = tex2D(TextureSampler0, PSIn.TextureCoords).r;
     
     Output.Color = lerp(baseColor,1, cloudValue);        
	 Output.Normal = 1;
	 Output.Depth = 1;
     return Output;
 }
 
 //Techniques
 technique SkyDome
 {
     pass Pass0
     {
         VertexShader = compile vs_2_0 SkyDomeVS();
         PixelShader = compile ps_2_0 SkyDomePS();
     }
 }


technique Skinning
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}

technique NoSkinning
{
	pass p0
	{
		VertexShader = compile vs_3_0 VSNoSkin();
		PixelShader = compile ps_3_0 PS();
	}
}

technique MultiTextured
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 MultiTexturedVS();
        PixelShader = compile ps_3_0 MultiTexturedPS();
    }
}
