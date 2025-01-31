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

    #endregion default attribute names

    // ========================================================================

    private const int CACHED_NOT_FOUND = -1;
    private const int NOT_CACHED       = -2;

    // ========================================================================

    public bool     IsCompiled           { get; set; }
//    public string[] Attributes           { get; private set; } = null!;
//    public string[] Uniforms             { get; private set; } = null!;
    public string   VertexShaderSource   { get; }
    public string   FragmentShaderSource { get; }
    public int      Handle               { get; private set; }

    // ========================================================================
    /// <summary>
    /// flag indicating whether attributes & uniforms must be present at all times.
    /// </summary>
    public static readonly bool Pedantic = true;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// the list of currently available shaders
    /// </summary>
//    private static readonly Dictionary< IApplication, List< ShaderProgram >? > _shaders = new();
//
//    private readonly Dictionary< string, int > _attributes     = new();
//    private readonly Dictionary< string, int > _attributeSizes = new();
//    private readonly Dictionary< string, int > _attributeTypes = new();
//    private readonly Dictionary< string, int > _uniforms       = new();
//    private readonly Dictionary< string, int > _uniformSizes   = new();
//    private readonly Dictionary< string, int > _uniformTypes   = new();

    private int    _vertexShaderHandle;
    private int    _fragmentShaderHandle;
    private bool   _invalidated;
    private string _shaderLog = "";

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Constructs a new ShaderProgram from the supplied shaders and immediately compiles it.
    /// </summary>
    /// <param name="vertexShader"> the vertex shader </param>
    /// <param name="fragmentShader"> the fragment shader </param>
    public ShaderProgram( string vertexShader, string fragmentShader )
    {
        VertexShaderSource   = vertexShader;
        FragmentShaderSource = fragmentShader;

        CompileShaders( VertexShaderSource, FragmentShaderSource );

        if ( !IsCompiled )
        {
            Logger.Debug( $"Shader program {VertexShaderSource} has not been compiled." );

            return;
        }

        FetchAttributes();
        FetchUniforms();

        AddManagedShader( GdxApi.App, this );

        Logger.Debug( $"POSITION_ATTRIBUTE  : {GdxApi.Bindings.GetAttribLocation( ( uint )Handle, POSITION_ATTRIBUTE )}" );
        Logger.Debug( $"COLOR_ATTRIBUTE     : {GdxApi.Bindings.GetAttribLocation( ( uint )Handle, COLOR_ATTRIBUTE )}" );
        Logger.Debug( $"TEXCOORD_ATTRIBUTE  : {GdxApi.Bindings.GetAttribLocation( ( uint )Handle, TEXCOORD_ATTRIBUTE )}" );
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
    /// Trys to create a program object for this ShaderProgram.
    /// </summary>
    /// <returns>The program ID if created, otherwise -1.</returns>
    protected static int CreateProgram()
    {
        var program = ( int )GdxApi.Bindings.CreateProgram();

        return program != 0 ? program : -1;
    }

    /// <summary>
    /// Loads and compiles the shaders, creates a new program and links the shaders.
    /// </summary>
    /// <param name="vertexShader"> the vertex shader </param>
    /// <param name="fragmentShader"> the fragment shader </param>
    private void CompileShaders( string vertexShader, string fragmentShader )
    {
        _vertexShaderHandle   = LoadShader( IGL.GL_VERTEX_SHADER, vertexShader );
        _fragmentShaderHandle = LoadShader( IGL.GL_FRAGMENT_SHADER, fragmentShader );

        Logger.Debug( $"_vertexShaderHandle: {_vertexShaderHandle}, " +
                      $"_fragmentShaderHandle: {_fragmentShaderHandle}" );

        if ( ( _vertexShaderHandle == -1 ) || ( _fragmentShaderHandle == -1 ) )
        {
            IsCompiled = false;

            return;
        }

        Handle = LinkProgram( CreateProgram() );

        IsCompiled = ( Handle != -1 );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="program"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
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

        Logger.Debug( $"Link Status: {status[ 0 ]}" );

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

//    public static string ManagedStatus
//    {
//        get
//        {
//            var builder = new StringBuilder( "Managed shaders/app: { " );
//
//            foreach ( var app in _shaders.Keys )
//            {
//                builder.Append( _shaders[ app ]?.Count );
//                builder.Append( ' ' );
//            }
//
//            builder.Append( '}' );
//
//            return builder.ToString();
//        }
//    }

//    public static int NumManagedShaderPrograms => _shaders[ GdxApi.App ]!.Count;

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    public void Bind()
    {
        CheckManaged();
        GdxApi.Bindings.UseProgram( ( uint )Handle );
    }

    /// <summary>
    /// 
    /// </summary>
    public void Unbind()
    {
        CheckManaged();
        GdxApi.Bindings.UseProgram( 0 );
    }

    // ========================================================================

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