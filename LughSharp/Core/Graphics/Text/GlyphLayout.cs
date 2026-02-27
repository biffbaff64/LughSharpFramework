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
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

using LughSharp.Core.Maths;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Collections;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;
using LughSharp.Core.Utils.Pooling;

namespace LughSharp.Core.Graphics.Text;

/// <summary>
/// Stores <see cref="GlyphRun"/> runs of glyphs for a piece of text. The text may contain
/// newlines and color markup tags.
/// <para>
/// Where wrapping occurs is determined by <see cref="BitmapFontData.GetWrapIndex"/>.
/// Additionally, when <see cref="BitmapFontData.MarkupEnabled"/> is true wrapping
/// can occur at color start or end tags.
/// </para>
/// <para>
/// When wrapping occurs, whitespace is removed before and after the wrap position.
/// Whitespace is determined by <see cref="BitmapFontData.IsWhitespace(char)"/>.
/// </para>
/// <para>
/// Glyphs positions are determined by <see cref="BitmapFontData.GetGlyphs"/>.
/// </para>
/// <para>
/// This class is not thread safe, even if synchronized externally, and must only
/// be used from the game thread.
/// </para>
/// </summary>
[PublicAPI]
public class GlyphLayout : IResetable, IPoolable
{
    public struct GlyphColor( int index, int argb8888 )
    {
        public int GlyphIndex { get; } = index;
        public int Argb8888   { get; } = argb8888;
    }

    /// <summary>
    /// Each run has the glyphs for a line of text.
    /// <para>
    /// Runs are pooled, so references should not be kept past the next call to
    /// <see cref="SetText(BitmapFont, string, int, int, Color, float, int, bool, string)"/>
    /// or <see cref="Reset()"/>.
    /// </para>
    /// </summary>
    public List< GlyphRun > Runs { get; set; } = new( 1 );

    /// <summary>
    /// Determines the colors of the glpyhs in the <see cref="Runs"/>. Entries are
    /// instances of the <see cref="GlyphColor"/> struct, which holds the glyph index
    /// (across all runs) where the color starts, and the color encoded as ABGR8888.
    /// <para>
    /// For example: <code>[0, WHITE, 4, GREEN, 5, WHITE]</code>
    /// Glpyhs 0 to 3 are WHITE, 4 is GREEN and 5 to the end are WHITE.
    /// </para>
    /// <para>
    /// The List is empty if there are no runs, otherwise it has at least two entries:
    /// <code>[0, startColor]</code>
    /// </para>
    /// </summary>
    public List< GlyphColor > Colors { get; set; } = new( 1 );

    /// <summary>
    /// The number of glyphs across all runs.
    /// </summary>
    public int GlyphCount { get; set; }

    public float Width  { get; set; }
    public float Height { get; set; }

    // ========================================================================

    private const float EPSILON = 0.0001f;

    private readonly Pool< GlyphRun > _glyphRunPool = Pools.Get< GlyphRun >( () => new GlyphRun() );
    private readonly List< int >      _colorStack   = new( 4 );

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
    /// is not used). If <see cref="BitmapFontData.MarkupEnabled"/> is true, color
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
    /// is not used). If <see cref="BitmapFontData.MarkupEnabled"/> is true, color
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
    /// Calls <see cref="SetText(BitmapFont,string,int,int,Color,float,int,bool,string?)"/>
    /// with the whole string, the font's current color, and with no alignment or wrapping.
    /// </summary>
    /// <param name="font"> The font to use. </param>
    /// <param name="str"> A string holding the text. </param>
    public void SetText( BitmapFont font, string str )
    {
        SetText( font, str, 0, str.Length, font.GetColor(), 0, Align.LEFT, false, null );
    }

    /// <summary>
    /// Calls <see cref="SetText(BitmapFont,string,int,int,Color,float,int,bool,string?)"/>
    /// with the whole string and no truncation.
    /// </summary>
    /// <param name="font"> The font to use. </param>
    /// <param name="str"> A string holding the text. </param>
    /// <param name="color">
    /// The default color to use for the text (the BitmapFont <see cref="BitmapFont.GetColor()"/>
    /// is not used). If <see cref="BitmapFontData.MarkupEnabled"/> is true, color
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
    /// is not used). If <see cref="BitmapFontData.MarkupEnabled"/> is true, color
    /// markup tags in the specified string may change the color for portions of the text.
    /// </param>
    /// <param name="halign">
    /// Horizontal alignment of the text, see also <see cref="Align"/>.
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

        BitmapFontData fontData = font.FontData;

        if ( start == end )
        {
            // Empty string
            Width  = 0;
            Height = fontData.CapHeight;

            return;
        }

        if ( wrap )
        {
            // Avoid wrapping one line per character, which is very inefficient.
            targetWidth = Math.Max( targetWidth, fontData.SpaceXadvance * 3 );
        }

        bool wrapOrTruncate = wrap || ( truncate != null );
        var  currentColor   = ( int )color.PackedColorRgba();
        int  nextColor      = currentColor;

        Colors.Add( new GlyphColor( 0, currentColor ) );

        bool markupEnabled = fontData.MarkupEnabled;

        if ( markupEnabled )
        {
            _colorStack.Add( currentColor );
        }

        var   isLastRun = false;
        var   y         = 0f;
        float down      = fontData.Down;
        int   runStart  = start;

        // Collects glyphs for the current line.
        GlyphRun? lineRun = null;

        // Last glyph of the previous run on the same line, used for kerning between runs.
        Glyph? lastGlyph = null;

        while ( true )
        {
            var runEnd       = 0;
            var newline      = false;
            var breakToOuter = false;

            if ( start == end )
            {
                // End of text.
                if ( runStart == end )
                {
                    // No run to process, we're done.
                    break;
                }

                runEnd    = end; // Process the final run.
                isLastRun = true;
            }
            else
            {
                breakToOuter = ParseDelimiters( str,
                                                ref start,
                                                end,
                                                ref newline,
                                                ref runEnd,
                                                ref isLastRun,
                                                ref nextColor,
                                                ref markupEnabled );
            }

            if ( breakToOuter )
            {
                // Delimiter found
                goto outer;
            }

            // Handle run end
            {
                // Store the run that has ended.
                GlyphRun run = _glyphRunPool.Obtain();
                run.X = 0;
                run.Y = y;

                fontData.GetGlyphs( run, str, runStart, runEnd, lastGlyph );

                GlyphCount += run.Glyphs.Count;

                if ( nextColor != currentColor )
                {
                    // Can only be different if markupEnabled.
                    if ( Colors[ Colors.Count - 1 ].GlyphIndex == GlyphCount )
                    {
                        // Consecutive color changes, or after an empty run,
                        // or at the beginning of the string.
                        Colors[ Colors.Count - 1 ] = new GlyphColor( GlyphCount, nextColor );
                    }
                    else
                    {
                        Colors.Add( new GlyphColor( GlyphCount, nextColor ) );
                    }

                    currentColor = nextColor;
                }

                if ( run.Glyphs.Count == 0 )
                {
                    _glyphRunPool.Free( run );

                    if ( lineRun == null )
                    {
                        // Otherwise wrap and truncate must still
                        // be processed for lineRun.
                        goto runEnded;
                    }
                }
                else if ( lineRun == null )
                {
                    lineRun = run;
                    Runs.Add( lineRun );
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
                    lastGlyph = lineRun.Glyphs.Peek();
                }

                if ( !wrapOrTruncate || lineRun.Glyphs.Count == 0 )
                {
                    // No wrap or truncate, or no glyphs.
                }
                else
                {
                    if ( newline || isLastRun )
                    {
                        // Handle Wrap or truncate.
                        // First xadvance is the first glyph's X offset relative
                        // to the drawing position.

                        // At least the first glyph will fit.
                        float runWidth = lineRun.XAdvances.First() + lineRun.XAdvances[ 1 ];

                        for ( var i = 2; i < lineRun?.XAdvances.Count; i++ )
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
                            if ( ( wrapIndex == 0 && lineRun.X == 0 )
                              || wrapIndex >= lineRun.Glyphs.Count )
                            {
                                // Wrap at least the glyph that didn't fit.
                                wrapIndex = i - 1;
                            }

                            lineRun = Wrap( fontData, lineRun, wrapIndex );

                            if ( lineRun != null )
                            {
                                Runs.Add( lineRun );

                                y         += down;
                                lineRun.X =  0;
                                lineRun.Y =  y;

                                // Start the wrap loop again, another wrap might be necessary.
                                // At least the first glyph will fit.
                                runWidth = lineRun.XAdvances.First() + lineRun.XAdvances[ 1 ];

                                i = 1;
                            }
                        }
                    }
                }
            }

        runEnded:

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

        outer: ;
        }

        FinalizeRun( fontData, y, targetWidth, halign, markupEnabled );
    }

    /// <summary>
    /// Parses delimiters in a string to identify glyph layout boundaries
    /// such as newlines or markup tags.
    /// </summary>
    /// <param name="str">The string to parse for delimiters.</param>
    /// <param name="start">
    /// The starting position within the string to begin parsing. Updated after parsing.
    /// </param>
    /// <param name="end">The ending position within the string to stop parsing.</param>
    /// <param name="newline">
    /// A flag indicating whether a newline character was encountered. Updated after parsing.
    /// </param>
    /// <param name="runEnd">The position marking the end of the current run. Updated after parsing.</param>
    /// <param name="isLastRun">
    /// A flag indicating whether the current run is the last one. Updated after parsing.
    /// </param>
    /// <param name="nextColor">
    /// The next color to apply to text after parsing a color tag. Updated after parsing.
    /// </param>
    /// <param name="markupEnabled">A flag indicating whether text markup is enabled.</param>
    /// <returns>
    /// True if parsing should continue, false if a boundary like a newline is encountered.
    /// </returns>
    private bool ParseDelimiters( string str,
                                  ref int start,
                                  int end,
                                  ref bool newline,
                                  ref int runEnd,
                                  ref bool isLastRun,
                                  ref int nextColor,
                                  ref bool markupEnabled )
    {
        char character = str[ start++ ];

        // Each run is delimited by newline or left square bracket.
        if ( character == '\n' ) // End of line.
        {
            runEnd  = start - 1;
            newline = true;

            return false;
        }

        if ( ( character == '[' ) && markupEnabled ) // Possible color tag.
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

                return false;
            }

            if ( length == -2 )
            {
                start++; // Skip first of "[[" escape sequence.
            }
        }

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    private void FinalizeRun( BitmapFontData fontData,
                              float y,
                              float targetWidth,
                              int halign,
                              bool markupEnabled )
    {
        Height = fontData.CapHeight + Math.Abs( y );

        CalculateRunWidths( fontData );

        AlignRuns( targetWidth, halign );

        if ( markupEnabled )
        {
            _colorStack.Clear();
        }
    }

    /// <summary>
    /// Calculates the widths of all glyph runs.
    /// </summary>
    private void CalculateRunWidths( BitmapFontData fontData )
    {
        var        width     = 0f;
        GlyphRun[] runsItems = Runs.ToArray();

        for ( int i = 0, n = Runs.Count; i < n; i++ )
        {
            GlyphRun run       = runsItems[ i ];
            float[]  xAdvances = run.XAdvances.ToArray();

            // run.x is needed to ensure floats are rounded same as above.
            float runWidth = run.X + xAdvances[ 0 ];

            var     max    = 0f;
            Glyph[] glyphs = run.Glyphs.ToArray();

            for ( int ii = 0, nn = glyphs.Length; ii < nn; )
            {
                Glyph glyph      = glyphs[ ii ];
                float glyphWidth = GetGlyphWidth( glyph, fontData );

                // A glyph can extend past the right edge of subsequent glyphs.
                max = Math.Max( max, runWidth + glyphWidth );

                ii++;

                runWidth += xAdvances[ ii ];
            }

            run.Width = Math.Max( runWidth, max ) - run.X;
            width     = Math.Max( width, run.X + run.Width );
        }

        Width = width;
    }

    /// <summary>
    /// Align runs to center or right of targetWidth. Requires run.width of runs
    /// to be already set
    /// </summary>
    private void AlignRuns( float targetWidth, int halign )
    {
        if ( ( halign & Align.LEFT ) == 0 )
        {
            // Not left aligned, so must be center or right aligned.
            bool       center    = ( halign & Align.CENTER ) != 0;
            GlyphRun[] runsItems = Runs.ToArray();

            for ( int i = 0, n = Runs.Count; i < n; i++ )
            {
                GlyphRun run = runsItems[ i ];

                run.X += center
                    ? 0.5f * ( targetWidth - run.Width )
                    : targetWidth - run.Width;
            }
        }
    }

    /// <summary>
    /// Truncates a glyph run to fit within the specified target width by appending a truncate string if necessary.
    /// </summary>
    /// <param name="fontData">The font data used to obtain glyphs and their properties.</param>
    /// <param name="run">The glyph run to be truncated.</param>
    /// <param name="targetWidth">The maximum width that the text should occupy.</param>
    /// <param name="truncate">The string to append at the end if truncation is required.</param>
    /// <exception cref="RuntimeException">Thrown when a GlyphRun cannot be obtained from the pool.</exception>
    private void Truncate( BitmapFontData fontData,
                           GlyphRun run,
                           float targetWidth,
                           string truncate )
    {
        int glyphCount = run.Glyphs.Count;

        // Obtain a GlyphRun for the truncate string.
        GlyphRun? truncateRun = _glyphRunPool.Obtain();

        if ( truncateRun == null )
        {
            throw new RuntimeException( "Unable to obtain a GlyphRun!" );
        }

        // Populate the truncate run with glyphs from the truncate string.
        fontData.GetGlyphs( truncateRun, truncate, 0, truncate.Length, null );

        // Calculate the width of the truncate string.
        var truncateWidth = 0f;

        if ( truncateRun.XAdvances.Count > 0 )
        {
            SetLastGlyphXAdvance( fontData, truncateRun );

            float[] xAdvances = truncateRun.XAdvances.ToArray();

            // Sum the advances to get the total width, skipping the
            // first advance for tighter bounds.
            for ( var i = 1; i < truncateRun.XAdvances.Count; i++ )
            {
                truncateWidth += xAdvances[ i ];
            }
        }

        // Subtract the truncate string width from the target width.
        targetWidth -= truncateWidth;

        // Determine how many glyphs from the original run fit within
        // the remaining target width.
        var     count        = 0;
        float   width        = run.X;
        float[] runXAdvances = run.XAdvances.ToArray();

        while ( count < run.XAdvances.Count )
        {
            float xAdvance = runXAdvances[ count ];
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
            // If at least one glyph from the original run fits,
            // truncate the run and append the truncate glyphs.
            run.Glyphs.Truncate( count - 1 );
            run.XAdvances.Truncate( count );

            SetLastGlyphXAdvance( fontData, run );

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

        int droppedGlyphCount = glyphCount - run.Glyphs.Count;

        if ( droppedGlyphCount > 0 )
        {
            GlyphCount -= droppedGlyphCount;

            if ( fontData.MarkupEnabled )
            {
                while ( Colors.Count > 1 && Colors[ Colors.Count - 1 ].GlyphIndex >= GlyphCount )
                {
                    Colors.RemoveAt( Colors.Count - 1 );
                }
            }
        }


        // Add the truncate glyphs to the run.
        run.Glyphs.AddAll( truncateRun.Glyphs );

        GlyphCount += truncate.Length;

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
    private GlyphRun? Wrap( BitmapFontData? fontData, GlyphRun? first, int wrapIndex )
    {
        Guard.Against.Null( fontData );
        Guard.Against.Null( first );

        List< Glyph > glyphs2    = first.Glyphs; // Starts with all the glyphs.
        int           glyphCount = first.Glyphs.Count;
        List< float > xAdvances2 = first.XAdvances; // Starts with all the xAdvances.

        // Skip whitespace before the wrap index.
        int firstEnd = wrapIndex;

        for ( ; firstEnd > 0; firstEnd-- )
        {
            if ( !fontData.IsWhitespace( ( char )glyphs2[ firstEnd - 1 ].ID ) )
            {
                break;
            }
        }

        // Skip whitespace after the wrap index.
        int secondStart = wrapIndex;

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

            List< Glyph > glyphs1 = second.Glyphs; // Starts empty.

            glyphs1.AddAll( glyphs2, 0, firstEnd );
            glyphs2.RemoveRange( 0, secondStart - 1 );

            first.Glyphs  = glyphs1;
            second.Glyphs = glyphs2;

            List< float > xAdvances1 = second.XAdvances; // Starts empty.

            xAdvances1.AddAll( xAdvances2, 0, firstEnd + 1 );
            xAdvances2.RemoveRange( 1, secondStart ); // Leave first entry to be overwritten by next line.
            xAdvances2[ 0 ]  = GetLineOffset( glyphs2, fontData );
            first.XAdvances  = xAdvances1;
            second.XAdvances = xAdvances2;

            int firstGlyphCount   = first.Glyphs.Count; // After wrapping it.
            int secondGlyphCount  = second.Glyphs.Count;
            int droppedGlyphCount = glyphCount - firstGlyphCount - secondGlyphCount;

            GlyphCount -= droppedGlyphCount;

            if ( fontData.MarkupEnabled && droppedGlyphCount > 0 )
            {
                int reductionThreshold = GlyphCount - secondGlyphCount;

                for ( int i = Colors.Count - 1; i >= 1; i-- )
                {
                    int colorChangeIndex = Colors[ i ].GlyphIndex;

                    if ( colorChangeIndex <= reductionThreshold )
                    {
                        break;
                    }

                    Colors[ i ] = new GlyphColor( colorChangeIndex - droppedGlyphCount,
                                                  Colors[ i ].Argb8888 );
                }
            }
        }
        else
        {
            // Second run is empty, just trim whitespace glyphs from end of first run.
            glyphs2.Truncate( firstEnd );
            xAdvances2.Truncate( firstEnd + 1 );

            int droppedGlyphCount = secondStart - firstEnd;

            if ( droppedGlyphCount > 0 )
            {
                GlyphCount -= droppedGlyphCount;

                if ( fontData.MarkupEnabled && Colors[ Colors.Count - 1 ].GlyphIndex > GlyphCount )
                {
                    // Many color changes can be hidden in the dropped whitespace, so keep only the very last color entry.
                    int lastColor = Colors.Peek().Argb8888;

                    while ( Colors[ Colors.Count - 1 ].GlyphIndex > GlyphCount )
                    {
                        Colors.Pop();
                    }

                    // Update the color change index and color entry.
                    Colors[ Colors.Count - 1 ] = new GlyphColor( GlyphCount, lastColor );
                }
            }
        }

        if ( firstEnd == 0 )
        {
            // If the first run is now empty, remove it.
            _glyphRunPool.Free( first );
            Runs.Pop();
        }
        else
        {
            SetLastGlyphXAdvance( fontData, first );
        }

        return second;
    }

    /// <summary>
    /// Sets the xadvance of the last glyph to use its width instead of xadvance.
    /// </summary>
    private void SetLastGlyphXAdvance( BitmapFontData fontData, GlyphRun run )
    {
        Glyph last = run.Glyphs.Peek();

        if ( !last.FixedWidth )
        {
            run.XAdvances[ run.XAdvances.Count - 1 ] = GetGlyphWidth( last, fontData );
        }
    }

    /// <summary>
    /// Returns the distance from the glyph's drawing position to the right edge of the glyph.
    /// </summary>
    private float GetGlyphWidth( Glyph glyph, BitmapFontData fontData )
    {
        return ( ( glyph.FixedWidth
            ? glyph.Xadvance
            : glyph.Width + glyph.Xoffset ) * fontData.ScaleX ) - fontData.PadRight;
    }

    /// <summary>
    /// Returns an X offset for the first glyph so when drawn, none of it is left of the line's drawing position.
    /// </summary>
    private float GetLineOffset( List< Glyph > glyphs, BitmapFontData fontData )
    {
        Glyph first = glyphs.First();

        return ( first.FixedWidth
            ? 0
            : -first.Xoffset * fontData.ScaleX ) - fontData.PadLeft;
    }

    /// <summary>
    /// Parses a color markup within the specified string and range.
    /// </summary>
    /// <param name="str"> The input string containing the markup. </param>
    /// <param name="start"> The start index of the markup. </param>
    /// <param name="end"> The end index of the markup. </param>
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
                // Parse hex color RRGGBBAA to an ABGR int, where AA
                // is optional and defaults to FF if omitted.
                var color = 0;

                for ( int i = start + 1; i < end; i++ )
                {
                    char ch = str[ i ];

                    if ( ch == ']' )
                    {
                        if ( i < start + 2 || i > start + 9 )
                        {
                            break; // Illegal number of hex digits.
                        }

                        if ( i - start < 8 )
                        {
                            color = ( color << ( ( 9 - ( i - start ) ) << 2 ) ) | 0xff; // RRGGBB or fewer chars.
                        }

                        _colorStack.Add( NumberUtils.ReverseBytes( color ) );

                        return i - start;
                    }

                    color = ( color << 4 ) + ch;

                    if ( ch is >= '0' and <= '9' )
                    {
                        color -= '0';
                    }
                    else if ( ch is >= 'A' and <= 'F' )
                    {
                        color -= 'A' - 10;
                    }
                    else if ( ch is >= 'a' and <= 'f' )
                    {
                        color -= 'a' - 10;
                    }
                    else
                    {
                        break; // Unexpected character in hex color.
                    }
                }

                return -1;

            case '[': // "[[" is an escaped left square bracket.
                return -2;

            case ']': // "[]" is a "pop" color tag.
                if ( _colorStack.Count > 1 )
                {
                    _colorStack.Pop();
                }

                return 0;

            default:
                break;
        }

        // Parse named color.
        for ( int i = start + 1; i < end; i++ )
        {
            char ch = str[ i ];

            if ( ch != ']' )
            {
                continue;
            }

            Color? color = Graphics.Colors.Get( str.Substring( start, i - start ) );

            if ( color == null )
            {
                return -1; // Unknown color name.
            }

            _colorStack.Add( ( int )Color.ToAbgr8888( color ) );

            return i - start;
        }

        return -1; // Unclosed color tag.
    }

    /// <summary>
    /// Resets the object for reuse. Object references should be nulled and fields
    /// may be set to default values.
    /// </summary>
    public void Reset()
    {
        _glyphRunPool.FreeAll( Runs );
        _colorStack.Clear();

        Runs.Clear();
        Colors.Clear();

        GlyphCount = 0;
        Width      = 0;
        Height     = 0;
    }

// ========================================================================
// ========================================================================

    /// <summary>
    /// Stores glyphs and positions for a piece of text which is a single color and
    /// does not span multiple lines.
    /// </summary>
    [PublicAPI]
    public class GlyphRun : IResetable, IPoolable
    {
        /// <summary>
        /// Contains glyphs.size+1 entries:
        /// The first entry is the X offset relative to the drawing position.
        /// Subsequent entries are the X advance relative to previous glyph position.
        /// The last entry is the width of the last glyph.
        /// </summary>
        public List< float > XAdvances { get; set; } = [ ];

        public List< Glyph > Glyphs { get; set; } = [ ];
        public float         X      { get; set; }
        public float         Y      { get; set; }
        public float         Width  { get; set; }
        public Color         Color  { get; set; } = new();

        // ====================================================================

        public void AppendRun( GlyphRun run )
        {
            Glyphs.AddRange( run.Glyphs );

            // Remove the width of the last glyph. The first xadvance of the
            // appended run has kerning for the last glyph of this run.
            if ( XAdvances.Count > 0 )
            {
                XAdvances.RemoveAt( XAdvances.Count - 1 );
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
                Glyph g = Glyphs[ i ];
                buffer.Append( ( char )g.ID );
            }

            buffer.Append( $", #{Color}, {X}, {Y}, {Width}" );

            return buffer.ToString();
        }
    }
}

// ============================================================================
// ============================================================================