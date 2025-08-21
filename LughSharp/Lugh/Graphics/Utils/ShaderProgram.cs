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
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

using GLuint = uint;

namespace LughSharp.Lugh.Graphics.Utils;

[PublicAPI]
public struct ShaderData
{
    public int Program;
    public int VertexShader;
    public int FragmentShader;
}

/// <summary>
/// A shader program encapsulates a vertex and fragment shader pair linked to form a shader program.
/// <para>
/// After construction a ShaderProgram can be used to draw <see cref="Mesh"/>. To make the GPU use a
/// specific ShaderProgram the programs <see cref="ShaderProgram.Bind()"/> method must be used which
/// effectively binds the program.
/// </para>
/// <para>
/// When a ShaderProgram is bound one can set uniforms, vertex attributes and attributes as needed via
/// the appropriate methods.
/// </para>
/// <para>
/// A ShaderProgram must be disposed via a call to <see cref="ShaderProgram.Dispose()"/> when it is no
/// longer needed
/// </para>
/// </summary>
[PublicAPI]
public class ShaderProgram : IDisposable
{
    /// <summary>
    /// default name for position attributes
    /// </summary>
    public const string POSITION_ATTRIBUTE = "a_position";

    /// <summary>
    /// default name for normal attributes
    /// </summary>
    public const string NORMAL_ATTRIBUTE = "a_normal";

    /// <summary>
    /// default name for color attributes
    /// </summary>
    public const string COLOR_ATTRIBUTE = "a_color";

    /// <summary>
    /// default name for texcoords attributes, append texture unit number
    /// </summary>
    public const string TEXCOORD_ATTRIBUTE = "a_texCoord";

    /// <summary>
    /// Specifies the default shader language version to be used in the shader program.
    /// </summary>
    public const string GLSL_VERSION = "#version 450 core";

    // ========================================================================

    public bool       IsCompiled             { get; set; }
    public string     VertexShaderSource     { get; }
    public string     FragmentShaderSource   { get; }
    public int        ShaderProgramHandle    { get; private set; }
    public bool       Invalidated            { get; protected set; }
    public int        CombinedMatrixLocation { get; set; }
    public ShaderData ShaderData             { get; private set; }

    // ========================================================================

    /// <summary>
    /// Flag indicating whether attributes & uniforms must be present at all times.
    /// </summary>
    public static readonly bool Pedantic = true;

    private string _shaderLog = "";
    private int    _fragmentShaderHandle;
    private int    _vertexShaderHandle;

    // ========================================================================

    internal const int CACHED_NOT_FOUND = -1;
    internal const int NOT_CACHED       = -2;
    internal const int INVALID          = -1;

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
    /// <param name="vertexShaderSource"> the vertex shader </param>
    /// <param name="fragmentShaderSource"> the fragment shader </param>
    public void CompileShaders( string vertexShaderSource, string fragmentShaderSource )
    {
        // Vertex Shader
        _vertexShaderHandle = ( int )GL.CreateShader( ( int )ShaderType.VertexShader );
        GL.ShaderSource( _vertexShaderHandle, vertexShaderSource );
        GL.CompileShader( _vertexShaderHandle );
        CheckShaderLoadError( _vertexShaderHandle, ( int )ShaderType.VertexShader );

        // Fragment Shader
        _fragmentShaderHandle = ( int )GL.CreateShader( ( int )ShaderType.FragmentShader );
        GL.ShaderSource( _fragmentShaderHandle, fragmentShaderSource );
        GL.CompileShader( _fragmentShaderHandle );
        CheckShaderLoadError( _fragmentShaderHandle, ( int )ShaderType.FragmentShader );

        ShaderProgramHandle = ( int )GL.CreateProgram();
        GL.AttachShader( ShaderProgramHandle, _vertexShaderHandle );
        GL.AttachShader( ShaderProgramHandle, _fragmentShaderHandle );
        GL.LinkProgram( ShaderProgramHandle );

        // preserve handles for reference
        ShaderData = new ShaderData
        {
            Program        = ShaderProgramHandle,
            VertexShader   = _vertexShaderHandle,
            FragmentShader = _fragmentShaderHandle,
        };

        GL.DeleteShader( _vertexShaderHandle );
        GL.DeleteShader( _fragmentShaderHandle );

        IsCompiled = ShaderProgramHandle != -1;
        
        if ( !IsCompiled )
        {
            Logger.Debug( $"Shader program {vertexShaderSource} has not been compiled." );
        }
        
        var texLocation = GetUniformLocation( "u_texture" );
        
        if ( texLocation == INVALID )
        {
            Logger.Warning( $"Texture uniform not found in shader." );
            
            DebugShaderSources();
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="shader"></param>
    /// <param name="shaderType"></param>
    /// <exception cref="Exception"></exception>
    private unsafe void CheckShaderLoadError( int shader, int shaderType )
    {
        var status = stackalloc int[ 1 ];

        GL.GetShaderiv( shader, IGL.GL_COMPILE_STATUS, status );

        if ( *status == IGL.GL_FALSE )
        {
            var length = stackalloc int[ 1 ];

            GL.GetShaderiv( shader, IGL.GL_INFO_LOG_LENGTH, length );

            var infoLog = GL.GetShaderInfoLog( shader, *length );

            GL.DeleteShader( shader );

            _shaderLog += shaderType == IGL.GL_VERTEX_SHADER ? "Vertex shader\n" : "Fragment shader:\n";
            _shaderLog += infoLog;

            throw new Exception( $"Failed to loader shader {shader}: {infoLog}" );
        }
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
        var location = GL.GetUniformLocation( ShaderProgramHandle, name );

        if ( location == INVALID )
        {
            Logger.Debug( "***** Cannot perform action, Location is INVALID ( -1 ) *****" );

            return;
        }

        SetUniformMatrix( location, ref matrix, transpose );
    }

    /// <summary>
    /// Sets the values of a matrix uniform variable in a shader program.
    /// </summary>
    /// <param name="name">The name of the uniform variable in the shader program.</param>
    /// <param name="matrix">A reference to the matrix data to set.</param>
    /// <param name="transpose">
    /// Indicates whether the matrix should be transposed before being sent to the shader.
    /// </param>
    /// <typeparam name="T">The type of the matrix, which must be an unmanaged type.</typeparam>
    public virtual unsafe void SetUniformMatrix< T >( int name, ref T matrix, bool transpose = false )
        where T : unmanaged
    {
        const int MAT44 = 16;
        const int MAT33 = 9;
        const int MAT22 = 4;

        if ( name == INVALID )
        {
            Logger.Debug( $"***** Cannot perform action, Location is INVALID ( -1 ) *****" );

            return;
        }

        var matrixSize = Marshal.SizeOf< T >() / sizeof( float );

        fixed ( T* ptr = &matrix )
        {
            switch ( matrixSize )
            {
                case MAT44:
                    GL.UniformMatrix4fv( name, 1, transpose, ( float* )ptr );

                    break;

                case MAT33:
                    GL.UniformMatrix3fv( name, 1, transpose, ( float* )ptr );

                    break;

                case MAT22:
                    GL.UniformMatrix2fv( name, 1, transpose, ( float* )ptr );

                    break;

                default:
                    throw new ArgumentException( "Matrix must be 2x2, 3x3 or 4x4" );
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual int GetAttributeLocation( string name )
    {
        var location = GL.GetAttribLocation( ShaderProgramHandle, name );

        if ( location == INVALID )
        {
            Logger.Debug( $"***** WARNING, Location is INVALID ( -1 ) for {name} *****" );
        }

        return location;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual int GetUniformLocation( string name )
    {
        var location = GL.GetUniformLocation( ShaderProgramHandle, name );

        if ( location == INVALID )
        {
            Logger.Debug( $"***** WARNING, Location is INVALID ( -1 ) for {name} *****" );
        }

        return location;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public virtual void SetUniformi( string name, int value )
    {
        var location = GetUniformLocation( name );

        if ( location == INVALID )
        {
            Logger.Debug( $"***** Action cannot be performed, Location is INVALID ( -1 ) for {name} *****" );

            return;
        }

        GL.Uniform1i( location, value );
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public virtual void SetUniformf( string name, float value )
    {
        var location = GetUniformLocation( name );

        if ( location == INVALID )
        {
            Logger.Debug( $"***** Action cannot be performed, Location is INVALID ( -1 ) for {name} *****" );

            return;
        }

        GL.Uniform1f( location, value );
    }

    /// <summary>
    /// Sets the uniform matrix with the given name. The <see cref="ShaderProgram" />
    /// must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="matrix"> the matrix  </param>
    public virtual void SetUniformMatrix( string name, Matrix4 matrix )
    {
        SetUniformMatrix( name, matrix, false );
    }

    /// <summary>
    /// Sets the uniform matrix with the given name. The <see cref="ShaderProgram" />
    /// must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="matrix"> the matrix </param>
    /// <param name="transpose"> whether the matrix should be transposed  </param>
    public virtual void SetUniformMatrix( string name, Matrix4 matrix, bool transpose )
    {
        LogInvalidMatrix( matrix.Values );

        var location = GetUniformLocation( name );

        if ( location == INVALID )
        {
            Logger.Debug( $"***** Action cannot be performed, Location is INVALID ( -1 ) for {name} *****" );

            return;
        }

        SetUniformMatrix( location, matrix, transpose );
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="location"></param>
    /// <param name="matrix"></param>
    public virtual void SetUniformMatrix( int location, Matrix4 matrix )
    {
        LogInvalidMatrix( matrix.Values );

        if ( location == INVALID )
        {
            Logger.Debug( $"***** Cannot perform action, Location is INVALID ( -1 ) *****" );

            return;
        }

        SetUniformMatrix( location, matrix, false );
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="location"></param>
    /// <param name="matrix"></param>
    /// <param name="transpose"></param>
    public virtual void SetUniformMatrix( int location, Matrix4 matrix, bool transpose )
    {
        LogInvalidMatrix( matrix.Values );

        if ( location == INVALID )
        {
            Logger.Debug( $"***** Cannot perform action, Location is INVALID ( -1 ) *****" );

            return;
        }

        GL.UniformMatrix4fv( location, transpose, matrix.Val );
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="location"></param>
    public virtual void EnableVertexAttribute( int location )
    {
        if ( location == INVALID )
        {
            Logger.Debug( $"***** Cannot perform action, Location is INVALID ( -1 ) *****" );

            return;
        }

        GL.EnableVertexAttribArray( ( GLuint )location );
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="location"></param>
    /// <param name="size"></param>
    /// <param name="type"></param>
    /// <param name="normalize"></param>
    /// <param name="stride"></param>
    /// <param name="offset"></param>
    public virtual void SetVertexAttribute( int location, int size, int type, bool normalize, int stride, int offset )
    {
        if ( location == INVALID )
        {
            Logger.Debug( $"***** Action cannot be performed, Location is INVALID ( -1 ) *****" );

            return;
        }

        if ( size == 0 )
        {
            throw new GdxRuntimeException( "Size cannot be 0." );
        }

        GL.VertexAttribPointer( ( GLuint )location, size, type, normalize, stride, offset );
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="values"></param>
    public virtual void SetUniformMatrix4Fv( string name, params float[] values )
    {
        var location = GetUniformLocation( name );

        if ( location == INVALID )
        {
            Logger.Debug( $"***** Action cannot be performed, Location is INVALID ( -1 ) for {name} *****" );

            return;
        }

        SetUniformMatrix4Fv( location, values );
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="location"></param>
    /// <param name="values"></param>
    public virtual void SetUniformMatrix4Fv( int location, params float[] values )
    {
        if ( location == INVALID )
        {
            Logger.Debug( $"***** Action cannot be performed, Location is INVALID ( -1 ) *****" );

            return;
        }

        GL.UniformMatrix4fv( location, false, values );
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="location"></param>
    public virtual void DisableVertexAttribute( int location )
    {
        if ( location == INVALID )
        {
            Logger.Debug( $"***** Action cannot be performed, Location is INVALID ( -1 ) *****" );

            return;
        }

        GL.DisableVertexAttribArray( ( GLuint )location );
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    public virtual void DisableVertexAttribute( string name )
    {
        var location = GetAttributeLocation( name );

        if ( location == INVALID )
        {
            Logger.Debug( $"***** Action cannot be performed, Location is INVALID ( -1 ) for {name} *****" );

            return;
        }

        GL.DisableVertexAttribArray( ( GLuint )location );
    }

    /// <summary>
    /// Bind this shader to the renderer.
    /// </summary>
    public virtual void Bind()
    {
        if ( GL.IsProgram( ShaderProgramHandle ) )
        {
            GL.UseProgram( ShaderProgramHandle );
        }
    }

    /// <summary>
    /// Unbind this shader from the renderer.
    /// </summary>
    public virtual void Unbind()
    {
        GL.UseProgram( 0 );
    }

    // ========================================================================

    /// <summary>
    /// Logs details of an invalid matrix, including checks for null values, improper
    /// lengths, and invalid elements such as NaN or Infinity.
    /// </summary>
    /// <param name="matrix">The matrix to validate and log. Can be null.</param>
    public static void LogInvalidMatrix( float[]? matrix )
    {
        if ( !IsMatrixValid( matrix ) )
        {
            Logger.Debug( "Invalid Matrix Detected:" );

            if ( matrix == null )
            {
                Logger.Debug( "Matrix is null." );
            }

            if ( matrix?.Length != 16 )
            {
                Logger.Debug( $"Matrix length is {matrix?.Length}, expected 16." );
            }

            for ( var i = 0; i < matrix?.Length; i++ )
            {
                Logger.Debug( $"{i}: [{matrix[ i ]}]" );

                if ( float.IsNaN( matrix[ i ] ) )
                {
                    Logger.Debug( $"Element [{i}] is NaN." );
                }
                else if ( float.IsInfinity( matrix[ i ] ) )
                {
                    Logger.Debug( $"Element [{i}] is Infinity." );
                }
            }

            throw new GdxRuntimeException( "LogInvalidMatrix found Invalid Matrix." );
        }
    }

    /// <summary>
    /// Validates a 4x4 matrix, ensuring it has the correct length and no invalid
    /// values such as NaN or Infinity.
    /// </summary>
    /// <param name="matrix">The matrix to validate, represented as a float array.</param>
    /// <returns>True if the matrix is valid; otherwise, false.</returns>
    private static bool IsMatrixValid( float[]? matrix )
    {
        if ( matrix is not { Length: 16 } )
        {
            return false;
        }

        foreach ( var t in matrix )
        {
            if ( float.IsNaN( t ) || float.IsInfinity( t ) )
            {
                return false;
            }
        }

        return true;
    }

    // ========================================================================
    /// <summary>
    /// The log info for the shader compilation and program linking stage.
    /// The shader needs to be bound for this method to have an effect.
    /// </summary>
    public unsafe string ShaderLog
    {
        get
        {
            if ( IsCompiled )
            {
                var length = stackalloc int[ 1 ];

                GL.GetProgramiv( ShaderProgramHandle, IGL.GL_INFO_LOG_LENGTH, length );

                _shaderLog = GL.GetProgramInfoLog( ShaderProgramHandle, *length );
            }

            return _shaderLog;
        }
    }

    /// <summary>
    /// Write the source for the vertex and fragment shaders to console.
    /// Will only work in DEBUG builds.
    /// </summary>
    #if DEBUG
    internal void DebugShaderSources()
    {
        Logger.Debug( $"vertex shader: {GL.GetShaderSource( ShaderData.VertexShader )}" );
        Logger.Debug( $"fragment shader: {GL.GetShaderSource( ShaderData.FragmentShader )}" );
    }
    #endif

    // ========================================================================

    /// <summary>
    /// Disposes all resources associated with this shader.
    /// Must be called when the shader is no longer used.
    /// </summary>
    public void Dispose()
    {
        Unbind();
        GL.DeleteShader( _vertexShaderHandle );
        GL.DeleteShader( _fragmentShaderHandle );
        GL.DeleteProgram( ShaderProgramHandle );

        GC.SuppressFinalize( this );
    }
}