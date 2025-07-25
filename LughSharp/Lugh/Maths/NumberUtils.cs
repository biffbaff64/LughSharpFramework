﻿// ///////////////////////////////////////////////////////////////////////////////
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

namespace LughSharp.Lugh.Maths;

[PublicAPI]
public abstract class NumberUtils
{
    public const int   NOT_SET           = -1;
    public const float PI                = 3.14159265358979323846f;
    public const float MIN_NORMAL        = 1.17549435E-38f;
    public const int   SIGNIFICAND_WIDTH = 24;
    public const int   MAX_EXPONENT      = 127;
    public const int   MIN_EXPONENT      = -126;
    public const int   MIN_SUB_EXPONENT  = -149;
    public const int   EXP_BIAS          = 127;
    public const int   SIGN_BIT_MASK     = int.MinValue;
    public const int   EXP_BIT_MASK      = 2139095040;
    public const int   SIGNIF_BIT_MASK   = 8388607;
    public const float FLOAT_TOLERANCE   = 0.000000000000001f; // 32 bits
    public const float FLOAT_EPSILON     = 1e-6f;              // 32 bits

    // ========================================================================

    /// <summary>
    /// Converts the given floating-point value to its integer bit representation.
    /// </summary>
    /// <param name="value"> The floating-point value to convert. </param>
    /// <returns> The integer bit representation of the floating-point value. </returns>
    public static int FloatToIntBits( float value )
    {
        var result = FloatToRawIntBits( value );

        // Check for NaN based on values of bit fields, maximum
        // exponent and nonzero significand.
        if ( ( ( result & EXP_BIT_MASK ) == EXP_BIT_MASK )
             && ( ( result & SIGNIF_BIT_MASK ) != 0 ) )
        {
            result = 0x7fc00000;
        }

        return result;
    }

    /// <summary>
    /// Converts the given floating-point value to its raw integer bit representation.
    /// </summary>
    /// <param name="value"> The floating-point value to convert. </param>
    /// <returns>
    /// The raw integer bit representation of the floating-point value.
    /// </returns>
    public static int FloatToRawIntBits( float value )
    {
        return BitConverter.SingleToInt32Bits( value );
    }

    /// <summary>
    /// Converts a float value to its hexadecimal representation.
    /// <example>
    /// string hexValue = FloatToHex(-1.7014117E+38f);
    /// Console.WriteLine(hexValue); // Outputs: FF7FFFFF
    /// </example>
    /// </summary>
    /// <param name="value">The float value to convert.</param>
    /// <returns>A string representing the hexadecimal value.</returns>
    public static string FloatToHexString( float value )
    {
        unsafe
        {
            var intBits = *( int* )&value;
            var result  = intBits.ToString( "X8" );

            return result;
        }
    }

    /// <summary>
    /// Converts the given integer bit representation to its floating-point value.
    /// </summary>
    /// <param name="value"> The integer bit representation to convert. </param>
    /// <returns> The floating-point value represented by the integer bits. </returns>
    public static float IntBitsToFloat( int value )
    {
        return BitConverter.Int32BitsToSingle( value );
    }

    /// <summary>
    /// Converts the given floating-point color value to its integer bit representation.
    /// </summary>
    /// <param name="value"> The floating-point color value to convert. </param>
    /// <returns>
    /// The integer bit representation of the floating-point color value.
    /// </returns>
    public static int FloatToIntColor( float value )
    {
        var intBits = FloatToRawIntBits( value );

        intBits |= ( int )( ( intBits >>> 24 ) * ( 255f / 254f ) ) << 24;

        return intBits;
    }

    /// <summary>
    /// Converts the given integer bit representation to its floating-point color value.
    /// </summary>
    /// <param name="value"> The integer bit representation to convert. </param>
    /// <returns> The floating-point color value represented by the integer bits. </returns>
    public static float IntToFloatColor( int value )
    {
        return BitConverter.Int32BitsToSingle( value & 0x7fffffff );
    }

    /// <summary>
    /// Converts the given unsigned integer bit representation to its floating-point color value.
    /// </summary>
    /// <param name="value"> The integer bit representation to convert. </param>
    /// <returns> The floating-point color value represented by the integer bits. </returns>
    public static float UIntToFloatColor( uint value )
    {
        return BitConverter.UInt32BitsToSingle( value & 0xfeffffff );
    }

    /// <summary>
    /// Converts the given double-precision floating-point value to its long bit representation.
    /// </summary>
    /// <param name="value"> The double-precision floating-point value to convert. </param>
    /// <returns> The long bit representation of the double-precision floating-point value. </returns>
    public static long DoubleToLongBits( double value )
    {
        return BitConverter.DoubleToInt64Bits( value );
    }

    /// <summary>
    /// Converts the given long bit representation to its double-precision floating-point value.
    /// </summary>
    /// <param name="value"> The long bit representation to convert. </param>
    /// <returns> The double-precision floating-point value represented by the long bits. </returns>
    public static double LongBitsToDouble( long value )
    {
        return BitConverter.Int64BitsToDouble( value );
    }

    /// <summary>
    /// Parses the given string as an integer.
    /// </summary>
    /// <param name="str"> The string to parse. </param>
    /// <returns>
    /// The parsed integer value, or null if the string could not be parsed as an integer.
    /// </returns>
    public static int? ParseInt( string? str )
    {
        if ( int.TryParse( str, out var parsedValue ) )
        {
            return parsedValue;
        }

        return null;
    }

    /// <summary>
    /// Parses the given string as a floating-point number.
    /// </summary>
    /// <param name="str"> The string to parse. </param>
    /// <returns>
    /// The parsed floating-point value, or null if the string could not be parsed
    /// as a floating-point number.
    /// </returns>
    public static float? ParseFloat( string? str )
    {
        if ( float.TryParse( str, out var parsedValue ) )
        {
            return parsedValue;
        }

        return null;
    }

    /// <summary>
    /// Determines if a character is a valid hexadecimal digit.
    /// </summary>
    /// <param name="ch"> The character to check. </param>
    /// <returns> True if the character is a hexadecimal digit; otherwise, false. </returns>
    public static bool IsHexDigit( char ch )
    {
        return ch is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
    }

    /// <summary>
    /// Converts a hexadecimal digit character to its integer value.
    /// </summary>
    /// <param name="ch"> The hexadecimal digit character. </param>
    /// <returns> The integer value of the hexadecimal digit. </returns>
    public static int HexValue( char ch )
    {
        if ( ch is >= '0' and <= '9' )
        {
            return ch - '0';
        }

        if ( ch is >= 'a' and <= 'f' )
        {
            return 10 + ( ch - 'a' );
        }

        return 10 + ( ch - 'A' );
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string IntToBinaryString( int value )
    {
        //TODO:
        return value.ToString( "B8" );
    }

    /// <summary>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static byte Compare( byte x, byte y )
    {
        return ( byte )( x - y );
    }

    /// <summary>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static char Compare( char x, char y )
    {
        return ( char )( x - y );
    }

    /// <summary>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static int Compare( int x, int y )
    {
        return x - y;
    }

    /// <summary>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static float Compare( float x, float y )
    {
        return x - y;
    }

    // ========================================================================

    /// <summary>
    /// Returns the value of the specified number as an <tt>int</tt>
    /// which may involve rounding or truncation.
    /// </summary>
    /// <returns>
    /// the numeric value represented by this object after conversion
    /// to type <tt>int</tt>.
    /// </returns>
    public abstract int IntValue();

    /// <summary>
    /// Returns the value of the specified number as a <tt>long</tt>
    /// which may involve rounding or truncation.
    /// </summary>
    /// <returns>
    /// the numeric value represented by this object after conversion
    /// to type <tt>long</tt>.
    /// </returns>
    public abstract long LongValue();

    /// <summary>
    /// Returns the value of the specified number as a <tt>float</tt>
    /// which may involve rounding.
    /// </summary>
    /// <returns>
    /// the numeric value represented by this object after conversion
    /// to type <tt>float</tt>.
    /// </returns>
    public abstract float FloatValue();

    /// <summary>
    /// Returns the value of the specified number as a <tt>double</tt>
    /// which may involve rounding.
    /// </summary>
    /// <returns>
    /// the numeric value represented by this object after conversion
    /// to type <tt>double</tt>.
    /// </returns>
    public abstract double DoubleValue();

    /// <summary>
    /// Returns the value of the specified number as a <tt>byte</tt>
    /// which may involve rounding or truncation.
    /// <para>
    /// This implementation returns the result of <see cref="IntValue" /> cast
    /// to a <tt>byte</tt>.
    /// </para>
    /// </summary>
    /// <returns>
    /// the numeric value represented by this object after conversion
    /// to type <tt>byte</tt>.
    /// </returns>
    public sbyte ByteValue()
    {
        return ( sbyte )IntValue();
    }

    /// <summary>
    /// Returns the value of the specified number as a <tt>short</tt>
    /// which may involve rounding or truncation.
    /// <para>
    /// This implementation returns the result of <see cref="IntValue" />
    /// cast to a <tt>short</tt>.
    /// </para>
    /// </summary>
    /// <returns>
    /// the numeric value represented by this object after conversion
    /// to type <tt>short</tt>.
    /// </returns>
    public short ShortValue()
    {
        return ( short )IntValue();
    }
}

// ========================================================================
// ========================================================================