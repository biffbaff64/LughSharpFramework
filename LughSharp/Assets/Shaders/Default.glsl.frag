#version 450 core

layout (location = 0) in vec4 v_color;
layout (location = 1) in vec2 v_texCoords;
layout (binding = 0) uniform sampler2D u_texture;
layout (location = 0) out vec4 fragColor;

void main()
{
    fragColor = texture(u_texture, v_texCoords) * v_color;
}
