// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Richard Ikin.
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

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Graphics.OpenGL.Enums;
using LughSharp.Lugh.Maths;
using LughSharp.Lugh.Utils;

namespace LughSharp.Lugh.Graphics.GLUtils;

[PublicAPI]
public class ShaderProgram : IDisposable
{
    internal const int CACHED_NOT_FOUND = -1;
    internal const int NOT_CACHED       = -2;

    // ========================================================================

    /// <summary>
    /// code that is always added to the vertex and fragments shaders, to inject a #version line.
    /// </summary>
    public const string SHADER_VERSION_CODE = "#version 450 core";

    public const string POSITION_ATTRIBUTE   = "a_position";
    public const string NORMAL_ATTRIBUTE     = "a_normal";
    public const string COLOR_ATTRIBUTE      = "a_colorPacked";
    public const string TEXCOORD_ATTRIBUTE   = "a_texCoord";
    public const string TANGENT_ATTRIBUTE    = "a_tangent";
    public const string BINORMAL_ATTRIBUTE   = "a_binormal";
    public const string BONEWEIGHT_ATTRIBUTE = "a_boneWeight";

    public const string COMBINED_MATRIX_UNIFORM = "u_combinedMatrix";

    // ========================================================================
    /// <summary>
    /// flag indicating whether attributes & uniforms must be present at all times.
    /// </summary>
    public static readonly bool Pedantic = true;

    // ========================================================================

    public bool   IsCompiled             { get; set; }
    public string VertexShaderSource     { get; }
    public string FragmentShaderSource   { get; }
    public int    Handle                 { get; private set; }
    public bool   Invalidated            { get; protected set; }
    public int    CombinedMatrixLocation { get; set; }

    // ========================================================================

    private int    _vertexShaderHandle;
    private int    _fragmentShaderHandle;
    private string _shaderLog = "";

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="vertexShader"></param>
    /// <param name="fragmentShader"></param>
    public ShaderProgram( string vertexShader, string fragmentShader )
    {
        VertexShaderSource   = vertexShader;
        FragmentShaderSource = fragmentShader;

        CompileShaders( VertexShaderSource, FragmentShaderSource );

        if ( !IsCompiled )
        {
            Logger.Debug( $"Shader program {VertexShaderSource} has not been compiled." );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="vertexShader"> the vertex shader </param>
    /// <param name="fragmentShader"> the fragment shader </param>
    public ShaderProgram( FileSystemInfo vertexShader, FileSystemInfo fragmentShader )
        : this( File.ReadAllText( vertexShader.Name ), File.ReadAllText( fragmentShader.Name ) )
    {
    }

    // ========================================================================

    /// <summary>
    /// Loads and compiles the shaders, creates a new program and links the shaders.
    /// </summary>
    /// <param name="vertexShader"> the vertex shader </param>
    /// <param name="fragmentShader"> the fragment shader </param>
    public void CompileShaders( string vertexShader, string fragmentShader )
    {
        _vertexShaderHandle   = LoadShader( ( int )ShaderType.VertexShader, vertexShader );
        _fragmentShaderHandle = LoadShader( ( int )ShaderType.FragmentShader, fragmentShader );

        if ( ( _vertexShaderHandle == -1 ) || ( _fragmentShaderHandle == -1 ) )
        {
            IsCompiled = false;

            return;
        }

        Handle = LinkProgram( CreateProgram() );

        IsCompiled = ( Handle != -1 );
    }

    /// <summary>
    /// </summary>
    /// <param name="program"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public unsafe int LinkProgram( int program )
    {
        if ( program == -1 )
        {
            return -1;
        }

        GdxApi.Bindings.AttachShader( ( uint )program, ( uint )_vertexShaderHandle );
        GdxApi.Bindings.AttachShader( ( uint )program, ( uint )_fragmentShaderHandle );
        GdxApi.Bindings.LinkProgram( ( uint )program );

        var status = stackalloc int[ 1 ];

        GdxApi.Bindings.UseProgram( ( uint )program );
        GdxApi.Bindings.GetProgramiv( ( uint )program, IGL.GL_LINK_STATUS, status );

        if ( *status == IGL.GL_FALSE )
        {
            var length = stackalloc int[ 1 ];

            GdxApi.Bindings.GetProgramiv( ( uint )program, IGL.GL_INFO_LOG_LENGTH, length );

            _shaderLog = GdxApi.Bindings.GetProgramInfoLog( ( uint )program, *length );

            throw new Exception( $"Failed to link shader program {program}: {_shaderLog}" );
        }

        GdxApi.Bindings.DetachShader( ( uint )program, ( uint )_vertexShaderHandle );   // Detach vertex shader
        GdxApi.Bindings.DetachShader( ( uint )program, ( uint )_fragmentShaderHandle ); // Detach fragment shader
        GdxApi.Bindings.DeleteShader( ( uint )_vertexShaderHandle );                    // Delete vertex shader
        GdxApi.Bindings.DeleteShader( ( uint )_fragmentShaderHandle );                  // Delete fragment shader

        return program;
    }

    /// <summary>
    /// Trys to create a program object for this ShaderProgram.
    /// </summary>
    /// <returns>The program ID if created, otherwise -1.</returns>
    protected static int CreateProgram()
    {
        var program = ( int )GdxApi.Bindings.CreateProgram();

        return program != 0 ? program : -1;
    }

    // ========================================================================

    /// <summary>
    /// Loads the supplied shader into the relvant shader type.
    /// </summary>
    /// <param name="shaderType"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public unsafe int LoadShader( int shaderType, string source )
    {
        var shader = GdxApi.Bindings.CreateShader( shaderType );

        if ( shader == 0 )
        {
            return -1;
        }

        GdxApi.Bindings.ShaderSource( shader, source );
        GdxApi.Bindings.CompileShader( shader );

        var status = stackalloc int[ 1 ];

        GdxApi.Bindings.GetShaderiv( shader, IGL.GL_COMPILE_STATUS, status );

        if ( *status == IGL.GL_FALSE )
        {
            var length = stackalloc int[ 1 ];

            GdxApi.Bindings.GetShaderiv( shader, IGL.GL_INFO_LOG_LENGTH, length );

            var infoLog = GdxApi.Bindings.GetShaderInfoLog( shader, *length );

            GdxApi.Bindings.DeleteShader( shader );

            _shaderLog += shaderType == IGL.GL_VERTEX_SHADER ? "Vertex shader\n" : "Fragment shader:\n";
            _shaderLog += infoLog;

            throw new Exception( $"Failed to loader shader {shader}: {infoLog}" );
        }

        return ( int )shader;
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="matrix"></param>
    /// <param name="transpose"></param>
    /// <typeparam name="T"></typeparam>
    public virtual void SetUniformMatrix< T >( string name, ref T matrix, bool transpose = false )
        where T : unmanaged
    {
        var location = GdxApi.Bindings.GetUniformLocation( ( uint )Handle, name );

        SetUniformMatrix( location, ref matrix, transpose );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="matrix"></param>
    /// <param name="transpose"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="ArgumentException"></exception>
    public virtual unsafe void SetUniformMatrix< T >( int location, ref T matrix, bool transpose = false )
        where T : unmanaged
    {
        const int MAT4X4 = 16;
        const int MAT3X3 = 9;
        const int MAT2X2 = 4;

        if ( location == -1 ) return;

        var matrixSize = Marshal.SizeOf< T >() / sizeof( float );

        fixed ( T* ptr = &matrix )
        {
            if ( matrixSize == MAT4X4 )
            {
                GdxApi.Bindings.UniformMatrix4fv( location, 1, transpose, ( float* )ptr );
            }
            else if ( matrixSize == MAT3X3 )
            {
                GdxApi.Bindings.UniformMatrix3fv( location, 1, transpose, ( float* )ptr );
            }
            else if ( matrixSize == MAT2X2 )
            {
                GdxApi.Bindings.UniformMatrix2fv( location, 1, transpose, ( float* )ptr );
            }
            else
            {
                throw new ArgumentException( "Matrix must be 2x2, 3x3 or 4x4" );
            }
        }
    }

    public virtual int GetAttributeLocation( string name )
    {
        return GdxApi.Bindings.GetAttribLocation( ( uint )Handle, name );
    }

    public virtual void SetUniformi( string name, int value )
    {
        GdxApi.Bindings.Uniform1i( FetchUniformLocation( name ), value );
    }

    public virtual void SetUniformf( string name, float value )
    {
        GdxApi.Bindings.Uniform1f( FetchUniformLocation( name ), value );
    }

    /// <summary>
    /// Sets the uniform matrix with the given name. The <see cref="ShaderProgram"/>
    /// must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="matrix"> the matrix  </param>
    public virtual void SetUniformMatrix( string name, Matrix4 matrix )
    {
        SetUniformMatrix( name, matrix, false );
    }

    /// <summary>
    /// Sets the uniform matrix with the given name. The <see cref="ShaderProgram"/>
    /// must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="matrix"> the matrix </param>
    /// <param name="transpose"> whether the matrix should be transposed  </param>
    public virtual void SetUniformMatrix( string name, Matrix4 matrix, bool transpose )
    {
        SetUniformMatrix( FetchUniformLocation( name ), matrix, transpose );
    }

    public virtual void SetUniformMatrix( int location, Matrix4 matrix )
    {
        SetUniformMatrix( location, matrix, false );
    }

    public virtual void SetUniformMatrix( int location, Matrix4 matrix, bool transpose )
    {
        GdxApi.Bindings.UniformMatrix4fv( location, transpose, matrix.Val );
    }

    public virtual int FetchUniformLocation( string name )
    {
        return GdxApi.Bindings.GetUniformLocation( ( uint )Handle, name );
    }

    public virtual int FetchAttributeLocation( string name )
    {
        return GdxApi.Bindings.GetAttribLocation( ( uint )Handle, name );
    }

    public virtual void EnableVertexAttribute( int location )
    {
        GdxApi.Bindings.EnableVertexAttribArray( ( uint )location );
    }

    public virtual void SetVertexAttribute( int location, int size, int type, bool normalize, int stride, int offset )
    {
        GdxApi.Bindings.VertexAttribPointer( ( uint )location, size, type, normalize, stride, ( uint )offset );
    }

    public virtual void SetUniformMatrix4Fv( string name, params float[] values )
    {
        SetUniformMatrix4Fv( FetchUniformLocation( name ), values );
    }

    public virtual void SetUniformMatrix4Fv( int location, params float[] values )
    {
        GdxApi.Bindings.UniformMatrix4fv( location, false, values );
    }

    public virtual void DisableVertexAttribute( int location )
    {
        GdxApi.Bindings.DisableVertexAttribArray( ( uint )location );
    }

    public virtual void DisableVertexAttribute( string name )
    {
        var location = FetchAttributeLocation( name );

        if ( location == -1 )
        {
            return;
        }

        GdxApi.Bindings.DisableVertexAttribArray( ( uint )location );
    }

    /// <summary>
    /// Bind this shader to the renderer.
    /// </summary>
    public virtual void Bind()
    {
        GdxApi.Bindings.UseProgram( ( uint )Handle );
    }

    /// <summary>
    /// Unbind this shader from the renderer.
    /// </summary>
    public virtual void Unbind()
    {
        GdxApi.Bindings.UseProgram( 0 );
    }

    // ========================================================================

    /// <summary>
    /// the log info for the shader compilation and program linking stage.
    /// The shader needs to be bound for this method to have an effect.
    /// </summary>
    public unsafe string ShaderLog
    {
        get
        {
            if ( IsCompiled )
            {
                var length = stackalloc int[ 1 ];

                GdxApi.Bindings.GetProgramiv( ( uint )Handle, IGL.GL_INFO_LOG_LENGTH, length );

                _shaderLog = GdxApi.Bindings.GetProgramInfoLog( ( uint )Handle, *length );
            }

            return _shaderLog;
        }
    }

    /// <summary>
    /// Disposes all resources associated with this shader.
    /// Must be called when the shader is no longer used.
    /// </summary>
    public void Dispose()
    {
        GdxApi.Bindings.UseProgram( 0 );
        GdxApi.Bindings.DeleteShader( ( uint )_vertexShaderHandle );
        GdxApi.Bindings.DeleteShader( ( uint )_fragmentShaderHandle );
        GdxApi.Bindings.DeleteProgram( ( uint )Handle );

        GC.SuppressFinalize( this );
    }
}