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
public class OldSpriteBatch : IBatch
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

    // ========================================================================

    private const int MAX_VERTEX_INDEX = 32767;
    private const int MAX_SPRITES      = 8191; // 32767 is max vertex index, so 32767 / 4 vertices per sprite = 8191 sprites max.

    private readonly Color _color = Graphics.Color.Red;
    private readonly bool  _ownsShader;

    private Matrix4        _combinedMatrix = new();
    private Mesh?          _mesh;
    private ShaderProgram? _shader;
    private Texture?       _lastSuccessfulTexture = null;
    private int            _nullTextureCount      = 0;
    private ShaderProgram? _customShader          = null;
    private uint           _vbo;

    // ========================================================================

    /// <summary>
    /// Constructs a new SpriteBatch with a size of 1000, one buffer, and the default shader.
    /// </summary>
    public OldSpriteBatch() : this( 1000 )
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
    protected OldSpriteBatch( int size, ShaderProgram? defaultShader = null )
    {
        if ( size > MAX_SPRITES )
        {
            throw new ArgumentException( $"Can't have more than 8191 sprites per batch: {size}" );
        }

        IsDrawing = false;
        Vertices  = new float[ size * Sprite.SPRITE_SIZE ];

        if ( defaultShader == null )
        {
            _shader     = CreateDefaultShader();
            _ownsShader = true;
        }
        else
        {
            _shader = defaultShader;
        }

        unsafe
        {
            // Initialize the VBO
            var vboArray = new uint[ 1 ];

            fixed ( uint* ptr = &vboArray[ 0 ] )
            {
                GdxApi.Bindings.GenBuffers( 1, ptr );
                _vbo = *ptr;
            }
        }

        Initialise( size );
    }

    /// <summary>
    /// Takes away messy vertex attributes initialisation from the constructor, the GL side of
    /// which is done inside <see cref="SetupVertexAttributes"/>, 
    /// </summary>
    /// <param name="size"></param>
    private unsafe void Initialise( int size )
    {
        var vertexDataType = ( GdxApi.Bindings.GetOpenGLVersion().major >= 3 )
            ? Mesh.VertexDataType.VertexBufferObjectWithVAO
            : Mesh.VertexDataType.VertexArray;

        _mesh = new Mesh( vertexDataType,
                          false,
                          size * 4,
                          size * 6,
                          new VertexAttribute( ( int )VertexAttributes.Usage.POSITION, 2, ShaderProgram.POSITION_ATTRIBUTE ),
                          new VertexAttribute( ( int )VertexAttributes.Usage.COLOR_PACKED, 4, ShaderProgram.COLOR_ATTRIBUTE ),
                          new VertexAttribute( ( int )VertexAttributes.Usage.TEXTURE_COORDINATES, 2,
                                               $"{ShaderProgram.TEXCOORD_ATTRIBUTE}0" ) );

        ProjectionMatrix.SetToOrtho2D( 0, 0, GdxApi.Graphics.Width, GdxApi.Graphics.Height );

        var len     = size * 6;
        var indices = new short[ len ];

        for ( short i = 0, j = 0; i < len; i += 6, j += 4 )
        {
            indices[ i ]     = j;
            indices[ i + 1 ] = ( short )( j + 1 );
            indices[ i + 2 ] = ( short )( j + 2 );
            indices[ i + 3 ] = ( short )( j + 2 );
            indices[ i + 4 ] = ( short )( j + 3 );
            indices[ i + 5 ] = j;
        }

        _mesh.SetIndices( indices );

        _vbo = GdxApi.Bindings.GenBuffer();

        fixed ( void* ptr = &Vertices[ 0 ] )
        {
            GdxApi.Bindings.BindBuffer( ( int )BufferTarget.ArrayBuffer, _vbo );
            
            GdxApi.Bindings.BufferData( ( int )BufferTarget.ArrayBuffer,
                                        Vertices.Length * sizeof( float ),
                                        ptr,
                                        ( int )BufferUsageHint.StaticDraw );
        }

        SetupVertexAttributes();

        GdxApi.Bindings.BindBuffer( ( int )BufferTarget.ArrayBuffer, 0 );
    }

    /// <summary>
    /// Performs the OpenGL side of Vertex Attribute initialisation.
    /// </summary>
    private void SetupVertexAttributes()
    {
        GdxRuntimeException.ThrowIfNull( _shader );

        _shader.Bind();

        var positionAttribute  = ( uint )_shader.GetAttributeLocation( "a_position" );
        var colorAttribute     = ( uint )_shader.GetAttributeLocation( "a_colorPacked" );
        var texCoordsAttribute = ( uint )_shader.GetAttributeLocation( "a_texCoords" );

        var stride = 5 * sizeof( float );

        GdxApi.Bindings.EnableVertexAttribArray( positionAttribute );
        GdxApi.Bindings.VertexAttribPointer( positionAttribute, 2, IGL.GL_FLOAT, false, stride, 0 );

        GdxApi.Bindings.EnableVertexAttribArray( colorAttribute );
        GdxApi.Bindings.VertexAttribPointer( colorAttribute, 1, IGL.GL_FLOAT, false, stride, 2 * sizeof( float ) );

        GdxApi.Bindings.EnableVertexAttribArray( texCoordsAttribute );
        GdxApi.Bindings.VertexAttribPointer( texCoordsAttribute, 2, IGL.GL_FLOAT, false, stride, 3 * sizeof( float ) );

        _shader.Unbind();

        GdxApi.Bindings.BindBuffer( ( int )BufferTarget.ArrayBuffer, 0 );
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
    public void SetColor( Color tint )
    {
        SetColor( tint.R, tint.G, tint.B, tint.A );
    }

    /// <summary>
    /// Sets the Color for this SpriteBatch using the supplied
    /// RGBA Color components.
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
        get => Color.ToFloatBitsABGR();
        set { }
    }

    // ========================================================================

    #region Drawing methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="region"></param>
    /// <param name="origin"></param>
    /// <param name="scale"></param>
    /// <param name="rotation"></param>
    /// <param name="src"></param>
    /// <param name="flipX"></param>
    /// <param name="flipY"></param>
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

        SetVertices( x1, y1, ColorPackedABGR, u, v,
                     x2, y2, ColorPackedABGR, u, v2,
                     x3, y3, ColorPackedABGR, u2, v2,
                     x4, y4, ColorPackedABGR, u2, v );

        Idx += Sprite.SPRITE_SIZE;
    }

    /// <summary>
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="region"></param>
    /// <param name="src"></param>
    /// <param name="flipX"></param>
    /// <param name="flipY"></param>
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

        SetVertices( region.X, region.Y, ColorPackedABGR, u, v,
                     region.X, fy2, ColorPackedABGR, u, v2,
                     fx2, fy2, ColorPackedABGR, u2, v2,
                     fx2, region.Y, ColorPackedABGR, u2, v );

        Idx += Sprite.SPRITE_SIZE;
    }

    /// <summary>
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="src"></param>
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

        SetVertices( x, y, ColorPackedABGR, u, v,
                     x, fy2, ColorPackedABGR, u, v2,
                     fx2, fy2, ColorPackedABGR, u2, v2,
                     fx2, y, ColorPackedABGR, u2, v );

        Idx += Sprite.SPRITE_SIZE;
    }

    /// <summary>
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="region"></param>
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <param name="u2"></param>
    /// <param name="v2"></param>
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

        SetVertices( region.X, region.Y, ColorPackedABGR, u, v,
                     region.X, fy2, ColorPackedABGR, u, v2,
                     fx2, fy2, ColorPackedABGR, u2, v2,
                     fx2, region.Y, ColorPackedABGR, u2, v );

        Idx += Sprite.SPRITE_SIZE;
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
        else if ( Idx == Vertices.Length )
        {
            Flush();
        }

        var fx2 = posX + width;
        var fy2 = posY + height;

        const float U  = 0;
        const float V  = 1;
        const float U2 = 1;
        const float V2 = 0;

        SetVertices( posX, posY, ColorPackedABGR, U, V,
                     posX, fy2, ColorPackedABGR, U, V2,
                     fx2, fy2, ColorPackedABGR, U2, V2,
                     fx2, posY, ColorPackedABGR, U2, V );

        //TODO: Remove when drawing is fixed
        DebugVertices();

        Idx += Sprite.SPRITE_SIZE;
    }

    /// <summary>
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="spriteVertices"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
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
    /// </summary>
    /// <param name="region"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public virtual void Draw( TextureRegion? region, float x, float y )
    {
        Validate( region );

        Draw( region, x, y, region!.RegionWidth, region.RegionHeight );
    }

    /// <summary>
    /// </summary>
    /// <param name="region"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
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

        SetVertices( x, y, ColorPackedABGR, u, v,
                     x, fy2, ColorPackedABGR, u, v2,
                     fx2, fy2, ColorPackedABGR, u2, v2,
                     fx2, y, ColorPackedABGR, u2, v );

        Idx += Sprite.SPRITE_SIZE;
    }

    /// <summary>
    /// </summary>
    /// <param name="textureRegion"></param>
    /// <param name="region"></param>
    /// <param name="origin"></param>
    /// <param name="scale"></param>
    /// <param name="rotation"></param>
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

        SetVertices( x1, y1, ColorPackedABGR, textureRegion.U, textureRegion.V2,
                     x2, y2, ColorPackedABGR, textureRegion.U, textureRegion.V,
                     x3, y3, ColorPackedABGR, textureRegion.U2, textureRegion.V,
                     x4, y4, ColorPackedABGR, textureRegion.U2, textureRegion.V2 );

        Idx += Sprite.SPRITE_SIZE;
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

        SetVertices( x1, y1, ColorPackedABGR, u1, v1,
                     x2, y2, ColorPackedABGR, u2, v2,
                     x3, y3, ColorPackedABGR, u3, v3,
                     x4, y4, ColorPackedABGR, u4, v4 );

        Idx += Sprite.SPRITE_SIZE;
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

        SetVertices( x1, y1, ColorPackedABGR, region.U, region.V2,
                     x2, y2, ColorPackedABGR, region.U, region.V,
                     x3, y3, ColorPackedABGR, region.U2, region.V,
                     x4, y4, ColorPackedABGR, region.U2, region.V2 );

        Idx += Sprite.SPRITE_SIZE;
    }

    #endregion Drawing methods

    // ========================================================================

    private void SetVertices( float x1, float y1, float colorPackedABGR, float u1, float v1,
                              float x2, float y2, float colorPackedABGR1, float u2, float v2,
                              float x3, float y3, float colorPackedABGR2, float u3, float v3,
                              float x4, float y4, float colorPackedABGR3, float u4, float v4 )
    {
        Vertices[ Idx ]     = x1;
        Vertices[ Idx + 1 ] = y1;
        Vertices[ Idx + 2 ] = ColorPackedABGR;
        Vertices[ Idx + 3 ] = u1;
        Vertices[ Idx + 4 ] = v1;

        Vertices[ Idx + 5 ] = x2;
        Vertices[ Idx + 6 ] = y2;
        Vertices[ Idx + 7 ] = ColorPackedABGR;
        Vertices[ Idx + 8 ] = u2;
        Vertices[ Idx + 9 ] = v2;

        Vertices[ Idx + 10 ] = x3;
        Vertices[ Idx + 11 ] = y3;
        Vertices[ Idx + 12 ] = ColorPackedABGR;
        Vertices[ Idx + 13 ] = u3;
        Vertices[ Idx + 14 ] = v3;

        Vertices[ Idx + 15 ] = x4;
        Vertices[ Idx + 16 ] = y4;
        Vertices[ Idx + 17 ] = ColorPackedABGR;
        Vertices[ Idx + 18 ] = u4;
        Vertices[ Idx + 19 ] = v4;
    }

    private bool _first = true;

    /// <summary>
    /// </summary>
    public void Flush()
    {
        if ( Idx == 0 ) return;

        RenderCalls++;
        TotalRenderCalls++;

        var spritesInBatch = Idx / 20;

        if ( spritesInBatch > MaxSpritesInBatch ) MaxSpritesInBatch = spritesInBatch;

        var count = spritesInBatch * 6;

        if ( LastTexture == null )
        {
            _nullTextureCount++;

            Logger.Error( $"Attempt to flush with null texture. This batch will be skipped. " +
                          $"Null texture count: {_nullTextureCount}. " +
                          $"Last successful texture: {_lastSuccessfulTexture?.ToString() ?? "None"}" );

            Idx = 0;

            return;
        }

        GdxApi.Bindings.ActiveTexture( TextureUnit.Texture0 );

        LastTexture.Bind();

        if ( _mesh == null )
        {
            Idx = 0;

            return;
        }

        _mesh.SetVertices( Vertices, 0, Idx );
        _mesh.IndicesBuffer.Position = 0;
        _mesh.IndicesBuffer.Limit    = count;

        if ( BlendingDisabled )
        {
            GdxApi.Bindings.Disable( IGL.GL_BLEND );
        }
        else
        {
            GdxApi.Bindings.Enable( IGL.GL_BLEND );

            if ( BlendSrcFunc != -1 )
            {
                if ( _first )
                {
                    Logger.Debug( $"BlendSrcFunc     : 0x{BlendSrcFunc:X}" );
                    Logger.Debug( $"BlendDstFunc     : 0x{BlendDstFunc:X}" );
                    Logger.Debug( $"BlendSrcFuncAlpha: 0x{BlendSrcFuncAlpha:X}" );
                    Logger.Debug( $"BlendDstFuncAlpha: 0x{BlendDstFuncAlpha:X}" );

                    _first = false;
                }

                GdxApi.Bindings.BlendFuncSeparate( BlendSrcFunc, BlendDstFunc, BlendSrcFuncAlpha, BlendDstFuncAlpha );
            }
        }

        _mesh.Render( _customShader ?? _shader, IGL.GL_TRIANGLES, 0, count );

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

    /// <summary>
    /// Returns a new instance of the default shader used by SpriteBatch for GL2 when no shader is specified.
    /// </summary>
    public static ShaderProgram CreateDefaultShader()
    {
        const string VERTEX_SHADER = "in vec2 a_position;\n" +
                                     "in float a_colorPacked;\n" +
                                     "in vec2 a_texCoords;\n" +
                                     "out float v_colorPacked;\n" +
                                     "out vec2 v_texCoords;\n" +
                                     "uniform mat4 u_projTrans;\n" +
                                     "uniform mat4 u_viewMatrix;\n" +
                                     "uniform mat4 u_modelMatrix;\n" +
                                     "void main() {\n" +
                                     "    gl_Position = u_projTrans * u_viewMatrix * u_modelMatrix * vec4(a_position, 0.0, 1.0);\n" +
                                     "    v_colorPacked = a_colorPacked;\n" +
                                     "    v_texCoords = a_texCoords;\n" +
                                     "}\n";

        const string FRAGMENT_SHADER = "in float v_colorPacked;\n" +
                                       "in vec2 v_texCoords;\n" +
                                       "out vec4 FragColor;\n" +
                                       "uniform sampler2D u_texture;\n" +
                                       "vec4 unpackColor(float packedColor) {\n" +
                                       "    uint color = uint(packedColor);\n" +
                                       "    float b = float((color >> 0) & 0xFFu) / 255.0;\n" +
                                       "    float g = float((color >> 8) & 0xFFu) / 255.0;\n" +
                                       "    float r = float((color >> 16) & 0xFFu) / 255.0;\n" +
                                       "    float a = float((color >> 24) & 0xFFu) / 255.0;\n" +
                                       "    return vec4(r, g, b, a);\n" +
                                       "}\n" +
                                       "void main() {\n" +
                                       "    FragColor = texture(u_texture, v_texCoords) * unpackColor(v_colorPacked);\n" +
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

    // ========================================================================
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

            for ( var i = 0; i < 20; i++ )
            {
                Logger.Debug( i is 2 or 7 or 12 or 17
                                  ? $"Vertices[{i}]: {NumberUtils.FloatToHexString( Vertices[ i ] )}"
                                  : $"Vertices[{i}]: {Vertices[ i ]}" );
            }

            Logger.Debug( "End DebugVertices()" );

            _once = false;
        }
    }
}