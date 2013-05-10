//World
float4x4 World;

//View
float4x4 View;

//Inverse View
float4x4 inverseView;

//Projection
float4x4 Projection;

//Inverse View Projection
float4x4 InverseViewProjection;

//Camera Position
float3 CameraPosition;

//Light ViewProjection
float4x4 LightViewProjection;

//Light Position
float3 LightPosition;

//Light Color
float4 LightColor;

//Light Intensity
float LightIntensity;

//SpotLight Direction Vector
float3 S;

//Light Angle Cos
float LightAngleCos;

//The Height of the Light(AKA It's FarPlane)
float LightHeight;

//GBuffer Texture Size
float2 GBufferTextureSize;

//Shadows?
bool Shadows;

//ShadowMap size
float shadowMapSize;

//This is for modulating the Light's Depth Precision (100)
float DepthPrecision;

//DepthBias for the Shadowing... (1.0f / 2000.0f)
float DepthBias;

texture AttentuationTexture;
texture shadowMap;
texture ColorMap;
texture NormalMap;
texture DepthMap;

//GBuffer Texture0
sampler GBuffer0 =  sampler_state
{
	Texture = (ColorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

//GBuffer Texture1
sampler GBuffer1 = sampler_state
{
	Texture = (NormalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

//GBuffer Texture2
sampler GBuffer2 = sampler_state
{
	Texture = (DepthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

//Attenuation Cookie Sampler
sampler Cookie = sampler_state
{
	Texture = (AttentuationTexture);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

//ShadowMap
sampler ShadowMap = sampler_state
{
	Texture = (shadowMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

//Vertex Input Structure
struct VSI
{
	float4 Position : POSITION0;
};

//Vertex Output Structure
struct VSO
{
	float4 Position : POSITION0;
	float4 ScreenPosition : TEXCOORD0;
};

//Vertex Shader
VSO VS(VSI input)
{
	//Initialize Output
	VSO output;

	//Transform Position
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	//Pass to ScreenPosition
	output.ScreenPosition = output.Position;

	//Return
	return output;
}

//Manually Linear Sample
float4 manualSample(sampler Sampler, float2 UV, float2 textureSize)
{
	float2 texelpos = textureSize * UV; 
	float2 lerps = frac(texelpos); 
	float texelSize = 1.0 / textureSize;                 
 
	float4 sourcevals[4]; 
	sourcevals[0] = tex2D(Sampler, UV); 
	sourcevals[1] = tex2D(Sampler, UV + float2(texelSize, 0)); 
	sourcevals[2] = tex2D(Sampler, UV + float2(0, texelSize)); 
	sourcevals[3] = tex2D(Sampler, UV + float2(texelSize, texelSize));   
         
	float4 interpolated = lerp(lerp(sourcevals[0], sourcevals[1], lerps.x), lerp(sourcevals[2], sourcevals[3], lerps.x ), lerps.y); 

	return interpolated;
}

//Phong Shader
float4 Phong(float3 Position, float3 N, float radialAttenuation, float SpecularIntensity, float SpecularPower)
{
	//Calculate Light vector
    float3 L = LightPosition.xyz - Position.xyz;

    //Calculate height Attenuation
	float heightAttenuation = 1.0f - saturate(length(L) - (LightHeight / 2));

	//Calculate total Attenuation
	float Attenuation = min(radialAttenuation, heightAttenuation);
	
	//Now Normalize the Light
	L = normalize(L);

	//Calculate L.S
	float SL = dot(L, S);

	//No asymmetrical returns in HLSL, so work around with this
	float4 Shading = 0;

	//If this pixel is in the SpotLights Cone
	if(SL <= LightAngleCos)
	{
		//Calculate Reflection Vector
		float3 R = normalize(reflect(-L, N));

		//Calculate Eye Vector
		float3 E = normalize(CameraPosition - Position.xyz);
		
		//Calculate N.L
		float NL = dot(N, L);

		//Calculate Diffuse
		float3 Diffuse = NL * LightColor.xyz;

		//Calculate Specular
		float Specular = SpecularIntensity * pow(saturate(dot(R, E)), SpecularPower);

		//Calculate Final Product
		Shading = Attenuation * LightIntensity * float4(Diffuse.rgb, Specular);
	}

	//Return Shading Value
	return Shading;
}

//Decoding of GBuffer Normals
float3 decode(float3 enc)
{
	return (2.0f * enc.xyz- 1.0f);
}

//Decode Color Vector to Float Value for shadowMap
float RGBADecode(float4 value) 
{
    const float4 bits = float4(1.0 / (256.0 * 256.0 * 256.0),
                               1.0 / (256.0 * 256.0),
                               1.0 / 256.0,
                               1);

    return dot(value.xyzw , bits);
}

//Pixel Shader
float4 PS(VSO input) : COLOR0
{
	//Get Screen Position
	input.ScreenPosition.xy /= input.ScreenPosition.w;

	//Calculate UV from ScreenPosition
	float2 UV = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1) - float2(1.0f / GBufferTextureSize.xy);

	//Get All Data from Normal part of the GBuffer
	half4 encodedNormal = tex2D(GBuffer1, UV);

	//Decode Normal
	half3 Normal = decode(encodedNormal.xyz);
	
	//Get Specular Intensity from GBuffer
	float SpecularIntensity = tex2D(GBuffer0, UV).w;

	//Get Specular Power from GBuffer
	float SpecularPower = encodedNormal.w * 255;

	//Get Depth from GBuffer
	float Depth = manualSample(GBuffer2, UV, GBufferTextureSize).x;

	//Make Position in Homogenous Space using current ScreenSpace coordinates and the Depth from the GBuffer
	float4 Position = 1.0f;

    Position.xy = input.ScreenPosition.xy;

	Position.z = Depth;

	//Transform Position from Homogenous Space to World Space
	Position = mul(Position, InverseViewProjection);

	Position /= Position.w;
	
	//Calculate Homogenous Position with respect to light
	float4 LightScreenPos = mul(Position, LightViewProjection);

	LightScreenPos /= LightScreenPos.w;

	//Calculate Projected UV from Light POV
	float2 LUV = 0.5f * (float2(LightScreenPos.x, -LightScreenPos.y) + 1);

	//Load the Projected Depth from the Shadow Map, do manual linear filtering
	float lZ = manualSample(ShadowMap, LUV, shadowMapSize);

	//Get Attenuation factor from cookie
	float Attenuation = tex2D(Cookie, LUV).r;

	//Assymetric Workaround...
	float ShadowFactor = 1;

	//If Shadowing is on then get the Shadow Factor
	if(Shadows) 
	{
		// Calculate distance to the light
		float len = max(0.01f, length(LightPosition - Position)) / DepthPrecision;

		//Calculate the Shadow Factor
		ShadowFactor = (lZ * exp(-(DepthPrecision * 0.5f) * (len - DepthBias)));
	}

	//Return Phong Shaded Value Modulated by Shadows if Shadowing is on
	return ShadowFactor * Phong(Position.xyz, Normal, Attenuation, SpecularIntensity, SpecularPower);
}

//Technique
technique Default
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}
