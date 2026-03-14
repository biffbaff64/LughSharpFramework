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

using JetBrains.Annotations;

using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Graphics.Colors;

[PublicAPI]
public class NewColor4
{
    /// <summary>
    /// Red NewColor Component
    /// </summary>
    public byte R { get; set; }

    /// <summary>
    /// Green NewColor Component
    /// </summary>
    public byte G { get; set; }

    /// <summary>
    /// Blue NewColor Component
    /// </summary>
    public byte B { get; set; }

    /// <summary>
    /// Alpha NewColor Component
    /// </summary>
    public byte A { get; set; }

    /// <summary>
    /// NewColor Components packed into a <b>uint</b>, stored in <b>RGBA</b> format.
    /// </summary>
    public uint RGBAPackedColor { get; private set; }

    /// <summary>
    /// NewColor Components packed into a <b>uint</b>, stored in <b>ABGR</b> format.
    /// </summary>
    public uint ABGRPackedColor { get; private set; }

    // ========================================================================

    internal const int ARGBAlphaShift = 24;
    internal const int ARGBRedShift   = 16;
    internal const int ARGBGreenShift = 8;
    internal const int ARGBBlueShift  = 0;

    internal const int RGBARedShift   = 24;
    internal const int RGBAGreenShift = 16;
    internal const int RGBABlueShift  = 8;
    internal const int RGBAAlphaShift = 0;

    internal const uint RGBARedMask   = 0xff000000;
    internal const uint RGBAGreenMask = 0x00ff0000;
    internal const uint RGBABlueMask  = 0x0000ff00;
    internal const uint RGBAAlphaMask = 0x000000ff;

    // ========================================================================

    public NewColor4() : this( 0u )
    {
    }

    public NewColor4( ValidColor color )
    {
    }

    public NewColor4( uint rgba8888 )
    {
        R = ( byte )( ( rgba8888 & RGBARedMask ) >> RGBARedShift );
        G = ( byte )( ( rgba8888 & RGBAGreenMask ) >> RGBAGreenShift );
        B = ( byte )( ( rgba8888 & RGBABlueMask ) >> RGBABlueShift );
        A = ( byte )( ( rgba8888 & RGBAAlphaMask ) >> RGBAAlphaShift );

        Clamp();
    }

    public NewColor4( byte r, byte g, byte b, byte a )
    {
        R = r;
        G = g;
        B = b;
        A = a;

        Clamp();
    }

    public NewColor4( NewColor4 color )
        : this( color.R, color.G, color.B, color.A )
    {
    }

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <returns> This NewColor for chaining. </returns>
    private NewColor4 Clamp( bool showDebug = false )
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
        return ( uint )( ( r << RGBARedShift )
                       | ( g << RGBAGreenShift )
                       | ( b << RGBABlueShift ) | a );
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
        return ( uint )( ( a << ARGBAlphaShift )
                       | ( b << ARGBBlueShift )
                       | ( g << ARGBGreenShift )
                       | r );
    }

    /// <summary>
    /// Returns the given R.G.B colour components as a 32-bit uint in the following format:-
    /// <li>Bits  0 - 7  : Blue component</li>
    /// <li>Bits  8 - 15 : Green component</li>
    /// <li>Bits 16 - 23 : Red component</li>
    /// <li>Bits 24 - 31 : Undefined</li>
    /// </summary>
    /// <param name="r"> Red component </param>
    /// <param name="g"> Green component </param>
    /// <param name="b"> Blue component </param>
    public static uint Rgb888( byte r, byte g, byte b )
    {
        return ( uint )( ( r << 16 ) | ( g << 8 ) | b );
    }

    /// <summary>
    /// Returns the given <see cref="NewColor4"/> as a 32-bit uint in the following format:-
    /// <li>Bits  0 - 4  : Blue component</li>
    /// <li>Bits  5 - 10 : Green component</li>
    /// <li>Bits 11 - 15 : Red component</li>
    /// <li>Bits 16 - 31 : Undefined</li>
    /// </summary>
    /// <param name="r"> Red component </param>
    /// <param name="g"> Green component </param>
    /// <param name="b"> Blue component </param>
    public static uint ToRgb565( byte r, byte g, byte b )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns the given R.G.B colour components as a 32-bit uint in the following format:-
    /// <li>Bits  0 - 3  : Alpha component</li>
    /// <li>Bits  4 - 7  : Blue component</li>
    /// <li>Bits  8 - 11 : Green component</li>
    /// <li>Bits 12 - 15 : Red component</li>
    /// <li>Bits 16 - 31 : Undefined</li>
    /// </summary>
    /// <param name="r"> Red component </param>
    /// <param name="g"> Green component </param>
    /// <param name="b"> Blue component </param>
    /// <param name="a"> Alpha component </param>
    public static uint ToRgba4444( byte r, byte g, byte b, byte a )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sets this colors components using the components from the supplied color.
    /// </summary>
    /// <param name="color"> The NewColor to set. This cannot be null. </param>
    /// <returns> This NewColor for chaining. </returns>
    public NewColor4 Set( NewColor4 color )
    {
        R = color.R;
        G = color.G;
        B = color.B;
        A = color.A;

        return Clamp();
    }

    /// <summary>
    /// Sets this color's component values through an integer representation.
    /// </summary>
    /// <param name="rgba"> The integer representation. </param>
    /// <returns> This color for chaining. </returns>
    public NewColor4 Set( uint rgba )
    {
        R = ( byte )( ( rgba & RGBARedMask ) >> RGBARedShift );
        G = ( byte )( ( rgba & RGBAGreenMask ) >> RGBAGreenShift );
        B = ( byte )( ( rgba & RGBABlueMask ) >> RGBABlueShift );
        A = ( byte )( ( rgba & RGBAAlphaMask ) >> RGBAAlphaShift );

        return Clamp();
    }

    /// <summary>
    /// Sets this colors components using the supplied r,g,b,a components.
    /// </summary>
    /// <param name="r"> Red component </param>
    /// <param name="g"> Green component </param>
    /// <param name="b"> Blue component </param>
    /// <param name="a"> Alpha component </param>
    /// <returns> This NewColor for chaining. </returns>
    public NewColor4 Set( byte r, byte g, byte b, byte a )
    {
        R = r;
        G = g;
        B = b;
        A = a;

        return Clamp();
    }

    /// <summary>
    /// Adds the components from the supplied NewColor to the corresponding
    /// components of this color.
    /// </summary>
    /// <param name="color"> The NewColor to add. </param>
    /// <returns> This NewColor for chaining. </returns>
    public NewColor4 Add( NewColor4 color )
    {
        R += color.R;
        G += color.G;
        B += color.B;
        A += color.A;

        return Clamp();
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
        Maroon
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
        0xB03060FF  // Maroon
    };
}

// ============================================================================
// ============================================================================