// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Circa64 Software Projects
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

using LughSharp.Source.Graphics.G2D;
using LughSharp.Source.Graphics.OpenGL;
using LughSharp.Source.Graphics.OpenGL.Enums;

using NUnit.Framework;

using GLBindings = LughSharp.Source.Graphics.OpenGL.Bindings.GLBindings;

namespace LughSharp.Tests.Source;

[PublicAPI]
public unsafe class OpenGLTest : ILughTest
{
    private static readonly GLBindings _gl = new();

    private const string FragmentShaderSource =
        "#version 450 core\n" +
        "out vec4 fragColor;\n" +
        "void main() {\n" +
        "    fragColor = vec4(1.0, 1.0, 1.0, 1.0);\n" +
        "}\n";

    private const string VertexShaderSource =
        "#version 450 core\n" +
        "layout (location = 0) in vec3 aPosition;\n" +
        "void main() {\n" +
        "    gl_Position = vec4(aPosition, 1.0);\n" +
        "}\n";

    private readonly short[] _indices = [ 0, 1, 2 ];

    private readonly float[] _vertices =
    [
        -0.5f, -0.5f, -.0f,
        0.5f, -0.5f, 0.0f,
        0.0f, 0.5f, 0.0f
    ];

    private uint _shaderProgram;
    private uint _vao, _vbo, _ibo;

    // ========================================================================
    // ========================================================================

    [SetUp]
    public void Setup()
    {
        _shaderProgram = CreateProgram( VertexShaderSource, FragmentShaderSource );

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray( _vao );

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer( BufferTarget.ArrayBuffer, _vbo );

        fixed ( float* ptr = _vertices )
        {
            _gl.BufferData( BufferTarget.ArrayBuffer,
                            _vertices.Length * sizeof( float ),
                            ( IntPtr )ptr,
                            BufferUsageHint.StaticDraw );
        }

        _gl.VertexAttribPointer( 0, 3, IGL.GLFloat, false, 3 * sizeof( float ), 0u );
        _gl.EnableVertexAttribArray( 0 );

        _ibo = _gl.GenBuffer();
        _gl.BindBuffer( BufferTarget.ElementArrayBuffer, _ibo );

        fixed ( short* ptr = _indices )
        {
            _gl.BufferData( BufferTarget.ElementArrayBuffer,
                            _indices.Length * sizeof( short ),
                            ( IntPtr )ptr,
                            BufferUsageHint.StaticDraw );
        }

        _gl.BindVertexArray( 0 );
        _gl.BindBuffer( BufferTarget.ArrayBuffer, 0 );
        _gl.BindBuffer( BufferTarget.ElementArrayBuffer, 0 );
    }

    [Test]
    public void Run()
    {
    }

    [TearDown]
    public void TearDown()
    {
    }

    public void Update()
    {
    }
    
    public void Render( SpriteBatch spriteBatch )
    {
        _gl.UseProgram( ( int )_shaderProgram );
        _gl.BindVertexArray( _vao );
        _gl.BindBuffer( BufferTarget.ElementArrayBuffer, _ibo );

//        var offsetInBytes = 0; //offset * sizeof( short );
        _gl.DrawElements( IGL.GLTriangles, _indices.Length, IGL.GLUnsignedShort, 0 );
    }

    private uint CreateProgram( string vertexShaderSource, string fragmentShaderSource )
    {
        uint vertexShader = _gl.CreateShader( IGL.GLVertexShader );
        _gl.ShaderSource( ( int )vertexShader, vertexShaderSource );
        _gl.CompileShader( ( int )vertexShader );

        uint fragmentShader = _gl.CreateShader( IGL.GLFragmentShader );
        _gl.ShaderSource( ( int )fragmentShader, fragmentShaderSource );
        _gl.CompileShader( ( int )fragmentShader );

        uint shaderProgram = _gl.CreateProgram();
        _gl.AttachShader( ( int )shaderProgram, ( int )vertexShader );
        _gl.AttachShader( ( int )shaderProgram, ( int )fragmentShader );
        _gl.LinkProgram( ( int )shaderProgram );

        _gl.DeleteShader( ( int )vertexShader );
        _gl.DeleteShader( ( int )fragmentShader );

        return shaderProgram;
    }
}