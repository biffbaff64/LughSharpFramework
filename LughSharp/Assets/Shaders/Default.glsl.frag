#version 450 core

in vec4 v_color;
in vec2 v_texCoords;

layout (binding = 0) uniform sampler2D u_texture;
layout (location = 0) out vec4 fragColor;

void main()
{
    vec4 texColor = texture(u_texture, v_texCoords);
    
    // TEST: Ignore the vertex color and force Alpha to 1.0
    // If the image appears, your 'a_color' from C# is the problem.
    fragColor = vec4(texColor.rgb, 1.0);
    
    // If it's STILL black, force it to be RED so we can see if the quad is there
    if(length(texColor.rgb) < 0.01) fragColor = vec4(1.0, 0.0, 0.0, 1.0);
}
