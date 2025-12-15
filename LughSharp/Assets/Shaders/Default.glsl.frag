#version 450 core

in vec4 v_color;     // Unpacked RGBA from Vertex Shader
in vec2 v_texCoords; // Interpolated UVs

layout (binding = 0) uniform sampler2D u_texture;
layout (location = 0) out vec4 fragColor;

void main()
{
    // Sample the texture at the current UV coordinate
    vec4 texColor = texture(u_texture, v_texCoords);
    
    // Multiply by vertex color to apply tinting and alpha transparency
    // texColor.rgba * v_color.rgba
    fragColor = texColor * v_color;
}