// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

using LughSharp.Lugh.Graphics.Utils;

namespace LughSharp.Lugh.Graphics.G2D;

[PublicAPI]
[StructLayout( LayoutKind.Sequential )]
public struct Transform
{
    public System.Numerics.Vector2 Position; // Top Left position
    public System.Numerics.Vector2 Size;

    public Vector2Int AtlasOffset;
    public Vector2Int SpriteSize;
    public int        RenderOptions;
    public int        MaterialIndex;
    public float      Layer;
    public int        Padding; // Padding for alignment purposes
}

// Helper struct for integer vectors
[PublicAPI]
[StructLayout( LayoutKind.Sequential )]
public struct Vector2Int
{
    public int X;
    public int Y;
}

[PublicAPI]
[StructLayout( LayoutKind.Sequential )]
public struct Material : IEquatable< Material >
{
    public System.Numerics.Vector4 Color;

    // Constructor with default white color
    public Material()
    {
        Color = System.Numerics.Vector4.One; // Assuming Vector4.One represents white (1,1,1,1)
    }

    // Implement equality comparison
    public bool Equals( Material other )
    {
        return Color.Equals( other.Color );
    }

    // Override Object.Equals
    public override bool Equals( object? obj )
    {
        return obj is Material other && Equals( other );
    }

    // Override GetHashCode
    public override int GetHashCode()
    {
        return Color.GetHashCode();
    }

    // Implement == operator
    public static bool operator ==( Material left, Material right )
    {
        return left.Equals( right );
    }

    // Implement != operator
    public static bool operator !=( Material left, Material right )
    {
        return !( left == right );
    }
}

public partial class SpriteBatch
{
    public const string QUADS_VERTEX_SHADER =
        """
        #version 450 core
                layout( location = 0 ) out vec2 textureCoordsOut;
                layout (location = 1) out flat int renderOptions;
                layout (location = 2) out flat int materialIdx;
                layout (std430, binding = 0) buffer TransformSBO
                {
                  Transform transforms[];
                };
                uniform vec2 screenSize;
                uniform mat4 orthoProjection;
                void main()
                {
                  Transform transform = transforms[gl_InstanceID];
                  vec2 vertices[6] =
                  {
                    transform.pos,
                    vec2(transform.pos + vec2(0.0, transform.size.y)),
                    vec2(transform.pos + vec2(transform.size.x, 0.0)),
                    vec2(transform.pos + vec2(transform.size.x, 0.0)),
                    vec2(transform.pos + vec2(0.0, transform.size.y)),
                    transform.pos + transform.size
                  };
                  int left = transform.atlasOffset.x;
                  int top = transform.atlasOffset.y;
                  int right  = transform.atlasOffset.x + transform.spriteSize.x;
                  int bottom = transform.atlasOffset.y + transform.spriteSize.y;
                  if(bool(transform.renderOptions & RENDERING_OPTION_FLIP_X))
                  {
                    int tmp = left;
                    left = right;
                    right = tmp;
                  }
                  if(bool(transform.renderOptions & RENDERING_OPTION_FLIP_Y))
                  {
                    int tmp = top;
                    top    = bottom;
                    bottom = tmp;
                  }
                  vec2 textureCoords[6] = 
                  {
                    vec2(left, top),
                    vec2(left, bottom),
                    vec2(right, top),
                    vec2(right, top),
                    vec2(left, bottom),
                    vec2(right, bottom),
                  };
                  {
                    vec2 vertexPos = vertices[gl_VertexID];
                    gl_Position = orthoProjection * vec4(vertexPos, transform.layer, 1.0);
                  }
                  textureCoordsOut = textureCoords[gl_VertexID];
                  renderOptions = transform.renderOptions;
                  materialIdx = transform.materialIdx;
                }\n
        """;

    public const string QUADS_FRAGMENT_SHADER =
        """
        #version 450 core
                layout (location = 0) in vec2 textureCoordsIn;
                layout (location = 1) in flat int renderOptions;
                layout (location = 2) in flat int materialIdx;
                layout (location = 0) out vec4 fragColor;
                layout (binding = 0) uniform sampler2D textureAtlas;
                layout (binding = 1) uniform sampler2D fontAtlas;
                layout(std430, binding = 1) buffer Materials
                {
                  Material materials[];
                };
                void main()
                {
                  Material material = materials[materialIdx];
                  if(bool(renderOptions & RENDERING_OPTION_FONT))
                  {
                    vec4 textureColor = texelFetch(fontAtlas, ivec2(textureCoordsIn), 0);
                    if(textureColor.r == 0.0)
                    {
                      discard;
                    }
                    fragColor = textureColor.r * material.color;
                  }
                  else
                  {
                    vec4 textureColor = texelFetch(textureAtlas, ivec2(textureCoordsIn), 0);
                    if(textureColor.a == 0.0)
                    {
                      discard;
                    }
                    fragColor = textureColor * material.color;
                  }
                }
        """;

    // ========================================================================

    public const string DEFAULT_VERTEX_SHADER = "#version 450 core\n" +
                                                "layout (location = 0) in vec4 " + ShaderProgram.POSITION_ATTRIBUTE + ";\n" +
                                                "layout (location = 1) in vec4 " + ShaderProgram.COLOR_ATTRIBUTE + ";\n" +
                                                "layout (location = 2) in vec2 " + ShaderProgram.TEXCOORD_ATTRIBUTE + "0;\n" +
                                                "uniform mat4 u_combinedMatrix;\n" +
                                                "layout (location = 0) out vec4 v_color;\n" +
                                                "layout (location = 1) out vec2 v_texCoords;\n" +
                                                "void main() {\n" +
                                                "    v_color = " + ShaderProgram.COLOR_ATTRIBUTE + ";\n" +
                                                "    v_texCoords = " + ShaderProgram.TEXCOORD_ATTRIBUTE + "0;\n" +
                                                "    gl_Position = u_combinedMatrix * " + ShaderProgram.POSITION_ATTRIBUTE + ";\n" +
                                                "}\n";

    public const string DEFAULT_FRAGMENT_SHADER = "#version 450 core\n" +
                                                  "layout (location = 0) in vec4 v_color;\n" +
                                                  "layout (location = 1) in vec2 v_texCoords;\n" +
                                                  "uniform sampler2D u_texture;\n" +
                                                  "layout (location = 0) out vec4 fragColor;\n" +
                                                  "void main() {\n" +
                                                  "    fragColor = v_color * texture(u_texture, v_texCoords);\n" +
                                                  "}\n";
}

// ========================================================================
// ========================================================================