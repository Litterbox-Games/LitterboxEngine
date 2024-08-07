#version 450

layout(set = 2, binding = 0) uniform texture2D textures[8]; // TODO: this can be increased to 32 at a maximum - using texture atlases should be sufficient to not need more than 32 4k textures (2097152 16x16 sprites)
layout(set = 2, binding = 1) uniform sampler samp;

layout(location = 0) in vec4 fragColor;
layout(location = 1) in vec2 fragTexCoords;
layout(location = 2) flat in int fragTexIndex;

layout(location = 0) out vec4 outColor;

void main() {
    vec4 textureColor = texture(sampler2D(textures[fragTexIndex], samp), fragTexCoords);
    if (textureColor.w == 0.0f)
        discard;
    else
        outColor = textureColor * fragColor;
}