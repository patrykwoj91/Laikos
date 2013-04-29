// Pixel shader: vertex lighting.
float4 CustomPS(CustomVSOutput pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
    
    AddSpecular(color, pin.Specular.rgb);
    ApplyFog(color, pin.Specular.w);
    
    // Custom: Apply basic toon shading
    color = round(color * 5) / 5;
    
    return color;
}


// Pixel shader: vertex lighting, no fog.
float4 CustomPSNoFog(CustomVSOutput pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
    
    AddSpecular(color, pin.Specular.rgb);
    
    // Custom: Apply basic toon shading
    color = round(color * 5) / 5;
    
    return color;
}


// Pixel shader: pixel lighting.
float4 CustomPSPixelLighting(CustomVSOutputPixelLighting pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
    
    float3 eyeVector = normalize(EyePosition - pin.PositionWS.xyz);
    float3 worldNormal = normalize(pin.NormalWS);
    
    ColorPair lightResult = ComputeLights(eyeVector, worldNormal, 3);
    
    color.rgb *= lightResult.Diffuse;

    AddSpecular(color, lightResult.Specular);
    ApplyFog(color, pin.PositionWS.w);
    
    // Custom: Apply basic toon shading
    color = round(color * 5) / 5;
    
    return color;
}