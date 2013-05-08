float4x4 World;
float4x4 View;
float4x4 Projection;
float3 LightPosition;
float DepthPrecision;

struct VSI
{
	float4 Position	: POSITION0;
};

struct VSO
{
	float4 Position		 : POSITION0;
	float4 WorldPosition : TEXCOORD0;
};

VSO VS(VSI input)
{
	VSO output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, worldPosition);

	output.WorldPosition = worldPosition;

	return output;
}

float4 PS(VSO input) : COLOR0
{
	input.WorldPosition /= input.WorldPosition.w;

	float Depth = max(0.01f, length(LightPosition - input.WorldPosition)) / DepthPrecision;

	return exp((DepthPrecision, 0.5f) * Depth);
}

technique Default
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}