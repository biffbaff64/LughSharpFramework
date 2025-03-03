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
using LughSharp.Lugh.Utils.Exceptions;

using GLuint = uint;

namespace LughSharp.Lugh.Graphics.GLUtils;

[PublicAPI]
public struct ShaderData
{
    public int Program;
    public int VertexShader;
    public int FragmentShader;
}

[PublicAPI]
public class ShaderProgram : IDisposable
{
    internal const int CACHED_NOT_FOUND = -1;
    internal const int NOT_CACHED       = -2;

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

    private int    _fragmentShaderHandle;
    private string _shaderLog = "";
    private int    _vertexShaderHandle;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="vertexShader"></param>
    /// <param name="fragmentShader"></param>
    public ShaderProgram( string vertexShader, string fragmentShader )
    {
        Logger.Checkpoint();

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

                GdxApi.Bindings.GetProgramiv( ShaderProgramHandle, IGL.GL_INFO_LOG_LENGTH, length );

                _shaderLog = GdxApi.Bindings.GetProgramInfoLog( ShaderProgramHandle, *length );
            }

            return _shaderLog;
        }
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
        _vertexShaderHandle = ( int )GdxApi.Bindings.CreateShader( ( int )ShaderType.VertexShader );
        GdxApi.Bindings.ShaderSource( _vertexShaderHandle, vertexShaderSource );
        GdxApi.Bindings.CompileShader( _vertexShaderHandle );
        CheckShaderLoadError( _vertexShaderHandle, ( int )ShaderType.VertexShader );

        // Fragment Shader
        _fragmentShaderHandle = ( int )GdxApi.Bindings.CreateShader( ( int )ShaderType.FragmentShader );
        GdxApi.Bindings.ShaderSource( _fragmentShaderHandle, fragmentShaderSource );
        GdxApi.Bindings.CompileShader( _fragmentShaderHandle );
        CheckShaderLoadError( _fragmentShaderHandle, ( int )ShaderType.FragmentShader );

        ShaderProgramHandle = ( int )GdxApi.Bindings.CreateProgram();

        GdxApi.Bindings.AttachShader( ShaderProgramHandle, _vertexShaderHandle );
        GdxApi.Bindings.AttachShader( ShaderProgramHandle, _fragmentShaderHandle );
        GdxApi.Bindings.LinkProgram( ShaderProgramHandle );

        // preserve handles for reference
        ShaderData = new ShaderData
        {
            Program        = ShaderProgramHandle,
            VertexShader   = _vertexShaderHandle,
            FragmentShader = _fragmentShaderHandle,
        };

        GdxApi.Bindings.DeleteShader( _vertexShaderHandle );
        GdxApi.Bindings.DeleteShader( _fragmentShaderHandle );

        IsCompiled = ShaderProgramHandle != -1;
    }

    /// <summary>
    /// Write the source for the vertex and fragment shaders to console.
    /// Will only work in DEBUG builds.
    /// </summary>
    internal void DebugShaderSources()
    {
        #if DEBUG
        Logger.Debug( $"vertex shader: {GL.GetShaderSource( ShaderData.VertexShader )}" );
        Logger.Debug( $"fragment shader: {GL.GetShaderSource( ShaderData.FragmentShader )}" );
        #endif
    }

    /// <summary>
    /// </summary>
    /// <param name="shader"></param>
    /// <param name="shaderType"></param>
    /// <exception cref="Exception"></exception>
    private unsafe void CheckShaderLoadError( int shader, int shaderType )
    {
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
        var location = GdxApi.Bindings.GetUniformLocation( ShaderProgramHandle, name );

        if ( location == -1 )
        {
            Logger.Debug( $"***** Cannot perform action, Location is -1 *****" );

            return;
        }

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

        if ( location == -1 )
        {
            Logger.Debug( $"***** Cannot perform action, Location is -1 *****" );

            return;
        }

        var matrixSize = Marshal.SizeOf< T >() / sizeof( float );

        fixed ( T* ptr = &matrix )
        {
            switch ( matrixSize )
            {
                case MAT4X4:
                    GdxApi.Bindings.UniformMatrix4fv( location, 1, transpose, ( float* )ptr );

                    break;

                case MAT3X3:
                    GdxApi.Bindings.UniformMatrix3fv( location, 1, transpose, ( float* )ptr );

                    break;

                case MAT2X2:
                    GdxApi.Bindings.UniformMatrix2fv( location, 1, transpose, ( float* )ptr );

                    break;

                default:
                    throw new System.ArgumentException( "Matrix must be 2x2, 3x3 or 4x4" );
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
        var location = GdxApi.Bindings.GetAttribLocation( ShaderProgramHandle, name );

        if ( location == -1 )
        {
            Logger.Debug( $"***** WARNING, Location is -1 for {name} *****" );
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
        var location = FetchUniformLocation( name );

        if ( location == -1 )
        {
            Logger.Debug( $"***** Action cannot be performed, Location is -1 for {name} *****" );

            return;
        }

        GdxApi.Bindings.Uniform1i( location, value );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public virtual void SetUniformf( string name, float value )
    {
        var location = FetchUniformLocation( name );

        if ( location == -1 )
        {
            Logger.Debug( $"***** Action cannot be performed, Location is -1 for {name} *****" );

            return;
        }

        GdxApi.Bindings.Uniform1f( location, value );
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
        var location = FetchUniformLocation( name );

        if ( location == -1 )
        {
            Logger.Debug( $"***** Action cannot be performed, Location is -1 for {name} *****" );

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
        if ( location == -1 )
        {
            Logger.Debug( $"***** Cannot perform action, Location is -1 *****" );

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
        if ( location == -1 )
        {
            Logger.Debug( $"***** Cannot perform action, Location is -1 *****" );

            return;
        }

        GdxApi.Bindings.UniformMatrix4fv( location, transpose, matrix.Val );
    }

    public static void LogInvalidMatrix( float[]? matrix )
    {
        Logger.Checkpoint();

        var isValid = IsMatrixValid( matrix );

        if ( !isValid )
        {
            Logger.Debug( "Invalid Matrix Detected:" );

            if ( matrix == null )
            {
                Logger.Debug( "Matrix is null." );

                return;
            }

            if ( matrix.Length != 16 )
            {
                Logger.Debug( $"Matrix length is {matrix.Length}, expected 16." );

                return;
            }
        }

        for ( var i = 0; i < matrix?.Length; i++ )
        {
            Logger.Debug( $"{i}: [{matrix[i]}]" );

            if ( float.IsNaN( matrix[ i ] ) )
            {
                Logger.Debug( $"Element [{i}] is NaN." );
            }
            else if ( float.IsInfinity( matrix[ i ] ) )
            {
                Logger.Debug( $"Element [{i}] is Infinity." );
            }
        }
    }

    private static bool IsMatrixValid( float[]? matrix )
    {
        if ( matrix is not { Length: 16 } )
        {
            return false; // Invalid matrix
        }

        foreach ( var t in matrix )
        {
            if ( float.IsNaN( t ) || float.IsInfinity( t ) )
            {
                return false; // Found NaN or Infinity
            }
        }

        Logger.Debug( "Matrix is valid." );

        return true; // Matrix is valid
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual int FetchUniformLocation( string name )
    {
        var location = GdxApi.Bindings.GetUniformLocation( ShaderProgramHandle, name );

        if ( location == -1 )
        {
            Logger.Debug( $"***** Location is -1 for {name} *****" );
        }

        return location;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual int FetchAttributeLocation( string name )
    {
        var location = GdxApi.Bindings.GetAttribLocation( ShaderProgramHandle, name );

        if ( location == -1 )
        {
            Logger.Debug( $"***** Location is -1 for {name} *****" );
        }

        return location;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    public virtual void EnableVertexAttribute( int location )
    {
        if ( location == -1 )
        {
            Logger.Debug( $"***** Cannot perform action, Location is -1 *****" );

            return;
        }

        GdxApi.Bindings.EnableVertexAttribArray( ( GLuint )location );
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
        Logger.Debug( $"location: {location}, size: {size}, type: {type}, normalize: {normalize}, stride: {stride}, offset: {offset}" );
        
        if ( location == -1 )
        {
            Logger.Debug( $"***** Action cannot be performed, Location is -1 *****" );

            return;
        }

        if ( size == 0 ) throw new GdxRuntimeException( "Size cannot be 0." );  
        
        GdxApi.Bindings.VertexAttribPointer( ( GLuint )location, size, type, normalize, stride, offset );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="values"></param>
    public virtual void SetUniformMatrix4Fv( string name, params float[] values )
    {
        var location = FetchUniformLocation( name );

        if ( location == -1 )
        {
            Logger.Debug( $"***** Action cannot be performed, Location is -1 for {name} *****" );

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
        if ( location == -1 )
        {
            Logger.Debug( $"***** Action cannot be performed, Location is -1 *****" );

            return;
        }

        GdxApi.Bindings.UniformMatrix4fv( location, false, values );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    public virtual void DisableVertexAttribute( int location )
    {
        if ( location == -1 )
        {
            Logger.Debug( $"***** Action cannot be performed, Location is -1 *****" );

            return;
        }

        GdxApi.Bindings.DisableVertexAttribArray( ( GLuint )location );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    public virtual void DisableVertexAttribute( string name )
    {
        var location = FetchAttributeLocation( name );

        if ( location == -1 )
        {
            Logger.Debug( $"***** Action cannot be performed, Location is -1 for {name} *****" );

            return;
        }

        GdxApi.Bindings.DisableVertexAttribArray( ( GLuint )location );
    }

    /// <summary>
    /// Bind this shader to the renderer.
    /// </summary>
    public virtual void Bind()
    {
        if ( GdxApi.Bindings.IsProgram( ShaderProgramHandle ) )
        {
            GdxApi.Bindings.UseProgram( ShaderProgramHandle );
        }
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
    /// Disposes all resources associated with this shader.
    /// Must be called when the shader is no longer used.
    /// </summary>
    public void Dispose()
    {
        Unbind();
        GdxApi.Bindings.DeleteShader( _vertexShaderHandle );
        GdxApi.Bindings.DeleteShader( _fragmentShaderHandle );
        GdxApi.Bindings.DeleteProgram( ShaderProgramHandle );

        GC.SuppressFinalize( this );
    }
}