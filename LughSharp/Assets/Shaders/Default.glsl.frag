#version 450 core
layout (location = 0) in vec4 v_color;
layout (location = 1) in vec2 v_texCoords;
layout (binding = 0) uniform sampler2D u_texture;
layout (location = 0) out vec4 fragColor;

void main() {
    // If you see a RED box at the bottom-left, the Matrix is working.
    // If you see a TEXTURED box, the UVs and Texture are working.
    vec4 tex = texture(u_texture, v_texCoords);
    fragColor = vec4(1.0, 0.0, 0.0, 1.0); 
}