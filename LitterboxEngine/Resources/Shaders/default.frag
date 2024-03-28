#version 450

layout(set = 1, binding = 0) uniform texture2D textures[8];
layout(set = 1, binding = 1) uniform sampler samp;

layout(location = 0) in vec4 fragColor;
layout(location = 1) in vec2 fragTexCoords;
layout(location = 2) flat in int fragTexIndex;

layout(location = 0) out vec4 outColor;

void main() {
    outColor = texture(sampler2D(textures[fragTexIndex], samp), fragTexCoords) * fragColor;
}