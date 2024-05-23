#version 450

// TODO: this can be changed to a normal uniform mat instead of a buffer most likely
layout(set = 0, binding = 0) uniform MVPBuffer {
    mat4x4 uMVP;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec4 Color;
layout(location = 2) in vec2 TexCoords;
layout(location = 3) in int TexIndex;

layout(location = 0) out vec4 fragColor;
layout(location = 1) out vec2 fragTexCoords;
layout(location = 2) out int fragTexIndex;

void main() {
    gl_Position = uMVP * vec4(Position, 1.0);
    fragColor = Color;
    fragTexCoords = TexCoords;
    fragTexIndex = TexIndex;
}