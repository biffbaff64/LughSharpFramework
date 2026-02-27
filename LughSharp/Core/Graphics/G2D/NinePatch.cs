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

using JetBrains.Annotations;

using LughSharp.Core.Graphics.OpenGL.Enums;
using LughSharp.Core.Maths;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Graphics.G2D;

/// <summary>
/// A 3x3 grid of texture regions. Any of the regions may be omitted.
/// Padding may be set as a hint on how to inset content on top of the ninepatch
/// (by default the eight "edge" textures of the ninepatch define the padding).
/// When drawn, the four corner patches will not be scaled, the interior patch will
/// be scaled in both directions, and the middle patch for each edge will be scaled
/// in only one direction.
/// <para>
/// Note this class does not accept ".9.png" textures that include the metadata
/// border pixels describing the splits (and padding) for the ninepatch. That
/// information is either passed to a constructor or defined implicitly by the
/// size of the individual patch textures. TextureAtlas is one way to generate
/// a postprocessed ninepatch texture regions from ".9.png" files.
/// </para>
/// </summary>
[PublicAPI]
public class NinePatch
{
    public const int TOP_LEFT      = 0;
    public const int TOP_CENTER    = 1;
    public const int TOP_RIGHT     = 2;
    public const int MIDDLE_LEFT   = 3;
    public const int MIDDLE_CENTER = 4;
    public const int MIDDLE_RIGHT  = 5;
    public const int BOTTOM_LEFT   = 6;
    public const int BOTTOM_CENTER = 7;
    public const int BOTTOM_RIGHT  = 8;

    // ========================================================================

    public float[]  Vertices     { get; set; } = new float[ 9 * 4 * 5 ];
    public Color    Color        { get; set; } = new( Color.White );
    public Texture? Texture      { get; set; }
    public int      Idx          { get; set; }
    public int      BottomLeft   { get; set; }
    public int      BottomCenter { get; set; }
    public int      BottomRight  { get; set; }
    public int      MiddleLeft   { get; set; }
    public int      MiddleCenter { get; set; }
    public int      MiddleRight  { get; set; }
    public int      TopLeft      { get; set; }
    public int      TopCenter    { get; set; }
    public int      TopRight     { get; set; }
    public float    LeftWidth    { get; set; }
    public float    RightWidth   { get; set; }
    public float    MiddleWidth  { get; set; }
    public float    MiddleHeight { get; set; }
    public float    TopHeight    { get; set; }
    public float    BottomHeight { get; set; }

    public float TotalWidth => LeftWidth + MiddleWidth + RightWidth;

    public float TotalHeight => TopHeight + MiddleHeight + BottomHeight;

    public float PadLeft
    {
        get => _padLeft <= 0.0001f ? LeftWidth : _padLeft;
        set => _padLeft = value;
    }

    public float PadRight
    {
        get => _padRight <= 0.0001f ? RightWidth : _padRight;
        set => _padRight = value;
    }

    public float PadTop
    {
        get => _padTop <= 0.0001f ? TopHeight : _padTop;
        set => _padTop = value;
    }

    public float PadBottom
    {
        get => _padBottom <= 0.0001f ? BottomHeight : _padBottom;
        set => _padBottom = value;
    }

    // ========================================================================

    private const float TOLERANCE = 0.1f;

    private static readonly Color _tmpDrawColor = new();

    private float _padBottom = -1;
    private float _padLeft   = -1;
    private float _padRight  = -1;
    private float _padTop    = -1;

    // ========================================================================

    /// <summary>
    /// Create a ninepatch by cutting up the given texture into nine patches.
    /// The subsequent parameters define the 4 lines that will cut the texture
    /// region into 9 pieces.
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="left">Pixels from left edge.</param>
    /// <param name="right">Pixels from right edge.</param>
    /// <param name="top">Pixels from top edge.</param>
    /// <param name="bottom">Pixels from bottom edge.</param>
    public NinePatch( Texture texture, int left, int right, int top, int bottom )
        : this( new TextureRegion( texture ), left, right, top, bottom )
    {
    }

    /// <summary>
    /// Create a ninepatch by cutting up the given texture region into nine patches.
    /// The subsequent parameters define the 4 lines that will cut the texture region
    /// into 9 pieces.
    /// </summary>
    /// <param name="region"></param>
    /// <param name="left"> Pixels from left edge. </param>
    /// <param name="right"> Pixels from right edge. </param>
    /// <param name="top"> Pixels from top edge. </param>
    /// <param name="bottom"> Pixels from bottom edge.  </param>
    public NinePatch( TextureRegion region, int left, int right, int top, int bottom )
    {
        Guard.Against.Null( region );

        int middleWidth  = region.GetRegionWidth() - left - right;
        int middleHeight = region.GetRegionHeight() - top - bottom;
        var patches      = new TextureRegion?[ 9 ];

        if ( top > 0 )
        {
            if ( left > 0 )
            {
                patches[ TOP_LEFT ] = new TextureRegion( region, 0, 0, left, top );
            }

            if ( middleWidth > 0 )
            {
                patches[ TOP_CENTER ] = new TextureRegion( region, left, 0, middleWidth, top );
            }

            if ( right > 0 )
            {
                patches[ TOP_RIGHT ] = new TextureRegion( region, left + middleWidth, 0, right, top );
            }
        }

        if ( middleHeight > 0 )
        {
            if ( left > 0 )
            {
                patches[ MIDDLE_LEFT ] = new TextureRegion( region, 0, top, left, middleHeight );
            }

            if ( middleWidth > 0 )
            {
                patches[ MIDDLE_CENTER ] = new TextureRegion( region, left, top, middleWidth, middleHeight );
            }

            if ( right > 0 )
            {
                patches[ MIDDLE_RIGHT ] = new TextureRegion( region, left + middleWidth, top, right, middleHeight );
            }
        }

        if ( bottom > 0 )
        {
            if ( left > 0 )
            {
                patches[ BOTTOM_LEFT ] = new TextureRegion( region, 0, top + middleHeight, left, bottom );
            }

            if ( middleWidth > 0 )
            {
                patches[ BOTTOM_CENTER ] = new TextureRegion( region, left, top + middleHeight, middleWidth, bottom );
            }

            if ( right > 0 )
            {
                patches[ BOTTOM_RIGHT ] = new TextureRegion( region,
                                                             left + middleWidth,
                                                             top + middleHeight,
                                                             right,
                                                             bottom );
            }
        }

        // If split only vertical, move splits from right to center.
        if ( ( left == 0 ) && ( middleWidth == 0 ) )
        {
            patches[ TOP_CENTER ]    = patches[ TOP_RIGHT ];
            patches[ MIDDLE_CENTER ] = patches[ MIDDLE_RIGHT ];
            patches[ BOTTOM_CENTER ] = patches[ BOTTOM_RIGHT ];
            patches[ TOP_RIGHT ]     = null;
            patches[ MIDDLE_RIGHT ]  = null;
            patches[ BOTTOM_RIGHT ]  = null;
        }

        // If split only horizontal, move splits from bottom to center.
        if ( ( top == 0 ) && ( middleHeight == 0 ) )
        {
            patches[ MIDDLE_LEFT ]   = patches[ BOTTOM_LEFT ];
            patches[ MIDDLE_CENTER ] = patches[ BOTTOM_CENTER ];
            patches[ MIDDLE_RIGHT ]  = patches[ BOTTOM_RIGHT ];
            patches[ BOTTOM_LEFT ]   = null;
            patches[ BOTTOM_CENTER ] = null;
            patches[ BOTTOM_RIGHT ]  = null;
        }

        Load( patches );
    }

    /// <summary>
    /// Construct a degenerate "nine" patch with only a center component.
    /// </summary>
    public NinePatch( Texture texture, Color color )
        : this( texture )
    {
        Color = color;
    }

    /// Construct a degenerate "nine" patch with only a center component.
    public NinePatch( Texture texture )
        : this( new TextureRegion( texture ) )
    {
    }

    /// <summary>
    /// Construct a degenerate "nine" patch with only a center component.
    /// </summary>
    public NinePatch( TextureRegion region, Color color )
        : this( region )
    {
        Color = color;
    }

    /// <summary>
    /// Construct a degenerate "nine" patch with only a center component.
    /// </summary>
    public NinePatch( TextureRegion region )
    {
        Load( [
            null, null, null,
            null, region, null,
            null, null, null
        ] );
    }

    /// <summary>
    /// Construct a nine patch from the given nine texture regions. The provided
    /// patches must be consistently sized (e.g., any left edge textures must have
    /// the same width, etc). Patches may be <tt>null</tt>. Patch indices are
    /// specified via the public members <see cref="TopLeft"/>, <see cref="TopCenter"/>,
    /// etc.
    /// </summary>
    public NinePatch( params TextureRegion?[] patches )
    {
        if ( patches is not { Length: 9 } )
        {
            throw new ArgumentException( "NinePatch needs nine TextureRegions" );
        }

        Load( patches );

        if ( ( ( patches[ TOP_LEFT ] != null )
            && ( Math.Abs( patches[ TOP_LEFT ]!.GetRegionWidth() - LeftWidth ) > TOLERANCE ) )
          || ( ( patches[ MIDDLE_LEFT ] != null )
            && ( Math.Abs( patches[ MIDDLE_LEFT ]!.GetRegionWidth() - LeftWidth ) > TOLERANCE ) )
          || ( ( patches[ BOTTOM_LEFT ] != null )
            && ( Math.Abs( patches[ BOTTOM_LEFT ]!.GetRegionWidth() - LeftWidth ) > TOLERANCE ) ) )
        {
            throw new RuntimeException( "Left side patches must have the same width" );
        }

        if ( ( ( patches[ TOP_RIGHT ] != null )
            && ( Math.Abs( patches[ TOP_RIGHT ]!.GetRegionWidth() - RightWidth ) > TOLERANCE ) )
          || ( ( patches[ MIDDLE_RIGHT ] != null )
            && ( Math.Abs( patches[ MIDDLE_RIGHT ]!.GetRegionWidth() - RightWidth ) > TOLERANCE ) )
          || ( ( patches[ BOTTOM_RIGHT ] != null )
            && ( Math.Abs( patches[ BOTTOM_RIGHT ]!.GetRegionWidth() - RightWidth ) > TOLERANCE ) ) )
        {
            throw new RuntimeException( "Right side patches must have the same width" );
        }

        if ( ( ( patches[ BOTTOM_LEFT ] != null )
            && ( Math.Abs( patches[ BOTTOM_LEFT ]!.GetRegionHeight() - BottomHeight ) > TOLERANCE ) )
          || ( ( patches[ BOTTOM_CENTER ] != null )
            && ( Math.Abs( patches[ BOTTOM_CENTER ]!.GetRegionHeight() - BottomHeight ) > TOLERANCE ) )
          || ( ( patches[ BOTTOM_RIGHT ] != null )
            && ( Math.Abs( patches[ BOTTOM_RIGHT ]!.GetRegionHeight() - BottomHeight ) > TOLERANCE ) ) )
        {
            throw new RuntimeException( "Bottom side patches must have the same height" );
        }

        if ( ( ( patches[ TOP_LEFT ] != null )
            && ( Math.Abs( patches[ TOP_LEFT ]!.GetRegionHeight() - TopHeight ) > TOLERANCE ) )
          || ( ( patches[ TOP_CENTER ] != null )
            && ( Math.Abs( patches[ TOP_CENTER ]!.GetRegionHeight() - TopHeight ) > TOLERANCE ) )
          || ( ( patches[ TOP_RIGHT ] != null )
            && ( Math.Abs( patches[ TOP_RIGHT ]!.GetRegionHeight() - TopHeight ) > TOLERANCE ) ) )
        {
            throw new RuntimeException( "Top side patches must have the same height" );
        }
    }

    /// <summary>
    /// Creates a ninepatch from the supplied ninepatch.
    /// </summary>
    /// <param name="ninePatch"></param>
    public NinePatch( NinePatch ninePatch )
        : this( ninePatch, ninePatch.Color )
    {
    }

    /// <summary>
    /// Creates a ninepatch from the supplied ninepatch and sets
    /// it to the supplied Color.
    /// </summary>
    /// <param name="ninePatch"></param>
    /// <param name="color"></param>
    public NinePatch( NinePatch ninePatch, Color color )
    {
        Texture = ninePatch.Texture;

        BottomLeft   = ninePatch.BottomLeft;
        BottomCenter = ninePatch.BottomCenter;
        BottomRight  = ninePatch.BottomRight;
        MiddleLeft   = ninePatch.MiddleLeft;
        MiddleCenter = ninePatch.MiddleCenter;
        MiddleRight  = ninePatch.MiddleRight;
        TopLeft      = ninePatch.TopLeft;
        TopCenter    = ninePatch.TopCenter;
        TopRight     = ninePatch.TopRight;

        LeftWidth    = ninePatch.LeftWidth;
        RightWidth   = ninePatch.RightWidth;
        MiddleWidth  = ninePatch.MiddleWidth;
        MiddleHeight = ninePatch.MiddleHeight;
        TopHeight    = ninePatch.TopHeight;
        BottomHeight = ninePatch.BottomHeight;

        _padLeft   = ninePatch._padLeft;
        _padTop    = ninePatch._padTop;
        _padBottom = ninePatch._padBottom;
        _padRight  = ninePatch._padRight;

        Vertices = new float[ ninePatch.Vertices.Length ];

        Array.Copy( ninePatch.Vertices,
                    0,
                    Vertices,
                    0,
                    ninePatch.Vertices.Length );

        Idx = ninePatch.Idx;
        Color.Set( color );
    }

    /// <summary>
    /// </summary>
    /// <param name="patches"></param>
    private void Load( IReadOnlyList< TextureRegion? > patches )
    {
        if ( patches[ BOTTOM_LEFT ] != null )
        {
            BottomLeft   = Add( patches[ BOTTOM_LEFT ], false, false );
            LeftWidth    = patches[ BOTTOM_LEFT ]!.GetRegionWidth();
            BottomHeight = patches[ BOTTOM_LEFT ]!.GetRegionHeight();
        }
        else
        {
            BottomLeft = -1;
        }

        if ( patches[ BOTTOM_CENTER ] != null )
        {
            BottomCenter = Add( patches[ BOTTOM_CENTER ],
                                ( patches[ BOTTOM_LEFT ] != null ) || ( patches[ BOTTOM_RIGHT ] != null ),
                                false );

            MiddleWidth  = Math.Max( MiddleWidth, patches[ BOTTOM_CENTER ]!.GetRegionWidth() );
            BottomHeight = Math.Max( BottomHeight, patches[ BOTTOM_CENTER ]!.GetRegionHeight() );
        }
        else
        {
            BottomCenter = -1;
        }

        if ( patches[ BOTTOM_RIGHT ] != null )
        {
            BottomRight  = Add( patches[ BOTTOM_RIGHT ], false, false );
            RightWidth   = Math.Max( RightWidth, patches[ BOTTOM_RIGHT ]!.GetRegionWidth() );
            BottomHeight = Math.Max( BottomHeight, patches[ BOTTOM_RIGHT ]!.GetRegionHeight() );
        }
        else
        {
            BottomRight = -1;
        }

        if ( patches[ MIDDLE_LEFT ] != null )
        {
            MiddleLeft = Add( patches[ MIDDLE_LEFT ],
                              false,
                              ( patches[ TOP_LEFT ] != null ) || ( patches[ BOTTOM_LEFT ] != null ) );

            LeftWidth    = Math.Max( LeftWidth, patches[ MIDDLE_LEFT ]!.GetRegionWidth() );
            MiddleHeight = Math.Max( MiddleHeight, patches[ MIDDLE_LEFT ]!.GetRegionHeight() );
        }
        else
        {
            MiddleLeft = -1;
        }

        if ( patches[ MIDDLE_CENTER ] != null )
        {
            MiddleCenter = Add( patches[ MIDDLE_CENTER ],
                                ( patches[ MIDDLE_LEFT ] != null ) || ( patches[ MIDDLE_RIGHT ] != null ),
                                ( patches[ TOP_CENTER ] != null ) || ( patches[ BOTTOM_CENTER ] != null ) );

            MiddleWidth  = Math.Max( MiddleWidth, patches[ MIDDLE_CENTER ]!.GetRegionWidth() );
            MiddleHeight = Math.Max( MiddleHeight, patches[ MIDDLE_CENTER ]!.GetRegionHeight() );
        }
        else
        {
            MiddleCenter = -1;
        }

        if ( patches[ MIDDLE_RIGHT ] != null )
        {
            MiddleRight = Add( patches[ MIDDLE_RIGHT ],
                               false,
                               ( patches[ TOP_RIGHT ] != null ) || ( patches[ BOTTOM_RIGHT ] != null ) );

            RightWidth   = Math.Max( RightWidth, patches[ MIDDLE_RIGHT ]!.GetRegionWidth() );
            MiddleHeight = Math.Max( MiddleHeight, patches[ MIDDLE_RIGHT ]!.GetRegionHeight() );
        }
        else
        {
            MiddleRight = -1;
        }

        if ( patches[ TOP_LEFT ] != null )
        {
            TopLeft   = Add( patches[ TOP_LEFT ], false, false );
            LeftWidth = Math.Max( LeftWidth, patches[ TOP_LEFT ]!.GetRegionWidth() );
            TopHeight = Math.Max( TopHeight, patches[ TOP_LEFT ]!.GetRegionHeight() );
        }
        else
        {
            TopLeft = -1;
        }

        if ( patches[ TOP_CENTER ] != null )
        {
            TopCenter = Add( patches[ TOP_CENTER ],
                             ( patches[ TOP_LEFT ] != null )
                          || ( patches[ TOP_RIGHT ] != null ),
                             false );

            MiddleWidth = Math.Max( MiddleWidth, patches[ TOP_CENTER ]!.GetRegionWidth() );
            TopHeight   = Math.Max( TopHeight, patches[ TOP_CENTER ]!.GetRegionHeight() );
        }
        else
        {
            TopCenter = -1;
        }

        if ( patches[ TOP_RIGHT ] != null )
        {
            TopRight   = Add( patches[ TOP_RIGHT ], false, false );
            RightWidth = Math.Max( RightWidth, patches[ TOP_RIGHT ]!.GetRegionWidth() );
            TopHeight  = Math.Max( TopHeight, patches[ TOP_RIGHT ]!.GetRegionHeight() );
        }
        else
        {
            TopRight = -1;
        }

        if ( Idx < Vertices.Length )
        {
            var newVertices = new float[ Idx ];

            Array.Copy( Vertices,
                        0,
                        newVertices,
                        0,
                        Idx );

            Vertices = newVertices;
        }
    }

    private int Add( TextureRegion? region, bool isStretchW, bool isStretchH )
    {
        Guard.Against.Null( region );

        if ( Texture == null )
        {
            Texture = region.Texture;
        }
        else if ( Texture != region.Texture )
        {
            throw new ArgumentException( "All regions must be from the same texture." );
        }

        // Add half pixel offsets on stretchable dimensions to avoid color bleeding when GL_LINEAR
        // filtering is used for the texture. This nudges the texture coordinate to the center
        // of the texel where the neighboring pixel has 0% contribution in linear blending mode.
        float u  = region.U;
        float v  = region.V2;
        float u2 = region.U2;
        float v2 = region.V;

        if ( ( Texture?.MagFilter == TextureFilterMode.Linear )
          || ( Texture?.MinFilter == TextureFilterMode.Linear ) )
        {
            if ( isStretchW )
            {
                float halfTexelWidth = 0.5f * 1f / Texture.Width;
                u  += halfTexelWidth;
                u2 -= halfTexelWidth;
            }

            if ( isStretchH )
            {
                float halfTexelHeight = 0.5f * 1f / Texture.Height;
                v  -= halfTexelHeight;
                v2 += halfTexelHeight;
            }
        }

        int i = Idx;

        Vertices[ i + 3 ] = u;
        Vertices[ i + 4 ] = v;

        Vertices[ i + 8 ] = u;
        Vertices[ i + 9 ] = v2;

        Vertices[ i + 13 ] = u2;
        Vertices[ i + 14 ] = v2;

        Vertices[ i + 18 ] = u2;
        Vertices[ i + 19 ] = v;

        Idx += 20;

        return i;
    }

    /// <summary>
    /// Set the coordinates and color of a ninth of the patch.
    /// </summary>
    private void Set( int idx, float x, float y, float width, float height, float color )
    {
        float fx2 = x + width;
        float fy2 = y + height;

        Vertices[ idx ]     = x;
        Vertices[ idx + 1 ] = y;
        Vertices[ idx + 2 ] = color;

        Vertices[ idx + 5 ] = x;
        Vertices[ idx + 6 ] = fy2;
        Vertices[ idx + 7 ] = color;

        Vertices[ idx + 10 ] = fx2;
        Vertices[ idx + 11 ] = fy2;
        Vertices[ idx + 12 ] = color;

        Vertices[ idx + 15 ] = fx2;
        Vertices[ idx + 16 ] = y;
        Vertices[ idx + 17 ] = color;
    }

    private void PrepareVertices( IBatch batch, float x, float y, float width, float height )
    {
        float centerX      = x + LeftWidth;
        float centerY      = y + BottomHeight;
        float centerWidth  = width - RightWidth - LeftWidth;
        float centerHeight = height - TopHeight - BottomHeight;
        float rightX       = x + width - RightWidth;
        float topY         = y + height - TopHeight;
        float c            = _tmpDrawColor.Set( Color ).Mul( batch.Color ).ToFloatBitsAbgr();

        if ( BottomLeft != -1 )
        {
            Set( BottomLeft, x, y, LeftWidth, BottomHeight, c );
        }

        if ( BottomCenter != -1 )
        {
            Set( BottomCenter, centerX, y, centerWidth, BottomHeight, c );
        }

        if ( BottomRight != -1 )
        {
            Set( BottomRight, rightX, y, RightWidth, BottomHeight, c );
        }

        if ( MiddleLeft != -1 )
        {
            Set( MiddleLeft, x, centerY, LeftWidth, centerHeight, c );
        }

        if ( MiddleCenter != -1 )
        {
            Set( MiddleCenter, centerX, centerY, centerWidth, centerHeight, c );
        }

        if ( MiddleRight != -1 )
        {
            Set( MiddleRight, rightX, centerY, RightWidth, centerHeight, c );
        }

        if ( TopLeft != -1 )
        {
            Set( TopLeft, x, topY, LeftWidth, TopHeight, c );
        }

        if ( TopCenter != -1 )
        {
            Set( TopCenter, centerX, topY, centerWidth, TopHeight, c );
        }

        if ( TopRight != -1 )
        {
            Set( TopRight, rightX, topY, RightWidth, TopHeight, c );
        }
    }

    public void Draw( IBatch batch, float x, float y, float width, float height )
    {
        if ( Texture != null )
        {
            PrepareVertices( batch, x, y, width, height );
            batch.Draw( Texture, Vertices, 0, Idx );
        }
    }

    public void Draw( IBatch batch,
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
        PrepareVertices( batch, x, y, width, height );

        float worldOriginX = x + originX, worldOriginY = y + originY;

        int     n        = Idx;
        float[] vertices = Vertices;

        if ( rotation != 0 )
        {
            for ( var i = 0; i < n; i += 5 )
            {
                float vx = ( vertices[ i ] - worldOriginX ) * scaleX;
                float vy = ( vertices[ i + 1 ] - worldOriginY ) * scaleY;

                float cos = MathUtils.CosDeg( rotation ), sin = MathUtils.SinDeg( rotation );

                vertices[ i ]     = ( cos * vx ) - ( sin * vy ) + worldOriginX;
                vertices[ i + 1 ] = ( sin * vx ) + ( cos * vy ) + worldOriginY;
            }
        }
        else if ( ( Math.Abs( scaleX - 1 ) > TOLERANCE ) || ( Math.Abs( scaleY - 1 ) > TOLERANCE ) )
        {
            for ( var i = 0; i < n; i += 5 )
            {
                vertices[ i ]     = ( ( vertices[ i ] - worldOriginX ) * scaleX ) + worldOriginX;
                vertices[ i + 1 ] = ( ( vertices[ i + 1 ] - worldOriginY ) * scaleY ) + worldOriginY;
            }
        }

        if ( Texture != null )
        {
            batch.Draw( Texture, vertices, 0, n );
        }
    }

    /// <summary>
    /// Set the padding for content inside this ninepatch. By default the padding
    /// is set to match the exterior of the ninepatch, so the content should fit
    /// exactly within the middle patch.
    /// </summary>
    public void SetPadding( float left, float right, float top, float bottom )
    {
        PadLeft   = left;
        PadRight  = right;
        PadTop    = top;
        PadBottom = bottom;
    }

    /// <summary>
    /// Multiplies the top/left/bottom/right sizes and padding by the specified amount.
    /// </summary>
    public void Scale( float scaleX, float scaleY )
    {
        LeftWidth    *= scaleX;
        RightWidth   *= scaleX;
        TopHeight    *= scaleY;
        BottomHeight *= scaleY;
        MiddleWidth  *= scaleX;
        MiddleHeight *= scaleY;

        if ( PadLeft is not -1f )
        {
            PadLeft *= scaleX;
        }

        if ( PadRight is not -1 )
        {
            PadRight *= scaleX;
        }

        if ( PadTop is not -1 )
        {
            PadTop *= scaleY;
        }

        if ( PadBottom is not -1 )
        {
            PadBottom *= scaleY;
        }
    }

    // ========================================================================
}