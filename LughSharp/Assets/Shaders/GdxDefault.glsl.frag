#version 450 core

in vec4 v_color;
in vec2 v_texCoords;

uniform sampler2D u_texture;

out vec4 FragColor;

void main()
{
  FragColor = v_color * texture(u_texture, v_texCoords);
}
