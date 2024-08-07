#version 450

// This struct is 56 bytes in total, using an alignment of 16
struct Quad { 
    vec2 min;
    vec2 max;
    vec2 texMin;
    vec2 texMax;
    vec4 color;
    float depth;
    int texIndex; // TODO: convert to uint or even byte (only limited by the max number of textures that can be bound at once - Vulkan spec requires at least 32)
};

// TODO: this can be changed to a normal uniform mat instead of a buffer most likely or could use a push_constant instead
// TODO: Need to find which way is faster to copy memory too - push_constant should be fastest for the gpu to read from 
layout(set = 0, binding = 0) uniform MVPBuffer {
    mat4 uMVP;
};

// std430 will use the alignment of the largest item in the Quad struct e.g. 16 bytes for vec4
// This means the stride for each element in the quads array is 64 bytes, 56 bytes + 8 byte padding
layout(std430, set = 1, binding = 0) buffer QuadBlock {
    Quad quads[];
};

layout(location = 0) out vec4 fragColor;
layout(location = 1) out vec2 fragTexCoords;
layout(location = 2) out int fragTexIndex;

void main() {
    int quadIndex = gl_VertexIndex / 6;
    int cornerIndex = gl_VertexIndex % 6;
    
    Quad quad = quads[quadIndex];
    
    if (cornerIndex == 0) {
        // Bottom Right
        gl_Position = uMVP * vec4(quad.max, quad.depth, 1.0);
        fragTexCoords = quad.texMax;
    } else if (cornerIndex == 1 || cornerIndex == 5) {
        // Bottom Left
        gl_Position = uMVP * vec4(quad.min.x, quad.max.y, quad.depth, 1.0);
        fragTexCoords = vec2(quad.texMin.x, quad.texMax.y);
    } else if (cornerIndex == 2 || cornerIndex == 4) {
        // Top Right
        gl_Position = uMVP * vec4(quad.max.x, quad.min.y, quad.depth, 1.0);
        fragTexCoords = vec2(quad.texMax.x, quad.texMin.y);
    } else {
        // Top Left
        gl_Position = uMVP * vec4(quad.min, quad.depth, 1.0);
        fragTexCoords = quad.texMin;
    }
    
    fragColor = quad.color;
    fragTexIndex = quad.texIndex;
}