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
using LughSharp.Lugh.Utils.Collections;

using Buffer = LughSharp.Lugh.Utils.Buffers.Buffer;

namespace LughSharp.Lugh.Graphics.GLUtils;

/// <summary>
/// A shader program encapsulates a vertex and fragment shader pairlinked to
/// form a shader program. After construction a ShaderProgram can be used to
/// draw <see cref="Mesh"/>. To make the GPU use a specific ShaderProgram the
/// programs <see cref="Bind()"/> method must be used which effectively binds
/// the program. When a ShaderProgram is bound one can set uniforms, vertex
/// attributes and attributes as needed via the respective methods.
/// <para>
/// A ShaderProgram must be disposed via a call to <see cref="Dispose()"/>
/// when it is no longer needed
/// </para>
/// <para>
/// ShaderPrograms are managed. In case the OpenGL context is lost all shaders
/// get invalidated and have to be reloaded. Managed ShaderPrograms are
/// automatically reloaded when the OpenGL context is recreated so you don't
/// have to do this manually.
/// </para>
/// </summary>
[PublicAPI]
public partial class ShaderProgram
{
    #region default attribute names

    public const string POSITION_ATTRIBUTE   = "a_position";
    public const string NORMAL_ATTRIBUTE     = "a_normal";
    public const string COLOR_ATTRIBUTE      = "a_colorPacked";
    public const string TEXCOORD_ATTRIBUTE   = "a_texCoord";
    public const string TANGENT_ATTRIBUTE    = "a_tangent";
    public const string BINORMAL_ATTRIBUTE   = "a_binormal";
    public const string BONEWEIGHT_ATTRIBUTE = "a_boneWeight";

    #endregion default attribute names

    // ========================================================================

    private const int CACHED_NOT_FOUND = -1;
    private const int NOT_CACHED       = -2;
    
    // ========================================================================

    public bool     IsCompiled           { get; set; }
    public string[] Attributes           { get; private set; } = null!;
    public string[] Uniforms             { get; private set; } = null!;
    public string   VertexShaderSource   { get; }
    public string   FragmentShaderSource { get; }
    public int      Handle               { get; private set; }

    // ========================================================================
    /// <summary>
    /// flag indicating whether attributes & uniforms must be present at all times.
    /// </summary>
    public static readonly bool Pedantic = true;

    /// <summary>
    /// code that is always added to the vertex shader code, typically used to inject a #version
    /// line. Note that this is added as-is, you should include a newline (`\n`) if needed.
    /// </summary>
    public static readonly string PrependVertexCode = "#version 460 core\n";

    /// <summary>
    /// code that is always added to every fragment shader code, typically used to inject a #version
    /// line. Note that this is added as-is, you should include a newline (`\n`) if needed.
    /// </summary>
    public static readonly string PrependFragmentCode = "#version 460 core\n";

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// the list of currently available shaders
    /// </summary>
    private static readonly Dictionary< IApplication, List< ShaderProgram >? > _shaders = new();

    private readonly Dictionary< string, int > _attributes     = new();
    private readonly Dictionary< string, int > _attributeSizes = new();
    private readonly Dictionary< string, int > _attributeTypes = new();
    private readonly Dictionary< string, int > _uniforms       = new();
    private readonly Dictionary< string, int > _uniformSizes   = new();
    private readonly Dictionary< string, int > _uniformTypes   = new();

    private int    _fragmentShaderHandle;
    private bool   _invalidated;
    private string _shaderLog = "";
    private int    _vertexShaderHandle;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Constructs a new ShaderProgram from the supplied shaders and immediately compiles it.
    /// </summary>
    /// <param name="vertexShader"> the vertex shader </param>
    /// <param name="fragmentShader"> the fragment shader </param>
    public ShaderProgram( string vertexShader, string fragmentShader )
    {
        if ( !string.IsNullOrEmpty( PrependVertexCode ) )
        {
            vertexShader = PrependVertexCode + vertexShader;
        }

        if ( !string.IsNullOrEmpty( PrependFragmentCode ) )
        {
            fragmentShader = PrependFragmentCode + fragmentShader;
        }

        VertexShaderSource   = vertexShader;
        FragmentShaderSource = fragmentShader;

        CompileShaders( vertexShader, fragmentShader );

        if ( !IsCompiled )
        {
            Logger.Debug( $"Shader program {vertexShader} has not been compiled." );

            return;
        }

        FetchAttributes();
        FetchUniforms();

        AddManagedShader( GdxApi.App, this );
    }

    /// <summary>
    /// Constructs a new shaderprgram.
    /// </summary>
    /// <param name="vertexShader"> the vertex shader </param>
    /// <param name="fragmentShader"> the fragment shader </param>
    public ShaderProgram( FileSystemInfo vertexShader, FileSystemInfo fragmentShader )
        : this( File.ReadAllText( vertexShader.Name ), File.ReadAllText( fragmentShader.Name ) )
    {
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
    /// 
    /// </summary>
    public static string ManagedStatus
    {
        get
        {
            var builder = new StringBuilder( "Managed shaders/app: { " );

            foreach ( var app in _shaders.Keys )
            {
                builder.Append( _shaders[ app ]?.Count );
                builder.Append( ' ' );
            }

            builder.Append( '}' );

            return builder.ToString();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static int NumManagedShaderPrograms => _shaders[ GdxApi.App ]!.Count;

    // ========================================================================

    /// <summary>
    /// Sets the vertex attribute with the given name.
    /// The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="size">
    /// The number of components, must be >= 1 and &lt;= 4.
    /// </param>
    /// <param name="type">
    /// The type, must be one of IGL.GL_Byte, IGL.GL_Unsigned_Byte, IGL.GL_Short,
    /// IGL.GL_Unsigned_Short, IGL.GL_Fixed, or IGL.GL_Float.
    /// <para>GL_F will not work on the desktop.</para>
    /// </param>
    /// <param name="normalize">
    /// Whether fixed point data should be normalized. Will not work on the desktop.
    /// </param>
    /// <param name="stride">The stride in bytes between successive attributes.</param>
    /// <param name="buffer">The buffer containing the vertex attributes.</param>
    public void SetVertexAttribute( string name, int size, int type, bool normalize, int stride, Buffer buffer )
    {
        CheckManaged();

        var location = FetchAttributeLocation( name );

        if ( location == -1 )
        {
            return;
        }

        unsafe
        {
            fixed ( void* ptr = &buffer.BackingArray()[ 0 ] )
            {
                GdxApi.Bindings.VertexAttribPointer( ( uint )location, size, type, normalize, stride, ptr );
            }
        }
    }

    public unsafe void SetVertexAttribute( int location, int size, int type, bool normalize, int stride, Buffer buffer )
    {
        CheckManaged();

        fixed ( void* ptr = &buffer.BackingArray()[ 0 ] )
        {
            GdxApi.Bindings.VertexAttribPointer( ( uint )location, size, type, normalize, stride, ptr );
        }
    }

    /// <summary>
    /// Sets the vertex attribute with the given name.
    /// The <see cref="ShaderProgram"/> must be bound for this to work.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="size">The number of components, must be >= 1 and &lt;= 4.</param>
    /// <param name="type">
    /// The type, must be one of IGL.GL_Byte, IGL.GL_Unsigned_Byte, IGL.GL_Short,
    /// IGL.GL_Unsigned_Short, IGL.GL_Fixed, or IGL.GL_Float.
    /// <para>GL_Fixed will not work on the desktop.</para>
    /// </param>
    /// <param name="normalize">
    /// Whether fixed point data should be normalized. Will not work on the desktop.
    /// </param>
    /// <param name="stride">The stride in bytes between successive attributes.</param>
    /// <param name="offset">
    /// Byte offset into the vertex buffer object bound to IGL.GL_Array_Buffer.
    /// </param>
    public void SetVertexAttribute( string name, int size, int type, bool normalize, int stride, int offset )
    {
        CheckManaged();

        var location = FetchAttributeLocation( name );

        if ( location == -1 )
        {
            return;
        }

        GdxApi.Bindings.VertexAttribPointer( ( uint )location, size, type, normalize, stride, ( uint )offset );
    }

    public void SetVertexAttribute( int location, int size, int type, bool normalize, int stride, int offset )
    {
        CheckManaged();
        GdxApi.Bindings.VertexAttribPointer( ( uint )location, size, type, normalize, stride, ( uint )offset );
    }

    public void Bind()
    {
        CheckManaged();
        GdxApi.Bindings.UseProgram( ( uint )Handle );
    }

    public void Unbind()
    {
        CheckManaged();
        GdxApi.Bindings.UseProgram( 0 );
    }

    /// <summary>
    /// Disables the vertex attribute with the given name
    /// </summary>
    /// <param name="name"> the vertex attribute name  </param>
    public void DisableVertexAttribute( string name )
    {
        CheckManaged();

        var location = FetchAttributeLocation( name );

        if ( location == -1 )
        {
            return;
        }

        GdxApi.Bindings.DisableVertexAttribArray( ( uint )location );
    }

    public void DisableVertexAttribute( int location )
    {
        CheckManaged();
        GdxApi.Bindings.DisableVertexAttribArray( ( uint )location );
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
    /// 
    /// </summary>
    /// <param name="location"></param>
    public void EnableVertexAttribute( int location )
    {
        CheckManaged();
        GdxApi.Bindings.EnableVertexAttribArray( ( uint )location );
    }

    /// <summary>
    /// Sets the given attribute
    /// </summary>
    /// <param name="name"> the name of the attribute </param>
    /// <param name="value1"> the first value </param>
    /// <param name="value2"> the second value </param>
    /// <param name="value3"> the third value </param>
    /// <param name="value4"> the fourth value  </param>
    public void SetAttributef( string name, float value1, float value2, float value3, float value4 )
    {
        GdxApi.Bindings.VertexAttrib4f( ( uint )FetchAttributeLocation( name ), value1, value2, value3, value4 );
    }

    /// <summary>
    /// </summary>
    /// <param name="name"> the name of the attribute </param>
    /// <returns> whether the attribute is available in the shader  </returns>
    public bool HasAttribute( string name )
    {
        return _attributes.ContainsKey( name );
    }

    /// <param name="name"> the name of the attribute </param>
    /// <returns>
    /// the type of the attribute, one of <see cref="IGL.GL_FLOAT"/>,
    /// <see cref="IGL.GL_FLOAT_VEC2"/> etc.
    /// </returns>
    public int GetAttributeType( string name )
    {
        return _attributeTypes.GetValueOrDefault( name, 0 );
    }

    /// <param name="name"> the name of the attribute </param>
    /// <returns> the location of the attribute or -1.  </returns>
    public int GetAttributeLocation( string name )
    {
        return _attributes.GetValueOrDefault( name, 0 );
    }

    /// <param name="name"> the name of the attribute </param>
    /// <returns> the size of the attribute or 0.</returns>
    public int GetAttributeSize( string name )
    {
        return _attributeSizes.GetValueOrDefault( name, 0 );
    }

    // ========================================================================
    
    /// <summary>
    /// Loads and compiles the shaders, creates a new program and links the shaders.
    /// </summary>
    /// <param name="vertexShader"> the vertex shader </param>
    /// <param name="fragmentShader"> the fragment shader </param>
    private void CompileShaders( string vertexShader, string fragmentShader )
    {
        _vertexShaderHandle   = LoadShader( IGL.GL_VERTEX_SHADER, vertexShader );
        _fragmentShaderHandle = LoadShader( IGL.GL_FRAGMENT_SHADER, fragmentShader );

        if ( ( _vertexShaderHandle == -1 ) || ( _fragmentShaderHandle == -1 ) )
        {
            IsCompiled = false;

            return;
        }

        Handle = LinkProgram( CreateProgram() );

        IsCompiled = ( Handle != -1 );
    }

    /// <summary>
    /// Loads the supplied shader into the relvant shader type.
    /// </summary>
    /// <param name="shaderType"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private unsafe int LoadShader( int shaderType, string source )
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    /// <param name="shaderProgram"></param>
    private void AddManagedShader( IApplication app, ShaderProgram shaderProgram )
    {
        List< ShaderProgram >? managedResources;

        if ( !_shaders.ContainsKey( app ) || ( _shaders[ app ] == null ) )
        {
            managedResources = new List< ShaderProgram >();
        }
        else
        {
            managedResources = _shaders[ app ];
        }

        managedResources?.Add( shaderProgram );

        _shaders.Put( app, managedResources );
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

        _shaders[ GdxApi.App ]?.Remove( this );
    }
}

