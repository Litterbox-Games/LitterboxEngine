#version 450

layout(location = 0) in vec4 Color;
layout(location = 1) in vec2 TexCoord;

layout(location = 0) out vec4 color;

layout(set = 1, binding = 0) uniform texture2D tex0;
layout(set = 1, binding = 1) uniform sampler Sampler;

void main()
{
    vec4 textureColor = texture(sampler2D(tex0, Sampler), TexCoord);
    
    color = textureColor * Color;
}