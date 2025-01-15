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

using LughSharp.Lugh.Graphics.OpenGL;

namespace LughSharp.Lugh.Graphics.GLUtils;

public partial class ShaderProgram
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    public static void ClearAllShaderPrograms( IApplication app )
    {
        _shaders.Remove( app );
    }

    /// <summary>
    /// Invalidates all shaders so the next time they are used new
    /// handles are generated.
    /// </summary>
    /// <param name="app">  </param>
    public static void InvalidateAllShaderPrograms( IApplication app )
    {
        List< ShaderProgram >? shaderArray;

        if ( !_shaders.TryGetValue( app, out var value ) || ( value == null ) )
        {
            shaderArray = [ ];
        }
        else
        {
            shaderArray = value;
        }

        foreach ( var sp in shaderArray! )
        {
            sp._invalidated = true;
            sp.CheckManaged();
        }
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    protected static int CreateProgram()
    {
        var program = ( int )GdxApi.Bindings.CreateProgram();

        return program != 0 ? program : -1;
    }

    private unsafe int LinkProgram( int program )
    {
        if ( program == -1 )
        {
            return -1;
        }

        GdxApi.Bindings.AttachShader( ( uint )program, ( uint )_vertexShaderHandle );
        GdxApi.Bindings.AttachShader( ( uint )program, ( uint )_fragmentShaderHandle );
        GdxApi.Bindings.LinkProgram( ( uint )program );

        var status = stackalloc int[ 1 ];

        GdxApi.Bindings.GetProgramiv( ( uint )program, IGL.GL_LINK_STATUS, status );

        if ( *status == IGL.GL_FALSE )
        {
            var length = stackalloc int[ 1 ];

            GdxApi.Bindings.GetProgramiv( ( uint )program, IGL.GL_INFO_LOG_LENGTH, length );

            _shaderLog = GdxApi.Bindings.GetProgramInfoLog( ( uint )program, *length );

            throw new Exception( $"Failed to link shader program {program}: {_shaderLog}" );
        }

        return program;
    }
    
    /// <summary>
    /// 
    /// </summary>
    private void CheckManaged()
    {
        if ( _invalidated )
        {
            CompileShaders( VertexShaderSource, FragmentShaderSource );
            _invalidated = false;
        }
    }
}

