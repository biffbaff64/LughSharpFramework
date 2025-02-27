#version 450 core

in vec4 v_colorPacked;
in vec2 v_texCoords;

out vec4 FragColor;

uniform sampler2D u_texture;

void main()
{
    FragColor = texture(u_texture, v_texCoords) * v_colorPacked;
}
