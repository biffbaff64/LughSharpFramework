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

using System.Buffers.Binary;
using LughSharp.Core.Maths;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Collections;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Pooling;

namespace LughSharp.Core.Graphics.Text;

/// <summary>
/// Stores <see cref="GlyphRun"/> runs of glyphs for a piece of text. The text may contain
/// newlines and color markup tags.
/// <para>
/// Where wrapping occurs is determined by <see cref="BitmapFont.BitmapFontData.GetWrapIndex"/>.
/// Additionally, when <see cref="BitmapFont.BitmapFontData.MarkupEnabled"/> is true wrapping
/// can occur at color start or end tags.
/// </para>
/// <para>
/// When wrapping occurs, whitespace is removed before and after the wrap position.
/// Whitespace is determined by <see cref="BitmapFont.BitmapFontData.IsWhitespace(char)"/>.
/// </para>
/// <para>
/// Glyphs positions are determined by <see cref="BitmapFont.BitmapFontData.GetGlyphs"/>.
/// </para>
/// <para>
/// This class is not thread safe, even if synchronized externally, and must only
/// be used from the game thread.
/// </para>
/// </summary>
[PublicAPI]
public class GlyphLayout : IResetable, IPoolable
{
    public List< GlyphRun > Runs       { get; set; } = new( 1 );
    public List< int >      Colors     { get; set; } = new();
    public float            Width      { get; set; }
    public float            Height     { get; set; }
    public int              GlyphCount { get; set; }

    // ========================================================================

    private const float EPSILON = 0.0001f;

//    private readonly Pool< Color >    _colorPool    = Pools.Get< Color >( () => new Color() );
    private readonly List< int >      _colorStack   = new( 4 );
    private readonly Pool< GlyphRun > _glyphRunPool = Pools.Get( () => new GlyphRun() );

    // ========================================================================

    /// <summary>
    /// Creates an empty GlyphLayout.
    /// </summary>
    public GlyphLayout()
    {
    }

    /// <summary>
    /// Creates a new GlyphLayout, using the supplied <see cref="BitmapFont"/> and text.
    /// </summary>
    /// <param name="font"> The font to use. </param>
    /// <param name="str"> A string holding the text. </param>
    public GlyphLayout( BitmapFont font, string str )
    {
        SetText( font, str );
    }

    /// <summary>
    /// Creates a new GlyphLayout, using the supplied <see cref="BitmapFont"/>, text message,
    /// <see cref="Color"/>, target width, horizontal alignment, and wrap.
    /// </summary>
    /// <param name="font"> The font to use. </param>
    /// <param name="str"> A string holding the text. </param>
    /// <param name="color">
    /// The default color to use for the text (the BitmapFont <see cref="BitmapFont.GetColor()"/>
    /// is not used). If <see cref="BitmapFont.BitmapFontData.MarkupEnabled"/> is true, color
    /// markup tags in the specified string may change the color for portions of the text.
    /// </param>
    /// <param name="targetWidth"></param>
    /// <param name="halign"></param>
    /// <param name="wrap"></param>
    public GlyphLayout( BitmapFont font, string str, Color color, float targetWidth, int halign, bool wrap )
    {
        SetText( font, str, color, targetWidth, halign, wrap );
    }

    /// <summary>
    /// </summary>
    /// <param name="font"> The font to use. </param>
    /// <param name="str"> A string holding the text. </param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="color">
    /// The default color to use for the text (the BitmapFont <see cref="BitmapFont.GetColor()"/>
    /// is not used). If <see cref="BitmapFont.BitmapFontData.MarkupEnabled"/> is true, color
    /// markup tags in the specified string may change the color for portions of the text.
    /// </param>
    /// <param name="targetWidth"></param>
    /// <param name="halign"></param>
    /// <param name="wrap"></param>
    /// <param name="truncate"></param>
    public GlyphLayout( BitmapFont font,
                        string str,
                        int start,
                        int end,
                        Color color,
                        float targetWidth,
                        int halign,
                        bool wrap,
                        string truncate )
    {
        SetText( font, str, start, end, color, targetWidth, halign, wrap, truncate );
    }

    /// <summary>
    /// Calls <see cref="SetText(LughSharp.Core.Graphics.Text.BitmapFont,string,int,int,Color,float,int,bool,string?)"/>
    /// with the whole string, the font's current color, and with no alignment or wrapping.
    /// </summary>
    /// <param name="font"> The font to use. </param>
    /// <param name="str"> A string holding the text. </param>
    public void SetText( BitmapFont font, string str )
    {
        SetText( font, str, 0, str.Length, font.GetColor(), 0, Alignment.LEFT, false, null );
    }

    /// <summary>
    /// Calls <see cref="SetText(LughSharp.Core.Graphics.Text.BitmapFont,string,int,int,Color,float,int,bool,string?)"/>
    /// with the whole string and no truncation.
    /// </summary>
    /// <param name="font"> The font to use. </param>
    /// <param name="str"> A string holding the text. </param>
    /// <param name="color">
    /// The default color to use for the text (the BitmapFont <see cref="BitmapFont.GetColor()"/>
    /// is not used). If <see cref="BitmapFont.BitmapFontData.MarkupEnabled"/> is true, color
    /// markup tags in the specified string may change the color for portions of the text.
    /// </param>
    /// <param name="targetWidth"></param>
    /// <param name="halign"></param>
    /// <param name="wrap"></param>
    public void SetText( BitmapFont font, string str, Color color, float targetWidth, int halign, bool wrap )
    {
        SetText( font, str, 0, str.Length, color, targetWidth, halign, wrap, null );
    }

    /// <summary>
    /// </summary>
    /// <param name="font"> The font to use. </param>
    /// <param name="str"> A string holding the text. </param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="color">
    /// The default color to use for the text (the BitmapFont <see cref="BitmapFont.GetColor()"/>
    /// is not used). If <see cref="BitmapFont.BitmapFontData.MarkupEnabled"/> is true, color
    /// markup tags in the specified string may change the color for portions of the text.
    /// </param>
    /// <param name="halign">
    /// Horizontal alignment of the text, see also <see cref="Alignment"/>.
    /// </param>
    /// <param name="targetWidth">
    /// The width used for alignment, line wrapping, and truncation. May be zero if
    /// those features are not used.
    /// </param>
    /// <param name="wrap"></param>
    /// <param name="truncate">
    /// If not null and the width of the glyphs exceed targetWidth, the glyphs are
    /// truncated and the glyphs for the specified truncate string are placed at the end.
    /// Empty string can be used to truncate without adding glyphs. Truncate should not
    /// be used with text that contains multiple lines. Wrap is ignored if truncate is
    /// not null.
    /// </param>
    public void SetText( BitmapFont font, string str, int start, int end, Color color,
                         float targetWidth, int halign, bool wrap, string? truncate )
    {
        Reset();

        var fontData = font.Data;

        if ( start == end )
        {
            // Empty string
            Width  = 0;
            Height = fontData.CapHeight;

            return;
        }

        // --------------------------------------

        if ( wrap )
        {
            targetWidth = Math.Max( targetWidth, fontData.SpaceXadvance * 3 );
        }

        var wrapOrTruncate = wrap || ( truncate != null );
        var currentColor   = ( int )color.PackedColorAbgr();
        var nextColor      = currentColor;

        Colors.Add( currentColor );

        var markupEnabled = fontData.MarkupEnabled;

        if ( markupEnabled )
        {
            _colorStack.Add( currentColor );
        }

        // --------------------------------------

        var isLastRun = false;
        var y         = 0f;
        var down      = fontData.Down;

        GlyphRun? lineRun   = null;
        Glyph?    lastGlyph = null;
        var       runStart  = start;

    outer:
        while ( true )
        {
            int runEnd;
            var newline = false;

            if ( start == end )
            {
                // End of text.
                if ( runStart == end )
                {
                    break; // No run to process, we're done.
                }

                runEnd    = end; // Process the final run.
                isLastRun = true;
            }
            else
            {
                // Each run is delimited by newline or left square bracket.
                switch ( str[ start++ ] )
                {
                    case '\n': // End of line.
                        runEnd  = start - 1;
                        newline = true;
                        break;

                    case '[': // Possible color tag.
                        if ( markupEnabled )
                        {
                            int length = ParseColorMarkup( str, start, end );

                            if ( length >= 0 )
                            {
                                runEnd =  start - 1;
                                start  += length + 1;
                                if ( start == end )
                                {
                                    isLastRun = true; // Color tag is the last element in the string.
                                }
                                else
                                {
                                    nextColor = _colorStack.Peek();
                                }

                                break;
                            }

                            if ( length == -2 )
                            {
                                start++; // Skip first of "[[" escape sequence.
                            }
                        }

                        goto outer;
                    
                    default:
                        goto outer;
                }
            }

        runEnded:
            {
                // Store the run that has ended.
                var run = _glyphRunPool.Obtain();
                run?.X = 0;
                run?.Y = y;

                fontData.GetGlyphs( run, str, runStart, runEnd, lastGlyph );

                GlyphCount += run?.Glyphs.Count ?? 0;

                if ( nextColor != currentColor )
                {
                    // Can only be different if markupEnabled.
                    if ( Colors[ Colors.Count - 2 ] == GlyphCount )
                    {
                        // Consecutive color changes, or after an empty run, or at the beginning of the string.
                        Colors[ Colors.Count - 1 ] = nextColor;
                    }
                    else
                    {
                        Colors.Add( GlyphCount );
                        Colors.Add( nextColor );
                    }

                    currentColor = nextColor;
                }

                if ( run?.Glyphs.Count == 0 )
                {
                    _glyphRunPool.Free( run );

                    if ( lineRun == null )
                    {
                        goto runEnded; // Otherwise wrap and truncate must still be processed for lineRun.
                    }
                }
                else if ( lineRun == null )
                {
                    lineRun = run;
                    Runs.Add( lineRun! );
                }
                else
                {
                    lineRun.AppendRun( run );
                    _glyphRunPool.Free( run );
                }

                if ( newline || isLastRun )
                {
                    SetLastGlyphXAdvance( fontData, lineRun );
                    lastGlyph = null;
                }
                else
                {
                    lastGlyph = lineRun?.Glyphs.Peek();
                }

                if ( !wrapOrTruncate || lineRun?.Glyphs.Count == 0 )
                {
                    goto runEnded; // No wrap or truncate, or no glyphs.
                }

                if ( newline || isLastRun )
                {
                    // Wrap or truncate. First xadvance is the first glyph's X offset relative to the drawing position.
                    var runWidth = lineRun?.XAdvances.First() + lineRun?.XAdvances[ 1 ]; // At least the first glyph will fit.
                    
                    for ( int i = 2; i < lineRun?.XAdvances.Count; i++ )
                    {
                        Glyph glyph      = lineRun.Glyphs[ i - 1 ];
                        float glyphWidth = GetGlyphWidth( glyph, fontData );
                        
                        if ( runWidth + glyphWidth - EPSILON <= targetWidth )
                        {
                            // Glyph fits.
                            runWidth += lineRun.XAdvances[ i ];
                            continue;
                        }

                        if ( truncate != null )
                        {
                            // Truncate.
                            Truncate( fontData, lineRun, targetWidth, truncate );

                            goto outer;
                        }

                        // Wrap.
                        int wrapIndex = fontData.GetWrapIndex( lineRun.Glyphs, i );

                        // Require at least one glyph per line.
                        if ( ( wrapIndex == 0 && lineRun.X == 0 ) || wrapIndex >= lineRun.Glyphs.Count )
                        {
                            // Wrap at least the glyph that didn't fit.
                            wrapIndex = i - 1;
                        }

                        lineRun = Wrap( fontData, lineRun, wrapIndex );

                        if ( lineRun == null )
                        {
                            goto runEnded; // All wrapped glyphs were whitespace.
                        }

                        Runs.Add( lineRun );

                        y         += down;
                        lineRun.X =  0;
                        lineRun.Y =  y;

                        // Start the wrap loop again, another wrap might be necessary.
                        runWidth = lineRun.XAdvances.First() +
                                   lineRun.XAdvances[ 1 ]; // At least the first glyph will fit.
                        i = 1;
                    }
                }
            }

            if ( newline )
            {
                lineRun   = null;
                lastGlyph = null;

                // Next run will be on the next line.
                if ( runEnd == runStart ) // Blank line.
                {
                    y += down * fontData.BlankLineScale;
                }
                else
                {
                    y += down;
                }
            }

            runStart = start;
        }

        // --------------------------------------

        Height = fontData.CapHeight + Math.Abs( y );

        CalculateRunWidths( fontData );

        AlignRuns( targetWidth, halign );

        if ( markupEnabled )
        {
            _colorStack.Clear();
        }
    }

    /// <summary>
    /// Advances the x position for the current run.
    /// </summary>
    /// <param name="run">Current glyph run.</param>
    /// <param name="fontData">Font data containing font-specific information.</param>
    /// <param name="x">Current x position.</param>
    private void AdvanceXForRun( GlyphRun run, BitmapFont.BitmapFontData fontData, ref float x )
    {
        if ( fontData.MarkupEnabled )
        {
            foreach ( var xAdvance in run.XAdvances )
            {
                x += xAdvance;
            }
        }
    }

    /// <summary>
    /// Ensures a minimum number of glyphs per line when wrapping text.
    /// </summary>
    /// <param name="run">Current glyph run.</param>
    /// <param name="wrapIndex">Index at which to wrap the text.</param>
    /// <returns>Adjusted wrap index.</returns>
    private int EnsureMinimumGlyphsPerLine( GlyphRun run, int wrapIndex )
    {
        if ( ( ( wrapIndex == 0 ) && ( run.X == 0 ) ) || ( wrapIndex >= run.Glyphs.Count ) )
        {
            wrapIndex = run.Glyphs.Count - 1;
        }

        return wrapIndex;
    }

    /// <summary>
    /// Calculates the widths of all glyph runs.
    /// </summary>
    private void CalculateRunWidths( BitmapFont.BitmapFontData fontData )
    {
        float width = 0;

        foreach ( var run in Runs )
        {
            var   xAdvances = run.XAdvances.ToArray();
            var   runWidth  = xAdvances[ 0 ];
            float maxWidth  = 0;

            foreach ( var glyph in run.Glyphs )
            {
                var glyphWidth = ( ( glyph.Width + glyph.Xoffset ) * fontData.ScaleX ) - fontData.PadRight;

                maxWidth =  Math.Max( maxWidth, runWidth + glyphWidth );
                runWidth += xAdvances.ElementAtOrDefault( run.Glyphs.IndexOf( glyph ) + 1 );
            }

            run.Width = Math.Max( runWidth, maxWidth );
            width     = Math.Max( width, run.X + run.Width );
        }

        Width = width;
    }

    /// <summary>
    /// Aligns all glyph runs based on the target width and alignment option.
    /// </summary>
    /// <param name="targetWidth">Target width for text alignment.</param>
    /// <param name="halign">Horizontal alignment option.</param>
    private void AlignRuns( float targetWidth, int halign )
    {
        var isCenterAligned = ( halign & Alignment.CENTER ) != 0;
        var lineWidth       = 0f;
        var lineY           = float.MinValue;
        var lineStart       = 0;

        foreach ( var run in Runs )
        {
            if ( !run.Y.Equals( lineY ) )
            {
                lineY = run.Y;
                var shift = targetWidth - lineWidth;
                shift = isCenterAligned ? shift / 2 : shift;

                while ( lineStart < Runs.IndexOf( run ) )
                {
                    Runs[ lineStart++ ].X += shift;
                }

                lineWidth = run.X + run.Width;
            }
            else
            {
                lineWidth = Math.Max( lineWidth, run.X + run.Width );
            }
        }

        var widthShift = targetWidth - lineWidth;
        widthShift = isCenterAligned ? widthShift / 2 : widthShift;

        while ( lineStart < Runs.Count )
        {
            Runs[ lineStart++ ].X += widthShift;
        }
    }

    /// <summary>
    /// Truncates a glyph run to fit within the specified target width by appending a truncate string if necessary.
    /// </summary>
    /// <param name="fontData">The font data used to obtain glyphs and their properties.</param>
    /// <param name="run">The glyph run to be truncated.</param>
    /// <param name="targetWidth">The maximum width that the text should occupy.</param>
    /// <param name="truncate">The string to append at the end if truncation is required.</param>
    /// <exception cref="GdxRuntimeException">Thrown when a GlyphRun cannot be obtained from the pool.</exception>
    private void Truncate( BitmapFont.BitmapFontData fontData,
                           GlyphRun run,
                           float targetWidth,
                           string truncate )
    {
        // Obtain a GlyphRun for the truncate string.
        var truncateRun = _glyphRunPool.Obtain();

        if ( truncateRun == null )
        {
            throw new GdxRuntimeException( "Unable to obtain a GlyphRun!" );
        }

        // Populate the truncate run with glyphs from the truncate string.
        fontData.GetGlyphs( truncateRun, truncate, 0, truncate.Length, null );

        // Calculate the width of the truncate string.
        float truncateWidth = 0;

        if ( truncateRun.XAdvances.Count > 0 )
        {
            AdjustLastGlyph( fontData, truncateRun );
            var xAdvances = truncateRun.XAdvances.ToArray();

            // Sum the advances to get the total width, skipping the first advance for tighter bounds.
            for ( var i = 1; i < truncateRun.XAdvances.Count; i++ )
            {
                truncateWidth += xAdvances[ i ];
            }
        }

        // Subtract the truncate string width from the target width.
        targetWidth -= truncateWidth;

        // Determine how many glyphs from the original run fit within the remaining target width.
        var count        = 0;
        var width        = run.X;
        var runXAdvances = run.XAdvances.ToArray();

        while ( count < run.XAdvances.Count )
        {
            var xAdvance = runXAdvances[ count ];
            width += xAdvance;

            // Stop if adding the next glyph would exceed the target width.
            if ( width > targetWidth )
            {
                break;
            }

            count++;
        }

        if ( count > 1 )
        {
            // If at least one glyph from the original run fits, truncate the run and append the truncate glyphs.
            run.Glyphs.Truncate( count - 1 );
            run.XAdvances.Truncate( count );

            AdjustLastGlyph( fontData, run );

            if ( truncateRun.XAdvances.Count > 0 )
            {
                run.XAdvances.AddAll( truncateRun.XAdvances, 1, truncateRun.XAdvances.Count - 1 );
            }
        }
        else
        {
            // If no glyphs from the original run fit, use only the truncate glyphs.
            run.Glyphs.Clear();
            run.XAdvances.Clear();
            run.XAdvances.AddAll( truncateRun.XAdvances );
        }

        // Add the truncate glyphs to the run.
        run.Glyphs.AddAll( truncateRun.Glyphs );

        // Free the truncate run.
        _glyphRunPool.Free( truncateRun );
    }

    /// <summary>
    /// Breaks a run into two runs at the specified wrapIndex.
    /// </summary>
    /// <param name="fontData"></param>
    /// <param name="first"></param>
    /// <param name="wrapIndex"></param>
    /// <returns> May be null if second run is all whitespace. </returns>
    private GlyphRun? Wrap( BitmapFont.BitmapFontData? fontData, GlyphRun? first, int wrapIndex )
    {
        Guard.Against.Null( fontData );
        Guard.Against.Null( first );

        var glyphs2    = first.Glyphs; // Starts with all the glyphs.
        var glyphCount = first.Glyphs.Count;
        var xAdvances2 = first.XAdvances; // Starts with all the xAdvances.

        // Skip whitespace before the wrap index.
        var firstEnd = wrapIndex;

        for ( ; firstEnd > 0; firstEnd-- )
        {
            if ( !fontData.IsWhitespace( ( char )glyphs2[ firstEnd - 1 ].ID ) )
            {
                break;
            }
        }

        // Skip whitespace after the wrap index.
        var secondStart = wrapIndex;

        for ( ; secondStart < glyphCount; secondStart++ )
        {
            if ( !fontData.IsWhitespace( ( char )glyphs2[ secondStart ].ID ) )
            {
                break;
            }
        }

        // Copy wrapped glyphs and xAdvances to second run. The second run will
        // contain the remaining glyph data, so swap instances rather than copying.
        GlyphRun? second = null;

        if ( secondStart < glyphCount )
        {
            second = _glyphRunPool.Obtain();
            second?.Color.Set( first.Color );

            var glyphs1 = second?.Glyphs; // Starts empty.

            glyphs1?.AddAll( glyphs2, 0, firstEnd );
            glyphs2.RemoveRange( 0, secondStart - 1 );

            first.Glyphs   = glyphs1!;
            second!.Glyphs = glyphs2;

            var xAdvances1 = second.XAdvances; // Starts empty.

            xAdvances1.AddAll( xAdvances2, 0, firstEnd + 1 );
            xAdvances2.RemoveRange( 1, secondStart ); // Leave first entry to be overwritten by next line.
            xAdvances2[ 0 ]  = ( -glyphs2.First().Xoffset * fontData.ScaleX ) - fontData.PadLeft;
            first.XAdvances  = xAdvances1;
            second.XAdvances = xAdvances2;
        }
        else
        {
            // Second run is empty, just trim whitespace glyphs from end of first run.
            glyphs2.Truncate( firstEnd );
            xAdvances2.Truncate( firstEnd + 1 );
        }

        if ( firstEnd == 0 )
        {
            // If the first run is now empty, remove it.
            _glyphRunPool.Free( first );
            Runs.Pop();
        }
        else
        {
            AdjustLastGlyph( fontData, first );
        }

        return second;
    }

    /// <summary>
    /// Adjusts the xadvance of the last glyph to use its width instead of xadvance.
    /// </summary>
    /// <param name="fontData"></param>
    /// <param name="run"></param>
    /// <seealso cref="GlyphRun.XAdvances"/>
    private void AdjustLastGlyph( BitmapFont.BitmapFontData fontData, GlyphRun run )
    {
        var last = run.Glyphs.Peek();

        if ( last.FixedWidth )
        {
            return;
        }

        var width = ( ( last.Width + last.Xoffset ) * fontData.ScaleX ) - fontData.PadRight;

        run.XAdvances.ToArray()[ run.XAdvances.Count - 1 ] = width;
    }

    /// <summary>
    /// Parses a color markup within the specified string and range.
    /// </summary>
    /// <param name="str"> The input string containing the markup. </param>
    /// <param name="start"> The start index of the markup. </param>
    /// <param name="end"> The end index of the markup. </param>
    /// <param name="colorpool"> The pool from which to obtain color instances. </param>
    /// <returns>
    /// An integer indicating the number of characters processed:
    /// - -1 if the string ends with "["
    /// - -2 if the markup is "[["
    /// - 0 if the markup is "[]"
    /// - The number of characters processed for valid color markups
    /// </returns>
    private int ParseColorMarkup( string str, int start, int end )
    {
        if ( start == end )
        {
            return -1; // String ended with "[".
        }

        switch ( str[ start ] )
        {
            case '#':
                return ParseHexColor( str, start, end );

            case '[': // "[[" is an escaped left square bracket.
                return -2;

            case ']': // "[]" is a "pop" color tag.
                if ( _colorStack.Count > 1 )
                {
                    _colorStack.Pop();
                }

                return 0;

            default:
                return ParseNamedColor( str, start, end );
        }
    }

    /// <summary>
    /// Parses a hexadecimal color markup within the specified string and range.
    /// </summary>
    /// <param name="str"> The input string containing the markup. </param>
    /// <param name="start"> The start index of the markup. </param>
    /// <param name="end"> The end index of the markup. </param>
    /// <param name="colorpool"> The pool from which to obtain color instances. </param>
    /// <returns>
    /// An integer indicating the number of characters processed, or -1 if the markup is invalid.
    /// </returns>
    private int ParseHexColor( string str, int start, int end )
    {
        uint colorInt = 0;

        for ( var i = start + 1; i < end; i++ )
        {
            var ch = str[ i ];

            if ( ch == ']' )
            {
                if ( ( i < ( start + 2 ) ) || ( i > ( start + 9 ) ) )
                {
                    break; // Illegal number of hex digits.
                }

                if ( ( i - start ) <= 7 )
                {
                    // RRGGBB or fewer chars.
                    colorInt <<= ( 9 - ( i - start ) << 2 ) | 0xff;
                }
                
                _colorStack.Add( ( int )BinaryPrimitives.ReverseEndianness( colorInt ) );

                return i - start;
            }

            if ( NumberUtils.IsHexDigit( ch ) )
            {
                colorInt = ( uint )( ( colorInt * 16 ) + NumberUtils.HexValue( ch ) );
            }
            else
            {
                break; // Unexpected character in hex color.
            }
        }

        return -1;
    }

    /// <summary>
    /// Parses a named color markup within the specified string and range.
    /// </summary>
    /// <param name="str"> The input string containing the markup. </param>
    /// <param name="start"> The start index of the markup. </param>
    /// <param name="end"> The end index of the markup. </param>
    /// <param name="colorpool"> The pool from which to obtain color instances. </param>
    /// <returns>
    /// An integer indicating the number of characters processed, or -1 if the markup is invalid.
    /// </returns>
    private int ParseNamedColor( string str, int start, int end )
    {
        var colorStart = start;

        for ( var i = start + 1; i < end; i++ )
        {
            var ch = str[ i ];

            if ( ch == ']' )
            {
                var colorName  = str.Substring( colorStart, i - colorStart );
                var namedColor = Graphics.Colors.Get( colorName );

                if ( namedColor == null )
                {
                    return -1; // Unknown color name.
                }

                _colorStack.Add( ( int )namedColor.PackedColorAbgr() );

                return i - start;
            }
        }

        return -1; // Unclosed color tag.
    }

    private void SetLastGlyphXAdvance( BitmapFont.BitmapFontData fontData, GlyphRun run )
    {
        var last = run.Glyphs.Peek();

        if ( !last.FixedWidth )
        {
            run.XAdvances[ run.XAdvances.Count - 1 ] = GetGlyphWidth( last, fontData );
        }
    }

    private float GetGlyphWidth( Glyph glyph, BitmapFont.BitmapFontData fontData )
     {
        return ( glyph.FixedWidth
            ? glyph.Xadvance
            : glyph.Width + glyph.Xoffset ) * fontData.ScaleX - fontData.PadRight;
    }

    /// <summary>
    /// Resets the object for reuse. Object references should be nulled and fields
    /// may be set to default values.
    /// </summary>
    public void Reset()
    {
        _glyphRunPool.FreeAll( Runs! );
        _colorStack.Clear();

        Colors.Clear();
        Runs.Clear();

        Width  = 0;
        Height = 0;
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Stores glyphs and positions for a piece of text which is a single color and
    /// does not span multiple lines.
    /// </summary>
    [PublicAPI]
    public class GlyphRun : IResetable
    {
        public List< Glyph > Glyphs    { get; set; } = [ ];
        public List< float > XAdvances { get; set; } = [ ];
        public float         X         { get; set; }
        public float         Y         { get; set; }
        public float         Width     { get; set; }
        public Color         Color     { get; set; } = new();

        public void AppendRun( GlyphRun run )
        {
            Glyphs.AddRange( run.Glyphs );

            // Remove the width of the last glyph. The first xadvance of the
            // appended run has kerning for the last glyph of this run.
            if ( XAdvances.Count > 0 )
            {
                XAdvances.Pop();
            }

            XAdvances.AddRange( run.XAdvances );
        }

        /// <summary>
        /// Resets the object for reuse. Object references should be nulled and fields
        /// may be set to default values.
        /// </summary>
        public void Reset()
        {
            Glyphs.Clear();
            XAdvances.Clear();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder buffer = new( Glyphs.Count + 32 );

            for ( int i = 0, n = Glyphs.Count; i < n; i++ )
            {
                var g = Glyphs[ i ];
                buffer.Append( ( char )g.ID );
            }

            buffer.Append( $", #{Color}, {X}, {Y}, {Width}" );

            return buffer.ToString();
        }
    }
}