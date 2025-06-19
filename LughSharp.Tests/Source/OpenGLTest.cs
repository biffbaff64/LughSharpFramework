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

using JetBrains.Annotations;

using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Graphics.OpenGL.Enums;

using GLBindings = LughSharp.Lugh.Graphics.OpenGL.GLBindings;

namespace LughSharp.Tests.Source;

[PublicAPI]
public unsafe class OpenGLTest
{
    private static readonly GLBindings _gl = new();

    private readonly string _fragmentShaderSource =
        "#version 450 core\n" +
        "out vec4 FragColor;\n" +
        "void main() {\n" +
        "    FragColor = vec4(1.0, 1.0, 1.0, 1.0);\n" +
        "}\n";

    private readonly string _vertexShaderSource =
        "#version 450 core\n" +
        "layout (location = 0) in vec3 aPosition;\n" +
        "void main() {\n" +
        "    gl_Position = vec4(aPosition, 1.0);\n" +
        "}\n";

    private readonly short[] indices = [ 0, 1, 2 ];

    private readonly float[] vertices =
    [
        -0.5f, -0.5f, -.0f,
        0.5f, -0.5f, 0.0f,
        0.0f, 0.5f, 0.0f,
    ];

    private uint _shaderProgram;

    private uint _vao, _vbo, _ibo;

    // ========================================================================
    // ========================================================================

    public void Create()
    {
        _shaderProgram = CreateProgram( _vertexShaderSource, _fragmentShaderSource );

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray( _vao );

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer( ( int )BufferTarget.ArrayBuffer, _vbo );

        fixed ( float* ptr = vertices )
        {
            _gl.BufferData( ( int )BufferTarget.ArrayBuffer, vertices.Length * sizeof( float ), ( IntPtr )ptr, IGL.GL_STATIC_DRAW );
        }

        _gl.VertexAttribPointer( 0, 3, IGL.GL_FLOAT, false, 3 * sizeof( float ), 0u );
        _gl.EnableVertexAttribArray( 0 );

        _ibo = _gl.GenBuffer();
        _gl.BindBuffer( IGL.GL_ELEMENT_ARRAY_BUFFER, _ibo );

        fixed ( short* ptr = indices )
        {
            _gl.BufferData( IGL.GL_ELEMENT_ARRAY_BUFFER, indices.Length * sizeof( short ), ( IntPtr )ptr, IGL.GL_STATIC_DRAW );
        }

        _gl.BindVertexArray( 0 );
        _gl.BindBuffer( ( int )BufferTarget.ArrayBuffer, 0 );
        _gl.BindBuffer( IGL.GL_ELEMENT_ARRAY_BUFFER, 0 );
    }

    public void Render()
    {
        _gl.UseProgram( ( int )_shaderProgram );
        _gl.BindVertexArray( _vao );
        _gl.BindBuffer( IGL.GL_ELEMENT_ARRAY_BUFFER, _ibo );

//        var offsetInBytes = 0; //offset * sizeof( short );
        _gl.DrawElements( IGL.GL_TRIANGLES, indices.Length, IGL.GL_UNSIGNED_SHORT, 0 );
    }

    private uint CreateProgram( string vertexShaderSource, string fragmentShaderSource )
    {
        var vertexShader = _gl.CreateShader( IGL.GL_VERTEX_SHADER );
        _gl.ShaderSource( ( int )vertexShader, vertexShaderSource );
        _gl.CompileShader( ( int )vertexShader );

        var fragmentShader = _gl.CreateShader( IGL.GL_FRAGMENT_SHADER );
        _gl.ShaderSource( ( int )fragmentShader, fragmentShaderSource );
        _gl.CompileShader( ( int )fragmentShader );

        var shaderProgram = _gl.CreateProgram();
        _gl.AttachShader( ( int )shaderProgram, ( int )vertexShader );
        _gl.AttachShader( ( int )shaderProgram, ( int )fragmentShader );
        _gl.LinkProgram( ( int )shaderProgram );

        _gl.DeleteShader( ( int )vertexShader );
        _gl.DeleteShader( ( int )fragmentShader );

        return shaderProgram;
    }
}