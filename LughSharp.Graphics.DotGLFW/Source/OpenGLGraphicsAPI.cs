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

using LughSharp.Graphics.Abstractions.Source.Enums;
using LughSharp.Graphics.Abstractions.Source.Interfaces;
using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Graphics.OpenGL.Enums;

namespace LughSharp.Graphics.DotGLFW.Source;

[PublicAPI]
public class OpenGLGraphicsAPI : IGraphicsAPI
{
    private readonly GLBindings _gl;

    public OpenGLGraphicsAPI( GLBindings gl )
    {
        _gl = gl;
    }

    /// <inheritdoc />
    public IBuffer? CreateBuffer( BufferType type, BufferUsage usage )
    {
        return null;
    }

    /// <inheritdoc />
    public IShaderProgram? CreateShaderProgram()
    {
        return null;
    }

    /// <inheritdoc />
    public IShader? CreateShader( ShaderType type )
    {
        return null;
    }

    /// <inheritdoc />
    public IVertexLayout? CreateVertexLayout()
    {
        return null;
    }

    /// <inheritdoc />
    public void SetBlendMode( BlendMode mode )
    {
    }

    /// <inheritdoc />
    public void SetDepthTest( bool enabled )
    {
    }

    /// <inheritdoc />
    public void Draw( PrimitiveType primitive, int first, int count )
    {
    }

    /// <inheritdoc />
    public void DrawIndexed( PrimitiveType primitive, int count, IndexType type )
    {
    }
}

// ========================================================================
// ========================================================================