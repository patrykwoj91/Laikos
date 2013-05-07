float4x4 World;
float4x4 View;
float4x4 Projection;

//color of the light 
float3 Color; 

//position of the camera, for specular light
float3 cameraPosition; 

//this is used to compute the world-position
float4x4 InvertViewProjection; 

//this is the position of the light
float3 lightPosition;

//how far does this light reach
float lightRadius;

//control the brightness of the light
float lightIntensity = 1.0f;

bool shadows;
float shadowMapSize;
float depthPrecision;
float depthBias;

// diffuse color, and specularIntensity in the alpha channel
texture colorMap; 
// normals, and specularPower in the alpha channel
texture normalMap;
//depth
texture depthMap;
//shadows
texture shadowMap;

sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};
sampler depthSampler = sampler_state
{
    Texture = (depthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};
sampler normalSampler = sampler_state
{
    Texture = (normalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

sampler shadowSampler = sampler_state
{
	Texture = (shadowMap);
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = POINT;
	MinFilter = POINT;
	MipFilter = POINT;
};

struct VertexShaderInput
{
    float3 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 ScreenPosition : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    //processing geometry coordinates
    float4 worldPosition = mul(float4(input.Position,1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.ScreenPosition = output.Position;
    return output;
}

//Manually Linear Sample a Cube Map
float4 manualSampleCUBE(sampler Sampler, float3 UVW, float3 textureSize)
{
	//Calculate the reciprocal
	float3 textureSizeDiv = 1 / textureSize;

	//Multiply coordinates by the texture size
	float3 texPos = UVW * textureSize;

	//Compute first integer coordinates
	float3 texPos0 = floor(texPos + 0.5f);

	//Compute second integer coordinates
	float3 texPos1 = texPos0 + 1.0f;

	//Perform division on integer coordinates
	texPos0 = texPos0 * textureSizeDiv;
	texPos1 = texPos1 * textureSizeDiv;

	//Compute contributions for each coordinate
	float3 blend = frac(texPos + 0.5f);

	//Construct 8 new coordinates
	float3 texPos000 = texPos0;
	float3 texPos001 = float3(texPos0.x, texPos0.y, texPos1.z);
	float3 texPos010 = float3(texPos0.x, texPos1.y, texPos0.z);
	float3 texPos011 = float3(texPos0.x, texPos1.y, texPos1.z);
	float3 texPos100 = float3(texPos1.x, texPos0.y, texPos0.z);
	float3 texPos101 = float3(texPos1.x, texPos0.y, texPos1.z);
	float3 texPos110 = float3(texPos1.x, texPos1.y, texPos0.z);
	float3 texPos111 = texPos1;

	//Sample Cube Map
	float3 C000 = texCUBE(Sampler, texPos000);
	float3 C001 = texCUBE(Sampler, texPos001);
	float3 C010 = texCUBE(Sampler, texPos010);
	float3 C011 = texCUBE(Sampler, texPos011);
	float3 C100 = texCUBE(Sampler, texPos100);
	float3 C101 = texCUBE(Sampler, texPos101);
	float3 C110 = texCUBE(Sampler, texPos110);
	float3 C111 = texCUBE(Sampler, texPos111);

	//Compute final value by lerping everything
	float3 C = lerp(lerp(lerp(C000, C010, blend.y), lerp(C100, C110, blend.y), blend.x), lerp( lerp(C001, C011, blend.y), lerp(C101, C111, blend.y), blend.x), blend.z);
	
	//Return
	return float4(C, 1);
}

float2 halfPixel;
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    //obtain screen position
    input.ScreenPosition.xy /= input.ScreenPosition.w;

    //obtain textureCoordinates corresponding to the current pixel
    //the screen coordinates are in [-1,1]*[1,-1]
    //the texture coordinates need to be in [0,1]*[0,1]
    float2 texCoord = 0.5f * (float2(input.ScreenPosition.x,-input.ScreenPosition.y) + 1);
    //allign texels to pixels
    texCoord -=halfPixel;

    //get normal data from the normalMap
    float4 normalData = tex2D(normalSampler,texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = 2.0f * normalData.xyz - 1.0f;
    //get specular power
    float specularPower = normalData.a * 255;
    //get specular intensity from the colorMap
    float specularIntensity = tex2D(colorSampler, texCoord).a;

    //read depth
    float depthVal = tex2D(depthSampler,texCoord).r;

    //compute screen-space position
    float4 position;
    position.xy = input.ScreenPosition.xy;
    position.z = depthVal;
    position.w = 1.0f;
    //transform to world space
    position = mul(position, InvertViewProjection);
    position /= position.w;

    //surface-to-light vector
    float3 lightVector = lightPosition - position;

    //compute attenuation based on distance - linear attenuation
    float attenuation = saturate(1.0f - length(lightVector)/lightRadius); 

    //normalize light vector
    lightVector = normalize(lightVector); 

    //compute diffuse light
    float NdL = max(0,dot(normal,lightVector));
    float3 diffuseLight = NdL * Color.rgb;

    //reflection vector
    float3 reflectionVector = normalize(reflect(-lightVector, normal));
    //camera-to-surface vector
    float3 directionToCamera = normalize(cameraPosition - position);
    //compute specular light
    float specularLight = specularIntensity * pow( saturate(dot(reflectionVector, directionToCamera)), specularPower);

	float lightZ = manualSampleCUBE(shadowSampler, float3(-lightVector.xy, lightVector.z), shadowMapSize).r;
	float shadowFactor = 1;
	if(shadows)
	{
		float len = max(0.01f, length(lightPosition - input.ScreenPosition)) / depthPrecision;
		//shadowFactor = (lightZ * exp(-(depthPrecision * 0.5f) * (len - depthBias)));
	}

    //take into account attenuation and lightIntensity.
    return shadowFactor * attenuation * lightIntensity * float4(diffuseLight.rgb,specularLight);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
