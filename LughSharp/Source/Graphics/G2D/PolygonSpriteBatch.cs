// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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

using LughSharp.Source.Graphics.Images;
using LughSharp.Source.Graphics.OpenGL;
using LughSharp.Source.Graphics.OpenGL.Enums;
using LughSharp.Source.Graphics.Shaders;
using LughSharp.Source.Graphics.Utils;

namespace LughSharp.Source.Graphics.G2D;

/// <summary>
/// A PolygonSpriteBatch is used to Draw 2D polygons that reference a
/// texture (region). The class will batch the drawing commands and
/// optimize them for processing by the GPU.
/// <para>
/// To Draw something with a PolygonSpriteBatch one has to first call the
/// <see cref="PolygonSpriteBatch.Begin(bool)"/> method which will setup appropriate
/// render states. When you are done with drawing you have to call
/// <see cref="PolygonSpriteBatch.End()"/> which will actually Draw the things
/// you specified.
/// </para>
/// <para>
/// All drawing commands of the PolygonSpriteBatch operate in screen coordinates.
/// The screen coordinate system has an x-axis pointing to the right, an y-axis
/// pointing upwards and the origin is in the lower left corner of the screen. You
/// can also provide your own transformation and projection matrices if you so wish.
/// A PolygonSpriteBatch is managed. In case the OpenGL context is lost all OpenGL
/// resources a PolygonSpriteBatch uses internally get invalidated. A context is lost
/// when a user switches to another application or receives an incoming call on Android.
/// A SpritPolygonSpriteBatcheBatch will be automatically reloaded after the OpenGL
/// context is restored.
/// </para>
/// <para>
/// A PolygonSpriteBatch is a pretty heavy object so you should only ever have one
/// in your program.
/// </para>
/// <para>
/// A PolygonSpriteBatch has to be disposed if it is no longer used.
/// </para>
/// </summary>
//TODO: Update this class as with SpriteBatch()
[PublicAPI]
public class PolygonSpriteBatch : IPolygonBatch
{
    /// <summary>
    /// The maximum number of triangles rendered in one batch so far.
    /// </summary>
    public int MaxTrianglesInBatch { get; set; }

    /// <summary>
    /// Number of render calls since the last call to Begin()
    /// </summary>
    public int RenderCalls { get; set; }

    /// <summary>
    /// Number of rendering calls since this PolygonSpriteBatch was created
    /// Will not be reset unless set manually.
    /// </summary>
    public int TotalRenderCalls { get; set; }

    /// <summary>
    /// The source blend factor used for RGB channels.
    /// </summary>
    public BlendMode BlendSrcFunc { get; private set; } = BlendMode.SrcAlpha;

    /// <summary>
    /// The destination blend factor used for RGB channels.
    /// </summary>
    public BlendMode BlendDstFunc { get; private set; } = BlendMode.OneMinusSrcAlpha;

    /// <summary>
    /// The source blend factor used for the alpha channel.
    /// </summary>
    public BlendMode BlendSrcFuncAlpha { get; private set; } = BlendMode.SrcAlpha;

    /// <summary>
    /// The destination blend factor used for the alpha channel.
    /// </summary>
    public BlendMode BlendDstFuncAlpha { get; private set; } = BlendMode.OneMinusSrcAlpha;

    /// <summary>
    /// The orthographic projection matrix applied to the batch.
    /// </summary>
    public Matrix4 ProjectionMatrix { get; set; } = new();

    /// <summary>
    /// The model/world transform matrix applied before projection.
    /// </summary>
    public Matrix4 TransformMatrix { get; set; } = new();

    /// <summary>
    /// Returns <c>true</c> when the batch is between a <see cref="Begin"/> and
    /// <see cref="End"/> call.
    /// </summary>
    public bool IsDrawing { get; set; }

    // ========================================================================
    // ========================================================================

    private readonly Color          _color          = new( 1, 1, 1, 1 );
    private readonly Matrix4        _combinedMatrix = new();
    private readonly Mesh           _mesh;
    private readonly bool           _ownsShader;
    private readonly ShaderProgram? _shader;
    private readonly int[]          _triangles;
    private readonly float[]        _vertices;

    private bool           _blendingDisabled;
    private float          _colorPacked = Graphics.Color.WhiteFloatBits;
    private ShaderProgram? _customShader;
    private float          _invTexHeight;
    private float          _invTexWidth;
    private Texture2D?     _lastTexture;
    private int            _triangleIndex;
    private int            _vertexIndex;
    private bool           _originalDepthMask;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Constructs a PolygonSpriteBatch with the default shader, size vertices,
    /// and size * 2 triangles.
    /// </summary>
    /// <param name="size">
    /// The max number of vertices and number of triangles in a single batch. Max of 32767.
    /// </param>
    public PolygonSpriteBatch( int size )
        : this( size, size * 2, null )
    {
    }

    /// <summary>
    /// Constructs a PolygonSpriteBatch with the specified shader, size vertices
    /// and size * 2 triangles.
    /// </summary>
    /// <param name="size">
    /// The max number of vertices and number of triangles in a single batch. Max of 32767.
    /// </param>
    /// <param name="defaultShader"></param>
    public PolygonSpriteBatch( int size, ShaderProgram? defaultShader = null )
        : this( size, size * 2, defaultShader )
    {
    }

    /// <summary>
    /// Constructs a new PolygonSpriteBatch. Sets the projection matrix to an orthographic
    /// projection with y-axis point upwards, x-axis point to the right and the origin
    /// being in the bottom left corner of the screen. The projection will be pixel perfect
    /// with respect to the current screen resolution.
    /// <para>
    /// The defaultShader specifies the shader to use. Note that the names for uniforms for
    /// this default shader are different than the ones expect for shaders set with
    /// <see cref="Shader"/>.
    /// </para>
    /// </summary>
    /// <param name="maxVertices"> The max number of vertices in a single batch. Max of 32767.</param>
    /// <param name="maxTriangles"> The max number of triangles in a single batch. </param>
    /// <param name="defaultShader">
    /// The default shader to use. This is not owned by the PolygonSpriteBatch and must
    /// be disposed separately. May be null to use the default shader.
    /// </param>
    public PolygonSpriteBatch( int maxVertices, int maxTriangles, ShaderProgram? defaultShader )
    {
        // 32767 is max vertex index.
        if ( maxVertices > 32767 )
        {
            throw new ArgumentException( "Can't have more than 32767 vertices per batch: " + maxVertices );
        }

        _mesh = new Mesh( VertexDataType.VertexBufferObjectWithVAO,
                          false,
                          maxVertices,
                          maxTriangles * 3,
                          new VertexAttribute( ( int )VertexConstants.Usage.Position, 4, ShaderConstants.APosition ),
                          new VertexAttribute( ( int )VertexConstants.Usage.ColorPacked, 4, ShaderConstants.AColor ),
                          new VertexAttribute( ( int )VertexConstants.Usage.TextureCoordinates,
                                               2,
                                               ShaderConstants.ATexCoord0 ) );

        _vertices  = new float[ maxVertices * Sprite2D.VertexSize ];
        _triangles = new int[ maxTriangles * 3 ];

        if ( defaultShader == null )
        {
            _shader     = SpriteBatch.CreateDefaultShader();
            _ownsShader = true;
        }
        else
        {
            _shader = defaultShader;
        }

        ProjectionMatrix.SetToOrtho2D( 0, 0, Engine.Graphics.WindowWidth, Engine.Graphics.WindowHeight );
    }

    /// <summary>
    /// Sets up the Batch for drawing. This will disable depth buffer writing. It enables blending
    /// and texturing. If you have more texture units enabled than the first one you have to disable
    /// them before calling this. Uses a screen coordinate system by default where everything is
    /// given in pixels. You can specify your own projection and modelview matrices via
    /// <see cref="IBatch.SetProjectionMatrix"/> and <see cref="IBatch.SetTransformMatrix"/>.
    /// </summary>
    /// <param name="depthMaskEnabled"> Enable or Disable DepthMask. Defaults to false. </param>
    public void Begin( bool depthMaskEnabled = false )
    {
        if ( IsDrawing )
        {
            throw new RuntimeException( "PolygonSpriteBatch.end must be called before begin." );
        }

        RenderCalls = 0;

        _originalDepthMask = Engine.GL.IsEnabled( EnableCap.DepthTest );
        Engine.GL.DepthMask( depthMaskEnabled );

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
    /// Finishes off rendering. Enables depth writes, disables blending and texturing.
    /// Must always be called after a call to <see cref="IBatch.Begin"/>
    /// </summary>
    public void End()
    {
        if ( !IsDrawing )
        {
            throw new RuntimeException( "PolygonSpriteBatch.begin must be called before end." );
        }

        if ( _vertexIndex > 0 )
        {
            Flush();
        }

        _lastTexture = null;
        IsDrawing    = false;

        Engine.GL.DepthMask( true );

        if ( IsBlendingEnabled() )
        {
            Engine.GL.Disable( EnableCap.Blend );
        }
    }

    /// <returns>
    /// The rendering color of this Batch.
    /// </returns>
    public Color Color
    {
        get => _color;
        set
        {
            _color.Set( value );
            _colorPacked = value.ToFloatBitsRgba();
        }
    }

    /// <summary>
    /// Sets the rendering color of this Batch.
    /// </summary>
    /// <param name="r"> The red component of the color </param>
    /// <param name="g"> The green component of the color </param>
    /// <param name="b"> The blue component of the color </param>
    /// <param name="a"> The aalpha component of the color </param>
    public void SetColor( float r, float g, float b, float a )
    {
        _color.Set( r, g, b, a );
        _colorPacked = _color.ToFloatBitsRgba();
    }

    /// <summary>
    /// This batch's Color packed into a float ABGR format.
    /// </summary>
    public float ColorPackedABGR => Color.ToFloatBitsAbgr( Color.A, Color.B, Color.G, Color.R );

    /// <summary>
    /// This batch's Color packed into a float RGBA format.
    /// </summary>
    public float ColorPackedRGBA => Color.ToFloatBitsRgba( Color.R, Color.G, Color.B, Color.A );

    /// <summary>
    /// Draws the supplied <see cref="PolygonRegion"/> at the given corrdinates.
    /// </summary>
    /// <param name="region"> The Polygon Region to draw </param>
    /// <param name="x"> X coordinate </param>
    /// <param name="y"> Y coordinate </param>
    /// <exception cref="RuntimeException">
    /// Thrown if this method was called before a call to Begin().
    /// </exception>
    public void Draw( PolygonRegion region, float x, float y )
    {
        if ( !IsDrawing )
        {
            throw new RuntimeException( "PolygonSpriteBatch.begin must be called before Draw." );
        }

        if ( region.Region.Texture == null )
        {
            Logger.Error( "Cannot draw a null texture!" );

            return;
        }

        if ( region.Region.Texture != _lastTexture )
        {
            SwitchTexture( region.Region.Texture );
        }
        else if ( ( ( _triangleIndex + region.Triangles.Length ) > _triangles.Length )
               || ( ( _vertexIndex + ( region.Vertices?.Length * Sprite2D.VertexSize / 2 ) ) > _vertices.Length ) )
        {
            Flush();
        }

        foreach ( short t in region.Triangles )
        {
            _triangles[ _triangleIndex++ ] = ( short )( t + ( _vertexIndex / Sprite2D.VertexSize ) );
        }

        for ( var i = 0; i < region.Vertices?.Length; i += 2 )
        {
            _vertices[ _vertexIndex++ ] = region.Vertices[ i ] + x;
            _vertices[ _vertexIndex++ ] = region.Vertices[ i + 1 ] + y;
            _vertices[ _vertexIndex++ ] = _colorPacked;
            _vertices[ _vertexIndex++ ] = region.TextureCoords[ i ];
            _vertices[ _vertexIndex++ ] = region.TextureCoords[ i + 1 ];
        }
    }

    /// <summary>
    /// Draws a polygon region with the bottom left corner at x,y and stretching
    /// the region to cover the given width and height.
    /// </summary>
    /// <param name="region"> The region to draw. </param>
    /// <param name="x"> X coordinate </param>
    /// <param name="y"> Y coordinate </param>
    /// <param name="width"> Width of the region. </param>
    /// <param name="height"> Height of the region. </param>
    /// <exception cref="RuntimeException">
    /// Thrown if this method was called before a call to Begin().
    /// </exception>
    public void Draw( PolygonRegion region, float x, float y, float width, float height )
    {
        if ( !IsDrawing )
        {
            throw new RuntimeException( "PolygonSpriteBatch.begin must be called before Draw." );
        }

        if ( region.Region.Texture == null )
        {
            Logger.Error( "Cannot draw a null texture!" );

            return;
        }

        if ( region.Region.Texture != _lastTexture )
        {
            SwitchTexture( region.Region.Texture );
        }
        else if ( ( ( _triangleIndex + region.Triangles.Length ) > _triangles.Length )
               || ( ( _vertexIndex + ( region.Vertices?.Length * Sprite2D.VertexSize / 2 ) ) > _vertices.Length ) )
        {
            Flush();
        }

        int startVertex = _vertexIndex / Sprite2D.VertexSize;

        for ( int i = 0, n = region.Triangles.Length; i < n; i++ )
        {
            _triangles[ _triangleIndex++ ] = ( short )( region.Triangles[ i ] + startVertex );
        }

        float sX = width / region.Region.GetRegionWidth();
        float sY = height / region.Region.GetRegionHeight();

        for ( var i = 0; i < region.Vertices?.Length; i += 2 )
        {
            _vertices[ _vertexIndex++ ] = ( region.Vertices[ i ] * sX ) + x;
            _vertices[ _vertexIndex++ ] = ( region.Vertices[ i + 1 ] * sY ) + y;
            _vertices[ _vertexIndex++ ] = _colorPacked;
            _vertices[ _vertexIndex++ ] = region.TextureCoords[ i ];
            _vertices[ _vertexIndex++ ] = region.TextureCoords[ i + 1 ];
        }
    }

    /// <summary>
    /// Draws the polygon region with the bottom left corner at x,y and stretching the region to
    /// cover the given width and height. The polygon region is offset by originX, originY relative
    /// to the origin. Scale specifies the scaling factor by which the polygon region should be
    /// scaled around originX, originY. Rotation specifies the angle of counter clockwise rotation
    /// of the rectangle around originX originY.
    /// </summary>
    /// <param name="region"> The region to draw. </param>
    /// <param name="x"> X coordinate </param>
    /// <param name="y"> Y coordinate </param>
    /// <param name="originX"> X coordinate of the origin. </param>
    /// <param name="originY"> Y coordinate of the origin. </param>
    /// <param name="width"> Width of the region. </param>
    /// <param name="height"> Height of the region. </param>
    /// <param name="scaleX"> Scale factor along the x-axis. </param>
    /// <param name="scaleY"> Scale factor along the y-axis. </param>
    /// <param name="rotation">The rotation angle of the texture in radians.</param>
    public void Draw( PolygonRegion region,
                      float x,
                      float y,
                      float originX,
                      float originY,
                      float width,
                      float height,
                      float scaleX,
                      float scaleY,
                      float rotation )
    {
        if ( !IsDrawing )
        {
            throw new RuntimeException( "PolygonSpriteBatch.begin must be called before Draw." );
        }

        if ( region.Region.Texture == null )
        {
            Logger.Error( "Cannot draw a null texture!" );

            return;
        }

        if ( region.Region.Texture != _lastTexture )
        {
            SwitchTexture( region.Region.Texture );
        }
        else if ( ( ( _triangleIndex + region.Triangles.Length ) > _triangles.Length )
               || ( ( _vertexIndex + ( region.Vertices?.Length * Sprite2D.VertexSize / 2 ) ) > _vertices.Length ) )
        {
            Flush();
        }

        int startVertex = _vertexIndex / Sprite2D.VertexSize;

        foreach ( short triangle in region.Triangles )
        {
            _triangles[ _triangleIndex++ ] = ( short )( triangle + startVertex );
        }

        float worldOriginX = x + originX;
        float worldOriginY = y + originY;
        float sX           = width / region.Region.GetRegionWidth();
        float sY           = height / region.Region.GetRegionHeight();
        float cos          = MathUtils.CosDeg( rotation );
        float sin          = MathUtils.SinDeg( rotation );

        for ( var i = 0; i < region.Vertices?.Length; i += 2 )
        {
            float fx = ( ( region.Vertices[ i ] * sX ) - originX ) * scaleX;
            float fy = ( ( region.Vertices[ i + 1 ] * sY ) - originY ) * scaleY;

            _vertices[ _vertexIndex++ ] = ( cos * fx ) - ( sin * fy ) + worldOriginX;
            _vertices[ _vertexIndex++ ] = ( sin * fx ) + ( cos * fy ) + worldOriginY;
            _vertices[ _vertexIndex++ ] = _colorPacked;
            _vertices[ _vertexIndex++ ] = region.TextureCoords[ i ];
            _vertices[ _vertexIndex++ ] = region.TextureCoords[ i + 1 ];
        }
    }

    /// <summary>
    /// Draws a polygon using the specified texture, vertices, and triangles.
    /// </summary>
    /// <param name="texture"> The texture to apply to the polygon. </param>
    /// <param name="polygonVertices"> An array of vertex coordinates defining the polygon. </param>
    /// <param name="verticesOffset">
    /// The starting offset in the vertex array to begin reading vertices.
    /// </param>
    /// <param name="verticesCount"> The number of vertices to read from the vertex array. </param>
    /// <param name="polygonTriangles">
    /// An array of triangle indices defining how the vertices are connected.
    /// </param>
    /// <param name="trianglesOffset">
    /// The starting offset in the triangle array to begin reading indices.
    /// </param>
    /// <param name="trianglesCount">
    /// The number of triangle indices to read from the triangle array.
    /// </param>
    public void Draw( Texture2D texture,
                      float[] polygonVertices,
                      int verticesOffset,
                      int verticesCount,
                      short[] polygonTriangles,
                      int trianglesOffset,
                      int trianglesCount )
    {
        if ( !IsDrawing )
        {
            throw new RuntimeException( "PolygonSpriteBatch.begin must be called before Draw." );
        }

        if ( texture != _lastTexture )
        {
            SwitchTexture( texture );
        }
        else if ( ( ( _triangleIndex + trianglesCount ) > _triangles.Length )
               || ( ( _vertexIndex + verticesCount ) > _vertices.Length ) )
        {
            Flush();
        }

        int startVertex = _vertexIndex / Sprite2D.VertexSize;

        for ( int i = trianglesOffset, n = i + trianglesCount; i < n; i++ )
        {
            _triangles[ _triangleIndex++ ] = ( short )( polygonTriangles[ i ] + startVertex );
        }

        Array.Copy( polygonVertices, verticesOffset, _vertices, _vertexIndex, verticesCount );
        _vertexIndex += verticesCount;
    }

    /// <summary>
    /// Draws a rectangle with the bottom left corner at regionX, regionX having the given width
    /// and height, from region.Width and region.Height, in pixels. The rectangle is offset by origin.X,
    /// origin.Y relative to the origin. Scale specifies the scaling factor by which the rectangle
    /// should be scaled around originX, originY. Rotation specifies the angle of counter clockwise
    /// rotation of the rectangle around originX, originY. The portion of the <see cref="Texture2D"/>
    /// given by srcX, srcY and srcWidth, srcHeight is used.
    /// <para>
    /// These coordinates and sizes are given in texels. FlipX and FlipY specify whether the texture
    /// portion should be flipped horizontally or vertically.
    /// </para>
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="region">
    /// the x &amp; y coordinates in screen space, and width &amp; height of the rectangle.
    /// </param>
    /// <param name="origin">
    /// the x &amp; y coordinates of the scaling and rotation origin relative to the screen space coordinates
    /// </param>
    /// <param name="scale"> the scale of the rectangle around originX/originY in x &amp; y </param>
    /// <param name="rotation">
    /// the angle of counter clockwise rotation of the rectangle around originX/originY
    /// </param>
    /// <param name="src">
    /// the x &amp; y coordinates in texel space, and source width &amp; height in texels.
    /// </param>
    /// <param name="flipX"> whether to flip the sprite horizontally </param>
    /// <param name="flipY"> whether to flip the sprite vertically </param>
    public void Draw( Texture2D texture,
                      GRect region,
                      Point2D origin,
                      Point2D scale,
                      float rotation,
                      GRect src,
                      bool flipX,
                      bool flipY )
    {
        if ( !IsDrawing )
        {
            throw new RuntimeException( "PolygonSpriteBatch.begin must be called before Draw." );
        }

        if ( texture != _lastTexture )
        {
            SwitchTexture( texture );
        }
        else if ( ( ( _triangleIndex + 6 ) > _triangles.Length )
               || ( ( _vertexIndex + Sprite2D.SpriteSize ) > _vertices.Length ) )
        {
            Flush();
        }

        int startVertex = _vertexIndex / Sprite2D.VertexSize;

        _triangles[ _triangleIndex++ ] = ( short )startVertex;
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 1 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 3 );
        _triangles[ _triangleIndex++ ] = ( short )startVertex;

        // bottom left and top right corner points relative to origin
        int worldOriginX = region.X + origin.X;
        int worldOriginY = region.Y + origin.Y;
        int fx           = -origin.X;
        int fy           = -origin.Y;
        int fx2          = region.Width - origin.X;
        int fy2          = region.Height - origin.Y;

        // scale
        if ( !scale.X.Equals( 1 ) || !scale.Y.Equals( 1 ) )
        {
            fx  *= scale.X;
            fy  *= scale.Y;
            fx2 *= scale.X;
            fy2 *= scale.Y;
        }

        // construct corner points, start from top left and go counter clockwise
        int p1X = fx;
        int p1Y = fy;
        int p2X = fx;
        int p2Y = fy2;
        int p3X = fx2;
        int p3Y = fy2;
        int p4X = fx2;
        int p4Y = fy;

        float x1;
        float y1;
        float x2;
        float y2;
        float x3;
        float y3;
        float x4;
        float y4;

        // rotate
        if ( rotation != 0 )
        {
            float cos = MathUtils.CosDeg( rotation );
            float sin = MathUtils.SinDeg( rotation );

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

        float u  = src.X * _invTexWidth;
        float v  = ( src.Y + src.Height ) * _invTexHeight;
        float u2 = ( src.X + src.Width ) * _invTexWidth;
        float v2 = src.Y * _invTexHeight;

        if ( flipX )
        {
            ( u, u2 ) = ( u2, u );
        }

        if ( flipY )
        {
            ( v, v2 ) = ( v2, v );
        }

        _vertices[ _vertexIndex++ ] = x1;
        _vertices[ _vertexIndex++ ] = y1;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u;
        _vertices[ _vertexIndex++ ] = v;

        _vertices[ _vertexIndex++ ] = x2;
        _vertices[ _vertexIndex++ ] = y2;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u;
        _vertices[ _vertexIndex++ ] = v2;

        _vertices[ _vertexIndex++ ] = x3;
        _vertices[ _vertexIndex++ ] = y3;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u2;
        _vertices[ _vertexIndex++ ] = v2;

        _vertices[ _vertexIndex++ ] = x4;
        _vertices[ _vertexIndex++ ] = y4;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u2;
        _vertices[ _vertexIndex++ ] = v;
    }

    /// <summary>
    /// Draws a rectangle with the bottom left corner at x,y having the given width and height
    /// in pixels. The portion of the <see cref="Texture2D"/> given by srcX, srcY and srcWidth,
    /// srcHeight is used. These coordinates and sizes are given in texels. FlipX and flipY
    /// specify whether the texture portion should be flipped horizontally or vertically.
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="region">
    /// the x &amp; y coordinates in screen space, and width &amp; height of the rectangle.
    /// </param>
    /// <param name="src">
    /// the x &amp; y coordinates in texel space, and source width &amp; height in texels.
    /// </param>
    /// <param name="flipX"> whether to flip the sprite horizontally </param>
    /// <param name="flipY"> whether to flip the sprite vertically </param>
    public void Draw( Texture2D texture,
                      GRect region,
                      GRect src,
                      bool flipX,
                      bool flipY )
    {
        if ( !IsDrawing )
        {
            throw new RuntimeException( "PolygonSpriteBatch.begin must be called before Draw." );
        }

        if ( texture != _lastTexture )
        {
            SwitchTexture( texture );
        }
        else if ( ( ( _triangleIndex + 6 ) > _triangles.Length )
               || ( ( _vertexIndex + Sprite2D.SpriteSize ) > _vertices.Length ) )
        {
            Flush();
        }

        int startVertex = _vertexIndex / Sprite2D.VertexSize;

        _triangles[ _triangleIndex++ ] = ( short )startVertex;
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 1 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 3 );
        _triangles[ _triangleIndex++ ] = ( short )startVertex;

        float u   = src.X * _invTexWidth;
        float v   = ( src.Y + src.Height ) * _invTexHeight;
        float u2  = ( src.X + src.Width ) * _invTexWidth;
        float v2  = src.Y * _invTexHeight;
        int   fx2 = region.X + region.Width;
        int   fy2 = region.Y + region.Height;

        if ( flipX )
        {
            ( u, u2 ) = ( u2, u );
        }

        if ( flipY )
        {
            ( v, v2 ) = ( v2, v );
        }

        _vertices[ _vertexIndex++ ] = region.X;
        _vertices[ _vertexIndex++ ] = region.Y;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u;
        _vertices[ _vertexIndex++ ] = v;

        _vertices[ _vertexIndex++ ] = region.X;
        _vertices[ _vertexIndex++ ] = fy2;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u;
        _vertices[ _vertexIndex++ ] = v2;

        _vertices[ _vertexIndex++ ] = fx2;
        _vertices[ _vertexIndex++ ] = fy2;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u2;
        _vertices[ _vertexIndex++ ] = v2;

        _vertices[ _vertexIndex++ ] = fx2;
        _vertices[ _vertexIndex++ ] = region.Y;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u2;
        _vertices[ _vertexIndex++ ] = v;
    }

    /// <summary>
    /// Draws a rectangle with the bottom left corner at x,y having the given width and height
    /// in pixels. The portion of the <see cref="Texture2D"/> given by srcX, srcY and srcWidth,
    /// srcHeight are used. These coordinates and sizes are given in texels.
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="x"> the x-coordinate in screen space </param>
    /// <param name="y"> the y-coordinate in screen space </param>
    /// <param name="src">
    /// the x &amp; y coordinates in texel space, and source width &amp; height in texels.
    /// </param>
    public void Draw( Texture2D texture, float x, float y, GRect src )
    {
        if ( !IsDrawing )
        {
            throw new RuntimeException( "PolygonSpriteBatch.begin must be called before Draw." );
        }

        if ( texture != _lastTexture )
        {
            SwitchTexture( texture );
        }
        else if ( ( ( _triangleIndex + 6 ) > _triangles.Length )
               || ( ( _vertexIndex + Sprite2D.SpriteSize ) > _vertices.Length ) )
        {
            Flush();
        }

        int startVertex = _vertexIndex / Sprite2D.VertexSize;

        _triangles[ _triangleIndex++ ] = ( short )startVertex;
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 1 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 3 );
        _triangles[ _triangleIndex++ ] = ( short )startVertex;

        float u   = src.X * _invTexWidth;
        float v   = ( src.Y + src.Height ) * _invTexHeight;
        float u2  = ( src.X + src.Width ) * _invTexWidth;
        float v2  = src.Y * _invTexHeight;
        float fx2 = x + src.Width;
        float fy2 = y + src.Height;

        _vertices[ _vertexIndex++ ] = x;
        _vertices[ _vertexIndex++ ] = y;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u;
        _vertices[ _vertexIndex++ ] = v;

        _vertices[ _vertexIndex++ ] = x;
        _vertices[ _vertexIndex++ ] = fy2;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u;
        _vertices[ _vertexIndex++ ] = v2;

        _vertices[ _vertexIndex++ ] = fx2;
        _vertices[ _vertexIndex++ ] = fy2;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u2;
        _vertices[ _vertexIndex++ ] = v2;

        _vertices[ _vertexIndex++ ] = fx2;
        _vertices[ _vertexIndex++ ] = y;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u2;
        _vertices[ _vertexIndex++ ] = v;
    }

    /// <summary>
    /// Draws a rectangle with the bottom left corner at x,y having the given width and height
    /// in pixels. The portion of the <see cref="Texture2D"/> given by u, v and u2, v2 are used.
    /// These coordinates and sizes are given in texture size percentage. The rectangle will
    /// have the given tint <see cref="IBatch.Color"/>.
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="region">
    /// the x &amp; y coordinates in screen space, and width &amp; height of the rectangle.
    /// </param>
    /// <param name="u">  The u-coordinate in texture space </param>
    /// <param name="v">  The v-coordinate in texture space </param>
    /// <param name="u2"> The u2-coordinate in texture space </param>
    /// <param name="v2"> The v2-coordinate in texture space </param>
    public void Draw( Texture2D texture, GRect region, float u, float v, float u2, float v2 )
    {
        if ( !IsDrawing )
        {
            throw new RuntimeException( "PolygonSpriteBatch.begin must be called before Draw." );
        }

        if ( texture != _lastTexture )
        {
            SwitchTexture( texture );
        }
        else if ( ( ( _triangleIndex + 6 ) > _triangles.Length )
               || ( ( _vertexIndex + Sprite2D.SpriteSize ) > _vertices.Length ) )
        {
            Flush();
        }

        int startVertex = _vertexIndex / Sprite2D.VertexSize;

        _triangles[ _triangleIndex++ ] = ( short )startVertex;
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 1 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 3 );
        _triangles[ _triangleIndex++ ] = ( short )startVertex;

        int fx2 = region.X + region.Width;
        int fy2 = region.Y + region.Height;

        _vertices[ _vertexIndex++ ] = region.X;
        _vertices[ _vertexIndex++ ] = region.Y;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u;
        _vertices[ _vertexIndex++ ] = v;

        _vertices[ _vertexIndex++ ] = region.X;
        _vertices[ _vertexIndex++ ] = fy2;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u;
        _vertices[ _vertexIndex++ ] = v2;

        _vertices[ _vertexIndex++ ] = fx2;
        _vertices[ _vertexIndex++ ] = fy2;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u2;
        _vertices[ _vertexIndex++ ] = v2;

        _vertices[ _vertexIndex++ ] = fx2;
        _vertices[ _vertexIndex++ ] = region.X;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u2;
        _vertices[ _vertexIndex++ ] = v;
    }

    /// <summary>
    /// Draws a texture with the bottom left corner at x,y having the width and
    /// height of the texture.
    /// </summary>
    /// <param name="texture"> The texture to draw </param>
    /// <param name="x"> the x-coordinate in screen space </param>
    /// <param name="y"> the y-coordinate in screen space  </param>
    public void Draw( Texture2D texture, float x, float y )
    {
        Draw( texture, x, y, texture.Width, texture.Height );
    }

    /// <summary>
    /// Draws a rectangle with the bottom left corner at x,y and stretching the region
    /// to cover the given width and height.
    /// </summary>
    /// <param name="texture"> The texture to draw </param>
    /// <param name="posX"> the x-coordinate in screen space </param>
    /// <param name="posY"> the y-coordinate in screen space  </param>
    /// <param name="width"> The width of the rectangle </param>
    /// <param name="height"> The height of the rectangle </param>
    public void Draw( Texture2D texture, float posX, float posY, float width, float height )
    {
        if ( !IsDrawing )
        {
            throw new RuntimeException( "PolygonSpriteBatch.begin must be called before Draw." );
        }

        if ( texture != _lastTexture )
        {
            SwitchTexture( texture );
        }
        else if ( ( ( _triangleIndex + 6 ) > _triangles.Length )
               || ( ( _vertexIndex + Sprite2D.SpriteSize ) > _vertices.Length ) )
        {
            Flush();
        }

        int startVertex = _vertexIndex / Sprite2D.VertexSize;

        _triangles[ _triangleIndex++ ] = ( short )startVertex;
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 1 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 3 );
        _triangles[ _triangleIndex++ ] = ( short )startVertex;

        float fx2 = posX + width;
        float fy2 = posY + height;
        float u   = 0;
        float v   = 1;
        float u2  = 1;
        float v2  = 0;

        _vertices[ _vertexIndex++ ] = posX;
        _vertices[ _vertexIndex++ ] = posY;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u;
        _vertices[ _vertexIndex++ ] = v;

        _vertices[ _vertexIndex++ ] = posX;
        _vertices[ _vertexIndex++ ] = fy2;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u;
        _vertices[ _vertexIndex++ ] = v2;

        _vertices[ _vertexIndex++ ] = fx2;
        _vertices[ _vertexIndex++ ] = fy2;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u2;
        _vertices[ _vertexIndex++ ] = v2;

        _vertices[ _vertexIndex++ ] = fx2;
        _vertices[ _vertexIndex++ ] = posY;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u2;
        _vertices[ _vertexIndex++ ] = v;
    }

    /// <summary>
    /// Renders a set of sprite vertices using the specified texture.
    /// </summary>
    /// <param name="texture">The texture to be used for rendering. Can be null if not needed.</param>
    /// <param name="spriteVertices">An array of vertex data describing the sprites to be rendered.</param>
    /// <param name="offset">The starting index in the vertex array from which to begin processing.</param>
    /// <param name="count">The number of vertices to process starting from the specified offset.</param>
    public void Draw( Texture2D texture, float[] spriteVertices, int offset, int count )
    {
        if ( !IsDrawing )
        {
            throw new RuntimeException( "PolygonSpriteBatch.begin must be called before Draw." );
        }

        int triangleCount = count / Sprite2D.SpriteSize * 6;
        int batch;

        if ( texture != _lastTexture )
        {
            SwitchTexture( texture );
            batch = Math.Min( Math.Min( count, _vertices.Length - ( _vertices.Length % Sprite2D.SpriteSize ) ),
                              _triangles.Length / 6 * Sprite2D.SpriteSize );
            triangleCount = batch / Sprite2D.SpriteSize * 6;
        }
        else if ( ( ( _triangleIndex + triangleCount ) > _triangles.Length )
               || ( ( _vertexIndex + count ) > _vertices.Length ) )
        {
            Flush();
            batch = Math.Min( Math.Min( count, _vertices.Length - ( _vertices.Length % Sprite2D.SpriteSize ) ),
                              _triangles.Length / 6 * Sprite2D.SpriteSize );
            triangleCount = batch / Sprite2D.SpriteSize * 6;
        }
        else
        {
            batch = count;
        }

        var vertex = ( short )( _vertexIndex / Sprite2D.VertexSize );

        for ( int n = _triangleIndex + triangleCount; _triangleIndex < n; _triangleIndex += 6, vertex += 4 )
        {
            _triangles[ _triangleIndex ]     = vertex;
            _triangles[ _triangleIndex + 1 ] = ( short )( vertex + 1 );
            _triangles[ _triangleIndex + 2 ] = ( short )( vertex + 2 );
            _triangles[ _triangleIndex + 3 ] = ( short )( vertex + 2 );
            _triangles[ _triangleIndex + 4 ] = ( short )( vertex + 3 );
            _triangles[ _triangleIndex + 5 ] = vertex;
        }

        int vertexIndex   = _vertexIndex;
        int triangleIndex = _triangleIndex;

        while ( true )
        {
            Array.Copy( spriteVertices, offset, _vertices, vertexIndex, batch );
            _vertexIndex   =  vertexIndex + batch;
            _triangleIndex =  triangleIndex;
            count          -= batch;

            if ( count == 0 )
            {
                break;
            }

            offset += batch;
            Flush();
            vertexIndex = 0;

            if ( batch > count )
            {
                batch         = Math.Min( count, _triangles.Length / 6 * Sprite2D.SpriteSize );
                triangleIndex = batch / Sprite2D.SpriteSize * 6;
            }
        }
    }

    /// <summary>
    /// Draws a texture region with the bottom left corner at x,y having the width and
    /// height of the texture region.
    /// </summary>
    /// <param name="region"> The texture to draw </param>
    /// <param name="x"> the x-coordinate in screen space </param>
    /// <param name="y"> the y-coordinate in screen space  </param>
    public void Draw( TextureRegion region, float x, float y )
    {
        Draw( region, x, y, region.GetRegionWidth(), region.GetRegionHeight() );
    }

    /// <summary>
    /// Draws a texture region with the bottom left corner at x,y and stretching the region
    /// to cover the given width and height.
    /// </summary>
    /// <param name="region"> The texture to draw </param>
    /// <param name="x"> the x-coordinate in screen space </param>
    /// <param name="y"> the y-coordinate in screen space  </param>
    /// <param name="width"> The width of the rectangle </param>
    /// <param name="height"> The height of the rectangle </param>
    public void Draw( TextureRegion region, float x, float y, float width, float height )
    {
        if ( !IsDrawing )
        {
            throw new RuntimeException( "PolygonSpriteBatch.begin must be called before Draw." );
        }

        if ( region.Texture == null )
        {
            Logger.Error( "Cannot draw a null texture!" );

            return;
        }

        if ( region.Texture != _lastTexture )
        {
            SwitchTexture( region.Texture );
        }
        else if ( ( ( _triangleIndex + 6 ) > _triangles.Length )
               || ( ( _vertexIndex + Sprite2D.SpriteSize ) > _vertices.Length ) ) //
        {
            Flush();
        }

        int startVertex = _vertexIndex / Sprite2D.VertexSize;

        _triangles[ _triangleIndex++ ] = ( short )startVertex;
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 1 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 3 );
        _triangles[ _triangleIndex++ ] = ( short )startVertex;

        float fx2 = x + width;
        float fy2 = y + height;
        float u   = region.U;
        float v   = region.V2;
        float u2  = region.U2;
        float v2  = region.V;

        _vertices[ _vertexIndex++ ] = x;
        _vertices[ _vertexIndex++ ] = y;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u;
        _vertices[ _vertexIndex++ ] = v;

        _vertices[ _vertexIndex++ ] = x;
        _vertices[ _vertexIndex++ ] = fy2;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u;
        _vertices[ _vertexIndex++ ] = v2;

        _vertices[ _vertexIndex++ ] = fx2;
        _vertices[ _vertexIndex++ ] = fy2;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u2;
        _vertices[ _vertexIndex++ ] = v2;

        _vertices[ _vertexIndex++ ] = fx2;
        _vertices[ _vertexIndex++ ] = y;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u2;
        _vertices[ _vertexIndex++ ] = v;
    }

    /// <summary>
    /// Draws a rectangle with the bottom left corner at x,y and stretching the region to
    /// cover the given width and height. The rectangle is offset by originX, originY relative
    /// to the origin. Scale specifies the scaling factor by which the rectangle should be scaled
    /// around originX, originY. Rotation specifies the angle of counter clockwise rotation
    /// of the rectangle around originX, originY.
    /// </summary>
    /// <param name="textureRegion">The texture region to be drawn.</param>
    /// <param name="region">The rectangular region where the texture will be drawn.</param>
    /// <param name="origin">The origin point for transformations such as scaling and rotation.</param>
    /// <param name="scale">The scale factor to be applied to the texture region.</param>
    /// <param name="rotation">The rotation angle in radians to be applied to the texture region.</param>
    public void Draw( TextureRegion textureRegion,
                      GRect region,
                      Point2D origin,
                      Point2D scale,
                      float rotation )
    {
        if ( !IsDrawing )
        {
            throw new RuntimeException( "PolygonSpriteBatch.begin must be called before Draw." );
        }

        if ( textureRegion.Texture == null )
        {
            Logger.Error( "Cannot draw a null texture!" );

            return;
        }

        if ( textureRegion.Texture != _lastTexture )
        {
            SwitchTexture( textureRegion.Texture );
        }
        else if ( ( ( _triangleIndex + 6 ) > _triangles.Length )
               || ( ( _vertexIndex + Sprite2D.SpriteSize ) > _vertices.Length ) ) //
        {
            Flush();
        }

        int startVertex = _vertexIndex / Sprite2D.VertexSize;

        _triangles[ _triangleIndex++ ] = ( short )startVertex;
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 1 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 3 );
        _triangles[ _triangleIndex++ ] = ( short )startVertex;

        // bottom left and top right corner points relative to origin
        int worldOriginX = region.X + origin.X;
        int worldOriginY = region.Y + origin.Y;
        int fx           = -origin.X;
        int fy           = -origin.Y;
        int fx2          = region.Width - origin.X;
        int fy2          = region.Height - origin.Y;

        // scale
        if ( !scale.X.Equals( 1 ) || !scale.Y.Equals( 1 ) )
        {
            fx  *= scale.X;
            fy  *= scale.Y;
            fx2 *= scale.X;
            fy2 *= scale.Y;
        }

        // construct corner points, start from top left and go counter clockwise
        int p1X = fx;
        int p1Y = fy;
        int p2X = fx;
        int p2Y = fy2;
        int p3X = fx2;
        int p3Y = fy2;
        int p4X = fx2;
        int p4Y = fy;

        float x1;
        float y1;
        float x2;
        float y2;
        float x3;
        float y3;
        float x4;
        float y4;

        // rotate
        if ( rotation != 0 )
        {
            float cos = MathUtils.CosDeg( rotation );
            float sin = MathUtils.SinDeg( rotation );

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

        float u  = textureRegion.U;
        float v  = textureRegion.V2;
        float u2 = textureRegion.U2;
        float v2 = textureRegion.V;

        _vertices[ _vertexIndex++ ] = x1;
        _vertices[ _vertexIndex++ ] = y1;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u;
        _vertices[ _vertexIndex++ ] = v;

        _vertices[ _vertexIndex++ ] = x2;
        _vertices[ _vertexIndex++ ] = y2;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u;
        _vertices[ _vertexIndex++ ] = v2;

        _vertices[ _vertexIndex++ ] = x3;
        _vertices[ _vertexIndex++ ] = y3;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u2;
        _vertices[ _vertexIndex++ ] = v2;

        _vertices[ _vertexIndex++ ] = x4;
        _vertices[ _vertexIndex++ ] = y4;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u2;
        _vertices[ _vertexIndex++ ] = v;
    }

    /// <summary>
    /// Draws a rectangle with the texture coordinates rotated 90 degrees. The bottom
    /// left corner at x,y and stretching the region to cover the given width and height.
    /// The rectangle is offset by originX, originY relative to the origin. Scale specifies
    /// the scaling factor by which the rectangle should be scaled around originX, originY.
    /// Rotation specifies the angle of counter clockwise rotation of the rectangle around
    /// originX, originY.
    /// </summary>
    /// <param name="textureRegion"></param>
    /// <param name="region">
    /// the x &amp; y coordinates in screen space, and width &amp; height of the rectangle.
    /// </param>
    /// <param name="origin"></param>
    /// <param name="scale"></param>
    /// <param name="rotation"></param>
    /// <param name="clockwise">
    /// If true, the texture coordinates are rotated 90 degrees clockwise.
    /// If false, they are rotated 90 degrees counter clockwise.
    /// </param>
    public void Draw( TextureRegion textureRegion,
                      GRect region,
                      Point2D origin,
                      Point2D scale,
                      float rotation,
                      bool clockwise )
    {
        if ( !IsDrawing )
        {
            throw new RuntimeException( "PolygonSpriteBatch.begin must be called before Draw." );
        }

        if ( textureRegion.Texture == null )
        {
            Logger.Error( "Cannot draw a null texture!" );

            return;
        }

        if ( textureRegion.Texture != _lastTexture )
        {
            SwitchTexture( textureRegion.Texture );
        }
        else if ( ( ( _triangleIndex + 6 ) > _triangles.Length )
               || ( ( _vertexIndex + Sprite2D.SpriteSize ) > _vertices.Length ) )
        {
            Flush();
        }

        int startVertex = _vertexIndex / Sprite2D.VertexSize;

        _triangles[ _triangleIndex++ ] = ( short )startVertex;
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 1 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 3 );
        _triangles[ _triangleIndex++ ] = ( short )startVertex;

        // bottom left and top right corner points relative to origin
        int worldOriginX = region.X + origin.X;
        int worldOriginY = region.Y + origin.Y;
        int fx           = -origin.X;
        int fy           = -origin.Y;
        int fx2          = region.Width - origin.X;
        int fy2          = region.Height - origin.Y;

        // scale
        if ( !scale.X.Equals( 1 ) || !scale.Y.Equals( 1 ) )
        {
            fx  *= scale.X;
            fy  *= scale.Y;
            fx2 *= scale.X;
            fy2 *= scale.Y;
        }

        // construct corner points, start from top left and go counter clockwise
        int p1X = fx;
        int p1Y = fy;
        int p2X = fx;
        int p2Y = fy2;
        int p3X = fx2;
        int p3Y = fy2;
        int p4X = fx2;
        int p4Y = fy;

        float x1;
        float y1;
        float x2;
        float y2;
        float x3;
        float y3;
        float x4;
        float y4;

        // rotate
        if ( rotation != 0 )
        {
            float cos = MathUtils.CosDeg( rotation );
            float sin = MathUtils.SinDeg( rotation );

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

        _vertices[ _vertexIndex++ ] = x1;
        _vertices[ _vertexIndex++ ] = y1;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u1;
        _vertices[ _vertexIndex++ ] = v1;

        _vertices[ _vertexIndex++ ] = x2;
        _vertices[ _vertexIndex++ ] = y2;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u2;
        _vertices[ _vertexIndex++ ] = v2;

        _vertices[ _vertexIndex++ ] = x3;
        _vertices[ _vertexIndex++ ] = y3;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u3;
        _vertices[ _vertexIndex++ ] = v3;

        _vertices[ _vertexIndex++ ] = x4;
        _vertices[ _vertexIndex++ ] = y4;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u4;
        _vertices[ _vertexIndex++ ] = v4;
    }

    /// <summary>
    /// Draws a texture using a specified region, dimensions, and transformation parameters.
    /// </summary>
    /// <param name="region">The specific texture region to be drawn.</param>
    /// <param name="width">The width of the drawn texture.</param>
    /// <param name="height">The height of the drawn texture.</param>
    /// <param name="transform">The transformation to be applied to the texture.</param>
    public void Draw( TextureRegion region, float width, float height, Affine2 transform )
    {
        if ( !IsDrawing )
        {
            throw new RuntimeException( "PolygonSpriteBatch.begin must be called before Draw." );
        }

        if ( region.Texture == null )
        {
            Logger.Error( "Cannot draw a null texture!" );

            return;
        }

        if ( region.Texture != _lastTexture )
        {
            SwitchTexture( region.Texture );
        }
        else if ( ( ( _triangleIndex + 6 ) > _triangles.Length )
               || ( ( _vertexIndex + Sprite2D.SpriteSize ) > _vertices.Length ) )
        {
            Flush();
        }

        int startVertex = _vertexIndex / Sprite2D.VertexSize;

        _triangles[ _triangleIndex++ ] = ( short )startVertex;
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 1 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 2 );
        _triangles[ _triangleIndex++ ] = ( short )( startVertex + 3 );
        _triangles[ _triangleIndex++ ] = ( short )startVertex;

        // construct corner points
        float x1 = transform.M02;
        float y1 = transform.M12;
        float x2 = ( transform.M01 * height ) + transform.M02;
        float y2 = ( transform.M11 * height ) + transform.M12;
        float x3 = ( transform.M00 * width ) + ( transform.M01 * height ) + transform.M02;
        float y3 = ( transform.M10 * width ) + ( transform.M11 * height ) + transform.M12;
        float x4 = ( transform.M00 * width ) + transform.M02;
        float y4 = ( transform.M10 * width ) + transform.M12;

        float u  = region.U;
        float v  = region.V2;
        float u2 = region.U2;
        float v2 = region.V;

        _vertices[ _vertexIndex++ ] = x1;
        _vertices[ _vertexIndex++ ] = y1;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u;
        _vertices[ _vertexIndex++ ] = v;

        _vertices[ _vertexIndex++ ] = x2;
        _vertices[ _vertexIndex++ ] = y2;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u;
        _vertices[ _vertexIndex++ ] = v2;

        _vertices[ _vertexIndex++ ] = x3;
        _vertices[ _vertexIndex++ ] = y3;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u2;
        _vertices[ _vertexIndex++ ] = v2;

        _vertices[ _vertexIndex++ ] = x4;
        _vertices[ _vertexIndex++ ] = y4;
        _vertices[ _vertexIndex++ ] = _colorPacked;
        _vertices[ _vertexIndex++ ] = u2;
        _vertices[ _vertexIndex++ ] = v;
    }

    /// <summary>
    /// Renders the current batch of vertices and triangles to the graphics device.
    /// This method processes the existing data in the batch, applies the appropriate
    /// rendering settings, and clears the batch for new draw operations.
    /// It is automatically called when the batch is full or can be invoked manually.
    /// </summary>
    public void Flush()
    {
        if ( _vertexIndex == 0 )
        {
            return;
        }

        RenderCalls++;
        TotalRenderCalls++;

        int trianglesInBatch = _triangleIndex;

        if ( trianglesInBatch > MaxTrianglesInBatch )
        {
            MaxTrianglesInBatch = trianglesInBatch;
        }

        _lastTexture?.Bind();

        Mesh mesh = _mesh;

        mesh.SetVertices( _vertices, 0, _vertexIndex );
        mesh.SetIndices( _triangles, 0, trianglesInBatch );

        if ( _blendingDisabled )
        {
            Engine.GL.Disable( EnableCap.Blend );
        }
        else
        {
            Engine.GL.Enable( EnableCap.Blend );

            Engine.GL.BlendFuncSeparate( BlendSrcFunc, BlendDstFunc, BlendSrcFuncAlpha, BlendDstFuncAlpha );
        }

        mesh.Render( _customShader ?? _shader, IGL.GLTriangles, 0, trianglesInBatch );

        _vertexIndex   = 0;
        _triangleIndex = 0;
    }

    /// <summary>
    /// Disables blending.
    /// </summary>
    public void DisableBlending()
    {
        Flush();
        _blendingDisabled = true;
    }

    /// <summary>
    /// Enables blending.
    /// </summary>
    public void EnableBlending()
    {
        Flush();
        _blendingDisabled = false;
    }

    /// <summary>
    /// Sets the Blend Functions to use when rendering. The provided
    /// methods handle both Color and Alpha functions.
    /// </summary>
    /// <param name="srcFunc"> Source Function for Color and Alpha. </param>
    /// <param name="dstFunc"> Destination Function for Color and Alpha. </param>
    public void SetBlendFunction( BlendMode srcFunc, BlendMode dstFunc )
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
    public void SetBlendFunctionSeparate( BlendMode srcFuncColor, BlendMode dstFuncColor, BlendMode srcFuncAlpha,
                                          BlendMode dstFuncAlpha )
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

    /// <summary>
    /// Disposes of the resources used by this PolygonSpriteBatch.
    /// </summary>
    public void Dispose()
    {
        _mesh.Dispose();

        if ( _ownsShader && ( _shader != null ) )
        {
            _shader.Dispose();
        }
        
        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// Sets the projection matrix used for drawing.
    /// </summary>
    /// <param name="projection">The new projection matrix to be applied.</param>
    public void SetProjectionMatrix( Matrix4 projection )
    {
        if ( IsDrawing )
        {
            Flush();
        }

        ProjectionMatrix.Set( projection );

        if ( IsDrawing )
        {
            SetupMatrices();
        }
    }

    /// <summary>
    /// Sets the transformation matrix to be applied to the SpriteBatch during
    /// the rendering process.
    /// </summary>
    /// <param name="transform">
    /// A Matrix4 representing the new transformation to be applied.
    /// </param>
    public void SetTransformMatrix( Matrix4 transform )
    {
        if ( IsDrawing )
        {
            Flush();
        }

        TransformMatrix.Set( transform );

        if ( IsDrawing )
        {
            SetupMatrices();
        }
    }

    /// <summary>
    /// The shader program used for rendering polygons. This can be set to a custom
    /// shader or defaults to the internal shader if not specified.
    /// </summary>
    public ShaderProgram? Shader
    {
        get => _customShader ?? _shader;
        set
        {
            if ( IsDrawing )
            {
                Flush();
            }

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
    /// Configures the combined matrix by multiplying the projection and transform matrices,
    /// and uploads it to the active shader program. Additionally sets the texture uniform.
    /// </summary>
    public void SetupMatrices()
    {
        _combinedMatrix.Set( ProjectionMatrix ).Mul( TransformMatrix );

        if ( _customShader != null )
        {
            _customShader.SetUniformMatrix( "u_projTrans", _combinedMatrix );
            _customShader.SetUniformi( "u_texture", 0 );
        }
        else
        {
            _shader?.SetUniformMatrix( "u_projTrans", _combinedMatrix );
            _shader?.SetUniformi( "u_texture", 0 );
        }
    }

    /// <summary>
    /// Flushes the sprite batch, then sets the last texture to the given texture.
    /// </summary>
    private void SwitchTexture( Texture2D texture )
    {
        Flush();

        _lastTexture  = texture;
        _invTexWidth  = 1.0f / texture.Width;
        _invTexHeight = 1.0f / texture.Height;
    }

    /// <summary>
    /// Returns <c>true</c> if blending is enabled.
    /// </summary>
    public bool IsBlendingEnabled()
    {
        return !_blendingDisabled;
    }
}

// ============================================================================
// ============================================================================