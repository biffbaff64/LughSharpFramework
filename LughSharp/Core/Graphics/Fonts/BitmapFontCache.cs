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

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Images;
using LughSharp.Core.Maths;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Pooling;

namespace LughSharp.Core.Graphics.Fonts;

/// <summary>
/// Caches glyph geometry for a BitmapFont, providing a fast way to render
/// static text. This saves needing to compute the glyph geometry each frame.
/// </summary>
[PublicAPI]
public class BitmapFontCache
{
    /// <summary>
    /// Returns the x position of the cached string, relative to the
    /// position when the string was cached.
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Returns the y position of the cached string, relative to the
    /// position when the string was cached.
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// Specifies whether to use integer positions or not.
    /// Default is to use them so filtering doesn't kick in as badly.
    /// </summary>
    public bool UseIntegerPositions { get; set; }

    /// <summary>
    /// Represents the collection of <see cref="GlyphLayout"/> instances cached by the font.
    /// This property holds all the individual text layouts associated with the font cache,
    /// allowing text to be efficiently rendered or manipulated.
    /// </summary>
    public List< GlyphLayout > Layouts { get; set; } = [ ];

    /// <summary>
    /// 
    /// </summary>
    public BitmapFont Font { get; }

    public Color Color { get; set; } = new( 1f, 1f, 1f, 1f );

    // ========================================================================

    private const int   InitialGlyphCapacity = 100;
    private const int   VerticesPerGlyph     = 20;
    private const int   FloatsPerVertex      = 5;
    private const int   ColorOffset          = 2;
    private const int   AlphaBitShift        = 24;
    private const float AlphaScale           = 254f;

    private readonly FlushablePool< GlyphLayout > _pooledLayouts = new()
    {
        NewObjectFactory = () => new GlyphLayout()
    };

    private Color _tempColor = new( 1f, 1f, 1f, 1f );
    private uint  _currentTint;
    private int   _glyphCount;

    /// <summary>
    /// Number of vertex data entries per page.
    /// </summary>
    private int[] _idx;

    /// <summary>
    /// For each page, an array with a value for each glyph from that page, where
    /// the value is the index of the character in the full text being cached.
    /// </summary>
    private List< int >[] _pageGlyphIndices;

    /// <summary>
    /// Vertex data per page.
    /// </summary>
    private float[][] _pageVertices;

    /// <summary>
    /// Used internally to ensure a correct capacity for multi-page font vertex data.
    /// </summary>
    private int[] _tempGlyphCount;

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="font"></param>
    public BitmapFontCache( BitmapFont font ) : this( font, font.GetUseIntegerPositions() )
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="font"></param>
    /// <param name="integer">
    /// If true, rendering positions will be at integer values to avoid filtering artifacts.
    /// </param>
    /// <exception cref="ArgumentException"></exception>
    public BitmapFontCache( BitmapFont font, bool integer = true )
    {
        Font                = font;
        UseIntegerPositions = integer;

        int pageCount = font.GetRegions().Count;

        if ( pageCount == 0 )
        {
            throw new ArgumentException( "The specified font must contain at least one texture page." );
        }

        _pageVertices = new float[ pageCount ][];
        _idx          = new int[ pageCount ];

        for ( var i = 0; i < _pageVertices.Length; i++ )
        {
            _pageVertices[ i ] = new float[ VerticesPerGlyph * InitialGlyphCapacity ];
        }

        // Contains the indices of the glyph in the cache as they are added.
        _pageGlyphIndices = new List< int >[ pageCount ];

        for ( int i = 0, n = _pageGlyphIndices.Length; i < n; i++ )
        {
            _pageGlyphIndices[ i ] = new List< int >();
        }

        _tempGlyphCount = new int[ pageCount ];
    }

    /// <summary>
    /// Sets the position of the text, relative to the position when
    /// the cached text was created.
    /// </summary>
    /// <param name="x"> The x coordinate </param>
    /// <param name="y"> The y coordinate </param>
    public void SetPosition( float x, float y )
    {
        Translate( x - X, y - Y );
    }

    /// <summary>
    /// Sets the position of the text, relative to its current position.
    /// </summary>
    /// <param name="xAmount"> The amount in x to move the text </param>
    /// <param name="yAmount"> The amount in y to move the text </param>
    public void Translate( float xAmount, float yAmount )
    {
        if ( ( xAmount == 0 ) && ( yAmount == 0 ) )
        {
            return;
        }

        if ( UseIntegerPositions )
        {
            xAmount = ( float )Math.Round( xAmount );
            yAmount = ( float )Math.Round( yAmount );
        }

        X += xAmount;
        Y += yAmount;

        for ( int i = 0, n = _pageVertices.Length; i < n; i++ )
        {
            for ( int ii = 0, nn = _idx[ i ]; ii < nn; ii += FloatsPerVertex )
            {
                _pageVertices[ i ][ ii ]     += xAmount;
                _pageVertices[ i ][ ii + 1 ] += yAmount;
            }
        }
    }

    /// <summary>
    /// Tints all text currently in the cache. Does not affect subsequently added text
    /// only text that is currently present..
    /// </summary>
    public void Tint( Color tint )
    {
        uint newTint = tint.ABGRPackedColor;

        if ( _currentTint == newTint )
        {
            return;
        }

        _currentTint = newTint;

        Array.Fill( _tempGlyphCount, 0 );

        for ( int i = 0, n = Layouts.Count; i < n; i++ )
        {
            GlyphLayout layout = Layouts[ i ];

            var   colorsIndex         = 0;
            var   nextColorGlyphIndex = 0;
            var   glyphIndex          = 0;
            float lastColorFloatBits  = 0;

            for ( int ii = 0, nn = layout.Runs.Count; ii < nn; ii++ )
            {
                GlyphLayout.GlyphRun run    = layout.Runs[ ii ];
                List< Glyph >        glyphs = run.Glyphs;

                for ( int iii = 0, nnn = run.Glyphs.Count; iii < nnn; iii++ )
                {
                    if ( glyphIndex++ == nextColorGlyphIndex )
                    {
                        // Convert RGBA8888 to Color, multiply by tint, convert to ABGR float
                        int layoutColorInt = layout.Colors[ colorsIndex ].Color;
                        Color.Rgba8888ToColor( ref _tempColor, ( uint )layoutColorInt );

                        _tempColor.Mul( tint );

                        lastColorFloatBits = _tempColor.ToFloatBitsAbgr();
                        nextColorGlyphIndex = ++colorsIndex < layout.Colors.Count
                            ? layout.Colors[ colorsIndex ].GlyphIndex
                            : -1;
                    }

                    int page   = glyphs[ iii ].Page;
                    int offset = ( _tempGlyphCount[ page ] * VerticesPerGlyph ) + ColorOffset;

                    _tempGlyphCount[ page ]++;

                    _pageVertices[ page ][ offset ]                           = lastColorFloatBits;
                    _pageVertices[ page ][ offset + FloatsPerVertex ]         = lastColorFloatBits;
                    _pageVertices[ page ][ offset + ( FloatsPerVertex * 2 ) ] = lastColorFloatBits;
                    _pageVertices[ page ][ offset + ( FloatsPerVertex * 3 ) ] = lastColorFloatBits;
                }
            }
        }
    }

    /// <summary>
    /// Sets the alpha component of all text currently in the cache.
    /// Does not affect subsequently added text.
    /// </summary>
    public void SetAlphas( float alpha )
    {
        uint  alphaBits = ( uint )( AlphaScale * alpha ) << AlphaBitShift;
        float prev      = 0;
        float newColor  = 0;

        for ( int j = 0, length = _pageVertices.Length; j < length; j++ )
        {
            for ( int i = ColorOffset, n = _idx[ j ]; i < n; i += FloatsPerVertex )
            {
                float c = _pageVertices[ j ][ i ];

                if ( Math.Abs( c - prev ) < NumberUtils.FloatTolerance
                  && Math.Abs( i - 2f ) > NumberUtils.FloatTolerance )
                {
                    _pageVertices[ j ][ i ] = newColor;
                }
                else
                {
                    prev = c;

                    // It's ok to use FloatToIntColor here because we're not
                    // interested in the alpha component, as that will be 
                    // overwritten by the alphaBits.
                    var rgba = ( uint )NumberUtils.FloatToIntColor( c );

                    rgba     = ( rgba & 0x00FFFFFF ) | alphaBits;
                    newColor = NumberUtils.UIntToFloatColor( rgba );

                    _pageVertices[ j ][ i ] = newColor;
                }
            }
        }
    }

    // ========================================================================

    /// <summary>
    /// Sets the color of all text currently in the cache. Does not affect
    /// subsequently added text, only text already present.
    /// </summary>
    /// <param name="r"> Red component. </param>
    /// <param name="g"> Green component. </param>
    /// <param name="b"> Blue component. </param>
    /// <param name="a"> Alpha component. </param>
    public void SetColors( float r, float g, float b, float a )
    {
        uint intBits = ( ( uint )( 255 * a ) << 24 )
                     | ( ( uint )( 255 * b ) << 16 )
                     | ( ( uint )( 255 * g ) << 8 )
                     | ( uint )( 255 * r );

        SetColors( NumberUtils.UIntToFloatColor( intBits ) );
    }

    /// <summary>
    /// Sets the color of all text currently in the cache.
    /// Does not affect subsequently added text, only text already present.
    /// </summary>
    public void SetColors( float color )
    {
        for ( int j = 0, length = _pageVertices.Length; j < length; j++ )
        {
            for ( int i = ColorOffset, n = _idx[ j ]; i < n; i += FloatsPerVertex )
            {
                _pageVertices[ j ][ i ] = color;
            }
        }
    }

    /// <summary>
    /// Sets the color of the specified characters. This may only be called
    /// after calling any of the <c>SetText()</c> methods and is reset every
    /// time SetText is called.
    /// </summary>
    public void SetColors( Color color, int start, int end )
    {
        SetColors( color.ToFloatBitsAbgr(), start, end );
    }

    /// <summary>
    /// Sets the color of the specified characters. This may only be called
    /// after calling any of the <c>SetText()</c> methods and is reset every
    /// time SetText is called.
    /// </summary>
    public void SetColors( float color, int start, int end )
    {
        if ( _pageVertices.Length == 1 )
        {
            // One page.
            for ( int i = ( start * VerticesPerGlyph ) + ColorOffset,
                      n = Math.Min( end * VerticesPerGlyph, _idx[ 0 ] );
                 i < n;
                 i += FloatsPerVertex )
            {
                _pageVertices[ 0 ][ i ] = color;
            }

            return;
        }

        int pageCount = _pageVertices.Length;

        for ( var i = 0; i < pageCount; i++ )
        {
            List< int > glyphIndices = _pageGlyphIndices[ i ];

            // Loop through the indices and determine whether the glyph is inside begin/end.
            for ( int j = 0, n = glyphIndices.Count; j < n; j++ )
            {
                int glyphIndex = glyphIndices[ j ];

                // Break early if the glyph is out of bounds.
                if ( glyphIndex >= end )
                {
                    break;
                }

                // If inside start and end, change its colour.
                if ( glyphIndex >= start )
                {
                    // && glyphIndex < end
                    for ( var off = 0; off < VerticesPerGlyph; off += FloatsPerVertex )
                    {
                        _pageVertices[ i ][ off + ( j * VerticesPerGlyph ) + ColorOffset ] = color;
                    }
                }
            }
        }
    }

    // ========================================================================

    /// <summary>
    /// Renders the cached text using the provided sprite batch instance.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used to draw the cached text.</param>
    public virtual void Draw( IBatch spriteBatch )
    {
        List< TextureRegion > regions = Font.GetRegions();

        for ( int j = 0, n = _pageVertices.Length; j < n; j++ )
        {
            if ( _idx[ j ] > 0 )
            {
                // ignore if this texture has no glyphs
                if ( regions[ j ].Texture != null )
                {
                    spriteBatch.Draw( regions[ j ].Texture!, _pageVertices[ j ], 0, _idx[ j ] );
                }
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    protected virtual void Draw( IBatch spriteBatch, int start, int end )
    {
        if ( Font.GetRegion().Texture == null )
        {
            return;
        }

        if ( _pageVertices.Length == 1 )
        {
            // 1 page.
            spriteBatch.Draw( Font.GetRegion().Texture!,
                              _pageVertices[ 0 ],
                              start * VerticesPerGlyph,
                              ( end - start ) * VerticesPerGlyph );

            return;
        }

        // Determine vertex offset and count to render for each page.
        // Some pages might not need to be rendered at all.
        List< TextureRegion > regions = Font.GetRegions();

        for ( int i = 0, pageCount = _pageVertices.Length; i < pageCount; i++ )
        {
            int offset = -1;
            var count  = 0;

            // For each set of glyph indices, determine where to begin within the start/end bounds.
            List< int > glyphIndices = _pageGlyphIndices[ i ];

            for ( int ii = 0, n = glyphIndices.Count; ii < n; ii++ )
            {
                int glyphIndex = glyphIndices[ ii ];

                // Break early if the glyph is out of bounds.
                if ( glyphIndex >= end )
                {
                    break;
                }

                // Determine if this glyph is within bounds. Use the first match of that for the offset.
                if ( ( offset == -1 ) && ( glyphIndex >= start ) )
                {
                    offset = ii;
                }

                // Determine the vertex count by counting glyphs within bounds.
                if ( glyphIndex >= start )
                {
                    count++;
                }
            }

            // Page doesn't need to be rendered.
            if ( ( offset == -1 ) || ( count == 0 ) )
            {
                continue;
            }

            // Render the page vertex data with the offset and count.
            spriteBatch.Draw( regions[ i ].Texture!,
                              _pageVertices[ i ],
                              offset * VerticesPerGlyph,
                              count * VerticesPerGlyph );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="spriteBatch"></param>
    /// <param name="alphaModulation"></param>
    public void Draw( IBatch spriteBatch, float alphaModulation )
    {
        if ( alphaModulation.Equals( 1 ) )
        {
            Draw( spriteBatch );

            return;
        }

        Color color    = Color;
        float oldAlpha = color.A;

        color.A *= alphaModulation;
        SetColors( color.ToFloatBitsAbgr() );
        Draw( spriteBatch );
        color.A = oldAlpha;
        SetColors( color.ToFloatBitsAbgr() );
    }

    // ========================================================================

    /// <summary>
    /// Removes all glyphs in the cache.
    /// </summary>
    public void Clear()
    {
        X = 0;
        Y = 0;

        _pooledLayouts.Flush();
        Layouts.Clear();

        for ( int i = 0, n = _idx.Length; i < n; i++ )
        {
            _pageGlyphIndices[ i ].Clear();

            _idx[ i ] = 0;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="layout"></param>
    private void RequireGlyphs( GlyphLayout layout )
    {
        if ( _pageVertices.Length == 1 )
        {
            // Simpler counting if we just have one page.
            var newGlyphCount = 0;

            for ( int i = 0, n = layout.Runs.Count; i < n; i++ )
            {
                newGlyphCount += layout.Runs[ i ].Glyphs.Count;
            }

            RequirePageGlyphs( 0, newGlyphCount );
        }
        else
        {
            int[] tempGlyphCount = _tempGlyphCount;

            for ( int i = 0, n = tempGlyphCount.Length; i < n; i++ )
            {
                tempGlyphCount[ i ] = 0;
            }

            // Determine # of glyphs in each page.
            for ( int i = 0, n = layout.Runs.Count; i < n; i++ )
            {
                List< Glyph > glyphs = layout.Runs[ i ].Glyphs;

                for ( int ii = 0, nn = glyphs.Count; ii < nn; ii++ )
                {
                    tempGlyphCount[ glyphs[ ii ].Page ]++;
                }
            }

            // Require that many for each page.
            for ( int i = 0, n = tempGlyphCount.Length; i < n; i++ )
            {
                RequirePageGlyphs( i, tempGlyphCount[ i ] );
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="page"></param>
    /// <param name="glyphCount"></param>
    private void RequirePageGlyphs( int page, int glyphCount )
    {
        Guard.Against.Null( _pageVertices );
        Guard.Against.Null( _pageVertices[ page ] );
        Guard.Against.Null( _pageGlyphIndices );

        if ( glyphCount > _pageGlyphIndices[ page ].Count )
        {
            _pageGlyphIndices[ page ].EnsureCapacity( glyphCount - _pageGlyphIndices[ page ].Count );
        }

        int vertexCount = _idx[ page ] + ( glyphCount * VerticesPerGlyph );

        if ( _pageVertices[ page ].Length < vertexCount )
        {
            var newVertices = new float[ vertexCount ];

            Array.Copy( _pageVertices[ page ], 0, newVertices, 0, _idx[ page ] );

            _pageVertices[ page ] = newVertices;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pageCount"></param>
    private void SetPageCount( int pageCount )
    {
        var newPageVertices = new float[ pageCount ][];

        Array.Copy( _pageVertices, 0, newPageVertices, 0, _pageVertices.Length );

        _pageVertices = newPageVertices;

        var newIdx = new int[ pageCount ];

        Array.Copy( _idx, 0, newIdx, 0, _idx.Length );

        _idx = newIdx;

        var newPageGlyphIndices    = new List< int >[ pageCount ];
        int pageGlyphIndicesLength = _pageGlyphIndices.Length;

        Array.Copy( _pageGlyphIndices, 0, newPageGlyphIndices, 0, _pageGlyphIndices.Length );

        // Initialize only the NEW slots in the array
        for ( int i = pageGlyphIndicesLength; i < pageCount; i++ )
        {
            newPageGlyphIndices[ i ] = new List< int >();
        }

        _pageGlyphIndices = newPageGlyphIndices;
        _tempGlyphCount   = new int[ pageCount ];
    }

    /// <summary>
    /// Adds the specified <see cref="GlyphLayout"/> to the font cache at the specified position.
    /// </summary>
    /// <param name="layout">The <see cref="GlyphLayout"/> object representing the glyphs to be cached.</param>
    /// <param name="x">The x coordinate where the glyphs should be positioned.</param>
    /// <param name="y">The y coordinate where the glyphs should be positioned.</param>
    private void AddToCache( GlyphLayout layout, float x, float y )
    {
        int runCount = layout.Runs.Count;

        if ( runCount == 0 )
        {
            return;
        }

        // Check if the number of font pages has changed.
        if ( _pageVertices.Length < Font.GetRegions().Count )
        {
            SetPageCount( Font.GetRegions().Count );
        }

        Layouts.Add( layout );
        RequireGlyphs( layout );

        List< GlyphLayout.GlyphColor > colors = layout.Colors;

        var colorsIndex         = 0;
        var nextColorGlyphIndex = 0;
        var glyphIndex          = 0;
        var lastColorFloatBits  = 0f;

        for ( var i = 0; i < runCount; i++ )
        {
            GlyphLayout.GlyphRun run       = layout.Runs[ i ];
            List< Glyph >        glyphs    = run.Glyphs;
            List< float >        xAdvances = run.XAdvances;
            float                gx        = x + run.X;
            float                gy        = y + run.Y;

            for ( int ii = 0, nn = run.Glyphs.Count; ii < nn; ii++ )
            {
                if ( glyphIndex++ == nextColorGlyphIndex )
                {
                    Color.Rgba8888ToColor( ref _tempColor, ( uint )colors[ colorsIndex ].Color );
                    lastColorFloatBits = _tempColor.ToFloatBitsAbgr();

                    nextColorGlyphIndex = ++colorsIndex < colors.Count
                        ? colors[ colorsIndex ].GlyphIndex
                        : -1;
                }

                gx += xAdvances[ ii ];

                AddGlyph( glyphs[ ii ], gx, gy, lastColorFloatBits );
            }
        }

        // Cached glyphs have changed, reset the current tint.
        _currentTint = Color.White.ABGRPackedColor;
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="glyph"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="color"></param>
    private void AddGlyph( Glyph glyph, float x, float y, float color )
    {
        float scaleX = Font.FontData.ScaleX;
        float scaleY = Font.FontData.ScaleY;

//        x += glyph.Xoffset * scaleX;
//        y += glyph.Yoffset * scaleY;

        float width  = glyph.Width * scaleX;
        float height = glyph.Height * scaleY;
        float u      = glyph.U;
        float u2     = glyph.U2;
        float v      = glyph.V;
        float v2     = glyph.V2;

        if ( UseIntegerPositions )
        {
            x      = ( float )Math.Round( x );
            y      = ( float )Math.Round( y );
            width  = ( float )Math.Round( width );
            height = ( float )Math.Round( height );
        }

        float x2   = x + width;
        float y2   = y + height;
        int   page = glyph.Page;
        int   idx  = _idx[ page ];

        this._idx[ page ] += VerticesPerGlyph;

        _pageGlyphIndices[ page ].Add( _glyphCount++ );

        _pageVertices[ page ][ idx++ ] = x;
        _pageVertices[ page ][ idx++ ] = y;
        _pageVertices[ page ][ idx++ ] = color;
        _pageVertices[ page ][ idx++ ] = u;
        _pageVertices[ page ][ idx++ ] = v;

        _pageVertices[ page ][ idx++ ] = x;
        _pageVertices[ page ][ idx++ ] = y2;
        _pageVertices[ page ][ idx++ ] = color;
        _pageVertices[ page ][ idx++ ] = u;
        _pageVertices[ page ][ idx++ ] = v2;

        _pageVertices[ page ][ idx++ ] = x2;
        _pageVertices[ page ][ idx++ ] = y2;
        _pageVertices[ page ][ idx++ ] = color;
        _pageVertices[ page ][ idx++ ] = u2;
        _pageVertices[ page ][ idx++ ] = v2;

        _pageVertices[ page ][ idx++ ] = x2;
        _pageVertices[ page ][ idx++ ] = y;
        _pageVertices[ page ][ idx++ ] = color;
        _pageVertices[ page ][ idx++ ] = u2;
        _pageVertices[ page ][ idx ]   = v;
    }

    // ========================================================================

    /// <summary>
    /// Clears any cached glyphs and adds glyphs for the specified text.
    /// <see cref="AddText(string, float, float, int, int, float, Align, bool, string)"/>
    /// </summary>
    public GlyphLayout SetText( string str,
                                float x,
                                float y,
                                float targetWidth = 0,
                                Align halign = Align.Left,
                                bool wrap = false )
    {
        Clear();

        return AddText( str, x, y, 0, str.Length, targetWidth, halign, wrap );
    }

    /// <summary>
    /// Clears any cached glyphs and adds glyphs for the specified text.
    /// <see cref="AddText(string, float, float, int, int, float, Align, bool, string)"/>
    /// </summary>
    public GlyphLayout SetText( string str,
                                float x,
                                float y,
                                int start,
                                int end,
                                float targetWidth = 0,
                                Align halign = Align.Left,
                                bool wrap = false,
                                string? truncate = null )
    {
        Clear();

        return AddText( str, x, y, start, end, targetWidth, halign, wrap, truncate );
    }

    /// <summary>
    /// Clears any cached glyphs and adds glyphs for the specified text.
    /// <see cref="AddText(string, float, float, int, int, float, Align, bool, string)"/>
    /// </summary>
    public void SetText( GlyphLayout layout, float x, float y )
    {
        Clear();
        AddText( layout, x, y );
    }

    // ========================================================================

    /// <summary>
    /// Adds glyphs for the specified text.
    /// </summary>
    public GlyphLayout AddText( string str,
                                float x,
                                float y,
                                float targetWidth = 0,
                                Align halign = Align.Left,
                                bool wrap = false )
    {
        return AddText( str, x, y, 0, str.Length, targetWidth, halign, wrap );
    }

    /// <summary>
    /// Adds glyphs for the the specified text.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="x"> The x position for the left most character. </param>
    /// <param name="y">
    /// The y position for the top of most capital letters in the font
    /// (the <see cref="BitmapFontData.CapHeight"/>).
    /// </param>
    /// <param name="start"> The first character of the string to draw. </param>
    /// <param name="end"> The last character of the string to draw (exclusive). </param>
    /// <param name="targetWidth"> The width of the area the text will be drawn, for wrapping or truncation. </param>
    /// <param name="halign"> Horizontal alignment of the text, see <see cref="Align"/>. </param>
    /// <param name="wrap"> If true, the text will be wrapped within targetWidth. </param>
    /// <param name="truncate">
    /// If not null, the text will be truncated within targetWidth with this string appended.
    /// May be an empty string.
    /// </param>
    /// <returns>
    /// The glyph layout for the cached string (the layout's height is the distance from y to the baseline).
    /// </returns>
    public GlyphLayout AddText( string str,
                                float x,
                                float y,
                                int start,
                                int end,
                                float targetWidth = 0,
                                Align halign = Align.Left,
                                bool wrap = false,
                                string? truncate = null )
    {
        GlyphLayout? layout = _pooledLayouts.Obtain();

        Guard.Against.Null( layout );

        layout.SetText( Font, str, start, end, Color, targetWidth, halign, wrap, truncate );

        AddText( layout, x, y );

        return layout;
    }

    /// <summary>
    /// Adds the specified glyphs.
    /// </summary>
    public void AddText( GlyphLayout? layout, float x, float y )
    {
        if ( layout != null )
        {
            AddToCache( layout, x, y + Font.FontData.Ascent );
        }
    }

    // ========================================================================

    #if DEBUG
    public float[][] GetPageVertices() => _pageVertices;

    public int[] GetIdx() => _idx;
    #endif
}

// ============================================================================
// ============================================================================