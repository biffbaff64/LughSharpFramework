// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ///////////////////////////////////////////////////////////////////////////////

using JetBrains.Annotations;

namespace LughSharp.Core.Graphics.Shaders;

[PublicAPI]
public class Shaders
{
    public const string DEFAULT_VERTEX_SHADER =
        "#version 450 core\n" +
        "layout (location = 0) in vec2 a_position;\n" +
        "layout (location = 1) in float a_color;\n" +
        "layout (location = 2) in vec2 a_texCoord0;\n" +
        "layout (location = 0) uniform mat4 u_combinedMatrix;\n" +
        "out vec4 v_color;\n" +
        "out vec2 v_texCoords;\n" +
        "void main()\n" +
        "{\n" +
        "    v_texCoords = a_texCoord0;\n" +
        "    // --- COLOR UNPACKING ---\n" +
        "    // We treat the float bits as an integer, then extract the 4 bytes.\n" +
        "    // This matches the LibGDX / SpriteBatch 'ColorPacked' format.\n" +
        "    uint rgba = floatBitsToUint(a_color);\n" +
        "    v_color = vec4(\n" +
        "        float((rgba & uint(0x000000FF))) / 255.0,         // Red\n" +
        "        float((rgba & uint(0x0000FF00)) >> 8) / 255.0,    // Green\n" +
        "        float((rgba & uint(0x00FF0000)) >> 16) / 255.0,   // Blue\n" +
        "        float((rgba & uint(0xFF000000)) >> 24) / 255.0    // Alpha\n" +
        "    );\n" +
        "    gl_Position = u_combinedMatrix * vec4(a_position, 0.0, 1.0);\n" +
        "}\n";

    public const string DEFAULT_FRAGMENT_SHADER =
        "#version 450 core\n" +
        "in vec4 v_color;     // Unpacked RGBA from Vertex Shader\n" +
        "in vec2 v_texCoords; // Interpolated UVs\n" +
        "layout (binding = 0) uniform sampler2D u_texture;\n" +
        "layout (location = 0) out vec4 fragColor;\n" +
        "void main()\n" +
        "{\n" +
        "    // Sample the texture at the current UV coordinate\n" +
        "    fragColor = texture(u_texture, v_texCoords);\n" +
        "}\n";

    public const string OLD_DEFAULT_FRAGMENT_SHADER =
        "#version 450 core\n" +
        "in vec4 v_color;     // Unpacked RGBA from Vertex Shader\n" +
        "in vec2 v_texCoords; // Interpolated UVs\n" +
        "layout (binding = 0) uniform sampler2D u_texture;\n" +
        "layout (location = 0) out vec4 fragColor;\n" +
        "void main()\n" +
        "{\n" +
        "    // Sample the texture at the current UV coordinate\n" +
        "    vec4 texColor = texture(u_texture, v_texCoords);\n" +
        "    fragColor = texColor * v_color;\n" +
        "}\n";
}

// ============================================================================
// ============================================================================