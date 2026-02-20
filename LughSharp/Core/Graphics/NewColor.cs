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

using JetBrains.Annotations;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Graphics;

[PublicAPI]
public class NewColor
{
    /// <summary>
    /// Red Color Component
    /// </summary>
    public byte R { get; set; }

    /// <summary>
    /// Green Color Component
    /// </summary>
    public byte G { get; set; }

    /// <summary>
    /// Blue Color Component
    /// </summary>
    public byte B { get; set; }

    /// <summary>
    /// Alpha Color Component
    /// </summary>
    public byte A { get; set; }

    /// <summary>
    /// Color Components packed into a <b>uint</b>, stored in RGBA format.
    /// </summary>
    public uint RGBAPackedColor { get; private set; }

    /// <summary>
    /// Color Components packed into a <b>uint</b>, stored in ABGR format.
    /// </summary>
    public uint ABGRPackedColor { get; private set; }

    // ========================================================================

    internal const int ARGB_ALPHA_SHIFT = 24;
    internal const int ARGB_RED_SHIFT   = 16;
    internal const int ARGB_GREEN_SHIFT = 8;
    internal const int ARGB_BLUE_SHIFT  = 0;
    
    internal const int RGBA_RED_SHIFT   = 24;
    internal const int RGBA_GREEN_SHIFT = 16;
    internal const int RGBA_BLUE_SHIFT  = 8;
    internal const int RGBA_ALPHA_SHIFT = 0;
    
    // ========================================================================

    internal NewColor()
    {
    }

    private NewColor( ValidColor color )
    {
    }

    public uint ARGB8888 => ( uint )( ( A << 24 ) | ( R << 16 ) | ( G << 8 ) | B );
    public uint RGBA8888 => ( uint )( ( R << 24 ) | ( G << 16 ) | ( B << 8 ) | A );

    /// <summary>
    /// Clamps this Colors RGBA components to a valid range [0 - 1]
    /// </summary>
    /// <returns> This Color for chaining. </returns>
    private NewColor Clamp( bool showDebug = false )
    {
        RGBAPackedColor = ToRgba8888( R, G, B, A );
        ABGRPackedColor = ToAbgr8888( A, B, G, R );

        if ( showDebug )
        {
            Logger.Debug( $"R: {R}, G: {G}, B: {B}, A: {A}" );
            Logger.Debug( $"RGBAPackedColor: {RGBAPackedColor}, {RGBAPackedColor:X8}" );
            Logger.Debug( $"ABGRPackedColor: {ABGRPackedColor}, {ABGRPackedColor:X8}" );
        }

        return this;
    }

    /// <summary>
    /// Converts the specified RGBA color components into a packed uint representation
    /// in RGBA8888 format.
    /// </summary>
    /// <param name="r">The red component of the color.</param>
    /// <param name="g">The green component of the color.</param>
    /// <param name="b">The blue component of the color.</param>
    /// <param name="a">The alpha (transparency) component of the color.</param>
    /// <returns>A uint value representing the color in RGBA8888 format.</returns>
    public uint ToRgba8888( byte r, byte g, byte b, byte a )
    {
        return ( uint )( ( r << RGBA_RED_SHIFT ) | ( g << RGBA_GREEN_SHIFT ) | ( b << RGBA_BLUE_SHIFT ) | a );
    }

    /// <summary>
    /// Converts the specified ABGR color components into a packed uint representation
    /// in ABGR8888 format.
    /// </summary>
    /// <param name="r">The red component of the color.</param>
    /// <param name="g">The green component of the color.</param>
    /// <param name="b">The blue component of the color.</param>
    /// <param name="a">The alpha (transparency) component of the color.</param>
    /// <returns>A uint value representing the color in ABGR8888 format.</returns>
    public uint ToAbgr8888( byte a, byte b, byte g, byte r )
    {
        return ( uint )( ( a << ARGB_ALPHA_SHIFT ) | ( b << ARGB_BLUE_SHIFT ) | ( g << ARGB_GREEN_SHIFT ) | r );
    }

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public enum ValidColor
    {
        Red,
        Green,
        Blue,
        Clear,
        White,
        Black,
        Gray,
        Lightgray,
        Darkgray,
        Slate,
        Navy,
        Royal,
        Sky,
        Cyan,
        Teal,
        Chartreuse,
        Lime,
        Forest,
        Olive,
        Yellow,
        Gold,
        Goldenrod,
        Orange,
        Brown,
        Tan,
        Firebrick,
        Scarlet,
        Coral,
        Salmon,
        Pink,
        Magenta,
        Purple,
        Violet,
        Maroon,
    };

    // ========================================================================

    private static readonly uint[] _validColorTable =
    {
        //RRGGBBAA
        0xFF0000FF, // Red
        0x00FF00FF, // Green
        0x0000FF00, // Blue
        0x00000000, // Clear
        0xFFFFFFFF, // White
        0x000000FF, // Black
        0x7F7F7FFF, // Gray
        0xBFBFBFFF, // Lightgray
        0x3F3F3FFF, // Darkgray
        0x708090FF, // Slate
        0x000080FF, // Navy
        0x4169E1FF, // Royal
        0x87CEEBFF, // Sky
        0x00FFFFFF, // Cyan
        0x007F7FFF, // Teal
        0x7FFF00FF, // Chartreuse
        0x32CD32FF, // Lime
        0x228B22FF, // Forest
        0x6B8E23FF, // Olive
        0xFFFF00FF, // Yellow
        0xFFD700FF, // Gold
        0xDAA520FF, // Goldenrod
        0xFFA500FF, // Orange
        0x8B4513FF, // Brown
        0xD2B48CFF, // Tan
        0xB22222FF, // Firebrick
        0xFF341CFF, // Scarlet
        0xFF7F50FF, // Coral
        0xFA8072FF, // Salmon
        0xFF69B4FF, // Pink
        0xFF00FFFF, // Magenta
        0xA020F0FF, // Purple
        0xEE82EEFF, // Violet
        0xB03060FF, // Maroon
    };
}

// ============================================================================
// ============================================================================