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

namespace LughSharp.Core.Graphics;

[PublicAPI]
public class ColorWIP
{
    /// <summary>
    /// Red ColorWIP Component
    /// </summary>
    public byte R { get; set; }

    /// <summary>
    /// Green ColorWIP Component
    /// </summary>
    public byte G { get; set; }

    /// <summary>
    /// Blue ColorWIP Component
    /// </summary>
    public byte B { get; set; }

    /// <summary>
    /// Alpha ColorWIP Component
    /// </summary>
    public byte A { get; set; }

    /// <summary>
    /// ColorWIP Components packed into a <b>uint</b>, stored in <b>RGBA</b> format.
    /// </summary>
    public uint RGBAPackedColor { get; private set; }

    /// <summary>
    /// ColorWIP Components packed into a <b>uint</b>, stored in <b>ABGR</b> format.
    /// </summary>
    public uint ABGRPackedColor { get; private set; }

    /// <summary>
    /// The color's name.
    /// </summary>
    public string Name { get; set; } = "ColorWIP";
    
    // ========================================================================

    private const int ARGBAlphaShift = 24;
    private const int ARGBRedShift   = 16;
    private const int ARGBGreenShift = 8;
    private const int ARGBBlueShift  = 0;

    private const int RGBARedShift   = 24;
    private const int RGBAGreenShift = 16;
    private const int RGBABlueShift  = 8;
    private const int RGBAAlphaShift = 0;

    private const uint RGBARedMask   = 0xff000000;
    private const uint RGBAGreenMask = 0x00ff0000;
    private const uint RGBABlueMask  = 0x0000ff00;
    private const uint RGBAAlphaMask = 0x000000ff;

    // ========================================================================

    /// <summary>
    /// Default constructor. Sets all components to 0.
    /// </summary>
    public ColorWIP() : this( 0u )
    {
    }

    /// <summary>
    /// Creates a new ColorWIP from the specified <see cref="ValidColor"/> enum value.
    /// </summary>
    public ColorWIP( ValidColor color )
    {
        ValidColorData colorData = ValidColors[ ( int )color ];
        uint           rgba      = colorData.RGBA8888;

        R = ( byte )( ( rgba & RGBARedMask ) >> RGBARedShift );
        G = ( byte )( ( rgba & RGBAGreenMask ) >> RGBAGreenShift );
        B = ( byte )( ( rgba & RGBABlueMask ) >> RGBABlueShift );
        A = ( byte )( ( rgba & RGBAAlphaMask ) >> RGBAAlphaShift );

        UpdatePackedColors();
    }

    /// <summary>
    /// Creates a new ColorWIP from the suppied RGBA8888 integer value.
    /// </summary>
    /// <param name="rgba8888"></param>
    public ColorWIP( uint rgba8888 )
        : this( ( byte )( ( rgba8888 & RGBARedMask ) >> RGBARedShift ),
                ( byte )( ( rgba8888 & RGBAGreenMask ) >> RGBAGreenShift ),
                ( byte )( ( rgba8888 & RGBABlueMask ) >> RGBABlueShift ),
                ( byte )( ( rgba8888 & RGBAAlphaMask ) >> RGBAAlphaShift ) )

    {
    }

    /// <summary>
    /// Creates a new ColorWIP from the components of the supplied ColorWIP object.
    /// </summary>
    /// <param name="color"></param>
    public ColorWIP( ColorWIP color )
        : this( color.R, color.G, color.B, color.A )
    {
    }

    /// <summary>
    /// Creates a new ColorWIP from the supplied float r,g,b and a components.
    /// The values must be in the range 0f - 1f.
    /// </summary>
    /// <param name="r"> The Red component value. </param>
    /// <param name="g"> The Green component value. </param>
    /// <param name="b"> The Blue component value. </param>
    /// <param name="a"> The ALpha component value. </param>
    public ColorWIP( float r, float g, float b, float a )
        : this( ( byte )( r * 255f ),
                ( byte )( g * 255f ),
                ( byte )( b * 255f ),
                ( byte )( a * 255f ) )
    {
    }

    /// <summary>
    /// Creates a new ColorWIP from the supplied r,g,b and a components.
    /// </summary>
    /// <param name="r"> The Red component value. </param>
    /// <param name="g"> The Green component value. </param>
    /// <param name="b"> The Blue component value. </param>
    /// <param name="a"> The ALpha component value. </param>
    public ColorWIP( byte r, byte g, byte b, byte a )
    {
        R = r;
        G = g;
        B = b;
        A = a;

        UpdatePackedColors();
    }

    // ========================================================================

    /// <summary>
    /// Updates the packed color representations (RGBA8888 and ABGR8888) based on
    /// the current component values.
    /// </summary>
    /// <param name="showDebug"> If true, outputs debug information to the logger. </param>
    /// <returns> This ColorWIP for chaining. </returns>
    private ColorWIP UpdatePackedColors( bool showDebug = false )
    {
        RGBAPackedColor = ToRgba8888( R, G, B, A );
        ABGRPackedColor = ToAbgr8888( A, B, G, R );

        foreach ( ValidColorData entry in ValidColors )
        {
            if ( entry.RGBA8888 == RGBAPackedColor )
            {
                Name = entry.Name;

                break;
            }
        }
        
        if ( showDebug )
        {
            Logger.Debug( $"R: {R}, G: {G}, B: {B}, A: {A}" );
            Logger.Debug( $"RGBAPackedColor: {RGBAPackedColor}, {RGBAPackedColor:X8}" );
            Logger.Debug( $"ABGRPackedColor: {ABGRPackedColor}, {ABGRPackedColor:X8}" );
        }

        return this;
    }

    /// <summary>
    /// Clamps a byte value to ensure it stays within valid range (0-255).
    /// </summary>
    private static byte ClampByte( int value )
    {
        return ( byte )Math.Clamp( value, 0, 255 );
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
    /// Returns the given <see cref="ColorWIP"/> as a 32-bit uint in the following format:-
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
        return ( uint )( ( ( r >> 3 ) << 11 ) | ( ( g >> 2 ) << 5 ) | ( b >> 3 ) );
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
        return ( uint )( ( ( r >> 4 ) << 12 ) | ( ( g >> 4 ) << 8 ) | ( ( b >> 4 ) << 4 ) | ( a >> 4 ) );
    }

    /// <summary>
    /// Sets this colors components using the components from the supplied color.
    /// </summary>
    /// <param name="color"> The ColorWIP to set. This cannot be null. </param>
    /// <returns> This ColorWIP for chaining. </returns>
    public ColorWIP Set( ColorWIP color )
    {
        R = color.R;
        G = color.G;
        B = color.B;
        A = color.A;

        return UpdatePackedColors();
    }

    /// <summary>
    /// Sets this color's component values through an integer representation.
    /// </summary>
    /// <param name="rgba"> The integer representation. </param>
    /// <returns> This color for chaining. </returns>
    public ColorWIP Set( uint rgba )
    {
        R = ( byte )( ( rgba & RGBARedMask ) >> RGBARedShift );
        G = ( byte )( ( rgba & RGBAGreenMask ) >> RGBAGreenShift );
        B = ( byte )( ( rgba & RGBABlueMask ) >> RGBABlueShift );
        A = ( byte )( ( rgba & RGBAAlphaMask ) >> RGBAAlphaShift );

        return UpdatePackedColors();
    }

    /// <summary>
    /// Sets this colors components using the supplied r,g,b,a components.
    /// </summary>
    /// <param name="r"> Red component </param>
    /// <param name="g"> Green component </param>
    /// <param name="b"> Blue component </param>
    /// <param name="a"> Alpha component </param>
    /// <returns> This ColorWIP for chaining. </returns>
    public ColorWIP Set( byte r, byte g, byte b, byte a )
    {
        R = r;
        G = g;
        B = b;
        A = a;

        return UpdatePackedColors();
    }

    /// <summary>
    /// Sets this colors components using the supplied float values (0-1 range).
    /// </summary>
    /// <param name="r"> Red component (0-1) </param>
    /// <param name="g"> Green component (0-1) </param>
    /// <param name="b"> Blue component (0-1) </param>
    /// <param name="a"> Alpha component (0-1) </param>
    /// <returns> This ColorWIP for chaining. </returns>
    public ColorWIP Set( float r, float g, float b, float a )
    {
        R = ( byte )( r * 255f );
        G = ( byte )( g * 255f );
        B = ( byte )( b * 255f );
        A = ( byte )( a * 255f );

        return UpdatePackedColors();
    }

    /// <summary>
    /// Adds the components from the supplied ColorWIP to the corresponding
    /// components of this color, with clamping to prevent overflow.
    /// </summary>
    /// <param name="color"> The ColorWIP to add. </param>
    /// <returns> This ColorWIP for chaining. </returns>
    public ColorWIP Add( ColorWIP color )
    {
        R = ClampByte( R + color.R );
        G = ClampByte( G + color.G );
        B = ClampByte( B + color.B );
        A = ClampByte( A + color.A );

        return UpdatePackedColors();
    }

    /// <summary>
    /// Subtracts the components of the supplied ColorWIP from the corresponding
    /// components of this color, with clamping to prevent underflow.
    /// </summary>
    /// <param name="color"> The ColorWIP to subtract. </param>
    /// <returns> This ColorWIP for chaining. </returns>
    public ColorWIP Subtract( ColorWIP color )
    {
        R = ClampByte( R - color.R );
        G = ClampByte( G - color.G );
        B = ClampByte( B - color.B );
        A = ClampByte( A - color.A );

        return UpdatePackedColors();
    }

    /// <summary>
    /// Multiplies the components of this color by the corresponding components
    /// of the supplied color. Useful for color modulation.
    /// </summary>
    /// <param name="color"> The ColorWIP to multiply by. </param>
    /// <returns> This ColorWIP for chaining. </returns>
    public ColorWIP Multiply( ColorWIP color )
    {
        R = ( byte )( ( R * color.R ) / 255 );
        G = ( byte )( ( G * color.G ) / 255 );
        B = ( byte )( ( B * color.B ) / 255 );
        A = ( byte )( ( A * color.A ) / 255 );

        return UpdatePackedColors();
    }

    /// <summary>
    /// Multiplies all color components by a scalar value.
    /// </summary>
    /// <param name="scalar"> The value to multiply by. </param>
    /// <returns> This ColorWIP for chaining. </returns>
    public ColorWIP Multiply( float scalar )
    {
        R = ClampByte( ( int )( R * scalar ) );
        G = ClampByte( ( int )( G * scalar ) );
        B = ClampByte( ( int )( B * scalar ) );
        A = ClampByte( ( int )( A * scalar ) );

        return UpdatePackedColors();
    }

    /// <summary>
    /// Linearly interpolates between this color and the target color.
    /// </summary>
    /// <param name="target"> The target color. </param>
    /// <param name="t"> The interpolation coefficient (0-1). </param>
    /// <returns> This ColorWIP for chaining. </returns>
    public ColorWIP Lerp( ColorWIP target, float t )
    {
        t = Math.Clamp( t, 0f, 1f );

        R = ( byte )( R + ( ( target.R - R ) * t ) );
        G = ( byte )( G + ( ( target.G - G ) * t ) );
        B = ( byte )( B + ( ( target.B - B ) * t ) );
        A = ( byte )( A + ( ( target.A - A ) * t ) );

        return UpdatePackedColors();
    }

    /// <summary>
    /// Creates a copy of this color.
    /// </summary>
    /// <returns> A new ColorWIP instance with the same component values. </returns>
    public ColorWIP Copy()
    {
        return new ColorWIP( R, G, B, A );
    }

    /// <summary>
    /// Returns a new color with the specified alpha value.
    /// </summary>
    /// <param name="alpha"> The new alpha value. </param>
    /// <returns> A new ColorWIP with the updated alpha. </returns>
    public ColorWIP WithAlpha( byte alpha )
    {
        return new ColorWIP( R, G, B, alpha );
    }

    /// <summary>
    /// Converts this color to grayscale using the standard luminosity method.
    /// </summary>
    /// <returns> This ColorWIP for chaining. </returns>
    public ColorWIP ToGrayscale()
    {
        var gray = ( byte )( ( 0.299f * R ) + ( 0.587f * G ) + ( 0.114f * B ) );
        R = G    = B = gray;

        return UpdatePackedColors();
    }

    /// <summary>
    /// Inverts the RGB components of this color (alpha remains unchanged).
    /// </summary>
    /// <returns> This ColorWIP for chaining. </returns>
    public ColorWIP Invert()
    {
        R = ( byte )( 255 - R );
        G = ( byte )( 255 - G );
        B = ( byte )( 255 - B );

        return UpdatePackedColors();
    }

    /// <summary>
    /// Premultiplies the RGB components by the alpha value.
    /// </summary>
    /// <returns> This ColorWIP for chaining. </returns>
    public ColorWIP Premultiply()
    {
        float alphaNormalized = A / 255f;

        R = ( byte )( R * alphaNormalized );
        G = ( byte )( G * alphaNormalized );
        B = ( byte )( B * alphaNormalized );

        return UpdatePackedColors();
    }

    /// <summary>
    /// Returns the red component as a normalized float (0-1).
    /// </summary>
    public float GetRedFloat() => R / 255f;

    /// <summary>
    /// Returns the green component as a normalized float (0-1).
    /// </summary>
    public float GetGreenFloat() => G / 255f;

    /// <summary>
    /// Returns the blue component as a normalized float (0-1).
    /// </summary>
    public float GetBlueFloat() => B / 255f;

    /// <summary>
    /// Returns the alpha component as a normalized float (0-1).
    /// </summary>
    public float GetAlphaFloat() => A / 255f;

    /// <summary>
    /// Converts this color to a hexadecimal string in RRGGBBAA format.
    /// </summary>
    /// <returns> Hexadecimal string representation. </returns>
    public string ToHex()
    {
        return $"{R:X2}{G:X2}{B:X2}{A:X2}";
    }

    /// <summary>
    /// Creates a ColorWIP from a hexadecimal string.
    /// Supports formats: RGB, RGBA, RRGGBB, RRGGBBAA, #RGB, #RGBA, #RRGGBB, #RRGGBBAA
    /// </summary>
    /// <param name="hex"> The hexadecimal string. </param>
    /// <returns> A new ColorWIP instance. </returns>
    public static ColorWIP FromHexString( string hex )
    {
        if ( string.IsNullOrEmpty( hex ) )
        {
            throw new ArgumentException( "Hex string cannot be null or empty.", nameof( hex ) );
        }

        hex = hex.TrimStart( '#' );

        return hex.Length switch
               {
                   3 => new ColorWIP( // RGB
                                  Convert.ToByte( new string( hex[ 0 ], 2 ), 16 ),
                                  Convert.ToByte( new string( hex[ 1 ], 2 ), 16 ),
                                  Convert.ToByte( new string( hex[ 2 ], 2 ), 16 ),
                                  255 ),
                   4 => new ColorWIP( // RGBA
                                  Convert.ToByte( new string( hex[ 0 ], 2 ), 16 ),
                                  Convert.ToByte( new string( hex[ 1 ], 2 ), 16 ),
                                  Convert.ToByte( new string( hex[ 2 ], 2 ), 16 ),
                                  Convert.ToByte( new string( hex[ 3 ], 2 ), 16 ) ),
                   6 => new ColorWIP( // RRGGBB
                                  Convert.ToByte( hex.Substring( 0, 2 ), 16 ),
                                  Convert.ToByte( hex.Substring( 2, 2 ), 16 ),
                                  Convert.ToByte( hex.Substring( 4, 2 ), 16 ),
                                  255 ),
                   8 => new ColorWIP( // RRGGBBAA
                                  Convert.ToByte( hex.Substring( 0, 2 ), 16 ),
                                  Convert.ToByte( hex.Substring( 2, 2 ), 16 ),
                                  Convert.ToByte( hex.Substring( 4, 2 ), 16 ),
                                  Convert.ToByte( hex.Substring( 6, 2 ), 16 ) ),
                   var _ => throw new ArgumentException( $"Invalid hex color format: {hex}", nameof( hex ) )
               };
    }

    /// <summary>
    /// Static factory method to create a color from RGB components.
    /// </summary>
    public static ColorWIP FromRgb( byte r, byte g, byte b )
    {
        return new ColorWIP( r, g, b, 255 );
    }

    /// <summary>
    /// Static factory method to create a color from RGBA components.
    /// </summary>
    public static ColorWIP FromRgba( byte r, byte g, byte b, byte a )
    {
        return new ColorWIP( r, g, b, a );
    }

    /// <summary>
    /// Static factory method to create a color from float components (0-1).
    /// </summary>
    public static ColorWIP FromFloats( float r, float g, float b, float a = 1f )
    {
        return new ColorWIP( r, g, b, a );
    }

    /// <summary>
    /// Converts an ARGB8888 packed value to a ColorWIP.
    /// </summary>
    public static ColorWIP FromArgb8888( uint argb )
    {
        return new ColorWIP( ( byte )( ( argb >> ARGBRedShift ) & 0xFF ),
                          ( byte )( ( argb >> ARGBGreenShift ) & 0xFF ),
                          ( byte )( ( argb >> ARGBBlueShift ) & 0xFF ),
                          ( byte )( ( argb >> ARGBAlphaShift ) & 0xFF ) );
    }

    // ========================================================================
    // Operator Overloads
    // ========================================================================

    /// <summary>
    /// Adds two colors component-wise with clamping.
    /// </summary>
    public static ColorWIP operator +( ColorWIP a, ColorWIP b )
    {
        return new ColorWIP( ClampByte( a.R + b.R ),
                          ClampByte( a.G + b.G ),
                          ClampByte( a.B + b.B ),
                          ClampByte( a.A + b.A ) );
    }

    /// <summary>
    /// Subtracts two colors component-wise with clamping.
    /// </summary>
    public static ColorWIP operator -( ColorWIP a, ColorWIP b )
    {
        return new ColorWIP( ClampByte( a.R - b.R ),
                          ClampByte( a.G - b.G ),
                          ClampByte( a.B - b.B ),
                          ClampByte( a.A - b.A ) );
    }

    /// <summary>
    /// Multiplies two colors component-wise.
    /// </summary>
    public static ColorWIP operator *( ColorWIP a, ColorWIP b )
    {
        return new ColorWIP( ( byte )( ( a.R * b.R ) / 255 ),
                          ( byte )( ( a.G * b.G ) / 255 ),
                          ( byte )( ( a.B * b.B ) / 255 ),
                          ( byte )( ( a.A * b.A ) / 255 ) );
    }

    /// <summary>
    /// Multiplies a color by a scalar value.
    /// </summary>
    public static ColorWIP operator *( ColorWIP color, float scalar )
    {
        return new ColorWIP( ClampByte( ( int )( color.R * scalar ) ),
                          ClampByte( ( int )( color.G * scalar ) ),
                          ClampByte( ( int )( color.B * scalar ) ),
                          ClampByte( ( int )( color.A * scalar ) ) );
    }

    /// <summary>
    /// Multiplies a color by a scalar value.
    /// </summary>
    public static ColorWIP operator *( float scalar, ColorWIP color )
    {
        return color * scalar;
    }

    /// <summary>
    /// Divides a color by a scalar value.
    /// </summary>
    public static ColorWIP operator /( ColorWIP color, float scalar )
    {
        if ( Math.Abs( scalar ) < 0.0001f )
        {
            throw new DivideByZeroException( "Cannot divide color by zero." );
        }

        return color * ( 1f / scalar );
    }

    /// <summary>
    /// Compares two colors for equality.
    /// </summary>
    public static bool operator ==( ColorWIP? a, ColorWIP? b )
    {
        if ( ReferenceEquals( a, b ) )
        {
            return true;
        }

        if ( a is null || b is null )
        {
            return false;
        }

        return ( a.R == b.R ) && ( a.G == b.G ) && ( a.B == b.B ) && ( a.A == b.A );
    }

    /// <summary>
    /// Compares two colors for inequality.
    /// </summary>
    public static bool operator !=( ColorWIP? a, ColorWIP? b )
    {
        return !( a == b );
    }

    // ========================================================================
    // Object Overrides
    // ========================================================================

    /// <summary>
    /// Returns a string representation of this color.
    /// </summary>
    public override string ToString()
    {
        return $"ColorWIP(R={R}, G={G}, B={B}, A={A}) [#{ToHex()}]";
    }

    /// <summary>
    /// Determines whether the specified object is equal to this color.
    /// </summary>
    public override bool Equals( object? obj )
    {
        return obj is ColorWIP other && this == other;
    }

    /// <summary>
    /// Returns a hash code for this color.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCodeValue();
    }

    private int HashCodeValue()
    {
        return HashCode.Combine( R, G, B, A );
    }

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    public static readonly ColorWIP Red        = new( 0xff0000ff );
    public static readonly ColorWIP Green      = new( 0x00ff00ff );
    public static readonly ColorWIP Blue       = new( 0x0000ffff );
    public static readonly ColorWIP Clear      = new( 0x00000000u );
    public static readonly ColorWIP White      = new( 0xffffffff );
    public static readonly ColorWIP Black      = new( 0x000000ff );
    public static readonly ColorWIP Gray       = new( 0x7f7f7fff );
    public static readonly ColorWIP LightGray  = new( 0xbfbfbfff );
    public static readonly ColorWIP DarkGray   = new( 0x3f3f3fff );
    public static readonly ColorWIP Slate      = new( 0x708090ff );
    public static readonly ColorWIP Navy       = new( 0x000080ff );
    public static readonly ColorWIP Royal      = new( 0x4169e1ff );
    public static readonly ColorWIP Sky        = new( 0x87ceebff );
    public static readonly ColorWIP Cyan       = new( 0x00ffffff );
    public static readonly ColorWIP Teal       = new( 0x007f7fff );
    public static readonly ColorWIP Chartreuse = new( 0x7fff00ff );
    public static readonly ColorWIP Lime       = new( 0x32cd32ff );
    public static readonly ColorWIP Forest     = new( 0x228b22ff );
    public static readonly ColorWIP Olive      = new( 0x6b8e23ff );
    public static readonly ColorWIP Yellow     = new( 0xffff00ff );
    public static readonly ColorWIP Gold       = new( 0xffd700ff );
    public static readonly ColorWIP Goldenrod  = new( 0xdaa520ff );
    public static readonly ColorWIP Orange     = new( 0xffa500ff );
    public static readonly ColorWIP Brown      = new( 0x8b4513ff );
    public static readonly ColorWIP Tan        = new( 0xd2b48cff );
    public static readonly ColorWIP Firebrick  = new( 0xb22222ff );
    public static readonly ColorWIP Scarlet    = new( 0xff341cff );
    public static readonly ColorWIP Coral      = new( 0xff7f50ff );
    public static readonly ColorWIP Salmon     = new( 0xfa8072ff );
    public static readonly ColorWIP Pink       = new( 0xff69b4ff );
    public static readonly ColorWIP Magenta    = new( 0xff00ffff );
    public static readonly ColorWIP Purple     = new( 0xa020f0ff );
    public static readonly ColorWIP Violet     = new( 0xee82eeff );
    public static readonly ColorWIP Maroon     = new( 0xb03060ff );

    /// <summary>
    /// Convenience for frequently used <c>White.ToFloatBits()</c>
    /// </summary>
    public static float WhiteAbgr => White.ToAbgr8888( White.A, White.B, White.G, White.R );

    // ========================================================================

    [PublicAPI]
    public struct ValidColorData
    {
        public ValidColor ColorWIP;
        public uint       RGBA8888;
        public string     Name;

        public ValidColorData( ValidColor color, uint rgba, string name )
        {
            ColorWIP    = color;
            RGBA8888 = rgba;
            Name     = name;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static readonly ValidColorData[] ValidColors =
    {
        //@formatter:off
        new( ValidColor.Red,        0xFF0000FF, "Red"        ),
        new( ValidColor.Green,      0x00FF00FF, "Green"      ),
        new( ValidColor.Blue,       0x0000FFFF, "Blue"       ),
        new( ValidColor.Clear,      0x00000000, "Clear"      ),
        new( ValidColor.White,      0xFFFFFFFF, "White"      ),
        new( ValidColor.Black,      0x000000FF, "Black"      ),
        new( ValidColor.Gray,       0x7F7F7FFF, "Gray"       ),
        new( ValidColor.Lightgray,  0xBFBFBFFF, "Lightgray"  ),
        new( ValidColor.Darkgray,   0x3F3F3FFF, "Darkgray"   ),
        new( ValidColor.Slate,      0x708090FF, "Slate"      ),
        new( ValidColor.Navy,       0x000080FF, "Navy"       ),
        new( ValidColor.Royal,      0x4169E1FF, "Royal"      ),
        new( ValidColor.Sky,        0x87CEEBFF, "Sky"        ),
        new( ValidColor.Cyan,       0x00FFFFFF, "Cyan"       ),
        new( ValidColor.Teal,       0x007F7FFF, "Teal"       ),
        new( ValidColor.Chartreuse, 0x7FFF00FF, "Chartreuse" ),
        new( ValidColor.Lime,       0x32CD32FF, "Lime"       ),
        new( ValidColor.Forest,     0x228B22FF, "Forest"     ),
        new( ValidColor.Olive,      0x6B8E23FF, "Olive"      ),
        new( ValidColor.Yellow,     0xFFFF00FF, "Yellow"     ),
        new( ValidColor.Gold,       0xFFD700FF, "Gold"       ),
        new( ValidColor.Goldenrod,  0xDAA520FF, "Goldenrod"  ),
        new( ValidColor.Orange,     0xFFA500FF, "Orange"     ),
        new( ValidColor.Brown,      0x8B4513FF, "Brown"      ),
        new( ValidColor.Tan,        0xD2B48CFF, "Tan"        ),
        new( ValidColor.Firebrick,  0xB22222FF, "Firebrick"  ),
        new( ValidColor.Scarlet,    0xFF341CFF, "Scarlet"    ),
        new( ValidColor.Coral,      0xFF7F50FF, "Coral"      ),
        new( ValidColor.Salmon,     0xFA8072FF, "Salmon"     ),
        new( ValidColor.Pink,       0xFF69B4FF, "Pink"       ),
        new( ValidColor.Magenta,    0xFF00FFFF, "Magenta"    ),
        new( ValidColor.Purple,     0xA020F0FF, "Purple"     ),
        new( ValidColor.Violet,     0xEE82EEFF, "Violet"     ),
        new( ValidColor.Maroon,     0xB03060FF, "Maroon"     ),
        //@formatter:on 
    };

    /// <summary>
    /// Public Enum holding the valid colors.
    /// </summary>
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
}

// ============================================================================
// ============================================================================