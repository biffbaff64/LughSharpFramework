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

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using LughSharp.Core.Maths;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Graphics;

/// <summary>
/// A color class, holding the r, g, b and alpha component as floats in the range [0,1].
/// All methods perform clamping on the internal values after execution.
/// </summary>
[PublicAPI]
public class Color : ICloneable, IEquatable< Color >
{
    #region color definitions

    public static readonly Color Red        = new( 0xff, 0x00, 0x00, 0xff, "RED" );
    public static readonly Color Green      = new( 0x00, 0xff, 0x00, 0xff, "GREEN" );
    public static readonly Color Blue       = new( 0x00, 0x00, 0xff, 0x00, "BLUE" );
    public static readonly Color Clear      = new( 0x00, 0x00, 0x00, 0x00, "CLEAR" );
    public static readonly Color White      = new( 0xff, 0xff, 0xff, 0xff, "WHITE" );
    public static readonly Color Black      = new( 0x00, 0x00, 0x00, 0xff, "BLACK" );
    public static readonly Color Gray       = new( 0x7f, 0x7f, 0x7f, 0xff, "GRAY" );
    public static readonly Color LightGray  = new( 0xbf, 0xbf, 0xbf, 0xff, "LIGHTGRAY" );
    public static readonly Color DarkGray   = new( 0x3f, 0x3f, 0x3f, 0xff, "DARKGRAY" );
    public static readonly Color Slate      = new( 0x70, 0x80, 0x90, 0xff, "SLATE" );
    public static readonly Color Navy       = new( 0x00, 0x00, 0x80, 0xff, "NAVY" );
    public static readonly Color Royal      = new( 0x41, 0x69, 0xe1, 0xff, "ROYAL" );
    public static readonly Color Sky        = new( 0x87, 0xce, 0xeb, 0xff, "SKY" );
    public static readonly Color Cyan       = new( 0x00, 0xff, 0xff, 0xff, "CYAN" );
    public static readonly Color Teal       = new( 0x00, 0x7f, 0x7f, 0xff, "TEAL" );
    public static readonly Color Chartreuse = new( 0x7f, 0xff, 0x00, 0xff, "CHARTREUSE" );
    public static readonly Color Lime       = new( 0x32, 0xcd, 0x32, 0xff, "LIME" );
    public static readonly Color Forest     = new( 0x22, 0x8b, 0x22, 0xff, "FOREST" );
    public static readonly Color Olive      = new( 0x6b, 0x8e, 0x23, 0xff, "OLIVE" );
    public static readonly Color Yellow     = new( 0xff, 0xff, 0x00, 0xff, "YELLOW" );
    public static readonly Color Gold       = new( 0xff, 0xd7, 0x00, 0xff, "GOLD" );
    public static readonly Color Goldenrod  = new( 0xda, 0xa5, 0x20, 0xff, "GOLDENROD" );
    public static readonly Color Orange     = new( 0xff, 0xa5, 0x00, 0xff, "ORANGE" );
    public static readonly Color Brown      = new( 0x8b, 0x45, 0x13, 0xff, "BROWN" );
    public static readonly Color Tan        = new( 0xd2, 0xb4, 0x8c, 0xff, "TAN" );
    public static readonly Color Firebrick  = new( 0xb2, 0x22, 0x22, 0xff, "FIREBRICK" );
    public static readonly Color Scarlet    = new( 0xff, 0x34, 0x1c, 0xff, "SCARLET" );
    public static readonly Color Coral      = new( 0xff, 0x7f, 0x50, 0xff, "CORAL" );
    public static readonly Color Salmon     = new( 0xfa, 0x80, 0x72, 0xff, "SALMON" );
    public static readonly Color Pink       = new( 0xff, 0x69, 0xb4, 0xff, "PINK" );
    public static readonly Color Magenta    = new( 0xff, 0x00, 0xff, 0xff, "MAGENTA" );
    public static readonly Color Purple     = new( 0xa0, 0x20, 0xf0, 0xff, "PURPLE" );
    public static readonly Color Violet     = new( 0xee, 0x82, 0xee, 0xff, "VIOLET" );
    public static readonly Color Maroon     = new( 0xb0, 0x30, 0x60, 0xff, "MAROON" );

    /// <summary>
    /// Convenience for frequently used <tt>White.ToFloatBits()</tt>
    /// </summary>
    public static float WhiteFloatBits => White.ToFloatBitsAbgr();

    #endregion color definitions

    public string Name { get; set; }

    // ========================================================================
    // ========================================================================

    private static Color _color = new();

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Red Color Component
    /// </summary>
    public float R { get; set; }

    /// <summary>
    /// Green Color Component
    /// </summary>
    public float G { get; set; }

    /// <summary>
    /// Blue Color Component
    /// </summary>
    public float B { get; set; }

    /// <summary>
    /// Alpha Color Component
    /// </summary>
    public float A { get; set; }

    /// <summary>
    /// Color Components packed into a <b>uint</b>, stored in RGBA format.
    /// </summary>
    public uint RGBAPackedColor { get; private set; }

    /// <summary>
    /// Color Components packed into a <b>uint</b>, stored in ABGR format.
    /// </summary>
    public uint ABGRPackedColor { get; private set; }

    /// <summary>
    /// Color Components packed into a <b>float</b>, stored in RGBA format.
    /// </summary>
    public double RGBAFloatPack { get; private set; }

    /// <summary>
    /// Color Components packed into a <b>float</b>, stored in ABGR format.
    /// </summary>
    public double ABGRFloatPack { get; private set; }

    // ========================================================================
    // ========================================================================

    public Color() : this( 0, 0, 0, 0 )
    {
    }
    
    /// <summary>
    /// Constructor, sets all the components to 0.
    /// </summary>
    public Color( string name = "" ) : this( 0, 0, 0, 0, name )
    {
    }

    /// <summary>
    /// Constructor, sets the Color components using the specified integer value in
    /// the format RGBA8888.
    /// </summary>
    /// <param name="rgba8888"> A uint color value in RGBA8888 format. </param>
    /// <param name="name"></param>
    public Color( uint rgba8888, string name = "" )
    {
        R = ( ( rgba8888 & 0xff000000 ) >> 24 ) / 255.0f;
        G = ( ( rgba8888 & 0x00ff0000 ) >> 16 ) / 255.0f;
        B = ( ( rgba8888 & 0x0000ff00 ) >> 8 ) / 255.0f;
        A = ( rgba8888 & 0x000000ff ) / 255.0f;

        Clamp();

        Name = name;
    }

    /// <summary>
    /// Constructor, sets the components of the color.
    /// </summary>
    /// <param name="r"> Red component </param>
    /// <param name="g"> Green component </param>
    /// <param name="b"> Blue component </param>
    /// <param name="a"> Alpha component </param>
    /// <param name="name"></param>
    public Color( float r, float g, float b, float a, string name = "" )
    {
        R = r;
        G = g;
        B = b;
        A = a;

        Clamp();

        Name = name;
    }

    /// <summary>
    /// Constructor, sets the components of the color.
    /// </summary>
    /// <param name="r"> Red component </param>
    /// <param name="g"> Green component </param>
    /// <param name="b"> Blue component </param>
    /// <param name="a"> Alpha component </param>
    /// <param name="name"></param>
    public Color( int r, int g, int b, int a, string name = "" )
    {
        R = r / 255.0f;
        G = g / 255.0f;
        B = b / 255.0f;
        A = a / 255.0f;

        Clamp();

        Name = name;
    }

    /// <summary>
    /// Constructs a new color using the components from the supplied color.
    /// </summary>
    public Color( Color color )
        : this( color.R, color.G, color.B, color.A )
    {
    }

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region float bits conversion

    /// <summary>
    /// Packs the color components into a 32-bit integer with the format ABGR and then converts it
    /// to a float. Alpha is compressed from 0-255 to use only even numbers between 0-254 to avoid
    /// using float bits in the NaN range.
    /// <para>
    /// Note: Converting a color to a float and back can be lossy for alpha.
    /// </para>
    /// </summary>
    /// <returns> The resulting float. </returns>
    /// <seealso cref="NumberUtils.UIntToFloatColor"/>
    public float ToFloatBitsRgba()
    {
        return ToFloatBitsRgba( R, G, B, A );
    }

    /// <summary>
    /// Packs the color components into a 32-bit integer with the format ABGR and then converts it
    /// to a float. Alpha is compressed from 0-255 to use only even numbers between 0-254 to avoid
    /// using float bits in the NaN range.
    /// <para>
    /// Note: Converting a color to a float and back can be lossy for alpha.
    /// </para>
    /// </summary>
    /// <param name="r"> Red component </param>
    /// <param name="g"> Green component </param>
    /// <param name="b"> Blue component </param>
    /// <param name="a"> Alpha component </param>
    /// <returns></returns>
    public static float ToFloatBitsRgba( float r, float g, float b, float a )
    {
        var floatBits = ( ( uint )( 255f * r ) << 24 )
                      | ( ( uint )( 255f * g ) << 16 )
                      | ( ( uint )( 255f * b ) << 8 )
                      | ( ( uint )( 255f * a ) );

        return floatBits;
    }

    /// <summary>
    /// Packs the color components into a 32-bit integer with the format ABGR and then
    /// converts it to a float. Alpha is compressed from 0-255 to use only even numbers
    /// between 0-254 to avoid using float bits in the NaN range.
    /// <para>
    /// Note: Converting a color to a float and back can be lossy for alpha.
    /// </para>
    /// </summary>
    /// <returns> The resulting float. </returns>
    /// <seealso cref="NumberUtils.UIntToFloatColor(uint)"/>
    public float ToFloatBitsAbgr()
    {
        return ToFloatBitsAbgr( A, B, G, R );
    }

    /// <summary>
    /// Packs the color components into a 32-bit integer with the format ABGR and then
    /// converts it to a float. Alpha is compressed from 0-255 to use only even numbers
    /// between 0-254 to avoid using float bits in the NaN range.
    /// <para>
    /// Note: Converting a color to a float and back can be lossy for alpha.
    /// </para>
    /// </summary>
    /// <param name="r"> Red component </param>
    /// <param name="g"> Green component </param>
    /// <param name="b"> Blue component </param>
    /// <param name="a"> Alpha component </param>
    /// <returns></returns>
    public static float ToFloatBitsAbgr( float a, float b, float g, float r )
    {
        var floatBits = ( ( uint )( a * 255f ) << 24 )
                      | ( ( uint )( b * 255f ) << 16 )
                      | ( ( uint )( g * 255f ) << 8 )
                      | ( ( uint )( r * 255f ) );

        return floatBits;
    }

    #endregion float bits conversion

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region packed colors

    /// <summary>
    /// Packs the color components into a 32-bit integer with the format RGBA.
    /// </summary>
    /// <returns> the packed color as a 32-bit int. </returns>
    public uint PackedColorRgba()
    {
        return ( ( uint )( 255f * R ) << 24 )
             | ( ( uint )( 255f * G ) << 16 )
             | ( ( uint )( 255f * B ) << 8 )
             | ( uint )( 255f * A );
    }

    /// <summary>
    /// Packs the color components into a 32-bit integer with the format ABGR.
    /// Note that no range checking is performed for higher performance.
    /// </summary>
    /// <param name="r"> Red component </param>
    /// <param name="g"> Green component </param>
    /// <param name="b"> Blue component </param>
    /// <param name="a"> Alpha component </param>
    /// <returns> the packed color as a 32-bit int. </returns>
    public static uint PackedColorAbgr( uint a, uint b, uint g, uint r )
    {
        return ( a << 24 ) | ( b << 16 ) | ( g << 8 ) | r;
    }

    /// <summary>
    /// Packs the color components into a 32-bit integer with the format ABGR.
    /// </summary>
    /// <returns> the packed color as a 32-bit int. </returns>
    public uint PackedColorAbgr()
    {
        return ( ( uint )( 255f * A ) << 24 )
             | ( ( uint )( 255f * B ) << 16 )
             | ( ( uint )( 255f * G ) << 8 )
             | ( uint )( 255f * R );
    }

    #endregion packed colors

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Component to color methods

    /// <summary>
    /// Converts the supplied color components to an <b>uint</b>.
    /// </summary>
    /// <param name="r"> Red component. </param>
    /// <param name="g"> Green component. </param>
    /// <param name="b"> Blue component. </param>
    /// <param name="a"> Alpha component. </param>
    /// <returns></returns>
    public static uint Rgba8888ToUInt( float r, float g, float b, float a )
    {
        return ( ( uint )( 255f * r ) << 24 )
             | ( ( uint )( 255f * g ) << 16 )
             | ( ( uint )( 255f * b ) << 8 )
             | ( uint )( 255f * a );
    }

    /// <summary>
    /// Returns the given <see cref="Color"/> as a 32-bit uint in the following format:-
    /// <li>Bits  0 - 4  : Blue component</li>
    /// <li>Bits  5 - 10 : Green component</li>
    /// <li>Bits 11 - 15 : Red component</li>
    /// <li>Bits 16 - 31 : Undefined</li>
    /// </summary>
    /// <param name="r"> Red component </param>
    /// <param name="g"> Green component </param>
    /// <param name="b"> Blue component </param>
    public static uint Rgb565( float r, float g, float b )
    {
        return ( ( uint )( r * 31 ) << 11 ) | ( ( uint )( g * 63 ) << 5 ) | ( uint )( b * 31 );
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
    public static uint Rgba4444( float r, float g, float b, float a )
    {
        return ( ( uint )( r * 15 ) << 12 )
             | ( ( uint )( g * 15 ) << 8 )
             | ( ( uint )( b * 15 ) << 4 )
             | ( uint )( a * 15 );
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
    public static uint Rgb888( float r, float g, float b )
    {
        return ( ( uint )( r * 255 ) << 16 ) | ( ( uint )( g * 255 ) << 8 ) | ( uint )( b * 255 );
    }

    /// <summary>
    /// Returns the given <see cref="Color"/> as a 32-bit uint in the
    /// following format:-
    /// <li>Bits  0 - 4  : Blue component</li>
    /// <li>Bits  5 - 10 : Green component</li>
    /// <li>Bits 11 - 15 : Red component</li>
    /// <li>Bits 16 - 31 : Undefined</li>
    /// </summary>
    /// <param name="color"> The colour. </param>
    public static uint Rgb565( Color color )
    {
        return ( ( uint )( color.R * 31 ) << 11 )
             | ( ( uint )( color.G * 63 ) << 5 )
             | ( uint )( color.B * 31 );
    }

    /// <summary>
    /// Returns the given <see cref="Color"/> as a 16-bit uint in the
    /// following format:-
    /// <li>Bits  0 - 3  : Alpha component</li>
    /// <li>Bits  4 - 7  : Blue component</li>
    /// <li>Bits  8 - 11 : Green component</li>
    /// <li>Bits 12 - 15 : Red component</li>
    /// <li>Bits 16 - 31 : Undefined</li>
    /// </summary>
    /// <param name="color"> The colour. </param>
    public static uint Rgba4444( Color color )
    {
        return ( ( uint )( color.R * 15 ) << 12 )
             | ( ( uint )( color.G * 15 ) << 8 )
             | ( ( uint )( color.B * 15 ) << 4 )
             | ( uint )( color.A * 15 );
    }

    /// <summary>
    /// Returns the given <see cref="Color"/> as a 32-bit uint in the
    /// following format:-
    /// <li>Bits  0 - 7  : Blue component</li>
    /// <li>Bits  8 - 15 : Green component</li>
    /// <li>Bits 16 - 23 : Red component</li>
    /// <li>Bits 24 - 31 : Undefined</li>
    /// </summary>
    /// <param name="color"> The colour. </param>
    public static uint Rgb888( Color color )
    {
        return ( ( uint )( color.R * 255 ) << 16 )
             | ( ( uint )( color.G * 255 ) << 8 )
             | ( uint )( color.B * 255 );
    }

    /// <summary>
    /// Converts a 32-bit ARGB8888 integer value to a Color object.
    /// </summary>
    /// <param name="color"> The Color object to assign the converted values to. </param>
    /// <param name="value"> The 32-bit ARGB8888 integer value. </param>
    public static void Argb8888ToColor( ref Color color, uint value )
    {
        color.A = ( ( value & 0xff000000 ) >>> 24 ) / 255f;
        color.R = ( ( value & 0x00ff0000 ) >>> 16 ) / 255f;
        color.G = ( ( value & 0x0000ff00 ) >>> 8 ) / 255f;
        color.B = ( value & 0x000000ff ) / 255f;
    }

    public static Color FromArgb( float a, float r, float g, float b ) => new( r, g, b, a );

    public static Color FromRgba( float r, float g, float b, float a ) => new( r, g, b, a );

    public static Color FromRgb( float r, float g, float b ) => new( r, g, b, 1.0f );

    /// <summary>
    /// Converts a 32-bit RGBA8888 integer value to a Color object.
    /// </summary>
    /// <param name="color"> The Color object to assign the converted values to. </param>
    /// <param name="value"> The 32-bit RGBA8888 integer value. </param>
    public static void Rgba8888ToColor( ref Color color, uint value )
    {
        color.R = ( ( value & 0xff000000 ) >>> 24 ) / 255f;
        color.G = ( ( value & 0x00ff0000 ) >>> 16 ) / 255f;
        color.B = ( ( value & 0x0000ff00 ) >>> 8 ) / 255f;
        color.A = ( value & 0x000000ff ) / 255f;
    }

    /// <summary>
    /// Converts a 16-bit RGB565 integer value to a Color object.
    /// </summary>
    /// <param name="color"> The Color object to assign the converted values to. </param>
    /// <param name="value"> The 16-bit RGB565 integer value. </param>
    public static void Rgb565ToColor( ref Color color, uint value )
    {
        // Ensure the value is within the valid range for 16-bit RGB565
        if ( value > 0xFFFF )
        {
            throw new ArgumentOutOfRangeException( nameof( value ),
                                                   "Value must be a 16-bit integer." );
        }

        color.R = ( ( value & 0xF800 ) >> 11 ) / 31f;
        color.G = ( ( value & 0x07E0 ) >> 5 ) / 63f;
        color.B = ( value & 0x001F ) / 31f;
    }

    /// <summary>
    /// Converts a 16-bit RGBA4444 integer value to a Color object.
    /// </summary>
    /// <param name="color"> The Color object to assign the converted values to. </param>
    /// <param name="value"> The 16-bit RGBA4444 integer value. </param>
    public static void Rgba4444ToColor( ref Color color, uint value )
    {
        color.R = ( ( value & 0xF000 ) >> 12 ) / 15f;
        color.G = ( ( value & 0x0F00 ) >> 8 ) / 15f;
        color.B = ( ( value & 0x00F0 ) >> 4 ) / 15f;
        color.A = ( value & 0x000F ) / 15f;
    }

    /// <summary>
    /// Sets the Color components using the specified float value in the format ABGR8888.
    /// </summary>
    /// <param name="color">The Color object to assign the converted values to.</param>
    /// <param name="value">The float value representing the color in ABGR8888 format.</param>
    public static void Abgr8888ToColor( ref Color color, float value )
    {
        // Convert the float value to an integer representing the color
        var c = NumberUtils.FloatToIntColor( value );

        // Extract and assign color components using bitwise operations
        color.A = ( ( c & 0xff000000 ) >>> 24 ) / 255f;
        color.B = ( ( c & 0x00ff0000 ) >>> 16 ) / 255f;
        color.G = ( ( c & 0x0000ff00 ) >>> 8 ) / 255f;
        color.R = ( c & 0x000000ff ) / 255f;
    }

    #endregion Component to color methods

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region To format methods

    /// <summary>
    /// Returns the given <see cref="Color"/> as a 32-bit uint in the
    /// following format:-
    /// <li>Bits  0 - 7  : Alpha component</li>
    /// <li>Bits  8 - 15 : Blue component</li>
    /// <li>Bits 16 - 23 : Green component</li>
    /// <li>Bits 24 - 31 : Red component</li>
    /// </summary>
    /// <param name="color"> The colour. </param>
    public static uint ToRgba8888( Color color )
    {
        return ( ( uint )( color.R * 255 ) << 24 )
             | ( ( uint )( color.G * 255 ) << 16 )
             | ( ( uint )( color.B * 255 ) << 8 )
             | ( uint )( color.A * 255 );
    }

    /// <summary>
    /// Returns the given seperate colour components as a 32-bit uint in the
    /// following format:-
    /// <li>Bits  0 - 7  : Alpha component</li>
    /// <li>Bits  8 - 15 : Blue component</li>
    /// <li>Bits 16 - 23 : Green component</li>
    /// <li>Bits 24 - 31 : Red component</li>
    /// </summary>
    /// <param name="r"> Red component </param>
    /// <param name="g"> Green component </param>
    /// <param name="b"> Blue component </param>
    /// <param name="a"> Alpha component </param>
    public static uint ToRgba8888( float r, float g, float b, float a )
    {
        return ( ( uint )( r * 255 ) << 24 )
             | ( ( uint )( g * 255 ) << 16 )
             | ( ( uint )( b * 255 ) << 8 )
             | ( uint )( a * 255 );
    }

    /// <summary>
    /// Returns the given seperate colour components as a 32-bit uint in the
    /// following format:-
    /// <li>Bits  0 - 7  : Alpha component</li>
    /// <li>Bits  8 - 15 : Blue component</li>
    /// <li>Bits 16 - 23 : Green component</li>
    /// <li>Bits 24 - 31 : Red component</li>
    /// </summary>
    /// <param name="r"> Red component </param>
    /// <param name="g"> Green component </param>
    /// <param name="b"> Blue component </param>
    /// <param name="a"> Alpha component </param>
    public static uint ToAbgr8888( float a, float b, float g, float r )
    {
        return ( ( uint )( a * 255 ) << 24 )
             | ( ( uint )( b * 255 ) << 16 )
             | ( ( uint )( g * 255 ) << 8 )
             | ( uint )( r * 255 );
    }

    /// <summary>
    /// Packs the color components into a 32-bit integer with the format ABGR.
    /// </summary>
    /// <returns> the packed color as a 32-bit int. </returns> 
    public uint ToAbgr8888()
    {
        return ( ( uint )( A * 255 ) << 24 )
             | ( ( uint )( B * 255 ) << 16 )
             | ( ( uint )( G * 255 ) << 8 )
             | ( uint )( R * 255 );
    }

    /// <summary>
    /// Returns the given seperate colour components as a 32-bit uint in the
    /// following format:-
    /// <li> Bits  0 - 7  : Blue component </li>
    /// <li> Bits  8 - 15 : Green component </li>
    /// <li> Bits 16 - 23 : Red component </li>
    /// <li> Bits 24 - 31 : Alpha component </li>
    /// </summary>
    /// <param name="a"> Alpha component </param>
    /// <param name="b"> Blue component </param>
    /// <param name="g"> Green component </param>
    /// <param name="r"> Red component </param>
    public static uint ToArgb8888( float a, float r, float g, float b )
    {
        return ( ( uint )( a * 255 ) << 24 )
             | ( ( uint )( r * 255 ) << 16 )
             | ( ( uint )( g * 255 ) << 8 )
             | ( uint )( b * 255 );
    }

    /// <summary>
    /// Returns the given <see cref="Color"/> as a 32-bit uint in the
    /// following format:-
    /// <li>Bits  0 - 7  : Blue component</li>
    /// <li>Bits  8 - 15 : Green component</li>
    /// <li>Bits 16 - 23 : Red component</li>
    /// <li>Bits 24 - 31 : Alpha component</li>
    /// </summary>
    /// <param name="color"> The colour. </param>
    public static uint ToArgb8888( Color color )
    {
        return ( ( uint )( color.A * 255 ) << 24 )
             | ( ( uint )( color.R * 255 ) << 16 )
             | ( ( uint )( color.G * 255 ) << 8 )
             | ( uint )( color.B * 255 );
    }

    /// <summary>
    /// Returns the given <see cref="Color"/> as a 32-bit uint in the
    /// following format:-
    /// <li>Bits  0 - 7  : Red component</li>
    /// <li>Bits  8 - 15 : Green component</li>
    /// <li>Bits 16 - 23 : Blue component</li>
    /// <li>Bits 24 - 31 : Alpha component</li>
    /// </summary>
    /// <param name="color"> The colour. </param>
    public static uint ToAbgr8888( Color color )
    {
        return ( ( uint )( color.A * 255f ) << 24 )
             | ( ( uint )( color.B * 255f ) << 16 )
             | ( ( uint )( color.G * 255f ) << 8 )
             | ( uint )( color.R * 255f );
    }

    #endregion To format methods

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region HSV methods

    /// <summary>
    /// Sets the RGB Color components using the specified Hue-Saturation-Value.
    /// Note that HSV components are voluntary not clamped to preserve high range
    /// color and can range beyond typical values.
    /// </summary>
    /// <param name="h">The Hue in degree from 0 to 360</param>
    /// <param name="s">The Saturation from 0 to 1</param>
    /// <param name="v">The Value (brightness) from 0 to 1</param>
    /// <returns>The modified Color for chaining.</returns>
    public Color FromHsv( float h, float s, float v )
    {
        h %= 360; // Ensure hue is in the range [0, 360]

        if ( h < 0 )
        {
            h += 360;
        }

        var i = ( uint )( h / 60 ) % 6;
        var f = ( h / 60 ) - i;
        var p = v * ( 1 - s );
        var q = v * ( 1 - ( s * f ) );
        var t = v * ( 1 - ( s * ( 1 - f ) ) );

        ( R, G, B ) = i switch
                      {
                          0     => ( v, t, p ),
                          1     => ( q, v, p ),
                          2     => ( p, v, t ),
                          3     => ( p, q, v ),
                          4     => ( t, p, v ),
                          var _ => ( v, p, q ),
                      };

        return Clamp();
    }

    /// <summary>
    /// Sets RGB components using the specified Hue-Saturation-Value. This is a
    /// convenient method for fromHsv(float, float, float). This is the inverse
    /// of toHsv(float[]).
    /// </summary>
    /// <param name="hsv"> The Hue-Saturation-Value. </param>
    /// <returns> The modified color for chaining. </returns>
    public Color FromHsv( float[] hsv )
    {
        return FromHsv( hsv[ 0 ], hsv[ 1 ], hsv[ 2 ] );
    }

    /// <summary>
    /// Converts the RGB color values to HSV and stores the result in the provided array.
    /// </summary>
    /// <param name="hsv">An array of at least 3 elements where the HSV values will be stored.</param>
    /// <returns>The array with HSV values.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the provided array does not have at least 3 elements.
    /// </exception>
    public float[] ToHsv( float[] hsv )
    {
        if ( hsv.Length < 3 )
        {
            throw new ArgumentException( "The hsv array must have at least 3 elements.", nameof( hsv ) );
        }

        var max   = Math.Max( Math.Max( R, G ), B );
        var min   = Math.Min( Math.Min( R, G ), B );
        var range = max - min;

        // Hue calculation
        if ( Math.Abs( range ) < NumberUtils.FLOAT_TOLERANCE )
        {
            hsv[ 0 ] = 0; // Undefined hue, achromatic case
        }
        else if ( Math.Abs( max - R ) < NumberUtils.FLOAT_TOLERANCE )
        {
            hsv[ 0 ] = ( ( ( 60 * ( G - B ) ) / range ) + 360 ) % 360;
        }
        else if ( Math.Abs( max - G ) < NumberUtils.FLOAT_TOLERANCE )
        {
            hsv[ 0 ] = ( ( ( 60 * ( B - R ) ) / range ) + 120 ) % 360;
        }
        else // max == B
        {
            hsv[ 0 ] = ( ( ( 60 * ( R - G ) ) / range ) + 240 ) % 360;
        }

        // Saturation calculation
        hsv[ 1 ] = max > 0 ? range / max : 0;

        // Value calculation
        hsv[ 2 ] = max;

        return hsv;
    }

    #endregion HSV methods

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region various manipulation methods

    /// <summary>
    /// Sets this colors components using the components from the supplied color.
    /// </summary>
    /// <returns> This Color for chaining. </returns>
    public Color Set( Color color )
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
    public Color Set( uint rgba )
    {
        var color = this;

        Rgba8888ToColor( ref color, rgba );

        R = color.R;
        G = color.G;
        B = color.B;
        A = color.A;

        return Clamp();
    }

    /// <summary>
    /// Sets this colors components using the supplied r,g,b,a components.
    /// </summary>
    /// <param name="r"> Red component </param>
    /// <param name="g"> Green component </param>
    /// <param name="b"> Blue component </param>
    /// <param name="a"> Alpha component </param>
    /// <returns> This Color for chaining. </returns>
    public Color Set( float r, float g, float b, float a )
    {
        R = r;
        G = g;
        B = b;
        A = a;

        return Clamp();
    }

    /// <summary>
    /// Multiplies each of this color objects components by the corresponding components
    /// in the supplied Color.
    /// </summary>
    /// <param name="color"> The supplied color. </param>
    /// <returns> This Color for chaining. </returns>
    public Color Mul( Color color )
    {
        R *= color.R;
        G *= color.G;
        B *= color.B;
        A *= color.A;

        return Clamp();
    }

    /// <summary>
    /// Multiplies the components of this Color object by the components
    /// of the supplied Color object and returns the result as a NEW Color
    /// object.
    /// </summary>
    public Color MulNew( Color color )
    {
        return new Color( R * color.R,
                          G * color.G,
                          B * color.B,
                          A * color.A ).Clamp();
    }

    /// <summary>
    /// Multiplies the colour components by the supplied value.
    /// </summary>
    /// <returns> This Color for chaining. </returns>
    public Color Mul( float value )
    {
        R *= value;
        G *= value;
        B *= value;
        A *= value;

        return Clamp();
    }

    /// <summary>
    /// Multiplies each of this color objects components by the corresponding supplied components.
    /// </summary>
    /// <param name="r"> Red component </param>
    /// <param name="g"> Green component </param>
    /// <param name="b"> Blue component </param>
    /// <param name="a"> Alpha component </param>
    /// <returns>This Color for chaining.</returns>
    public Color Mul( float r, float g, float b, float a )
    {
        R *= r;
        G *= g;
        B *= b;
        A *= a;

        return Clamp();
    }

    /// <summary>
    /// Adds the components from the supplied Color to the corresponding
    /// components of this color.
    /// </summary>
    /// <param name="color"> The Color to add. </param>
    /// <returns> This Color for chaining. </returns>
    public Color Add( Color color )
    {
        R += color.R;
        G += color.G;
        B += color.B;
        A += color.A;

        return Clamp();
    }

    /// <summary>
    /// Adds the components of the supplied Color object to the components
    /// of this Color object and returns the result as a NEW Color object.
    /// </summary>
    public Color AddNew( Color color )
    {
        return new Color( R + color.R,
                          G + color.G,
                          B + color.B,
                          A + color.A ).Clamp();
    }

    /// <summary>
    /// Adds the supplied Color components to the corresponding components of this Color.
    /// </summary>
    /// <param name="r"> Red component </param>
    /// <param name="g"> Green component </param>
    /// <param name="b"> Blue component </param>
    /// <param name="a"> Alpha component </param>
    /// <returns> This Color for chaining. </returns>
    public Color Add( float r, float g, float b, float a )
    {
        R += r;
        G += g;
        B += b;
        A += a;

        return Clamp();
    }

    /// <summary>
    /// Subtracts the elements in the supplied Color from the equivalent
    /// elements in this Color.
    /// </summary>
    /// <param name="color"> The color to subtract. </param>
    /// <returns> This Color for chaining. </returns>
    public Color Sub( Color color )
    {
        R -= color.R;
        G -= color.G;
        B -= color.B;
        A -= color.A;

        return Clamp();
    }

    /// <summary>
    /// Subtracts the components of the supplied Color object from the
    /// components of this Color object and returns the result as a NEW
    /// Color object.
    /// </summary>
    public Color SubNew( Color color )
    {
        return new Color( R - color.R,
                          G - color.G,
                          B - color.B,
                          A - color.A ).Clamp();
    }

    /// <summary>
    /// Subtracts the supplied elements from the equivalent elements in this Color.
    /// </summary>
    /// <param name="r"> Red component. </param>
    /// <param name="g"> Green component. </param>
    /// <param name="b"> Blue component. </param>
    /// <param name="a"> Alpha component. </param>
    /// <returns> This Color for chaining. </returns>
    public Color Sub( float r, float g, float b, float a )
    {
        R -= r;
        G -= g;
        B -= b;
        A -= a;

        return Clamp();
    }

    #endregion various manipulation methods

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Utility methods

    /// <summary>
    /// Multiplies the RGB values by the alpha.
    /// </summary>
    /// <returns>This color for chaining.</returns>
    public Color PremultiplyAlpha()
    {
        R *= A;
        G *= A;
        B *= A;

        return Clamp();
    }

    /// <summary>
    /// Clamps this Colors RGBA components to a valid range [0 - 1]
    /// </summary>
    /// <returns> This Color for chaining. </returns>
    private Color Clamp( bool showDebug = false )
    {
        R = R < 0f ? 0f : R > 1f ? 1f : R;
        G = G < 0f ? 0f : G > 1f ? 1f : G;
        B = B < 0f ? 0f : B > 1f ? 1f : B;
        A = A < 0f ? 0f : A > 1f ? 1f : A;

        RGBAPackedColor = ToRgba8888( R, G, B, A );
        ABGRPackedColor = ToAbgr8888( A, B, G, R );

        RGBAFloatPack = ToFloatBitsRgba( R, G, B, A );
        ABGRFloatPack = ToFloatBitsAbgr( A, B, G, R );

        if ( showDebug )
        {
            Logger.Debug( $"R: {R}, G: {G}, B: {B}, A: {A}" );
            Logger.Debug( $"RGBAPackedColor: {RGBAPackedColor}, {RGBAPackedColor:X8}" );
            Logger.Debug( $"ABGRPackedColor: {ABGRPackedColor}, {ABGRPackedColor:X8}" );
            Logger.Debug( $"RGBAFloatPack  : {RGBAFloatPack}" );
            Logger.Debug( $"ABGRFloatPack  : {ABGRFloatPack}" );
        }

        return this;
    }

    /// <summary>
    /// Linearly interpolates between this color and the target color by
    /// 'interpolationCoefficient' which is in the range [0,1].
    /// The result is stored in this color.
    /// </summary>
    /// <param name="target"> The target color. </param>
    /// <param name="interpolationCoefficient"> This value must be in the range [0, 1] </param>
    /// <returns>This Color for chaining.</returns>
    public Color Lerp( Color target, float interpolationCoefficient )
    {
        Guard.Against.Null( target );

        if ( interpolationCoefficient is < 0.0f or > 1.0f )
        {
            throw new ArgumentOutOfRangeException( nameof( interpolationCoefficient ),
                                                   "Interpolation coefficient must be between 0f and 1f." );
        }

        R += interpolationCoefficient * ( target.R - R );
        G += interpolationCoefficient * ( target.G - G );
        B += interpolationCoefficient * ( target.B - B );
        A += interpolationCoefficient * ( target.A - A );

        return Clamp();
    }

    /// <summary>
    /// Linearly interpolates between this color and the target color by
    /// 'interpolationCoefficient' which is in the range [0,1].
    /// The result is stored in this color.
    /// </summary>
    /// <param name="r"> Red component. </param>
    /// <param name="g"> Green component. </param>
    /// <param name="b"> Blue component. </param>
    /// <param name="a"> Alpha component. </param>
    /// <param name="interpolationCoefficient"> This value must be in the range [0, 1] </param>
    /// <returns> This Color for chaining. </returns>
    public Color Lerp( float r, float g, float b, float a, float interpolationCoefficient )
    {
        if ( interpolationCoefficient is < 0.0f or > 1.0f )
        {
            throw new ArgumentOutOfRangeException( nameof( interpolationCoefficient ),
                                                   "Interpolation coefficient must be between 0f and 1f." );
        }

        R += interpolationCoefficient * ( r - R );
        G += interpolationCoefficient * ( g - G );
        B += interpolationCoefficient * ( b - B );
        A += interpolationCoefficient * ( a - A );

        return Clamp();
    }

    /// <summary>
    /// Parses a hex color string and assigns the color values to the provided Color object.
    /// </summary>
    /// <param name="hex">The hex color string, which can optionally start with '#'.</param>
    /// <param name="color">The Color object to assign the parsed values to.</param>
    /// <returns>The Color object with the parsed color values.</returns>
    /// <exception cref="ArgumentException">Thrown if the hex string is not valid.</exception>
    public static Color FromHexString( string hex, ref Color color )
    {
        if ( string.IsNullOrEmpty( hex ) )
        {
            throw new ArgumentException( "Hex string cannot be null or empty.", nameof( hex ) );
        }

        // Remove the leading '#' if present
        if ( hex[ 0 ] == '#' )
        {
            hex = hex[ 1.. ];
        }

        if ( ( hex.Length != 6 ) && ( hex.Length != 8 ) )
        {
            throw new ArgumentException( "Hex string must be 6 or 8 characters long.", nameof( hex ) );
        }

        try
        {
            color.R = ParseHexComponent( hex.Substring( 0, 2 ) );
            color.G = ParseHexComponent( hex.Substring( 2, 2 ) );
            color.B = ParseHexComponent( hex.Substring( 4, 2 ) );
            color.A = hex.Length == 8 ? ParseHexComponent( hex.Substring( 6, 2 ) ) : 1f;
        }
        catch ( Exception ex ) when ( ex is FormatException or OverflowException )
        {
            throw new ArgumentException( "Hex string contains invalid characters or values.", nameof( hex ), ex );
        }

        return color;
    }

    /// <summary>
    /// Returns a new color from a hex string with the format <b>RRGGBBAA</b>.
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public static Color FromHexString( string hex )
    {
        var color = new Color();

        return FromHexString( hex, ref color );
    }

    /// <summary>
    /// Parses a hex component (2 characters) to a float value between 0 and 1.
    /// </summary>
    /// <param name="hexComponent">The hex component to parse.</param>
    /// <returns>The parsed float value.</returns>
    /// <exception cref="FormatException">Thrown if the hex component is not a valid hex number.</exception>
    /// <exception cref="OverflowException">Thrown if the hex component value is too large to fit in an Int32.</exception>
    private static float ParseHexComponent( string hexComponent )
    {
        return Convert.ToInt32( hexComponent, 16 ) / 255f;
    }

    /// <summary>
    /// Returns the given Alpha value as a 32-bit uint.
    /// </summary>
    /// <param name="alpha"> The Alpha value. </param>
    /// <returns> The uint result. </returns>
    public static uint AlphaToInt( float alpha )
    {
        return ( uint )( alpha * 255.0f );
    }

    /// <summary>
    /// </summary>
    /// <param name="luminance"></param>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public static uint LuminanceAlpha( float luminance, float alpha )
    {
        return ( ( uint )( luminance * 255.0f ) << 8 ) | ( uint )( alpha * 255 );
    }

    public static Color FromHex( uint hex ) => new( hex );

    #endregion Utility methods

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Returns a string representation of the Color components RGBA.
    /// </summary>
    public string RgbaToString()
    {
        return $"R:{R},G:{G},B:{B},A:{A}";
    }

    /// <summary>
    /// Returns a string representation of the Color components ABGR.
    /// </summary>
    public string AbgrToString()
    {
        return $"A:{A},B:{B},G:{G},R:{R}";
    }
    
    // ========================================================================
    // ========================================================================

    #region From ICloneable Interface

    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    public object Clone()
    {
        return new Color( this );
    }

    #endregion From ICloneable Interface

    // ========================================================================
    // ========================================================================

    #region From IEquatable Interface

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise false .
    /// </returns>
    public bool Equals( Color? other )
    {
        if ( other is null )
        {
            return false;
        }

        return PackedColorAbgr() == other.PackedColorAbgr();
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns> true if the specified object  is equal to the current object; otherwise false. </returns>
    public override bool Equals( object? obj )
    {
        if ( ( obj == null ) || ( GetType() != obj.GetType() ) )
        {
            return false;
        }

        if ( this == obj )
        {
            return true;
        }

        var color = ( Color )obj;

        return PackedColorAbgr() == color.PackedColorAbgr();
    }

    /// Not from IEquatable, but connected to it because of Equals()
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return PackedColorAbgr().GetHashCode();
    }

    #endregion From IEquatable Interface

    // ========================================================================
    // ========================================================================

    #region operator overloads

    /// <summary>
    /// Determines whether two <see cref="Color"/> objects are equal.
    /// </summary>
    /// <param name="c1"> The first <see cref="Color"/> object to compare, or <b>null</b>. </param>
    /// <param name="c2"> The second object to compare, or <b>null</b>. </param>
    /// <returns> <b>true</b> if the two objects are equal; otherwise, <b>false</b>. </returns>
    public static bool operator ==( Color? c1, object? c2 )
    {
        if ( c1 is null )
        {
            return c2 is null;
        }

        return c1.Equals( c2 );
    }

    /// <summary>
    /// Determines whether two <see cref="Color"/> objects are not equal.
    /// </summary>
    /// <param name="c1"> The first <see cref="Color"/> object to compare, or null. </param>
    /// <param name="c2"> The second object to compare, or null. </param>
    /// <returns><b>true</b> if the two objects are not equal; otherwise, <b>false</b>.</returns>
    public static bool operator !=( Color? c1, object? c2 )
    {
        return !( c1 == c2 );
    }

    /// <summary>
    /// Multiplies the Color Components of Color 1 by the Color Components of
    /// Color 2, ending with returning Color 1.
    /// </summary>
    /// <param name="c1"> Color 1, which will be returned. </param>
    /// <param name="c2"> Color 2. </param>
    public static Color operator *( Color? c1, Color? c2 )
    {
        Guard.Against.Null( c1 );
        Guard.Against.Null( c2 );

        c1.R *= c2.R;
        c1.G *= c2.G;
        c1.B *= c2.B;
        c1.A *= c2.A;

        return c1;
    }

    #endregion operator overloads

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Debugs the various methods and properties for this class.
    /// </summary>
    public void DebugPrint()
    {
        Logger.Divider();
        Logger.Debug( $"Name                    : {Name}" );
        Logger.Debug( $"RGBA                    : {R}.{G}.{B}.{A}" );
        Logger.Debug( $"PackedColorABGR         : {ABGRPackedColor} :: {ABGRPackedColor:X}" );
        Logger.Debug( $"PackedColorRGBA         : {RGBAPackedColor} :: {RGBAPackedColor:X}" );
        Logger.Debug( $"ABGRFloatPack           : {ABGRFloatPack}" );
        Logger.Debug( $"RGBAFloatPack           : {RGBAFloatPack}" );
        Logger.Debug( $"RGBA8888ToUInt(R,G,B,A) : {Rgba8888ToUInt( R, G, B, A ):X}" );
        Logger.Debug( $"ToFloatBitsABGR()       : {ToFloatBitsAbgr()}" );
        Logger.Debug( $"ToFloatBitsRGBA()       : {ToFloatBitsRgba()}" );
        Logger.Debug( $"ToFloatBitsABGR(F,F,F,F): {ToFloatBitsAbgr( A, B, G, R )}" );
        Logger.Debug( $"ToFloatBitsRGBA(F,F,F,F): {ToFloatBitsRgba( R, G, B, A )}" );
        Logger.Divider();
    }
}

// ============================================================================
// ============================================================================