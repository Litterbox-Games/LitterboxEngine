#version 450

layout(location = 0) in vec3 Position;
layout(location = 1) in vec4 Color;
layout(location = 2) in vec2 TexCoords;

layout(location = 0) out vec4 fragColor;
layout(location = 1) out vec2 fragTexCoords;

void main() {
    gl_Position = vec4(Position, 1.0);
    fragColor = Color;
    fragTexCoords = TexCoords;
}