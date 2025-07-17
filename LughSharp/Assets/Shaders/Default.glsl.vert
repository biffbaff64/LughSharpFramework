#version 450 core

in vec2 a_position;
in vec4 a_colorPacked;
in vec2 a_texCoord0;

uniform mat4 u_combinedMatrix;

out vec4 v_colorPacked;
out vec2 v_texCoords;

void main() {
    gl_Position = u_combinedMatrix * vec4(a_position, 0.0, 1.0);
    v_colorPacked = a_colorPacked;
    v_colorPacked.a = v_colorPacked.a * (255.0/254.0);
    v_texCoords = a_texCoord0;
}

