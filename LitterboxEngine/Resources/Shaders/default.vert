#version 450

layout(set = 0, binding = 0) uniform ProjectionViewBuffer {
    mat4x4 uProjection;
    mat4x4 uView;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec4 Color;
layout(location = 2) in vec2 TexCoords;
layout(location = 3) in int TexIndex;

layout(location = 0) out vec4 fragColor;
layout(location = 1) out vec2 fragTexCoords;
layout(location = 2) out int fragTexIndex;

void main() {
    gl_Position = uProjection * uView * vec4(Position, 1.0);
    fragColor = Color;
    fragTexCoords = TexCoords;
    fragTexIndex = TexIndex;
}