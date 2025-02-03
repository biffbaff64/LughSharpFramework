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
using LughSharp.Lugh.Maths;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Buffers;
using LughSharp.Lugh.Utils.Collections;

using Buffer = LughSharp.Lugh.Utils.Buffers.Buffer;

namespace LughSharp.Lugh.Graphics.GLUtils;

/// <summary>
/// A shader program encapsulates a vertex and fragment shader pairlinked to form a shader program.
/// After construction a ShaderProgram can be used to draw <see cref="Mesh"/>. To make the GPU use
/// a specific ShaderProgram the programs <see cref="Bind()"/> method must be used which effectively
/// binds the program. When a ShaderProgram is bound one can set uniforms, vertex attributes and
/// attributes as needed via the respective methods.
/// <para>
/// A ShaderProgram must be disposed via a call to <see cref="Dispose()"/> when it is no longer needed
/// </para>
/// <para>
/// ShaderPrograms are managed. In case the OpenGL context is lost all shaders get invalidated and
/// have to be reloaded. Managed ShaderPrograms are automatically reloaded when the OpenGL context is
/// recreated so you don't have to do this manually.
/// </para>
/// </summary>
[PublicAPI]
public class ManagedShaderProgram : ShaderProgram
{
    public string[] Attributes { get; private set; } = null!;
    public string[] Uniforms   { get; private set; } = null!;

    // ========================================================================
    // ========================================================================

    private static readonly Dictionary< IApplication, List< ManagedShaderProgram >? > _availableShaders = new();

    private readonly Dictionary< string, int > _attributes     = new();
    private readonly Dictionary< string, int > _attributeSizes = new();
    private readonly Dictionary< string, int > _attributeTypes = new();
    private readonly Dictionary< string, int > _uniforms       = new();
    private readonly Dictionary< string, int > _uniformSizes   = new();
    private readonly Dictionary< string, int > _uniformTypes   = new();

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Constructs a new ShaderProgram from the supplied shaders and immediately compiles it.
    /// </summary>
    /// <param name="vertexShader"> the vertex shader </param>
    /// <param name="fragmentShader"> the fragment shader </param>
    public ManagedShaderProgram( string vertexShader, string fragmentShader )
        : base( vertexShader, fragmentShader )
    {
        FetchAttributes();
        FetchUniforms();

        AddManagedShader( GdxApi.App, this );
    }

    /// <summary>
    /// </summary>
    /// <param name="vertexShader"> the vertex shader </param>
    /// <param name="fragmentShader"> the fragment shader </param>
    public ManagedShaderProgram( FileSystemInfo vertexShader, FileSystemInfo fragmentShader )
        : base( vertexShader, fragmentShader )
    {
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    public static string ManagedStatus
    {
        get
        {
            var builder = new StringBuilder( "Managed shaders/app: { " );

            foreach ( var app in _availableShaders.Keys )
            {
                builder.Append( _availableShaders[ app ]?.Count );
                builder.Append( ' ' );
            }

            builder.Append( '}' );

            return builder.ToString();
        }
    }

    /// <summary>
    /// Returns the number of Managed Shaders in the managed array.
    /// </summary>
    public static int NumManagedShaderPrograms => _availableShaders[ GdxApi.App ]!.Count;

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="name"> the name of the attribute </param>
    /// <returns> whether the attribute is available in the shader  </returns>
    public bool HasAttribute( string name )
    {
        return _attributes.ContainsKey( name );
    }

    /// <summary>
    /// </summary>
    /// <param name="name"> the name of the attribute </param>
    /// <returns>
    /// the type of the attribute, one of <see cref="IGL.GL_FLOAT"/>,
    /// <see cref="IGL.GL_FLOAT_VEC2"/> etc.
    /// </returns>
    public int GetAttributeType( string name )
    {
        return _attributeTypes.GetValueOrDefault( name, 0 );
    }

    /// <summary>
    /// </summary>
    /// <param name="name"> the name of the attribute </param>
    /// <returns> the location of the attribute or -1.  </returns>
    public override int GetAttributeLocation( string name )
    {
        return _attributes.GetValueOrDefault( name, 0 );
    }

    /// <summary>
    /// </summary>
    /// <param name="name"> the name of the attribute </param>
    /// <returns> the size of the attribute or 0.</returns>
    public int GetAttributeSize( string name )
    {
        return _attributeSizes.GetValueOrDefault( name, 0 );
    }
    
    // ========================================================================
    
    /// <summary>
    /// </summary>
    private unsafe void FetchUniforms()
    {
        var numUniforms = stackalloc int[ 1 ];

        GdxApi.Bindings.GetProgramiv( ( uint )Handle, IGL.GL_ACTIVE_UNIFORMS, numUniforms );

        Uniforms = new string[ *numUniforms ];

        for ( uint i = 0; i < *numUniforms; i++ )
        {
            var name = GdxApi.Bindings.GetActiveUniform( ( uint )Handle,
                                                         i,
                                                         IGL.GL_ACTIVE_UNIFORM_MAX_LENGTH,
                                                         out var size,
                                                         out var type );

            var location = GdxApi.Bindings.GetUniformLocation( ( uint )Handle, name );

            _uniforms[ name ]     = location;
            _uniformSizes[ name ] = size;
            _uniformTypes[ name ] = type;
            Uniforms[ i ]         = name;
        }
    }

    /// <summary>
    /// </summary>
    private unsafe void FetchAttributes()
    {
        var numAttributes = stackalloc int[ 1 ];

        GdxApi.Bindings.GetProgramiv( ( uint )Handle, IGL.GL_ACTIVE_ATTRIBUTES, numAttributes );

        Attributes = new string[ *numAttributes ];

        for ( var index = 0; index < *numAttributes; index++ )
        {
            var name = GdxApi.Bindings.GetActiveAttrib( ( uint )Handle,
                                                        ( uint )index,
                                                        IGL.GL_ACTIVE_ATTRIBUTE_MAX_LENGTH,
                                                        out var size,
                                                        out var type );

            var location = GdxApi.Bindings.GetAttribLocation( ( uint )Handle, name );

            _attributes[ name ]     = location;
            _attributeSizes[ name ] = size;
            _attributeTypes[ name ] = type;

            Attributes[ index ] = name;
        }
    }

    // ========================================================================
    
    /// <summary>
    /// </summary>
    /// <param name="app"></param>
    /// <param name="shaderProgram"></param>
    private void AddManagedShader( IApplication app, ManagedShaderProgram shaderProgram )
    {
        List< ManagedShaderProgram >? managedResources;

        if ( !_availableShaders.ContainsKey( app ) || ( _availableShaders[ app ] == null ) )
        {
            managedResources = [ ];
        }
        else
        {
            managedResources = _availableShaders[ app ];
        }

        managedResources?.Add( shaderProgram );

        _availableShaders.Put( app, managedResources );
    }
    
    // ========================================================================
    
    /// <summary>
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <returns> the location of the uniform or -1.</returns>
    public int GetUniformLocation( string name )
    {
        return _uniforms.GetValueOrDefault( name, -1 );
    }

    /// <summary>
    /// </summary>
    /// <param name="name">The name of the uniform</param>
    /// <returns> the size of the uniform or 0.</returns>
    public int GetUniformSize( string name )
    {
        return _uniformSizes.GetValueOrDefault( name, 0 );
    }

    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override int FetchUniformLocation( string name )
    {
        return FetchUniformLocation( name, Pedantic );
    }

    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override int FetchAttributeLocation( string name )
    {
        // -2 == not yet cached
        // -1 == cached but not found
        int location;

        if ( ( location = _attributes.Get( name, NOT_CACHED ) ) == NOT_CACHED )
        {
            location            = GdxApi.Bindings.GetAttribLocation( ( uint )Handle, name );
            _attributes[ name ] = location;
        }

        return location;
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    public override void DisableVertexAttribute( int location )
    {
        CheckManaged();
        GdxApi.Bindings.DisableVertexAttribArray( ( uint )location );
    }

    /// <summary>
    /// Disables the vertex attribute with the given name
    /// </summary>
    /// <param name="name"> the vertex attribute name  </param>
    public override void DisableVertexAttribute( string name )
    {
        CheckManaged();

        var location = FetchAttributeLocation( name );

        if ( location == -1 )
        {
            return;
        }

        GdxApi.Bindings.DisableVertexAttribArray( ( uint ) location );
    }

    /// <summary>
    /// Enables the vertex attribute with the given name
    /// </summary>
    /// <param name="name"> the vertex attribute name  </param>
    public void EnableVertexAttribute( string name )
    {
        CheckManaged();

        var location = FetchAttributeLocation( name );

        if ( location == -1 )
        {
            return;
        }

        GdxApi.Bindings.EnableVertexAttribArray( ( uint )location );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    public override void EnableVertexAttribute( int location )
    {
        CheckManaged();
        GdxApi.Bindings.EnableVertexAttribArray( ( uint )location );
    }

    /// <summary>
    /// </summary>
    /// <param name="app"></param>
    public static void ClearAllShaderPrograms( IApplication app )
    {
        _availableShaders.Remove( app );
    }

    /// <summary>
    /// Invalidates all shaders so the next time they are used new handles are generated.
    /// </summary>
    /// <param name="app">  </param>
    public static void InvalidateAllShaderPrograms( IApplication app )
    {
        List< ManagedShaderProgram >? shaderArray;

        if ( !_availableShaders.TryGetValue( app, out var value ) || ( value == null ) )
        {
            shaderArray = [ ];
        }
        else
        {
            shaderArray = value;
        }

        foreach ( var sp in shaderArray )
        {
            sp.Invalidated = true;
            sp.CheckManaged();
        }
    }
    
    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="matrix"></param>
    /// <param name="transpose"></param>
    /// <typeparam name="T"></typeparam>
    public override void SetUniformMatrix< T >( string name, ref T matrix, bool transpose = false )
    {
        var location = GetUniformLocation( name );

        SetUniformMatrix( location, ref matrix, transpose );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="matrix"></param>
    /// <param name="transpose"></param>
    /// <typeparam name="T"></typeparam>
    public override void SetUniformMatrix< T >( int location, ref T matrix, bool transpose = false )
    {
        CheckManaged();
        
        base.SetUniformMatrix< T >( location, ref matrix, transpose );
    }
    
    /// <summary>
    /// Sets the uniform matrix with the given name. The <see cref="ShaderProgram"/>
    /// must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="matrix"> the matrix  </param>
    public override void SetUniformMatrix( string name, Matrix4 matrix )
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
    public override void SetUniformMatrix( string name, Matrix4 matrix, bool transpose )
    {
        SetUniformMatrix( FetchUniformLocation( name ), matrix, transpose );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="matrix"></param>
    public override void SetUniformMatrix( int location, Matrix4 matrix )
    {
        SetUniformMatrix( location, matrix, false );
    }

    /// <summary>
    /// Sets the uniform matrix with the given name.
    /// The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="matrix"> the matrix  </param>
    public void SetUniformMatrix( string name, Matrix3 matrix )
    {
        SetUniformMatrix( name, matrix, false );
    }

    /// <summary>
    /// Sets the uniform matrix with the given name.
    /// The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="matrix"> the matrix </param>
    /// <param name="transpose"> whether the uniform matrix should be transposed  </param>
    public void SetUniformMatrix( string name, Matrix3 matrix, bool transpose )
    {
        SetUniformMatrix( FetchUniformLocation( name ), matrix, transpose );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="matrix"></param>
    public void SetUniformMatrix( int location, Matrix3 matrix )
    {
        SetUniformMatrix( location, matrix, false );
    }

    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="values"></param>
    public override void SetUniformMatrix4Fv( string name, params float[] values )
    {
        SetUniformMatrix4Fv( FetchUniformLocation( name ), values );
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="pedant"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public int FetchUniformLocation( string name, bool pedant )
    {
        // -2 == not yet cached
        // -1 == cached but not found
        int location;

        if ( ( location = _uniforms.Get( name, NOT_CACHED ) ) == NOT_CACHED )
        {
            location = GdxApi.Bindings.GetUniformLocation( ( uint )Handle, name );

            if ( ( location == CACHED_NOT_FOUND ) && pedant )
            {
                if ( IsCompiled )
                {
                    throw new ArgumentException( "No uniform with name '" + name + "' in shader" );
                }

                throw new InvalidOperationException( "An attempted fetch uniform from uncompiled shader \n" + ShaderLog );
            }

            _uniforms[ name ] = location;
        }

        return location;
    }

    /// <summary>
    /// Sets the vertex attribute with the given name. The <see cref="ShaderProgram"/> must
    /// be bound for this to work.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="size"> The number of components, must be >= 1 and &lt;= 4. </param>
    /// <param name="type">
    /// The type, must be one of IGL.GL_BYTE, IGL.GL_UNSIGNED_BYTE, IGL.GL_SHORT, IGL.GL_UNSIGNED_SHORT,
    /// IGL.GL_FIXED, or IGL.GL_FLOAT.
    /// </param>
    /// <param name="normalize"> Whether fixed point data should be normalized. Will not work on the desktop. </param>
    /// <param name="stride">The stride in bytes between successive attributes.</param>
    /// <param name="buffer">The buffer containing the vertex attributes.</param>
    public unsafe void SetVertexAttribute( string name, int size, int type, bool normalize, int stride, Buffer buffer )
    {
        CheckManaged();

        var location = FetchAttributeLocation( name );

        if ( location == -1 )
        {
            return;
        }

        fixed ( void* ptr = &buffer.BackingArray()[ 0 ] )
        {
            GdxApi.Bindings.VertexAttribPointer( ( uint ) location, size, type, normalize, stride, ptr );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="size"></param>
    /// <param name="type"></param>
    /// <param name="normalize"></param>
    /// <param name="stride"></param>
    /// <param name="buffer"></param>
    public unsafe void SetVertexAttribute( int location, int size, int type, bool normalize, int stride, Buffer buffer )
    {
        CheckManaged();

        fixed ( void* ptr = &buffer.BackingArray()[ 0 ] )
        {
            GdxApi.Bindings.VertexAttribPointer( ( uint ) location, size, type, normalize, stride, ptr );
        }
    }

    /// <summary>
    /// Sets the vertex attribute with the given name. The <see cref="ShaderProgram"/> must
    /// be bound for this to work.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="size">The number of components, must be >= 1 and &lt;= 4.</param>
    /// <param name="type">
    /// The type, must be one of IGL.GL_Byte, IGL.GL_Unsigned_Byte, IGL.GL_Short, IGL.GL_Unsigned_Short,
    /// IGL.GL_Fixed, or IGL.GL_Float.
    /// <para>GL_Fixed will not work on the desktop.</para>
    /// </param>
    /// <param name="normalize"> Whether fixed point data should be normalized. Will not work on the desktop.</param>
    /// <param name="stride">The stride in bytes between successive attributes.</param>
    /// <param name="offset"> Byte offset into the vertex buffer object bound to IGL.GL_Array_Buffer. </param>
    public void SetVertexAttribute( string name, int size, int type, bool normalize, int stride, int offset )
    {
        CheckManaged();

        var location = FetchAttributeLocation( name );

        if ( location == -1 )
        {
            return;
        }

        GdxApi.Bindings.VertexAttribPointer( ( uint ) location, size, type, normalize, stride, ( uint ) offset );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="size"></param>
    /// <param name="type"></param>
    /// <param name="normalize"></param>
    /// <param name="stride"></param>
    /// <param name="offset"></param>
    public override void SetVertexAttribute( int location, int size, int type, bool normalize, int stride, int offset )
    {
        CheckManaged();
        GdxApi.Bindings.VertexAttribPointer( ( uint ) location, size, type, normalize, stride, ( uint ) offset );
    }

    /// <summary>
    /// </summary>
    /// <param name="name"> the name of the uniform.</param>
    /// <returns> whether the uniform is available in the shader.</returns>
    public bool HasUniform( string name )
    {
        return _uniforms.ContainsKey( name );
    }

    /// <summary>
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <returns>
    /// the type of the uniform, one of <see cref="IGL.GL_FLOAT"/>,
    /// <see cref="IGL.GL_FLOAT_VEC2"/> etc.
    /// </returns>
    public int GetUniformType( string name )
    {
        return _uniformTypes.GetValueOrDefault( name, 0 );
    }

    /// <summary>
    /// Sets the uniform with the given name. The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="value"> the value  </param>
    public override void SetUniformi( string name, int value )
    {
        CheckManaged();

        GdxApi.Bindings.Uniform1i( FetchUniformLocation( name ), value );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="value"></param>
    public void SetUniformi( int location, int value )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform1i( location, value );
    }

    /// <summary>
    /// Sets the uniform with the given name. The <see cref="ShaderProgram"/>
    /// must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="count"> the first value </param>
    /// <param name="value"> the second value </param>
    public void SetUniformi( string name, int count, int value )
    {
        CheckManaged();

        GdxApi.Bindings.Uniform2i( FetchUniformLocation( name ), count, value );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="count"></param>
    /// <param name="value"></param>
    public void SetUniformi( int location, int count, int value )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform2i( location, count, value );
    }

    /// <summary>
    /// Sets the uniform with the given name.
    /// The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="value1"> the first value </param>
    /// <param name="value2"> the second value </param>
    /// <param name="value3"> the third value </param>
    public void SetUniformi( string name, int value1, int value2, int value3 )
    {
        CheckManaged();

        GdxApi.Bindings.Uniform3i( FetchUniformLocation( name ), value1, value2, value3 );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public void SetUniformi( int location, int x, int y, int z )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform3i( location, x, y, z );
    }

    /// <summary>
    /// Sets the uniform with the given name.
    /// The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="value"> the value  </param>
    public override void SetUniformf( string name, float value )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform1f( FetchUniformLocation( name ), value );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="value"></param>
    public void SetUniformf( int location, float value )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform1f( location, value );
    }

    /// <summary>
    /// Sets the uniform with the given name.
    /// The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="value1"> the first value </param>
    /// <param name="value2"> the second value  </param>
    public void SetUniformf( string name, float value1, float value2 )
    {
        CheckManaged();

        GdxApi.Bindings.Uniform2f( FetchUniformLocation( name ), value1, value2 );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    public void SetUniformf( int location, int value1, int value2 )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform2f( location, value1, value2 );
    }

    /// <summary>
    /// Sets the uniform with the given name.
    /// The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="value1"> the first value </param>
    /// <param name="value2"> the second value </param>
    /// <param name="value3"> the third value  </param>
    public void SetUniformf( string name, float value1, float value2, float value3 )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform3f( FetchUniformLocation( name ), value1, value2, value3 );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <param name="value3"></param>
    public void SetUniformf( int location, float value1, float value2, float value3 )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform3f( location, value1, value2, value3 );
    }

    /// <summary>
    /// Sets the uniform with the given name. The <see cref="ShaderProgram"/>
    /// must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="w"></param>
    public void SetUniformf( string name, float x, float y, float z, float w )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform4f( FetchUniformLocation( name ), x, y, z, w );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="w"></param>
    public void SetUniformf( int location, float x, float y, float z, float w )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform4f( location, x, y, z, w );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="matrix"></param>
    /// <param name="transpose"></param>
    public override void SetUniformMatrix( int location, Matrix4 matrix, bool transpose )
    {
        CheckManaged();
        GdxApi.Bindings.UniformMatrix4fv( location, transpose, matrix.Val );
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="matrix"></param>
    /// <param name="transpose"></param>
    public void SetUniformMatrix( int location, Matrix3 matrix, bool transpose )
    {
        CheckManaged();
        GdxApi.Bindings.UniformMatrix3fv( location, transpose, matrix.Val );
    }

    /// <summary>
    /// Sets an array of uniform matrices with the given name.
    /// The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name">the name of the uniform </param>
    /// <param name="buffer">buffer containing the matrix data </param>
    /// <param name="count"></param>
    /// <param name="transpose">whether the uniform matrix should be transposed  </param>
    public unsafe void SetUniformMatrix3Fv( string name, FloatBuffer buffer, int count, bool transpose )
    {
        CheckManaged();
        buffer.Position = 0;

        fixed ( float* ptr = &( buffer ).BackingArray()[ 0 ] )
        {
            GdxApi.Bindings.UniformMatrix3fv( FetchUniformLocation( name ), count, transpose, ptr );
        }
    }

    /// <summary>
    /// Sets an array of uniform matrices with the given name. The <see cref="ShaderProgram"/>
    /// must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="buffer"> buffer containing the matrix data </param>
    /// <param name="count"></param>
    /// <param name="transpose"> whether the uniform matrix should be transposed  </param>
    public unsafe void SetUniformMatrix4Fv( string name, FloatBuffer buffer, int count, bool transpose )
    {
        ArgumentNullException.ThrowIfNull( buffer );

        CheckManaged();
        buffer.Position = 0;

        fixed ( float* ptr = &buffer.BackingArray()[ 0 ] )
        {
            GdxApi.Bindings.UniformMatrix4fv( FetchUniformLocation( name ), count, transpose, ptr );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="location"></param>
    /// <param name="values"></param>
    public override void SetUniformMatrix4Fv( int location, params float[] values )
    {
        Logger.Debug( $"location: {location}" );
        Logger.Debug( $"values: {string.Join( ", ", values )}" );

        CheckManaged();
        GdxApi.Bindings.UniformMatrix4fv( location, false, values );
    }

    /// <summary>
    /// Sets the uniform with the given name. The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="values"> x and y as the first and second values respectively  </param>
    public void SetUniformf( string name, Vector2 values )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform2f( FetchUniformLocation( name ), values.X, values.Y );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="values"></param>
    public void SetUniformf( int location, Vector2 values )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform2f( location, values.X, values.Y );
    }

    /// <summary>
    /// Sets the uniform with the given name. The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="values"> x, y and z as the first, second and third values respectively  </param>
    public void SetUniformf( string name, Vector3 values )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform3f( FetchUniformLocation( name ), values.X, values.Y, values.Z );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="values"></param>
    public void SetUniformf( int location, Vector3 values )
    {
        CheckManaged();
        GdxApi.Bindings.Uniform3f( location, values.X, values.Y, values.Z );
    }

    /// <summary>
    /// Sets the uniform with the given name. The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name"> the name of the uniform </param>
    /// <param name="values"> r, g, b and a as the first through fourth values respectively  </param>
    public void SetUniformf( string name, Color values )
    {
        GdxApi.Bindings.Uniform4f( FetchUniformLocation( name ), values.R, values.G, values.B, values.A );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="values"></param>
    public void SetUniformf( int location, Color values )
    {
        GdxApi.Bindings.Uniform4f( location, values.R, values.G, values.B, values.A );
    }

    // ========================================================================

    /// <summary>
    /// Bind this shader to the renderer.
    /// </summary>
    public override void Bind()
    {
        CheckManaged();
        GdxApi.Bindings.UseProgram( ( uint )Handle );
    }

    /// <summary>
    /// Unbind this shader from the renderer.
    /// </summary>
    public override void Unbind()
    {
        CheckManaged();
        GdxApi.Bindings.UseProgram( 0 );
    }
    
    private void CheckManaged()
    {
        if ( Invalidated )
        {
            CompileShaders( VertexShaderSource, FragmentShaderSource );
            Invalidated = false;
        }
    }

    // ========================================================================

    /// <summary>
    /// Disposes all resources associated with this shader.
    /// Must be called when the shader is no longer used.
    /// </summary>
    public new void Dispose()
    {
        base.Dispose();

        _availableShaders[ GdxApi.App ]?.Remove( this );
    }
}