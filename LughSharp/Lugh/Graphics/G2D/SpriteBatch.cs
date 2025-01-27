﻿// ///////////////////////////////////////////////////////////////////////////////
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

using LughSharp.Lugh.Graphics.GLUtils;
using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Graphics.OpenGL.Enums;
using LughSharp.Lugh.Maths;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

using Matrix4 = LughSharp.Lugh.Maths.Matrix4;

namespace LughSharp.Lugh.Graphics.G2D;

// ============================================================================

[PublicAPI]
public class SpriteBatch : IBatch
{
    public bool    BlendingDisabled  { get; set; }         = false;
    public float   InvTexHeight      { get; set; }         = 0;
    public float   InvTexWidth       { get; set; }         = 0;
    public int     BlendSrcFunc      { get; private set; } = ( int )BlendingFactor.SrcColor;
    public int     BlendDstFunc      { get; private set; } = ( int )BlendingFactor.DstColor;
    public int     BlendSrcFuncAlpha { get; private set; } = ( int )BlendingFactor.OneMinusSrcAlpha;
    public int     BlendDstFuncAlpha { get; private set; } = ( int )BlendingFactor.OneMinusDstAlpha;
    public Matrix4 ProjectionMatrix  { get; }              = new();
    public Matrix4 TransformMatrix   { get; }              = new();
    public bool    IsDrawing         { get; set; }         = false;

    public int  RenderCalls       { get; set; } = 0; // Number of render calls since the last call to Begin()
    public long TotalRenderCalls  { get; set; } = 0; // Number of rendering calls, ever. Will not be reset unless set manually.
    public int  MaxSpritesInBatch { get; set; } = 0; // The maximum number of sprites rendered in one batch so far.

    // ========================================================================

    protected Texture? LastTexture { get; set; } = null;
    protected float[]  Vertices    { get; set; }
    protected int      Idx         { get; set; } = 0;

    #if DEBUG
    public int GetIdx() => Idx;
    #endif

    // ========================================================================

    private const int VERTICES_PER_SPRITE = 4; // Number of vertices per sprite (quad)
    private const int INDICES_PER_SPRITE  = 6; // Number of indices per sprite (two triangles)
    private const int VERTEX_SIZE         = 8; // Number of floats per vertex (x, y, color, u, v)
    private const int MAX_VERTEX_INDEX    = 32767;
    private const int MAX_SPRITES         = 8191; // 32767 is max vertex index, so 32767 / 4 vertices per sprite = 8191 sprites max.

    private readonly Color _color = Graphics.Color.Red;
    private          bool  _ownsShader;

    private Matrix4        _combinedMatrix = new();
    private Mesh?          _mesh;
    private ShaderProgram? _shader;
    private Texture?       _lastSuccessfulTexture = null;
    private int            _nullTextureCount      = 0;
    private ShaderProgram? _customShader          = null;
    private uint           _vao;
    private uint           _vbo;
    private uint           _ibo;

    private int _vertexSizeInFloats;

    // ========================================================================

    /// <summary>
    /// Constructs a new SpriteBatch with a size of 1000, one buffer, and the default shader.
    /// </summary>
    public SpriteBatch() : this( 1000 )
    {
    }

    /// <summary>
    /// Constructs a new SpriteBatch. Sets the projection matrix to an orthographic projection with
    /// y-axis point upwards, x-axis point to the right and the origin being in the bottom left corner
    /// of the screen. The projection will be pixel perfect with respect to the current screen resolution.
    /// The defaultShader specifies the shader to use. Note that the names for uniforms for this default
    /// shader are different than the ones expect for shaders set with <see cref="Shader"/>.
    /// See <see cref="CreateDefaultShader()"/>.
    /// </summary>
    /// <param name="size"> The max number of sprites in a single batch. Max of 8191. </param>
    /// <param name="defaultShader">
    /// The default shader to use. This is not owned by the SpriteBatch and must be disposed separately.
    /// </param>
    protected SpriteBatch( int size, ShaderProgram? defaultShader = null )
    {
        if ( size > MAX_SPRITES )
        {
            throw new ArgumentException( $"Can't have more than 8191 sprites per batch: {size}" );
        }

        IsDrawing = false;
        Vertices  = new float[ size * VERTICES_PER_SPRITE * VERTEX_SIZE ];

        Initialise( size, defaultShader );
    }

    /// <summary>
    /// Takes away messy vertex attributes initialisation from the constructor, the GL side of
    /// which is done inside <see cref="SetupVertexAttributes"/>, 
    /// </summary>
    /// <param name="size"></param>
    /// <param name="defaultShader"></param>
    private unsafe void Initialise( int size, ShaderProgram? defaultShader )
    {
        OpenGL.GLUtils.CheckOpenGLContext();
        
        var vertexDataType = ( GdxApi.Bindings.GetOpenGLVersion().major >= 3 )
            ? Mesh.VertexDataType.VertexBufferObjectWithVAO
            : Mesh.VertexDataType.VertexArray;

        // Initialize the mesh with vertex attributes for position,
        // color, and texture coordinates
        var va1 = new VertexAttribute( ( int )VertexAttributes.Usage.POSITION, 2, ShaderProgram.POSITION_ATTRIBUTE );
        var va2 = new VertexAttribute( ( int )VertexAttributes.Usage.COLOR_PACKED, 4, ShaderProgram.COLOR_ATTRIBUTE );
        var va3 = new VertexAttribute( ( int )VertexAttributes.Usage.TEXTURE_COORDINATES, 2, $"{ShaderProgram.TEXCOORD_ATTRIBUTE}0" );

        _mesh = new Mesh( vertexDataType, false, size * VERTICES_PER_SPRITE, size * INDICES_PER_SPRITE, va1, va2, va3 );

        // Set up an orthographic projection matrix for 2D rendering
        ProjectionMatrix.SetToOrtho2D( 0, 0, GdxApi.Graphics.Width, GdxApi.Graphics.Height );

        PopulateIndexBuffer( size, out var indices );

        // Set the indices on the mesh
        _mesh.SetIndices( indices );

        // Calculate the vertex size in floats
        _vertexSizeInFloats = _mesh.VertexAttributes!.VertexSize / sizeof( float );

        // ------------------------------------------------
        // Generate and bind the Vertex Array Object (VAO)
        _vao = GdxApi.Bindings.GenVertexArray();
        GdxApi.Bindings.BindVertexArray( _vao );

        // ------------------------------------------------
        // Generate and bind the Vertex Buffer Object (VBO)
        _vbo  = GdxApi.Bindings.GenBuffer();
        GdxApi.Bindings.BindBuffer( ( int ) BufferTarget.ArrayBuffer, _vbo );

        CheckBufferBinding( _vbo );

        // ------------------------------------------------
        // 
        GdxApi.Bindings.BufferData( target: ( int )BufferTarget.ArrayBuffer,
                                    size: Vertices.Length * sizeof( float ),
                                    data: ( void* )0,
                                    usage: ( int )BufferUsageHint.StaticDraw );

        GdxApi.Bindings.BindVertexArray( 0 );                             // Unbind VAO
        GdxApi.Bindings.BindBuffer( ( int )BufferTarget.ArrayBuffer, 0 ); // Unbind VBO

        if ( defaultShader == null )
        {
            _shader     = CreateDefaultShader();
            _ownsShader = true;
        }
        else
        {
            _shader = defaultShader;
        }
    }

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
    /// Performs the OpenGL side of Vertex Attribute initialisation.
    /// </summary>
    private void SetupVertexAttributes( ShaderProgram? program )
    {
        OpenGL.GLUtils.CheckOpenGLContext();

        // --------------------------------------------------------------------

        const int stride         = VERTEX_SIZE * sizeof( float );
        const int positionOffset = 0;
        const int colorOffset    = 2 * sizeof( float );
        const int texCoordOffset = 6 * sizeof( float );

        // --------------------------------------------------------------------

        if ( ( program == null ) || ( _mesh == null ) ) return;

        program.Bind();
        
        GdxApi.Bindings.BindVertexArray( _vao );
        GdxApi.Bindings.BindBuffer( ( int )BufferTarget.ArrayBuffer, _vbo );

        // Position Attribute
        var positionLocation = program.GetAttributeLocation( "a_position" );

        if ( positionLocation >= 0 )
        {
            program.EnableVertexAttribute( positionLocation );
            program.SetVertexAttribute( positionLocation, 2, IGL.GL_FLOAT, false, stride, positionOffset );
        }

        // Color Attribute
        var colorLocation = program.GetAttributeLocation( "a_colorPacked" );

        if ( colorLocation >= 0 )
        {
            program.EnableVertexAttribute( colorLocation );
            program.SetVertexAttribute( colorLocation, 4, IGL.GL_FLOAT, false, stride, colorOffset );
        }

        // Texture Coordinates Attribute
        var texCoordLocation = program.GetAttributeLocation( "a_texCoords" );

        if ( texCoordLocation >= 0 )
        {
            program.EnableVertexAttribute( texCoordLocation );
            program.SetVertexAttribute( texCoordLocation, 2, IGL.GL_FLOAT, false, stride, texCoordOffset );
        }

        GdxApi.Bindings.BindBuffer( ( int )BufferTarget.ArrayBuffer, 0 );
        GdxApi.Bindings.BindVertexArray( 0 );
        program.Unbind();
    }

    /// <summary>
    /// Begins a new sprite batch.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this method is called before a call to <see cref="End"/>.
    /// </exception>
    public void Begin()
    {
        if ( IsDrawing )
        {
            throw new InvalidOperationException( "SpriteBatch.End() must be called before Begin()" );
        }

        RenderCalls = 0;

        GdxApi.Bindings.DepthMask( false );

        if ( _customShader != null )
        {
            _customShader.Bind();
        }
        else
        {
            _shader?.Bind();
        }

        SetupMatrices();

        IsDrawing = true;
    }

    /// <summary>
    /// Flushes all batched text, textures and sprites to the screen. 
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this method is called BEFORE a call to <see cref="Begin"/>
    /// </exception>
    public void End()
    {
        if ( !IsDrawing )
        {
            throw new InvalidOperationException( "SpriteBatch.begin must be called before end." );
        }

        if ( Idx > 0 )
        {
            Flush();
        }

        LastTexture = null;
        IsDrawing   = false;

        GdxApi.Bindings.DepthMask( true );

        if ( IsBlendingEnabled )
        {
            GdxApi.Bindings.Disable( IGL.GL_BLEND );
        }

        _shader?.Unbind();
    }

    /// <summary>
    /// Sets the Color for this SpriteBatch to the supplied Color.
    /// </summary>
    public void SetColor( Color tint ) => SetColor( tint.R, tint.G, tint.B, tint.A );

    /// <summary>
    /// Sets the Color for this SpriteBatch using the supplied
    /// RGBA Color components.
    /// </summary>
    /// <param name="r"> Red. </param>
    /// <param name="g"> Green. </param>
    /// <param name="b"> Blue. </param>
    /// <param name="a"> Alpha. </param>
    public void SetColor( float r, float g, float b, float a ) => _color.Set( r, g, b, a );

    /// <summary>
    /// 
    /// </summary>
    public Color Color
    {
        get => _color;
        set => SetColor( value.R, value.G, value.B, value.A );
    }

    /// <summary>
    /// This batch's Color packed into a float ABGR format.
    /// </summary>
    public float ColorPackedABGR
    {
        get => Color.ToFloatBitsABGR( Color.A, Color.B, Color.G, Color.R );
        set { }
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
        Validate( texture );

        if ( texture != LastTexture )
        {
            SwitchTexture( texture );
        }
        else if ( Idx > ( Vertices.Length - ( VERTICES_PER_SPRITE * VERTEX_SIZE ) ) )
        {
            Flush();
        }

        var fx2 = posX + width;
        var fy2 = posY + height;

        const float U  = 0;
        const float V  = 1;
        const float U2 = 1;
        const float V2 = 0;

        SetVertices( posX, posY, Color.A, Color.B, Color.G, Color.R, U, V,
                     posX, fy2, Color.A, Color.B, Color.G, Color.R, U, V2,
                     fx2, fy2, Color.A, Color.B, Color.G, Color.R, U2, V2,
                     fx2, posY, Color.A, Color.B, Color.G, Color.R, U2, V );
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
        Validate( texture );

        if ( texture != LastTexture )
        {
            SwitchTexture( texture );
        }
        else if ( Idx == Vertices.Length )
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

        SetVertices( x1, y1, Color.A, Color.B, Color.G, Color.R, u, v,
                     x2, y2, Color.A, Color.B, Color.G, Color.R, u, v2,
                     x3, y3, Color.A, Color.B, Color.G, Color.R, u2, v2,
                     x4, y4, Color.A, Color.B, Color.G, Color.R, u2, v );
    }

    /// <summary>
    /// Draws a specified texture within a defined region, with optional flipping along both axes.
    /// </summary>
    /// <param name="texture">The texture to be drawn.</param>
    /// <param name="region">The destination rectangle on the target surface.</param>
    /// <param name="src">The source rectangle in the texture to be drawn.</param>
    /// <param name="flipX">Indicates whether to flip the texture horizontally.</param>
    /// <param name="flipY">Indicates whether to flip the texture vertically.</param>
    public virtual void Draw( Texture texture, GRect region, GRect src, bool flipX, bool flipY )
    {
        Validate( texture );

        if ( texture != LastTexture )
        {
            SwitchTexture( texture );
        }
        else if ( Idx == Vertices.Length )
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

        SetVertices( region.X, region.Y, Color.A, Color.B, Color.G, Color.R, u, v,
                     region.X, fy2, Color.A, Color.B, Color.G, Color.R, u, v2,
                     fx2, fy2, Color.A, Color.B, Color.G, Color.R, u2, v2,
                     fx2, region.Y, Color.A, Color.B, Color.G, Color.R, u2, v );
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
        Validate( texture );

        if ( texture != LastTexture )
        {
            SwitchTexture( texture );
        }
        else if ( Idx == Vertices.Length )
        {
            Flush();
        }

        var u   = src.X * InvTexWidth;
        var v   = ( src.Y + src.Height ) * InvTexHeight;
        var u2  = ( src.X + src.Width ) * InvTexWidth;
        var v2  = src.Y * InvTexHeight;
        var fx2 = x + src.Width;
        var fy2 = y + src.Height;

        SetVertices( x, y, Color.A, Color.B, Color.G, Color.R, u, v,
                     x, fy2, Color.A, Color.B, Color.G, Color.R, u, v2,
                     fx2, fy2, Color.A, Color.B, Color.G, Color.R, u2, v2,
                     fx2, y, Color.A, Color.B, Color.G, Color.R, u2, v );
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
        Validate( texture );

        if ( texture != LastTexture )
        {
            SwitchTexture( texture );
        }
        else if ( Idx == Vertices.Length )
        {
            Flush();
        }

        var fx2 = region.X + region.Width;
        var fy2 = region.Y + region.Height;

        SetVertices( region.X, region.Y, Color.A, Color.B, Color.G, Color.R, u, v,
                     region.X, fy2, Color.A, Color.B, Color.G, Color.R, u, v2,
                     fx2, fy2, Color.A, Color.B, Color.G, Color.R, u2, v2,
                     fx2, region.Y, Color.A, Color.B, Color.G, Color.R, u2, v );
    }

    /// <summary>
    /// Draw the given <see cref="Texture"/> at the given X and Y coordinates.
    /// </summary>
    /// <param name="texture"> The texture. </param>
    /// <param name="x"> X coordinate in pixels. </param>
    /// <param name="y"> Y coordinate in pixels. </param>
    public virtual void Draw( Texture texture, float x, float y )
    {
        Draw( texture, x, y, texture.Width, texture.Height );
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
        Validate( texture );

        var verticesLength    = Vertices.Length;
        var remainingVertices = verticesLength;

        if ( texture != LastTexture )
        {
            SwitchTexture( texture );
        }
        else
        {
            remainingVertices -= Idx;

            if ( remainingVertices == 0 )
            {
                Flush();
                remainingVertices = verticesLength;
            }
        }

        var copyCount = Math.Min( remainingVertices, count );

        Array.Copy( spriteVertices, offset, Vertices, Idx, copyCount );

        Idx   += copyCount;
        count -= copyCount;

        while ( count > 0 )
        {
            offset += copyCount;

            Flush();

            copyCount = Math.Min( verticesLength, count );

            Array.Copy( spriteVertices, offset, Vertices, 0, copyCount );

            Idx   += copyCount;
            count -= copyCount;
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
        Validate( region );

        Draw( region, x, y, region!.RegionWidth, region.RegionHeight );
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
        Validate( region );

        var texture = region!.Texture;

        if ( texture != LastTexture )
        {
            SwitchTexture( texture );
        }
        else if ( Idx == Vertices.Length )
        {
            Flush();
        }

        var fx2 = x + width;
        var fy2 = y + height;
        var u   = region.U;
        var v   = region.V2;
        var u2  = region.U2;
        var v2  = region.V;

        SetVertices( x, y, Color.A, Color.B, Color.G, Color.R, u, v,
                     x, fy2, Color.A, Color.B, Color.G, Color.R, u, v2,
                     fx2, fy2, Color.A, Color.B, Color.G, Color.R, u2, v2,
                     fx2, y, Color.A, Color.B, Color.G, Color.R, u2, v );
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
        Validate( textureRegion );

        var texture = textureRegion!.Texture;

        if ( texture != LastTexture )
        {
            SwitchTexture( texture );
        }
        else if ( Idx == Vertices.Length )
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

        SetVertices( x1, y1, Color.A, Color.B, Color.G, Color.R, textureRegion.U, textureRegion.V2,
                     x2, y2, Color.A, Color.B, Color.G, Color.R, textureRegion.U, textureRegion.V,
                     x3, y3, Color.A, Color.B, Color.G, Color.R, textureRegion.U2, textureRegion.V,
                     x4, y4, Color.A, Color.B, Color.G, Color.R, textureRegion.U2, textureRegion.V2 );
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
        Validate( textureRegion );

        var texture = textureRegion.Texture;

        if ( texture != LastTexture )
        {
            SwitchTexture( texture );
        }
        else if ( Idx == Vertices.Length )
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
        else
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

        SetVertices( x1, y1, Color.A, Color.B, Color.G, Color.R, u1, v1,
                     x2, y2, Color.A, Color.B, Color.G, Color.R, u2, v2,
                     x3, y3, Color.A, Color.B, Color.G, Color.R, u3, v3,
                     x4, y4, Color.A, Color.B, Color.G, Color.R, u4, v4 );
    }

    /// <summary>
    /// </summary>
    /// <param name="region"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="transform"></param>
    public virtual void Draw( TextureRegion region, float width, float height, Affine2 transform )
    {
        Validate( region );

        if ( region.Texture != LastTexture )
        {
            SwitchTexture( region.Texture );
        }
        else if ( Idx == Vertices.Length )
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

        SetVertices( x1, y1, Color.A, Color.B, Color.G, Color.R, region.U, region.V2,
                     x2, y2, Color.A, Color.B, Color.G, Color.R, region.U, region.V,
                     x3, y3, Color.A, Color.B, Color.G, Color.R, region.U2, region.V,
                     x4, y4, Color.A, Color.B, Color.G, Color.R, region.U2, region.V2 );
    }

    #endregion Drawing methods

    // ========================================================================

    private void SetVertices( float x1, float y1, float a1, float b1, float g1, float r1, float u1, float v1,
                              float x2, float y2, float a2, float b2, float g2, float r2, float u2, float v2,
                              float x3, float y3, float a3, float b3, float g3, float r3, float u3, float v3,
                              float x4, float y4, float a4, float b4, float g4, float r4, float u4, float v4 )
    {
        Logger.Debug( $"VERTICES_PER_SPRITE * VERTEX_SIZE: {VERTICES_PER_SPRITE * VERTEX_SIZE}" );
        Logger.Debug( $"Vertices.Length: {Vertices.Length}" );
        Logger.Debug( $"Idx: {Idx}" );
        Logger.Debug( $"( int )( Idx + ( long )( VERTICES_PER_SPRITE * VERTEX_SIZE ) ): {Idx + ( long )( VERTICES_PER_SPRITE * VERTEX_SIZE )}" );

        Vertices[ Idx ]     = x1; // X
        Vertices[ Idx + 1 ] = y1; // Y
        Vertices[ Idx + 2 ] = r1; // Store the unpacked red component
        Vertices[ Idx + 3 ] = g1; // Store the unpacked green component
        Vertices[ Idx + 4 ] = b1; // Store the unpacked blue component
        Vertices[ Idx + 5 ] = a1; // Store the unpacked alpha component
        Vertices[ Idx + 6 ] = u1; // Texture U
        Vertices[ Idx + 7 ] = v1; // Texture V

        Vertices[ Idx + 8 ]  = x2;
        Vertices[ Idx + 9 ]  = y2;
        Vertices[ Idx + 10 ] = r2;
        Vertices[ Idx + 11 ] = g2;
        Vertices[ Idx + 12 ] = b2;
        Vertices[ Idx + 13 ] = a2;
        Vertices[ Idx + 14 ] = u2;
        Vertices[ Idx + 15 ] = v2;

        Vertices[ Idx + 16 ] = x3;
        Vertices[ Idx + 17 ] = y3;
        Vertices[ Idx + 18 ] = r3;
        Vertices[ Idx + 19 ] = g3;
        Vertices[ Idx + 20 ] = b3;
        Vertices[ Idx + 21 ] = a3;
        Vertices[ Idx + 22 ] = u3;
        Vertices[ Idx + 23 ] = v3;

        Vertices[ Idx + 24 ] = x4;
        Vertices[ Idx + 25 ] = y4;
        Vertices[ Idx + 26 ] = r4;
        Vertices[ Idx + 27 ] = g4;
        Vertices[ Idx + 28 ] = b4;
        Vertices[ Idx + 29 ] = a4;
        Vertices[ Idx + 30 ] = u4;
        Vertices[ Idx + 31 ] = v4;

        Idx += ( VERTICES_PER_SPRITE * VERTEX_SIZE );
    }

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

    // ========================================================================

    private bool _once = true;

    private void DebugVertices()
    {
        if ( _once )
        {
            Logger.Debug( "Begin DebugVertices()" );

            Logger.Debug( $"ColorPacked ABGR: {Color.ToFloatBitsABGR():F1}" );
            Logger.Debug( $"ColorPacked RGBA: {Color.ToFloatBitsRGBA():F1}" );
            Logger.Debug( $"FloatToHexString ABGR: {NumberUtils.FloatToHexString( Color.ToFloatBitsABGR() )}" );
            Logger.Debug( $"FloatToHexString RGBA: {NumberUtils.FloatToHexString( Color.ToFloatBitsRGBA() )}" );
            Logger.Debug( $"Color: {Color.RGBAToString()}" );

            for ( var i = 0; i < ( VERTICES_PER_SPRITE * VERTEX_SIZE ); i++ )
            {
                Logger.Debug( $"Vertices[{i}]: {Vertices[ i ]}" );
            }

            Logger.Debug( "End DebugVertices()" );

            _once = false;
        }
    }

    // ========================================================================

    /// <summary>
    /// Flushes the current batch, sending all rendered vertices to the GPU for drawing.
    /// This method handles binding the appropriate Vertex Buffer Object (VBO), Vertex Array Object (VAO),
    /// textures, shaders, and performs the actual rendering of the accumulated sprites.
    /// </summary>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if there is no OpenGL context available on the current thread, if
    /// the index buffer (`Idx`) is less than zero, or if certain rendering conditions are not met.
    /// </exception>
    public unsafe void Flush()
    {
        OpenGL.GLUtils.CheckOpenGLContext();

        if ( ( Idx == 0 ) || ( _mesh == null ) )
        {
            // Ensure that Idx is zero, to cover the case where checking _mesh gets here.
            Idx = 0;

            return;
        }

        if ( Idx < 0 ) throw new GdxRuntimeException( "Idx is less than zero!" );

        RenderCalls++;
        TotalRenderCalls++;

        var spritesInBatch = ( int )( Idx / ( long )( VERTICES_PER_SPRITE * VERTEX_SIZE ) );

        if ( spritesInBatch > MaxSpritesInBatch ) MaxSpritesInBatch = spritesInBatch;

        if ( LastTexture == null )
        {
            Idx = 0;
            _nullTextureCount++;

            Logger.Error( $"Attempt to flush with null texture. This batch will be skipped. " +
                          $"Null texture count: {_nullTextureCount}. " +
                          $"Last successful texture: {_lastSuccessfulTexture?.ToString() ?? "None"}" );

            return;
        }

        GdxApi.Bindings.ActiveTexture( TextureUnit.Texture0 );
        LastTexture.Bind();

        SetupVertexAttributes( _customShader ?? _shader );

        GdxApi.Bindings.BindBuffer( ( int )BufferTarget.ArrayBuffer, _vbo );
        GdxApi.Bindings.BindVertexArray( _vao );

        // Only allocate initial storage once
        if ( Vertices.Length > 0 )
        {
            _mesh.SetVertices( Vertices );

            // Pin the Vertices array
            fixed ( float* ptr = Vertices )
            {
                GdxApi.Bindings.BufferData( ( int )BufferTarget.ArrayBuffer,
                                            Vertices.Length * sizeof( float ),
                                            ( void* )ptr,
                                            ( int )BufferUsageHint.DynamicDraw );
            }
        }

        fixed ( float* ptr = Vertices )
        {
            GdxApi.Bindings.BufferSubData( ( int )BufferTarget.ArrayBuffer, 0, Idx * sizeof( float ), ptr );
        }

        _mesh.Render( _customShader ?? _shader, IGL.GL_TRIANGLES, 0, spritesInBatch * INDICES_PER_SPRITE );

        GdxApi.Bindings.BindVertexArray( 0 );                             // Unbind VAO
        GdxApi.Bindings.BindBuffer( ( int )BufferTarget.ArrayBuffer, 0 ); // Unbind VBO

        Idx = 0;
    }

    /// <summary>
    /// Disables blending for textures.
    /// </summary>
    public void DisableBlending()
    {
        if ( BlendingDisabled ) return;

        Flush();
        BlendingDisabled = true;
    }

    /// <summary>
    /// Enables blending for textures.
    /// </summary>
    public void EnableBlending()
    {
        if ( !BlendingDisabled ) return;

        Flush();
        BlendingDisabled = false;
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

        Flush();

        BlendSrcFunc      = srcFuncColor;
        BlendDstFunc      = dstFuncColor;
        BlendSrcFuncAlpha = srcFuncAlpha;
        BlendDstFuncAlpha = dstFuncAlpha;
    }

    /// <inheritdoc />
    public void SetProjectionMatrix( Matrix4 projection )
    {
        if ( IsDrawing ) Flush();

        ProjectionMatrix.Set( projection );

        if ( IsDrawing ) SetupMatrices();
    }

    /// <inheritdoc />
    public virtual void SetTransformMatrix( Matrix4 transform )
    {
        if ( IsDrawing ) Flush();

        TransformMatrix.Set( transform );

        if ( IsDrawing ) SetupMatrices();
    }

    /// <summary>
    /// Shader property for this SpriteBatch.
    /// </summary>
    public ShaderProgram? Shader
    {
        get => _customShader ?? _shader;
        set
        {
            if ( IsDrawing ) Flush();

            _customShader = value;

            if ( IsDrawing )
            {
                if ( _customShader != null )
                {
                    _customShader.Bind();
                }
                else
                {
                    _shader?.Bind();
                }

                SetupMatrices();
            }
        }
    }

    public static ShaderProgram CreateMinimalShader()
    {
        const string VERTEX_SHADER = $"layout (location = 0) in vec2 {ShaderProgram.TEXCOORD_ATTRIBUTE};\n" +
                                     $"layout (location = 1) in vec4 {ShaderProgram.COLOR_ATTRIBUTE};\n" +
                                     "\n" +
                                     "out vec4 v_color;\n" +
                                     "\n" +
                                     "void main()\n" +
                                     "{\n" +
                                     "    gl_Position = vec4(a_position, 0.0, 1.0);\n" +
                                     $"    v_color = {ShaderProgram.COLOR_ATTRIBUTE};\n" +
                                     "}";

        const string FRAGMENT_SHADER = "in vec4 v_color;\n" +
                                       "out vec4 frag_color;\n" +
                                       "\n" +
                                       "void main()\n" +
                                       "{\n" +
                                       "    frag_color = v_color;\n" +
                                       "}";

        return new ShaderProgram( VERTEX_SHADER, FRAGMENT_SHADER );
    }

    /// <summary>
    /// Returns a new instance of the default shader used by SpriteBatch for GL2 when no shader is specified.
    /// </summary>
    public static ShaderProgram CreateDefaultShader()
    {
        const string VERTEX_SHADER = $"in vec2 {ShaderProgram.POSITION_ATTRIBUTE};\n" +
                                     $"in vec4 {ShaderProgram.COLOR_ATTRIBUTE};\n" +
                                     $"in vec2 {ShaderProgram.TEXCOORD_ATTRIBUTE};\n" +
                                     "out vec4 v_colorPacked;\n" +
                                     "out vec2 v_texCoords;\n" +
                                     "uniform mat4 u_projTrans;\n" +
                                     "uniform mat4 u_viewMatrix;\n" +
                                     "uniform mat4 u_modelMatrix;\n" +
                                     "void main() {\n" +
                                     "    gl_Position = u_projTrans * u_viewMatrix * u_modelMatrix * vec4(a_position, 0.0, 1.0);\n" +
                                     $"    v_colorPacked = {ShaderProgram.COLOR_ATTRIBUTE};\n" +
                                     $"    v_texCoords = {ShaderProgram.TEXCOORD_ATTRIBUTE};\n" +
                                     "}\n";

        const string FRAGMENT_SHADER = "in vec4 v_colorPacked;\n" +
                                       "in vec2 v_texCoords;\n" +
                                       "out vec4 FragColor;\n" +
                                       "uniform sampler2D u_texture;\n" +
                                       "void main() {\n" +
                                       "    FragColor = texture(u_texture, v_texCoords) * v_colorPacked;\n" +
                                       "}\n";

        return new ShaderProgram( VERTEX_SHADER, FRAGMENT_SHADER );
    }

    /// <summary>
    /// </summary>
    public virtual void SetupMatrices()
    {
        _combinedMatrix.Set( ProjectionMatrix ).Mul( TransformMatrix );

        if ( _customShader != null )
        {
            if ( !_customShader.IsCompiled )
            {
                Logger.Debug( "Custom Shader is not compiled." );

                return; // Return early if not compiled
            }

            _customShader.SetUniformMatrix( "u_projTrans", _combinedMatrix );
            _customShader.SetUniformi( "u_texture", 0 );
        }
        else if ( _shader != null )
        {
            if ( !_shader.IsCompiled )
            {
                Logger.Debug( "Shader is not compiled." );

                return; // Return early if not compiled
            }

            _shader?.SetUniformMatrix( "u_projTrans", _combinedMatrix );
            _shader?.SetUniformi( "u_texture", 0 );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="texture"></param>
    protected void SwitchTexture( Texture? texture )
    {
        if ( texture == null ) return;

        Flush();

        LastTexture            = texture;
        _lastSuccessfulTexture = texture;
        InvTexWidth            = 1.0f / texture.Width;
        InvTexHeight           = 1.0f / texture.Height;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _mesh?.Dispose();

        if ( _ownsShader && ( _shader != null ) )
        {
            _shader.Dispose();
        }

        _mesh         = null;
        _shader       = null;
        _customShader = null;

        unsafe
        {
            fixed ( uint* ptr = &_vbo )
            {
                GdxApi.Bindings.DeleteBuffers( 1, ptr ); // Correct way to delete a single buffer
            }
        }

        GdxApi.Bindings.DeleteVertexArrays( _vao );

        GC.SuppressFinalize( this );
    }

    // ========================================================================

    /// <summary>
    /// Convenience property. Returns TRUE if blending is not disabled.
    /// </summary>
    public bool IsBlendingEnabled => !BlendingDisabled;

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

        if ( !IsDrawing )
        {
            throw new InvalidOperationException( "Begin() must be called before Draw()." );
        }
    }

    /// <summary>
    /// Verifies whether the currently bound buffer matches the expected buffer ID.
    /// Logs an error if there is a mismatch.
    /// </summary>
    /// <param name="expectedBuffer">The identifier of the buffer that is expected to be currently bound.</param>
    private unsafe void CheckBufferBinding( uint expectedBuffer )
    {
        var currentBuffer = new int[ 1 ];

        fixed ( int* ptr = &currentBuffer[ 0 ] )
        {
            GdxApi.Bindings.GetIntegerv( IGL.GL_ARRAY_BUFFER_BINDING, ptr );
        }

        if ( currentBuffer[ 0 ] != expectedBuffer )
        {
            Logger.Error( $"Buffer not bound correctly! Expected {expectedBuffer}, got {currentBuffer[ 0 ]}" );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="stage"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private void CheckGLError( string stage )
    {
        var error = GdxApi.Bindings.GetError();

        if ( error != ( int ) ErrorCode.NoError )
        {
            throw new InvalidOperationException( $"OpenGL error at {stage}: {error}" );
        }
    }
}

