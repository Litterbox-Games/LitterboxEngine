#version 450

layout(set = 0, binding = 0) uniform ProjectionViewBuffer {
    mat4x4 uProjection;
    mat4x4 uView;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec4 Color;
layout(location = 2) in vec2 TexCoord;

layout(location = 0) out vec4 color;
layout(location = 1) out vec2 texCoord;

void main()
{
    gl_Position = uProjection * uView * vec4(Position, 1);

    color = Color;
    texCoord = TexCoord;
}