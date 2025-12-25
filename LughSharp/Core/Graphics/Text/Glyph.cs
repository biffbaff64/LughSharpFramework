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

namespace LughSharp.Core.Graphics.Text;

/// <summary>
/// Represents a single character in a font page.
/// </summary>
[PublicAPI]
public class Glyph
{
    public bool       FixedWidth { get; set; }
    public int        Height     { get; set; }
    public int        ID         { get; set; }
    public byte[]?[]? Kerning    { get; set; }
    public int        SrcX       { get; set; }
    public int        SrcY       { get; set; }
    public float      U          { get; set; }
    public float      U2         { get; set; }
    public float      V          { get; set; }
    public float      V2         { get; set; }
    public int        Width      { get; set; }
    public int        Xadvance   { get; set; }
    public int        Xoffset    { get; set; }
    public int        Yoffset    { get; set; }

    /// <summary>
    /// The index to the texture page that holds this glyph.
    /// </summary>
    public int Page { get; set; }

    // ====================================================================

    /// <summary>
    /// Retrieves the kerning value for the specified character. Kerning adjusts the spacing
    /// between the current glyph and the specified character, helping to enhance the visual
    /// appearance of text rendering.
    /// </summary>
    /// <param name="ch">The character for which to retrieve the kerning value.</param>
    /// <returns>
    /// The kerning value for the specified character. Returns 0 if no kerning data is available.
    /// </returns>
    public int GetKerning( char ch )
    {
        if ( Kerning != null )
        {
            var page = Kerning[ ch >>> BitmapFont.LOG2_PAGE_SIZE ];

            return page != null ? page[ ch & ( BitmapFont.PAGE_SIZE - 1 ) ] : 0;
        }

        return 0;
    }

    /// <summary>
    /// Sets the kerning value for a specified character. This adjusts the spacing
    /// between the current glyph and the given character to achieve optimal visual appearance.
    /// </summary>
    /// <param name="ch">The Unicode value of the character for which kerning is to be set.</param>
    /// <param name="value">The kerning value to be applied, typically in pixel units.</param>
    public void SetKerning( int ch, int value )
    {
        Kerning ??= new byte[ BitmapFont.PAGES ][];

        var page = Kerning[ ch >>> BitmapFont.LOG2_PAGE_SIZE ];

        if ( page == null )
        {
            Kerning[ ch >>> BitmapFont.LOG2_PAGE_SIZE ] = page = new byte[ BitmapFont.PAGE_SIZE ];
        }

        page[ ch & ( BitmapFont.PAGE_SIZE - 1 ) ] = ( byte )value;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return ID.ToString();
    }
}

// ============================================================================
// ============================================================================