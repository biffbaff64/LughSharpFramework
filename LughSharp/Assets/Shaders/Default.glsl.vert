#version 450 core

layout (location = 0) in vec4 a_position;
layout (location = 1) in vec4 a_color;
layout (location = 2) in vec2 a_texCoord0;

uniform mat4 u_combinedMatrix;

layout (location = 0) out vec4 v_color;
layout (location = 1) out vec2 v_texCoords;

void main()
{
    v_color = a_color;
    v_texCoords = a_texCoord0;
    gl_Position = u_combinedMatrix * a_position;
}
