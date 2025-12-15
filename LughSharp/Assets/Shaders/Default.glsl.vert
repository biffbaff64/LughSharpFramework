#version 450 core

layout (location = 0) in vec2 a_position;
layout (location = 1) in float a_color;
layout (location = 2) in vec2 a_texCoord0;

layout (location = 0) uniform mat4 u_combinedMatrix;

out vec4 v_color;
out vec2 v_texCoords;

void main()
{
    v_texCoords = a_texCoord0;

    // --- COLOR UNPACKING ---
    // We treat the float bits as an integer, then extract the 4 bytes.
    // This matches the LibGDX / SpriteBatch 'ColorPacked' format.
//    uint rgba = floatBitsToUint(a_color);
//    v_color = vec4(
//        float((rgba & uint(0x000000FF))) / 255.0,         // Red
//        float((rgba & uint(0x0000FF00)) >> 8) / 255.0,    // Green
//        float((rgba & uint(0x00FF0000)) >> 16) / 255.0,   // Blue
//        float((rgba & uint(0xFF000000)) >> 24) / 255.0    // Alpha
//    );

    // Inside Vertex Shader main()
    v_color = vec4(1.0, 1.0, 1.0, 1.0); // Force full white/opaque

    gl_Position = u_combinedMatrix * vec4(a_position, 0.0, 1.0);
}
