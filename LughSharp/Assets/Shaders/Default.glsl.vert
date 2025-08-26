#version 450 core

layout (location = 0) in vec2 a_position;
layout (location = 1) in float a_color;
layout (location = 2) in vec2 a_texCoord0;

uniform mat4 u_combinedMatrix;

layout (location = 0) out vec4 v_color;
layout (location = 1) out vec2 v_texCoords;

// Function to unpack color from float to vec4
vec4 unpackColor(float packedColor) {
    uint color = uint(packedColor);
    return vec4(
    float((color >> 0u) & 0xFFu) / 255.0, // R
    float((color >> 8u) & 0xFFu) / 255.0, // G  
    float((color >> 16u) & 0xFFu) / 255.0, // B
    float((color >> 24u) & 0xFFu) / 255.0// A
    );
}

void main()
{
    v_color = unpackColor(a_color);
    v_texCoords = a_texCoord0;
    gl_Position = u_combinedMatrix * vec4(a_position, 0.0, 1.0);
}
