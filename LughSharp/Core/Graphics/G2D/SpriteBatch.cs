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

using LughSharp.Core.Files;
using LughSharp.Core.Graphics.OpenGL;
using LughSharp.Core.Graphics.OpenGL.Enums;
using LughSharp.Core.Graphics.Utils;

namespace LughSharp.Core.Graphics.G2D;

// ============================================================================

/// <summary>
/// The SpriteBatch class is used for efficient, batched rendering of 2D sprites.
/// It minimizes the number of draw calls by buffering sprite data and rendering all at once.
/// </summary>
[PublicAPI]
public partial class SpriteBatch : IBatch, IDisposable
{
    public const int RENDERING_OPTION_FLIP_X = 1 << 0;
    public const int RENDERING_OPTION_FLIP_Y = 1 << 1;
    public const int RENDERING_OPTION_FONT   = 1 << 2;

    // ========================================================================

    public bool    BlendingEnabled   { get; set; }         = false;
    public float   InvTexHeight      { get; set; }         = 0;
    public float   InvTexWidth       { get; set; }         = 0;
    public Matrix4 CombinedMatrix    { get; set; }         = new();
    public Matrix4 ProjectionMatrix  { get; set; }         = new();
    public Matrix4 TransformMatrix   { get; set; }         = new();
    public int     RenderCalls       { get; set; }         = 0; // Number of render calls since the last call to Begin()
    public long    TotalRenderCalls  { get; set; }         = 0; // Number of rendering calls, ever. Will not be reset unless set manually.
    public int     MaxSpritesInBatch { get; set; }         = 0; // The maximum number of sprites rendered in one batch so far.
    public int     BlendSrcFunc      { get; private set; } = ( int )BlendMode.SrcColor;
    public int     BlendDstFunc      { get; private set; } = ( int )BlendMode.DstColor;
    public int     BlendSrcFuncAlpha { get; private set; } = ( int )BlendMode.OneMinusSrcAlpha;
    public int     BlendDstFuncAlpha { get; private set; } = ( int )BlendMode.OneMinusDstAlpha;
    public float[] Vertices          { get; set; }         = [ ];
    public int     TextureOffset     { get; set; }         = 0;

    public bool IsDrawing => CurrentBatchState == BatchState.Drawing;

    // ========================================================================

    protected Texture?     LastTexture        { get; set; } = null;
    protected int          Idx                { get; set; } = 0;
    protected BatchState?  CurrentBatchState  { get; set; }
    protected RenderState? CurrentRenderState { get; set; }

    // ========================================================================

    private const int VERTICES_PER_SPRITE = 4;     // Number of vertices per sprite (quad)
    private const int INDICES_PER_SPRITE  = 6;     // Number of indices per sprite (two triangles)
    private const int MAX_VERTEX_INDEX    = 32767; //
    private const int MAX_SPRITES         = 8191;  //
    private const int MAX_QUADS           = 100;   //

    // ========================================================================

    private readonly Color  _color      = Color.Red;
    private readonly object _lockObject = new();

    // Prevent reallocation of common vectors
    private static readonly Vector2 _defaultOrigin = Vector2.Zero;
    private static readonly Vector2 _defaultScale  = Vector2.One;

    private Texture?       _lastSuccessfulTexture = null;
    private ShaderProgram? _shader;
    private Mesh?          _mesh;

    private uint _vao;
    private uint _vbo;
    private uint _ebo;
    private int  _nullTextureCount    = 0;
    private int  _currentTextureIndex = 0;
    private int  _maxTextureUnits;
    private int  _maxVertices;
    private int  _combinedMatrixLocation;
    private int  _textureLocation;
    private bool _ownsShader;
    private bool _originalDepthMask;
    private bool _disposed = false;
    private bool _initialBlendingState;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Constructs a new SpriteBatch with a size of 1000, one buffer, and the default shader.
    /// </summary>
    public SpriteBatch() : this( 1000 )
    {
    }

    /// <summary>
    /// Constructs a new SpriteBatch. Sets the projection matrix to an orthographic
    /// projection with y-axis point upwards, x-axis point to the right and the origin
    /// being in the bottom left corner of the screen. The projection will be pixel
    /// perfect with respect to the current screen resolution.
    /// The defaultShader specifies the shader to use. Note that the names for uniforms
    /// for this default shader are different than the ones expect for shaders set with
    /// <see cref="Shader"/>.
    /// See <see cref="CreateDefaultShader()"/>.
    /// </summary>
    /// <param name="size">
    /// The max number of sprites in a single batch. Max of <see cref="MAX_SPRITES"/>.
    /// </param>
    /// <param name="defaultShader">
    /// The default shader to use. This is not owned by the SpriteBatch and must be disposed
    /// separately.
    /// </param>
    protected SpriteBatch( int size = MAX_QUADS, ShaderProgram? defaultShader = null )
    {
        // 32767 is max vertex index, so 32767 / 4 vertices per sprite = 8191 sprites max.
        Guard.NotGreaterThan( size, MAX_SPRITES, nameof( size ) );
        Guard.ThrowIfNegative( size, nameof( size ) );

        CurrentBatchState = BatchState.Ready;

        Initialise( size, defaultShader );
    }

    // ========================================================================

    /// <summary>
    /// Takes away messy vertex attributes initialisation from the constructor.
    /// </summary>
    /// <param name="size"> The max number of sprites in a single batch. Max of 8191. </param>
    /// <param name="defaultShader">
    /// The default shader to use. This is not owned by the SpriteBatch and must be disposed separately.
    /// </param>
    private void Initialise( int size, ShaderProgram? defaultShader )
    {
        GLUtils.CheckOpenGLContext();

        _maxVertices     = size * VERTICES_PER_SPRITE * VertexConstants.VERTEX_SIZE;
        _maxTextureUnits = QueryMaxSupportedTextureUnits();

        CreateVao();
        CreateVbo( size );
        CreateEbo( size );

        // Determine the vertex data type based on OpenGL version.
        // OpenGL 3.0 and later support Vertex Buffer Objects (VBOs) with Vertex Array Objects (VAOs),
        // which are more efficient. Earlier versions use Vertex Arrays.
        var vertexDataType = GL.GetOpenGLVersion().major >= 3
            ? VertexDataType.VertexBufferObjectWithVAO
            : VertexDataType.VertexArray;

        // Define the vertex attributes for the mesh.
        // These attributes specify the layout of vertex data in the VBO.
        // Usage.POSITION: 2 floats for x and y coordinates.
        // Usage.COLOR_PACKED: 1 floats for packed RGBA color component.
        // Usage.TEXTURE_COORDINATES: 2 floats for texture u and v coordinates.

        var va1 = VertexAttribute.Position( VertexConstants.POSITION_COMPONENTS, IGL.GL_FLOAT, false );
        ;

        var va2 = VertexAttribute.ColorPacked( VertexConstants.COLOR_COMPONENTS, IGL.GL_FLOAT, false );

        var va3 = VertexAttribute.TexCoords( 0, VertexConstants.TEXCOORD_COMPONENTS, IGL.GL_FLOAT, false );

        // Create the mesh object with the specified vertex attributes and size.
        // The mesh will hold the vertex and index data for rendering.
        _mesh = new Mesh( vertexDataType,
                          false,
                          size * VERTICES_PER_SPRITE,
                          size * INDICES_PER_SPRITE,
                          va1, va2, va3 );

        // Set up an orthographic projection matrix for 2D rendering.
        // This matrix transforms 2D coordinates into normalized device coordinates (NDC).
        // The origin (0, 0) is at the bottom-left corner of the screen.
        ProjectionMatrix.SetToOrtho2D( 0, 0, Api.Graphics.Width, Api.Graphics.Height );

        // Generate the index buffer data for the mesh.
        // This buffer specifies the order in which vertices are used to form triangles.
        PopulateIndexBuffer( size, out var indicesData );

        // Set the indices on the mesh.
        _mesh.SetIndices( indicesData );

        // Determine the shader program to use.
        // If a default shader is not provided, create one.
        if ( defaultShader == null )
        {
            _shader     = CreateDefaultShader();
            _ownsShader = true; // Indicate that this class owns the shader.
        }
        else
        {
            _shader = defaultShader;
        }

        // Get the location of the combined matrix uniform in the shader
        // program. This uniform will be used to pass the combined
        // transformation matrix to the shader.
        GetCombinedMatrixUniformLocation();

        // Get the location of the texture uniform in the shader program.
        GetTextureUniformLocation();
    }

    /// <summary>
    /// Begins a new sprite batch.
    /// </summary>
    /// <param name="depthMaskEnabled">
    /// Enable or Disable DepthMask. This parameter has a default value of <c>false</c>
    /// so this can be omitted from method calls for values of false.
    /// </param>
    public void Begin( bool depthMaskEnabled = false )
    {
        lock ( _lockObject )
        {
            ThrowIfDisposed();

            if ( CurrentBatchState == BatchState.Drawing )
            {
                throw new InvalidOperationException( "End() must be called before Begin()" );
            }

            if ( _shader == null )
            {
                throw new NullReferenceException( "Shader is null" );
            }

            RenderCalls           = 0;
            _initialBlendingState = BlendingEnabled;
            CurrentBatchState     = BatchState.Drawing;
            _originalDepthMask    = GL.IsEnabled( ( int )EnableCap.DepthTest );

            GL.DepthMask( depthMaskEnabled );

            if ( _shader != null )
            {
                _shader.Bind();
                SetupMatrices();
                SetupVertexAttributes( _shader );
            }
        }
    }

    /// <summary>
    /// Flushes all batched text, textures and sprites to the screen.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this method is called BEFORE a call to <see cref="Begin"/>
    /// </exception>
    public void End()
    {
        lock ( _lockObject )
        {
            if ( CurrentBatchState != BatchState.Drawing )
            {
                throw new InvalidOperationException( "SpriteBatch.begin must be called before end." );
            }

            if ( Idx > 0 )
            {
                // Flush this batch to ensure all pending drawing
                // operations are completed before ending the batch
                Flush();
            }

            LastTexture       = null;
            CurrentBatchState = BatchState.Ready;

            GL.DepthMask( _originalDepthMask );

            // Only modify blending if current state differs from initial state
            if ( BlendingEnabled != _initialBlendingState )
            {
                GL.EnableOrDisable( IGL.GL_BLEND, _initialBlendingState );
            }

            Idx = 0;
        }
    }

    /// <summary>
    /// Flushes the current batch, sending all rendered vertices to the GPU for drawing.
    /// This method handles binding the appropriate Vertex Buffer Object (VBO), Vertex Array Object (VAO),
    /// textures, shaders, and performs the actual rendering of the accumulated sprites.
    /// </summary>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if there is no OpenGL context available on the current thread, if
    /// the index buffer (`Idx`) is less than zero, or if certain rendering conditions are not met.
    /// </exception>
    public void Flush()
    {
        lock ( _lockObject )
        {
            GLUtils.CheckOpenGLContext();

            // Check if there is any data to flush.
            if ( Idx <= 0 )
            {
                Logger.Error( $"Flush cancelled: Idx: {Idx}" );

                // Ensure that Idx is reset to zero.
                Idx = 0;

                return;
            }

            // Increment render call counters.
            RenderCalls++;
            TotalRenderCalls++;

            // Calculate the number of sprites in the current batch.
            // Idx represents the number of floats in the Vertices array.
            // VERTICES_PER_SPRITE is the number of vertices per sprite (4).
            // VertexConstants.VERTEX_SIZE is the number of floats per vertex.
            var spritesInBatch = ( int )( Idx / ( long )( VERTICES_PER_SPRITE * VertexConstants.VERTEX_SIZE ) );

            // Update the maximum number of sprites in a batch.
            MaxSpritesInBatch = Math.Max( MaxSpritesInBatch, spritesInBatch );

            // Check if the texture is null.
            if ( LastTexture == null )
            {
                Idx = 0;
                _nullTextureCount++;

                Logger.Error( $"Attempt to flush with null texture. This batch will be skipped. " +
                                $"Null texture count: {_nullTextureCount}. " +
                                $"Last successful texture: {_lastSuccessfulTexture?.ToString() ?? "None"}" );

                return;
            }

            if ( _textureLocation != ShaderProgram.INVALID )
            {
                // Bind the texture to the current texture unit.
                GL.ActiveTexture( TextureUnit.Texture0 + _currentTextureIndex );
                LastTexture.Bind();
                GL.Uniform1i( _textureLocation, _currentTextureIndex );

                _mesh?.SetVertices( Vertices, 0, Idx );
                _mesh!.IndicesBuffer.Position = 0;
                _mesh!.IndicesBuffer.Limit    = spritesInBatch * INDICES_PER_SPRITE;
            }

            // Set up blending.
            if ( BlendingEnabled )
            {
                GL.Enable( IGL.GL_BLEND );

                // Set the blend function if specified.
                if ( BlendSrcFunc != -1 )
                {
                    GL.BlendFuncSeparate( BlendSrcFunc, BlendDstFunc, BlendSrcFuncAlpha, BlendDstFuncAlpha );
                }
            }
            else
            {
                GL.Disable( IGL.GL_BLEND );
            }

            // Render the sprites.
            _mesh?.Render( _shader, IGL.GL_TRIANGLES, 0, spritesInBatch * INDICES_PER_SPRITE );
        }
    }

    // ========================================================================

    /// <summary>
    /// Enables blending for textures.
    /// </summary>
    public void EnableBlending()
    {
        lock ( _lockObject )
        {
            if ( BlendingEnabled )
            {
                return;
            }

            if ( Idx > 0 )
            {
                // Necessary call to Flush() to ensure blending state
                // changes are handled correctly
                Flush();
            }

            BlendingEnabled = true;
            GL.Enable( IGL.GL_BLEND );

            // Restore blend function state
            GL.BlendFuncSeparate( BlendSrcFunc, BlendDstFunc, BlendSrcFuncAlpha, BlendDstFuncAlpha );
        }
    }

    /// <summary>
    /// Disables blending for textures.
    /// </summary>
    public void DisableBlending()
    {
        lock ( _lockObject )
        {
            if ( !BlendingEnabled )
            {
                return;
            }

            if ( Idx > 0 )
            {
                // Necessary call to Flush() to ensure blending state
                // changes are handled correctly
                Flush();
            }

            BlendingEnabled = false;
            GL.Disable( IGL.GL_BLEND );
        }
    }

    /// <summary>
    /// Sets the Blend Functions to use when rendering. The provided
    /// methods handle both Color and Alpha functions.
    /// </summary>
    /// <param name="srcFunc"> Source Function for Color and Alpha. </param>
    /// <param name="dstFunc"> Destination Function for Color and Alpha. </param>
    public void SetBlendFunction( int srcFunc, int dstFunc )
    {
        SetBlendFunctionSeparate( srcFunc, dstFunc, srcFunc, dstFunc );
    }

    /// <summary>
    /// Sets the Blend Functions to use when rendering. The provided
    /// methods handle Color or Alpha functions.
    /// </summary>
    /// <param name="srcFuncColor"> Source Function for Color. </param>
    /// <param name="dstFuncColor"> Destination Function for Color. </param>
    /// <param name="srcFuncAlpha"> Source Function for Alpha. </param>
    /// <param name="dstFuncAlpha"> Destination Function for Alpha. </param>
    public void SetBlendFunctionSeparate( int srcFuncColor, int dstFuncColor, int srcFuncAlpha, int dstFuncAlpha )
    {
        if ( ( BlendSrcFunc == srcFuncColor )
             && ( BlendDstFunc == dstFuncColor )
             && ( BlendSrcFuncAlpha == srcFuncAlpha )
             && ( BlendDstFuncAlpha == dstFuncAlpha ) )
        {
            return;
        }

        if ( Idx > 0 )
        {
            // Necessary call to Flush() to ensure blending function
            // changes are handled correctly
            Flush();
        }

        BlendSrcFunc      = srcFuncColor;
        BlendDstFunc      = dstFuncColor;
        BlendSrcFuncAlpha = srcFuncAlpha;
        BlendDstFuncAlpha = dstFuncAlpha;

        // Actually set the OpenGL blend function
        GL.BlendFuncSeparate( srcFuncColor, dstFuncColor, srcFuncAlpha, dstFuncAlpha );
    }

    /// <summary>
    /// Sets the projection matrix used for drawing.
    /// </summary>
    /// <param name="projection">The new projection matrix to be applied.</param>
    public void SetProjectionMatrix( Matrix4 projection )
    {
        if ( ( CurrentBatchState == BatchState.Drawing ) && ( Idx > 0 ) )
        {
            // Necessary call to Flush() to ensure projection matrix
            // changes are handled correctly
            Flush();
        }

        ProjectionMatrix.Set( projection );

        if ( CurrentBatchState == BatchState.Drawing )
        {
            SetupMatrices();
        }
    }

    /// <summary>
    /// Performs the OpenGL side of Vertex Attribute initialisation.
    /// </summary>
    public void SetupVertexAttributes( ShaderProgram? program )
    {
        lock ( _lockObject )
        {
            GLUtils.CheckOpenGLContext();

            // --------------------------------------------------------------------

            if ( ( program == null ) || ( _mesh == null ) )
            {
                throw new NullReferenceException( "program and _mesh cannot be null!" );
            }

            // --------------------------------------------------------------------

            var currentProgram = new int[ 1 ];

            GL.GetIntegerv( IGL.GL_CURRENT_PROGRAM, ref currentProgram );

            if ( currentProgram[ 0 ] != _shader?.ShaderProgramHandle )
            {
                throw new GdxRuntimeException( "***** Shader program is unbound *****" );
            }

            // --------------------------------------------------------------------

            GL.BindVertexArray( _vao );
            GL.BindBuffer( ( int )BufferTarget.ArrayBuffer, _vbo );

            const int STRIDE           = VertexConstants.VERTEX_SIZE_BYTES;
            const int POSITION_OFFSET  = VertexConstants.POSITION_OFFSET;
            const int COLOR_OFFSET     = VertexConstants.COLOR_OFFSET * sizeof( float );
            const int TEX_COORD_OFFSET = VertexConstants.TEXCOORD_OFFSET * sizeof( float );

            // Position Attribute
            var positionLocation = program.GetAttributeLocation( "a_position" );

            if ( positionLocation >= 0 )
            {
                program.EnableVertexAttribute( positionLocation );
                program.SetVertexAttribute( positionLocation,
                                            VertexConstants.POSITION_COMPONENTS,
                                            IGL.GL_FLOAT,
                                            false,
                                            STRIDE,
                                            POSITION_OFFSET );
            }

            // Color Attribute
            var colorLocation = program.GetAttributeLocation( ShaderConstants.A_COLOR );

            if ( colorLocation >= 0 )
            {
                program.EnableVertexAttribute( colorLocation );
                program.SetVertexAttribute( colorLocation,
                                            VertexConstants.COLOR_COMPONENTS,
                                            IGL.GL_FLOAT,
                                            false,
                                            STRIDE,
                                            COLOR_OFFSET );
            }

            // Texture Coordinates Attribute
            var texCoordLocation = program.GetAttributeLocation( "a_texCoord0" );

            if ( texCoordLocation >= 0 )
            {
                program.EnableVertexAttribute( texCoordLocation );
                program.SetVertexAttribute( texCoordLocation,
                                            VertexConstants.TEXCOORD_COMPONENTS,
                                            IGL.GL_FLOAT,
                                            false,
                                            STRIDE,
                                            TEX_COORD_OFFSET );
            }
        }
    }

    // ========================================================================

    /// <summary>
    /// Sets the vertex attributes for a quad, including positional, color, and texture coordinate data.
    /// </summary>
    /// <param name="x1">The X-coordinate of the first vertex.</param>
    /// <param name="y1">The Y-coordinate of the first vertex.</param>
    /// <param name="c1">The color for the first vertex.</param>
    /// <param name="u1">The U texture coordinate for the first vertex.</param>
    /// <param name="v1">The V texture coordinate for the first vertex.</param>
    /// <param name="x2">The X-coordinate of the second vertex.</param>
    /// <param name="y2">The Y-coordinate of the second vertex.</param>
    /// <param name="c2">The color for the second vertex.</param>
    /// <param name="u2">The U texture coordinate for the second vertex.</param>
    /// <param name="v2">The V texture coordinate for the second vertex.</param>
    /// <param name="x3">The X-coordinate of the third vertex.</param>
    /// <param name="y3">The Y-coordinate of the third vertex.</param>
    /// <param name="c3">The color for the third vertex.</param>
    /// <param name="u3">The U texture coordinate for the third vertex.</param>
    /// <param name="v3">The V texture coordinate for the third vertex.</param>
    /// <param name="x4">The X-coordinate of the fourth vertex.</param>
    /// <param name="y4">The Y-coordinate of the fourth vertex.</param>
    /// <param name="c4">The color for the fourth vertex.</param>
    /// <param name="u4">The U texture coordinate for the fourth vertex.</param>
    /// <param name="v4">The V texture coordinate for the fourth vertex.</param>
    private void SetVertices( float x1, float y1, float c1, float u1, float v1,
                              float x2, float y2, float c2, float u2, float v2,
                              float x3, float y3, float c3, float u3, float v3,
                              float x4, float y4, float c4, float u4, float v4 )
    {
        Vertices[ Idx + IBatch.X1 ] = x1; // X
        Vertices[ Idx + IBatch.Y1 ] = y1; // Y
        Vertices[ Idx + IBatch.C1 ] = c1; // Color
        Vertices[ Idx + IBatch.U1 ] = u1; // Texture U
        Vertices[ Idx + IBatch.V1 ] = v1; // Texture V

        Vertices[ Idx + IBatch.X2 ] = x2; // X
        Vertices[ Idx + IBatch.Y2 ] = y2; // Y
        Vertices[ Idx + IBatch.C2 ] = c2; // Color
        Vertices[ Idx + IBatch.U2 ] = u2; // Texture U
        Vertices[ Idx + IBatch.V2 ] = v2; // Texture V

        Vertices[ Idx + IBatch.X3 ] = x3; // X
        Vertices[ Idx + IBatch.Y3 ] = y3; // Y
        Vertices[ Idx + IBatch.C3 ] = c3; // Color
        Vertices[ Idx + IBatch.U3 ] = u3; // Texture U
        Vertices[ Idx + IBatch.V3 ] = v3; // Texture V

        Vertices[ Idx + IBatch.X4 ] = x4; // X
        Vertices[ Idx + IBatch.Y4 ] = y4; // Y
        Vertices[ Idx + IBatch.C4 ] = c4; // Color
        Vertices[ Idx + IBatch.U4 ] = u4; // Texture U
        Vertices[ Idx + IBatch.V4 ] = v4; // Texture V

        Idx += VERTICES_PER_SPRITE * VertexConstants.VERTEX_SIZE;
    }

    /// <summary>
    /// Fetches the location of the CombinedMatrix uniform in the shader and
    /// stores it in <see cref="_combinedMatrixLocation"/> for subsequent use.
    /// </summary>
    private void GetCombinedMatrixUniformLocation()
    {
        GdxRuntimeException.ThrowIfNull( _shader );

        _combinedMatrixLocation = GL.GetUniformLocation( _shader.ShaderProgramHandle, "u_combinedMatrix" );
    }

    /// <summary>
    /// Retrieves the uniform location of the texture in the shader program and
    /// stores it in <see cref="_textureLocation"/> for subsequent use.
    /// </summary>
    private void GetTextureUniformLocation()
    {
        GdxRuntimeException.ThrowIfNull( _shader );

        _textureLocation = GL.GetUniformLocation( _shader.ShaderProgramHandle,
                                                  "u_texture" );
    }

    /// <summary>
    /// Returns a new instance of the default shader used by SpriteBatch when
    /// no shader is specified.
    /// </summary>
    public static ShaderProgram CreateDefaultShader()
    {
        var vertexShader = ShaderLoader.Load( IOUtils.AssetsRoot + "shaders/default.glsl.vert" );
        var fragShader   = ShaderLoader.Load( IOUtils.AssetsRoot + "shaders/default.glsl.frag" );

        return new ShaderProgram( vertexShader, fragShader );
    }

    /// <summary>
    /// Switches the current texture to the specified texture and updates internal
    /// properties related to the texture dimensions. Also flushes any pending
    /// batched render operations.
    /// </summary>
    /// <param name="texture">The new texture to switch to. If null, no action is taken.</param>
    protected void SwitchTexture( Texture? texture )
    {
        if ( texture == null )
        {
            Logger.Debug( "NULL Texture passed to SwitchTexture" );

            return;
        }

        if ( Idx > 0 )
        {
            // Necessary call to Flush() to ensure texture switching is handled correctly
            Flush();
        }

        GL.ActiveTexture( TextureUnit.Texture0 + TextureOffset );
        texture.Bind();
        GL.Uniform1i( _textureLocation, TextureOffset );

        LastTexture            = texture;
        _lastSuccessfulTexture = LastTexture;
        InvTexWidth            = 1.0f / texture.Width;
        InvTexHeight           = 1.0f / texture.Height;
    }

    // ========================================================================

    /// <summary>
    /// Performs validation checks for Draw methods.
    /// Throws an exception if the supplied Texture is null, or not supported.
    /// Throws an exception if <see cref="Begin"/> was not called before entering
    /// the draw method.
    /// </summary>
    /// <param name="texture"> The Texture to check for null. </param>
    private void Validate< T >( T? texture )
    {
        if ( texture is not Texture or TextureRegion )
        {
            throw new GdxRuntimeException( "Invalid Texture or TextureRegion" );
        }

        if ( texture == null )
        {
            throw new ArgumentException( $"Texture is null: {texture}" );
        }

        if ( CurrentBatchState != BatchState.Drawing )
        {
            throw new InvalidOperationException( "Begin() must be called before Draw()." );
        }
    }

    /// <summary>
    /// Query OpenGL for the maximum number of texture units that can be supported
    /// </summary>
    /// <returns></returns>
    private unsafe int QueryMaxSupportedTextureUnits()
    {
        var maxTextureUnits = new int[ 1 ];

        fixed ( int* ptr = &maxTextureUnits[ 0 ] )
        {
            GL.GetIntegerv( ( int )TextureLimits.MaxTextureImageUnits, ptr );
        }

        if ( maxTextureUnits[ 0 ] < 32 )
        {
            Logger.Divider( '#', 100 );
            Logger.Error( $"Warning: Low MaxTextureUnits detected ({maxTextureUnits[ 0 ]}." );
            Logger.Divider( '#', 100 );
        }

        return maxTextureUnits[ 0 ];
    }

    // ========================================================================

    /// <summary>
    /// Sets the transformation matrix to be applied to the SpriteBatch during
    /// the rendering process.
    /// </summary>
    /// <param name="transform">
    /// A Matrix4 representing the new transformation to be applied.
    /// </param>
    public virtual void SetTransformMatrix( Matrix4 transform )
    {
        if ( ( CurrentBatchState == BatchState.Drawing ) && ( Idx > 0 ) )
        {
            // Necessary call to Flush() to ensure transformation matrix
            // #changes are handled correctly
            Flush();
        }

        TransformMatrix.Set( transform );

        if ( CurrentBatchState == BatchState.Drawing )
        {
            SetupMatrices();
        }
    }

    /// <summary>
    /// Configures the combined transformation matrix by multiplying the projection
    /// matrix with the transform matrix. Additionally, if a shader is assigned and
    /// compiled, updates the shader's uniform variables.
    /// </summary>
    public virtual void SetupMatrices()
    {
        // Note: Do not use the property 'Shader' here as its setter calls
        // this method, which would cause an infinite loop.
        CombinedMatrix = ProjectionMatrix.Mul( TransformMatrix );

        if ( _shader != null )
        {
            if ( !_shader.IsCompiled )
            {
                Logger.Error( $"Shader is not compiled: {Shader}" );

                return;
            }

            _shader.SetUniformMatrix( "u_combinedMatrix", CombinedMatrix );
            _shader.SetUniformi( "u_texture", 0 );
        }
    }

    // ========================================================================

    /// <summary>
    /// Populates an index buffer with indices required to render a specified number of sprites.
    /// </summary>
    /// <param name="size">The number of sprites for which to generate indices.</param>
    /// <param name="indices">
    /// An array to hold the generated indices for the specified number of sprites.
    /// </param>
    private static void PopulateIndexBuffer( int size, out short[] indices )
    {
        var len = size * INDICES_PER_SPRITE;
        indices = new short[ len ];

        for ( short i = 0, j = 0; i < len; i += INDICES_PER_SPRITE, j += 4 )
        {
            indices[ i ]     = j;
            indices[ i + 1 ] = ( short )( j + 1 );
            indices[ i + 2 ] = ( short )( j + 2 );
            indices[ i + 3 ] = ( short )( j + 2 );
            indices[ i + 4 ] = ( short )( j + 3 );
            indices[ i + 5 ] = j;
        }
    }

    /// <summary>
    /// Unbind VAO and VBO
    /// </summary>
    private static void UnbindBuffers()
    {
        GL.BindVertexArray( 0 );
        GL.BindBuffer( ( int )BufferTarget.ArrayBuffer, 0 );
    }

    // ========================================================================

    /// <summary>
    /// Gets or sets the custom shader used by the SpriteBatch for rendering.
    /// When a custom shader is set, it replaces the default shader provided by
    /// the SpriteBatch. If the custom shader is null, the SpriteBatch falls back
    /// to using its default shader. Modifying this property while the SpriteBatch
    /// is actively drawing will flush the current batch.
    /// </summary>
    public ShaderProgram? Shader
    {
        get => _shader;
        set
        {
            if ( ( CurrentBatchState == BatchState.Drawing ) && ( Idx > 0 ) )
            {
                Flush();
            }

            _shader = value;

            if ( CurrentBatchState == BatchState.Drawing )
            {
                _shader?.Bind();
                SetupMatrices();
            }
        }
    }

    /// <summary>
    /// Creates and binds a new Vertex Array Object (VAO) for OpenGL rendering.
    /// This method generates a unique VAO handle and binds it as the active VAO
    /// to configure and manage vertex attribute states efficiently.
    /// </summary>
    private void CreateVao()
    {
        _vao = GL.GenVertexArray();
        GL.BindVertexArray( _vao );
    }

    /// <summary>
    /// Creates a Vertex Buffer Object (VBO) for dynamic sprite batch rendering.
    /// </summary>
    /// <param name="size">The number of sprites that the VBO can accommodate.</param>
    private void CreateVbo( int size )
    {
        _vbo = GL.GenBuffer();
        GL.BindBuffer( ( int )BufferTarget.ArrayBuffer, _vbo );
        Vertices = new float[ size * VERTICES_PER_SPRITE * Sprite.VERTEX_SIZE ];

        GL.BufferData( ( int )BufferTarget.ArrayBuffer,
                       Vertices.Length * sizeof( float ),
                       0,
                       ( int )BufferUsageHint.DynamicDraw );
    }

    /// <summary>
    /// Creates and initializes an Element Buffer Object (EBO) using the specified size.
    /// The EBO is filled with index data used for rendering multiple sprites in a batch.
    /// </summary>
    /// <param name="size">The number of sprites to allocate indices for in the EBO.</param>
    private unsafe void CreateEbo( int size )
    {
        var indices = new uint[ size * INDICES_PER_SPRITE ];

        for ( uint i = 0, vertex = 0; i < indices.Length; i += INDICES_PER_SPRITE, vertex += 4 )
        {
            indices[ i + 0 ] = vertex + 0;
            indices[ i + 1 ] = vertex + 1;
            indices[ i + 2 ] = vertex + 2;
            indices[ i + 3 ] = vertex + 2;
            indices[ i + 4 ] = vertex + 1;
            indices[ i + 5 ] = vertex + 3;
        }

        _ebo = GL.GenBuffer();
        GL.BindBuffer( ( int )BufferTarget.ElementArrayBuffer, _ebo );

        fixed ( uint* ptr = indices )
        {
            GL.BufferData( ( int )BufferTarget.ElementArrayBuffer,
                           indices.Length * sizeof( uint ),
                           ( IntPtr )ptr,
                           ( int )BufferUsageHint.StaticDraw );
        }
    }

    // ========================================================================

    #region Drawing methods

    /// <summary>
    /// Draw the supplied Texture at the given coordinates. The texture will be
    /// of the specified width and height.
    /// </summary>
    /// <param name="texture"> The texture. </param>
    /// <param name="posX"> X coordinaste in pixels. </param>
    /// <param name="posY"> Y coordinate in pixels. </param>
    /// <param name="width"> Width of Texture in pixels. </param>
    /// <param name="height"> Height of Texture in pixerls. </param>
    public virtual void Draw( Texture texture, float posX, float posY, float width, float height )
    {
        lock ( _lockObject )
        {
            Validate( texture );

            if ( texture != LastTexture )
            {
                SwitchTexture( texture );
            }
            else if ( Idx > ( Vertices.Length - ( VERTICES_PER_SPRITE * VertexConstants.VERTEX_SIZE ) ) )
            {
                Flush();
            }

            var fx2 = posX + width;
            var fy2 = posY + height;

            const float U  = 0;
            const float V  = 1;
            const float U2 = 1;
            const float V2 = 0;

            SetVertices( posX, posY, ColorPackedRGBA, U, V2,
                         posX, fy2, ColorPackedRGBA, U, V,
                         fx2, fy2, ColorPackedRGBA, U2, V,
                         fx2, posY, ColorPackedRGBA, U2, V2 );
        }
    }

    /// <summary>
    /// Draws a textured region with specified transformations such as position, scale, rotation, and flipping options.
    /// </summary>
    /// <param name="texture">The texture to be drawn.</param>
    /// <param name="region">The region where the texture will be drawn on the target.</param>
    /// <param name="origin">The origin point of the region for transformations like rotation and scaling.</param>
    /// <param name="scale">The scaling factor for the texture in the X and Y axes.</param>
    /// <param name="rotation">The rotation angle of the texture in radians.</param>
    /// <param name="src">The source rectangle of the texture to be drawn.</param>
    /// <param name="flipX">Indicates whether the texture should be flipped horizontally.</param>
    /// <param name="flipY">Indicates whether the texture should be flipped vertically.</param>
    public virtual void Draw( Texture texture,
                              GRect region,
                              Point2D origin,
                              Point2D scale,
                              float rotation,
                              GRect src,
                              bool flipX,
                              bool flipY )
    {
        lock ( _lockObject )
        {
            Validate( texture );

            if ( texture != LastTexture )
            {
                SwitchTexture( texture );
            }
            else if ( Idx > ( Vertices.Length - ( VERTICES_PER_SPRITE * VertexConstants.VERTEX_SIZE ) ) )
            {
                Flush();
            }

            // bottom left and top right corner points relative to origin
            var worldOriginX = region.X + origin.X;
            var worldOriginY = region.Y + origin.Y;
            var fx           = -origin.X;
            var fy           = -origin.Y;
            var fx2          = region.Width - origin.X;
            var fy2          = region.Height - origin.Y;

            // scale
            if ( ( scale.X != 1 ) || ( scale.Y != 1 ) )
            {
                fx  *= scale.X;
                fy  *= scale.Y;
                fx2 *= scale.X;
                fy2 *= scale.Y;
            }

            // construct corner points, start from top left and go counter clockwise
            var p1X = fx;
            var p1Y = fy;
            var p2X = fx;
            var p2Y = fy2;
            var p3X = fx2;
            var p3Y = fy2;
            var p4X = fx2;
            var p4Y = fy;

            float x1, y1, x2, y2, x3, y3, x4, y4;

            // rotate
            if ( rotation != 0 )
            {
                var cos = MathUtils.CosDeg( rotation );
                var sin = MathUtils.SinDeg( rotation );

                x1 = ( cos * p1X ) - ( sin * p1Y );
                y1 = ( sin * p1X ) + ( cos * p1Y );

                x2 = ( cos * p2X ) - ( sin * p2Y );
                y2 = ( sin * p2X ) + ( cos * p2Y );

                x3 = ( cos * p3X ) - ( sin * p3Y );
                y3 = ( sin * p3X ) + ( cos * p3Y );

                x4 = x1 + ( x3 - x2 );
                y4 = y3 - ( y2 - y1 );
            }
            else
            {
                x1 = p1X;
                y1 = p1Y;

                x2 = p2X;
                y2 = p2Y;

                x3 = p3X;
                y3 = p3Y;

                x4 = p4X;
                y4 = p4Y;
            }

            x1 += worldOriginX;
            y1 += worldOriginY;
            x2 += worldOriginX;
            y2 += worldOriginY;
            x3 += worldOriginX;
            y3 += worldOriginY;
            x4 += worldOriginX;
            y4 += worldOriginY;

            var u  = src.X * InvTexWidth;
            var v  = ( src.Y + src.Height ) * InvTexHeight;
            var u2 = ( src.X + src.Width ) * InvTexWidth;
            var v2 = src.Y * InvTexHeight;

            if ( flipX )
            {
                ( u, u2 ) = ( u2, u );
            }

            if ( flipY )
            {
                ( v, v2 ) = ( v2, v );
            }

            SetVertices( x1, y1, ColorPackedRGBA, u, v2,
                         x2, y2, ColorPackedRGBA, u, v,
                         x3, y3, ColorPackedRGBA, u2, v,
                         x4, y4, ColorPackedRGBA, u2, v2 );
        }
    }

    /// <summary>
    /// Draws a specified texture within a defined region, with optional flipping along both axes.
    /// </summary>
    /// <param name="texture">The texture to be drawn.</param>
    /// <param name="region">The destination rectangle on the target surface.</param>
    /// <param name="src">The source rectangle in the texture to be drawn.</param>
    /// <param name="flipX">Indicates whether to flip the texture horizontally.</param>
    /// <param name="flipY">Indicates whether to flip the texture vertically.</param>
    public virtual void Draw( Texture texture, GRect region, GRect src, bool flipX = false, bool flipY = false )
    {
        lock ( _lockObject )
        {
            Validate( texture );

            if ( texture != LastTexture )
            {
                SwitchTexture( texture );
            }
            else if ( Idx > ( Vertices.Length - ( VERTICES_PER_SPRITE * VertexConstants.VERTEX_SIZE ) ) )
            {
                Flush();
            }

            var u   = src.X * InvTexWidth;
            var v   = ( src.Y + src.Height ) * InvTexHeight;
            var u2  = ( src.X + src.Width ) * InvTexWidth;
            var v2  = src.Y * InvTexHeight;
            var fx2 = region.X + region.Width;
            var fy2 = region.Y + region.Height;

            if ( flipX )
            {
                ( u, u2 ) = ( u2, u );
            }

            if ( flipY )
            {
                ( v, v2 ) = ( v2, v );
            }

            SetVertices( region.X, region.Y, ColorPackedRGBA, u, v2,
                         region.X, fy2, ColorPackedRGBA, u, v,
                         fx2, fy2, ColorPackedRGBA, u2, v,
                         fx2, region.Y, ColorPackedRGBA, u2, v2 );
        }
    }

    /// <summary>
    /// Draws a specified texture at the given position using the defined source rectangle.
    /// </summary>
    /// <param name="texture">The texture to be drawn.</param>
    /// <param name="x">The x-coordinate where the texture should be drawn.</param>
    /// <param name="y">The y-coordinate where the texture should be drawn.</param>
    /// <param name="src">The source rectangle portion of the texture to be drawn.</param>
    public virtual void Draw( Texture texture, float x, float y, GRect src )
    {
        lock ( _lockObject )
        {
            Validate( texture );

            if ( texture != LastTexture )
            {
                SwitchTexture( texture );
            }
            else if ( Idx > ( Vertices.Length - ( VERTICES_PER_SPRITE * VertexConstants.VERTEX_SIZE ) ) )
            {
                Flush();
            }

            var u   = src.X * InvTexWidth;
            var v   = ( src.Y + src.Height ) * InvTexHeight;
            var u2  = ( src.X + src.Width ) * InvTexWidth;
            var v2  = src.Y * InvTexHeight;
            var fx2 = x + src.Width;
            var fy2 = y + src.Height;

            SetVertices( x, y, ColorPackedRGBA, u, v2,
                         x, fy2, ColorPackedRGBA, u, v,
                         fx2, fy2, ColorPackedRGBA, u2, v,
                         fx2, y, ColorPackedRGBA, u2, v2 );
        }
    }

    /// <summary>
    /// Draws a textured rectangle on the screen using specified texture coordinates.
    /// </summary>
    /// <param name="texture">The texture to use for rendering.</param>
    /// <param name="region">The rectangular region where the texture will be drawn.</param>
    /// <param name="u">The U coordinate of the texture's top-left corner.</param>
    /// <param name="v">The V coordinate of the texture's top-left corner.</param>
    /// <param name="u2">The U coordinate of the texture's bottom-right corner.</param>
    /// <param name="v2">The V coordinate of the texture's bottom-right corner.</param>
    public virtual void Draw( Texture texture, GRect region, float u, float v, float u2, float v2 )
    {
        lock ( _lockObject )
        {
            Validate( texture );

            if ( texture != LastTexture )
            {
                SwitchTexture( texture );
            }
            else if ( Idx > ( Vertices.Length - ( VERTICES_PER_SPRITE * VertexConstants.VERTEX_SIZE ) ) )
            {
                Flush();
            }

            var fx2 = region.X + region.Width;
            var fy2 = region.Y + region.Height;

            SetVertices( region.X, region.Y, ColorPackedRGBA, u, v2,
                         region.X, fy2, ColorPackedRGBA, u, v,
                         fx2, fy2, ColorPackedRGBA, u2, v,
                         fx2, region.Y, ColorPackedRGBA, u2, v2 );
        }
    }

    /// <summary>
    /// Draw the given <see cref="Texture"/> at the given X and Y coordinates.
    /// </summary>
    /// <param name="texture"> The texture. </param>
    /// <param name="x"> X coordinate in pixels. </param>
    /// <param name="y"> Y coordinate in pixels. </param>
    public virtual void Draw( Texture? texture, float x, float y )
    {
        lock ( _lockObject )
        {
            if ( texture is null )
            {
                Logger.Debug( "Draw called with null texture" );

                return;
            }

            Draw( texture, x, y, texture.Width, texture.Height );
        }
    }

    /// <summary>
    /// Renders a set of sprite vertices using the specified texture.
    /// </summary>
    /// <param name="texture">The texture to be used for rendering. Can be null if not needed.</param>
    /// <param name="spriteVertices">An array of vertex data describing the sprites to be rendered.</param>
    /// <param name="offset">The starting index in the vertex array from which to begin processing.</param>
    /// <param name="count">The number of vertices to process starting from the specified offset.</param>
    public virtual void Draw( Texture? texture, float[] spriteVertices, int offset, int count )
    {
        lock ( _lockObject )
        {
            Validate( texture );

            var verticesLength    = Vertices.Length;
            var remainingVertices = verticesLength;

            if ( texture != LastTexture )
            {
                SwitchTexture( texture );
            }
            else if ( remainingVertices == 0 )
            {
                Flush();
                remainingVertices = verticesLength;
            }

            while ( count > 0 )
            {
                var copyCount = Math.Min( remainingVertices, count );
                Array.Copy( spriteVertices, offset, Vertices, Idx, copyCount );

                Idx    += copyCount;
                count  -= copyCount;
                offset += copyCount;

                if ( count > 0 )
                {
                    Flush();
                    remainingVertices = verticesLength;
                }
            }
        }
    }

    /// <summary>
    /// Draws the specified texture region at the given position.
    /// </summary>
    /// <param name="region">The texture region to be drawn. Can be null.</param>
    /// <param name="x">The x-coordinate of the position to draw the texture.</param>
    /// <param name="y">The y-coordinate of the position to draw the texture.</param>
    public virtual void Draw( TextureRegion? region, float x, float y )
    {
        lock ( _lockObject )
        {
            Validate( region );

            Draw( region, x, y, region!.RegionWidth, region.RegionHeight );
        }
    }

    /// <summary>
    /// Draws a texture region onto the batch with specified position and dimensions.
    /// </summary>
    /// <param name="region">The texture region to be drawn, which includes the texture and UV coordinates.</param>
    /// <param name="x">The X-coordinate of the bottom-left corner where the texture will be drawn.</param>
    /// <param name="y">The Y-coordinate of the bottom-left corner where the texture will be drawn.</param>
    /// <param name="width">The width of the texture region to be drawn.</param>
    /// <param name="height">The height of the texture region to be drawn.</param>
    public virtual void Draw( TextureRegion? region, float x, float y, float width, float height )
    {
        lock ( _lockObject )
        {
            Validate( region );

            var texture = region!.Texture;

            if ( texture != LastTexture )
            {
                SwitchTexture( texture );
            }

//            else if ( Idx > Vertices.Length )
            else if ( Idx > ( Vertices.Length - ( VERTICES_PER_SPRITE * VertexConstants.VERTEX_SIZE ) ) )
            {
                Flush();
            }

            var fx2 = x + width;
            var fy2 = y + height;
            var u   = region.U;
            var v   = region.V2;
            var u2  = region.U2;
            var v2  = region.V;

            SetVertices( x, y, ColorPackedRGBA, u, v2,
                         x, fy2, ColorPackedRGBA, u, v,
                         fx2, fy2, ColorPackedRGBA, u2, v,
                         fx2, y, ColorPackedRGBA, u2, v2 );
        }
    }

    /// <summary>
    /// Draws a texture region onto a specified region with transformations such as origin offset,
    /// scaling, and rotation.
    /// </summary>
    /// <param name="textureRegion">The texture region to be drawn.</param>
    /// <param name="region">The rectangular region where the texture will be drawn.</param>
    /// <param name="origin">The origin point for transformations such as scaling and rotation.</param>
    /// <param name="scale">The scale factor to be applied to the texture region.</param>
    /// <param name="rotation">The rotation angle in radians to be applied to the texture region.</param>
    public virtual void Draw( TextureRegion? textureRegion, GRect region, Point2D origin, Point2D scale, float rotation )
    {
        lock ( _lockObject )
        {
            Validate( textureRegion );

            var texture = textureRegion!.Texture;

            if ( texture != LastTexture )
            {
                SwitchTexture( texture );
            }

//            else if ( Idx > Vertices.Length )
            else if ( Idx > ( Vertices.Length - ( VERTICES_PER_SPRITE * VertexConstants.VERTEX_SIZE ) ) )
            {
                Flush();
            }

            // bottom left and top right corner points relative to origin
            var worldOriginX = region.X + origin.X;
            var worldOriginY = region.Y + origin.Y;
            var fx           = -origin.X;
            var fy           = -origin.Y;
            var fx2          = region.Width - origin.X;
            var fy2          = region.Height - origin.Y;

            // scale
            if ( ( Math.Abs( scale.X - 1 ) > 0 ) || ( Math.Abs( scale.Y - 1 ) > 0 ) )
            {
                fx  *= scale.X;
                fy  *= scale.Y;
                fx2 *= scale.X;
                fy2 *= scale.Y;
            }

            // construct corner points, start from top left and go counter clockwise
            var p1X = fx;
            var p1Y = fy;
            var p2X = fx;
            var p2Y = fy2;
            var p3X = fx2;
            var p3Y = fy2;
            var p4X = fx2;
            var p4Y = fy;

            float x1, y1, x2, y2, x3, y3, x4, y4;

            // rotate
            if ( rotation != 0 )
            {
                var cos = MathUtils.CosDeg( rotation );
                var sin = MathUtils.SinDeg( rotation );

                x1 = ( cos * p1X ) - ( sin * p1Y );
                y1 = ( sin * p1X ) + ( cos * p1Y );

                x2 = ( cos * p2X ) - ( sin * p2Y );
                y2 = ( sin * p2X ) + ( cos * p2Y );

                x3 = ( cos * p3X ) - ( sin * p3Y );
                y3 = ( sin * p3X ) + ( cos * p3Y );

                x4 = x1 + ( x3 - x2 );
                y4 = y3 - ( y2 - y1 );
            }
            else
            {
                x1 = p1X;
                y1 = p1Y;

                x2 = p2X;
                y2 = p2Y;

                x3 = p3X;
                y3 = p3Y;

                x4 = p4X;
                y4 = p4Y;
            }

            x1 += worldOriginX;
            y1 += worldOriginY;
            x2 += worldOriginX;
            y2 += worldOriginY;
            x3 += worldOriginX;
            y3 += worldOriginY;
            x4 += worldOriginX;
            y4 += worldOriginY;

            SetVertices( x1, y1, ColorPackedRGBA, textureRegion.U, textureRegion.V2,
                         x2, y2, ColorPackedRGBA, textureRegion.U, textureRegion.V,
                         x3, y3, ColorPackedRGBA, textureRegion.U2, textureRegion.V,
                         x4, y4, ColorPackedRGBA, textureRegion.U2, textureRegion.V2 );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="textureRegion"></param>
    /// <param name="region"></param>
    /// <param name="origin"></param>
    /// <param name="scale"></param>
    /// <param name="rotation"></param>
    /// <param name="clockwise"></param>
    public virtual void Draw( TextureRegion textureRegion,
                              GRect region,
                              Point2D origin,
                              Point2D scale,
                              float rotation,
                              bool clockwise )
    {
        lock ( _lockObject )
        {
            Validate( textureRegion );

            var texture = textureRegion.Texture;

            if ( texture != LastTexture )
            {
                SwitchTexture( texture );
            }

//            else if ( Idx > Vertices.Length )
            else if ( Idx > ( Vertices.Length - ( VERTICES_PER_SPRITE * VertexConstants.VERTEX_SIZE ) ) )
            {
                Flush();
            }

            // bottom left and top right corner points relative to origin
            var worldOriginX = region.X + origin.X;
            var worldOriginY = region.Y + origin.Y;
            var fx           = -origin.X;
            var fy           = -origin.Y;
            var fx2          = region.Width - origin.X;
            var fy2          = region.Height - origin.Y;

            // scale
            if ( ( Math.Abs( scale.X - 1 ) > 0 ) || ( Math.Abs( scale.Y - 1 ) > 0 ) )
            {
                fx  *= scale.X;
                fy  *= scale.Y;
                fx2 *= scale.X;
                fy2 *= scale.Y;
            }

            // construct corner points.
            // start from top left and go counter clockwise

            // -- Top left --
            var p1X = fx;
            var p1Y = fy;

            // -- Bottom left --
            var p2X = fx;
            var p2Y = fy2;

            // -- Bottom right --
            var p3X = fx2;
            var p3Y = fy2;

            // -- Top right --
            var p4X = fx2;
            var p4Y = fy;

            float x1, y1, x2, y2, x3, y3, x4, y4;

            // rotate
            if ( rotation != 0 )
            {
                var cos = MathUtils.CosDeg( rotation );
                var sin = MathUtils.SinDeg( rotation );

                x1 = ( cos * p1X ) - ( sin * p1Y );
                y1 = ( sin * p1X ) + ( cos * p1Y );

                x2 = ( cos * p2X ) - ( sin * p2Y );
                y2 = ( sin * p2X ) + ( cos * p2Y );

                x3 = ( cos * p3X ) - ( sin * p3Y );
                y3 = ( sin * p3X ) + ( cos * p3Y );

                x4 = x1 + ( x3 - x2 );
                y4 = y3 - ( y2 - y1 );
            }
            else
            {
                x1 = p1X;
                y1 = p1Y;

                x2 = p2X;
                y2 = p2Y;

                x3 = p3X;
                y3 = p3Y;

                x4 = p4X;
                y4 = p4Y;
            }

            x1 += worldOriginX;
            y1 += worldOriginY;
            x2 += worldOriginX;
            y2 += worldOriginY;
            x3 += worldOriginX;
            y3 += worldOriginY;
            x4 += worldOriginX;
            y4 += worldOriginY;

            float u1, v1, u2, v2, u3, v3, u4, v4;

            if ( clockwise )
            {
                u1 = textureRegion.U2;
                v1 = textureRegion.V2;
                u2 = textureRegion.U;
                v2 = textureRegion.V2;
                u3 = textureRegion.U;
                v3 = textureRegion.V;
                u4 = textureRegion.U2;
                v4 = textureRegion.V;
            }
            else // TODO: Check the orders of V and V2 here
            {
                u1 = textureRegion.U;
                v1 = textureRegion.V;
                u2 = textureRegion.U2;
                v2 = textureRegion.V;
                u3 = textureRegion.U2;
                v3 = textureRegion.V2;
                u4 = textureRegion.U;
                v4 = textureRegion.V2;
            }

            SetVertices( x1, y1, ColorPackedRGBA, u1, v1,
                         x2, y2, ColorPackedRGBA, u2, v2,
                         x3, y3, ColorPackedRGBA, u3, v3,
                         x4, y4, ColorPackedRGBA, u4, v4 );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="region"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="transform"></param>
    public virtual void Draw( TextureRegion region, float width, float height, Affine2 transform )
    {
        lock ( _lockObject )
        {
            Validate( region );

            if ( region.Texture != LastTexture )
            {
                SwitchTexture( region.Texture );
            }

//            else if ( Idx > Vertices.Length )
            else if ( Idx > ( Vertices.Length - ( VERTICES_PER_SPRITE * VertexConstants.VERTEX_SIZE ) ) )
            {
                Flush();
            }

            // construct corner points
            var x1 = transform.M02;
            var y1 = transform.M12;
            var x2 = ( transform.M01 * height ) + transform.M02;
            var y2 = ( transform.M11 * height ) + transform.M12;
            var x3 = ( transform.M00 * width ) + ( transform.M01 * height ) + transform.M02;
            var y3 = ( transform.M10 * width ) + ( transform.M11 * height ) + transform.M12;
            var x4 = ( transform.M00 * width ) + transform.M02;
            var y4 = ( transform.M10 * width ) + transform.M12;

            SetVertices( x1, y1, ColorPackedRGBA, region.U, region.V2,
                         x2, y2, ColorPackedRGBA, region.U, region.V,
                         x3, y3, ColorPackedRGBA, region.U2, region.V,
                         x4, y4, ColorPackedRGBA, region.U2, region.V2 );
        }
    }

    #endregion Drawing methods

    // ========================================================================

    #region Color handling

    /// <summary>
    /// Represents the current drawing color for the SpriteBatch.
    /// This property is used to tint textures and shapes drawn by the SpriteBatch.
    /// </summary>
    public Color Color
    {
        get => _color;
        set => SetColor( value.R, value.G, value.B, value.A );
    }

    /// <summary>
    /// Sets the Color for this SpriteBatch to the supplied Color.
    /// </summary>
    public void SetColor( Color tint )
    {
        SetColor( tint.R, tint.G, tint.B, tint.A );
    }

    /// <summary>
    /// Sets the Color for this SpriteBatch using the supplied RGBA Color components.
    /// </summary>
    /// <param name="r"> Red. </param>
    /// <param name="g"> Green. </param>
    /// <param name="b"> Blue. </param>
    /// <param name="a"> Alpha. </param>
    public void SetColor( float r, float g, float b, float a )
    {
        _color.Set( r, g, b, a );
    }

    /// <summary>
    /// This batch's Color packed into a float ABGR format.
    /// </summary>
    public float ColorPackedABGR => Color.ToFloatBitsABGR( Color.A, Color.B, Color.G, Color.R );

    /// <summary>
    /// This batch's Color packed into a float RGBA format.
    /// </summary>
    public float ColorPackedRGBA => Color.ToFloatBitsRGBA( Color.R, Color.G, Color.B, Color.A );

    public void DebugVertices()
    {
        for ( var i = 0; i < ( VERTICES_PER_SPRITE * VertexConstants.VERTEX_SIZE ); i++ )
        {
            if ( i is 2 or 7 or 12 or 17 )
            {
                Logger.Debug( $"Vertices[{i}]: {( uint )Vertices[ i ]:X}" );
            }
            else
            {
                Logger.Debug( $"Vertices[{i}]: {Vertices[ i ]}" );
            }
        }

        Logger.Debug( $"Idx: {Idx}" );

        // Verify texture coordinates in vertex data
        Logger.Debug( "Texture coordinates:" );
        Logger.Debug( $"UV1: ({Vertices[ IBatch.U1 ]}, {Vertices[ IBatch.V1 ]})" );
        Logger.Debug( $"UV2: ({Vertices[ IBatch.U2 ]}, {Vertices[ IBatch.V2 ]})" );
        Logger.Debug( $"UV3: ({Vertices[ IBatch.U3 ]}, {Vertices[ IBatch.V3 ]})" );
        Logger.Debug( $"UV4: ({Vertices[ IBatch.U4 ]}, {Vertices[ IBatch.V4 ]})" );
    }

    /// <summary>
    /// Unpacks a packed color value into its red, green, blue, and alpha components.
    /// </summary>
    /// <param name="packedColor">The packed color value in ABGR format as a float.</param>
    /// <param name="r">The output red component, normalized between 0.0 and 1.0.</param>
    /// <param name="g">The output green component, normalized between 0.0 and 1.0.</param>
    /// <param name="b">The output blue component, normalized between 0.0 and 1.0.</param>
    /// <param name="a">The output alpha component, normalized between 0.0 and 1.0.</param>
    private static void UnpackColor( float packedColor, out float r, out float g, out float b, out float a )
    {
        if ( float.IsNaN( packedColor ) || float.IsInfinity( packedColor ) )
        {
            // Set a default color (white)
            r = 1.0f;
            g = 1.0f;
            b = 1.0f;
            a = 1.0f;

            return;
        }

        packedColor = Math.Clamp( packedColor, 0, uint.MaxValue );

        var color = ( uint )packedColor;

        b = ( ( color >> 0 ) & 0xFFu ) / 255.0f;
        g = ( ( color >> 8 ) & 0xFFu ) / 255.0f;
        r = ( ( color >> 16 ) & 0xFFu ) / 255.0f;
        a = ( ( color >> 24 ) & 0xFFu ) / 255.0f;
    }

    #endregion Color handling

    // ========================================================================

    /// <inheritdoc />
    public void Dispose()
    {
        lock ( _lockObject )
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }
    }

    protected void Dispose( bool disposing )
    {
        if ( !_disposed )
        {
            if ( disposing )
            {
                UnbindBuffers();

                _mesh?.Dispose();

                if ( _ownsShader && ( _shader != null ) )
                {
                    _shader.Dispose();
                }

                _mesh   = null;
                _shader = null;
            }

            _disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        if ( _disposed )
        {
            throw new ObjectDisposedException( nameof( SpriteBatch ) );
        }
    }

    ~SpriteBatch()
    {
        Dispose( false );
    }

    // ========================================================================

    /// <summary>
    ///
    /// </summary>
    [PublicAPI]
    public enum BatchState
    {
        Ready,
        Drawing,
        Disposed,
    }

    // ========================================================================

    /// <summary>
    /// The RenderState record struct encapsulates rendering-related details within a sprite
    /// batch operation. It includes active texture information, the number of vertices to be
    /// rendered, and the transformation matrix applied during the rendering phase.
    /// </summary>
    [PublicAPI]
    public record struct RenderState
    {
        public Texture? CurrentTexture  { get; set; }
        public int      VertexCount     { get; set; }
        public Matrix4  TransformMatrix { get; set; }

        /// <summary>
        /// Represents the rendering state within a sprite batch, including texture information,
        /// vertex data, and transformation details.
        /// </summary>
        public RenderState( Texture? currentTexture, int vertexCount, Matrix4 transformMatrix )
        {
            CurrentTexture  = currentTexture;
            VertexCount     = vertexCount;
            TransformMatrix = transformMatrix;
        }

        private void Deconstruct( out Texture? texture, out int vertexCount, out Matrix4 transformMatrix )
        {
            texture         = CurrentTexture;
            vertexCount     = VertexCount;
            transformMatrix = TransformMatrix;
        }
    }

    // ========================================================================
    /// <summary>
    /// The Vertex struct represents a vertex in 2D space, containing position, texture
    /// coordinate, and color information.It is commonly used in graphical rendering contexts
    /// to define attributes of points in a rendered shape or mesh.
    /// </summary>
    [PublicAPI]
    public readonly record struct Vertex
    {
        public Vector2 Position          { get; init; }
        public Vector2 TextureCoordinate { get; init; }
        public Color   Color             { get; init; }

        public Vertex( Vector2 position, Vector2 textureCoordinate, Color color )
        {
            Position          = position;
            TextureCoordinate = textureCoordinate;
            Color             = color;
        }
    }
}

// ============================================================================
// ============================================================================