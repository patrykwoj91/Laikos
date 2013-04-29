CustomVSOutput CustomVS(VSInputNmTxWeights vin)
{
    CustomVSOutput output;
    
    float4 pos_ws = mul(vin.Position, World);
    float3 eyeVector = normalize(EyePosition - pos_ws.xyz);
    float3 worldNormal = normalize(mul(vin.Normal, WorldInverseTranspose));

    ColorPair lightResult = ComputeLights(eyeVector, worldNormal, 3);
    
    output.PositionPS = mul(vin.Position, WorldViewProj);
    output.Diffuse = float4(lightResult.Diffuse, DiffuseColor.a);
    output.Specular = float4(lightResult.Specular, ComputeFogFactor(vin.Position));
    
    output.TexCoord = vin.TexCoord;
    
    return output;
}

CustomVSOutput CustomVSLight(VSInputNmTxWeights vin)
{
    CustomVSOutput output;
    
    float4 pos_ws = mul(vin.Position, World);
    float3 eyeVector = normalize(EyePosition - pos_ws.xyz);
    float3 worldNormal = normalize(mul(vin.Normal, WorldInverseTranspose));

    ColorPair lightResult = ComputeLights(eyeVector, worldNormal, 1);
    
    output.PositionPS = mul(vin.Position, WorldViewProj);
    output.Diffuse = float4(lightResult.Diffuse, DiffuseColor.a);
    output.Specular = float4(lightResult.Specular, ComputeFogFactor(vin.Position));
    
    output.TexCoord = vin.TexCoord;
    
    return output;
}

CustomVSOutputPixelLighting CustomVSPixelLighting(VSInputNmTxWeights vin)
{
    CustomVSOutputPixelLighting output;
    
    output.PositionPS = mul(vin.Position, WorldViewProj);
    output.PositionWS = float4(mul(vin.Position, World).xyz, ComputeFogFactor(vin.Position));
    output.NormalWS = normalize(mul(vin.Normal, WorldInverseTranspose));
    
    output.Diffuse = float4(1, 1, 1, DiffuseColor.a);
    output.TexCoord = vin.TexCoord;

    return output;
}

