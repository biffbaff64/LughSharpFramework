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

using System.Globalization;

using JetBrains.Annotations;

namespace LughSharp.Core.Graphics.Text;

/// <summary>
/// Provides utility methods and constants for character-related operations.
/// </summary>
[PublicAPI]
public class CharacterUtils
{
    /// <summary>
    /// The maximum value of a Unicode high-surrogate code unit in the UTF-16 encoding,
    /// constant '\uDBFF'. A high-surrogate is also known as a leading-surrogate.
    /// </summary>
    public const char MaxHighSurrogate = '\uDBFF';

    /// <summary>
    /// The minimum value of a Unicode high-surrogate code unit in the UTF-16 encoding,
    /// constant '\uD800'. A high-surrogate is also known as a leading-surrogate.
    /// </summary>
    public const char MinHighSurrogate = '\uD800';

    /// <summary>
    /// The maximum value of a Unicode low-surrogate code unit in the UTF-16 encoding,
    /// constant '\uDFFF'. A low-surrogate is also known as a trailing-surrogate.
    /// </summary>
    public const char MaxLowSurrogate = '\uDFFF';

    /// <summary>
    /// The minimum value of a Unicode low-surrogate code unit in the UTF-16 encoding,
    /// constant '\uDC00'. A low-surrogate is also known as a trailing-surrogate.
    /// </summary>
    public const char MinLowSurrogate = '\uDC00';

    /// <summary>
    /// The minimum value of a
    /// <a href="http://www.unicode.org/glossary/#supplementary_code_point">
    /// Unicode supplementary code point
    /// </a>
    /// , constant {@code U+10000}.
    /// </summary>
    public const int MinSupplementaryCodePoint = 0x010000;

    /// <summary>
    /// The minimum radix available for conversion to and from strings. The constant value
    /// of this field is the smallest value permitted for the radix argument in radix-conversion
    /// methods such as the <tt>digit</tt> method, the <tt>forDigit</tt> method, and the
    /// <tt>toString</tt> method of class <tt>Integer</tt>.
    /// </summary>
    public const int MinRadix = 2;

    /// <summary>
    /// The maximum radix available for conversion to and from strings. The constant value of
    /// this field is the largest value permitted for the radix argument in radix-conversion
    /// methods such as the <tt>digit</tt> method, the <tt>forDigit</tt> method, and the
    /// <tt>toString</tt> method of class <tt>Integer</tt>.
    /// </summary>
    public const int MaxRadix = 36;

    /// <summary>
    /// The constant value of this field is the smallest value of type char, <tt>'\u005Cu0000'</tt>.
    /// </summary>
    public const char MinValue = '\u0000';

    /// <summary>
    /// The constant value of this field is the largest value of type char, <tt>'\u005CuFFFF'</tt>.
    /// </summary>
    public const char MaxValue = '\uFFFF';

    /// <summary>
    /// General category "Cn" in the Unicode specification.
    /// </summary>
    public const sbyte Unassigned = 0;

    /// <summary>
    /// General category "Lu" in the Unicode specification.
    /// </summary>
    public const sbyte UppercaseLetter = 1;

    /// <summary>
    /// General category "Ll" in the Unicode specification.
    /// </summary>
    public const sbyte LowercaseLetter = 2;

    /// <summary>
    /// General category "Lt" in the Unicode specification.
    /// </summary>
    public const sbyte TitlecaseLetter = 3;

    /// <summary>
    /// General category "Lm" in the Unicode specification.
    /// </summary>
    public const sbyte ModifierLetter = 4;

    /// <summary>
    /// General category "Lo" in the Unicode specification.
    /// </summary>
    public const sbyte OtherLetter = 5;

    /// <summary>
    /// General category "Mn" in the Unicode specification.
    /// </summary>
    public const sbyte NonSpacingMark = 6;

    /// <summary>
    /// General category "Me" in the Unicode specification.
    /// </summary>
    public const sbyte EnclosingMark = 7;

    /// <summary>
    /// General category "Mc" in the Unicode specification.
    /// </summary>
    public const sbyte CombiningSpacingMark = 8;

    /// <summary>
    /// General category "Nd" in the Unicode specification.
    /// </summary>
    public const sbyte DecimalDigitNumber = 9;

    /// <summary>
    /// General category "Nl" in the Unicode specification.
    /// </summary>
    public const sbyte LetterNumber = 10;

    /// <summary>
    /// General category "No" in the Unicode specification.
    /// </summary>
    public const sbyte OtherNumber = 11;

    /// <summary>
    /// General category "Zs" in the Unicode specification.
    /// </summary>
    public const sbyte SpaceSeparator = 12;

    /// <summary>
    /// General category "Zl" in the Unicode specification.
    /// </summary>
    public const sbyte LineSeparator = 13;

    /// <summary>
    /// General category "Zp" in the Unicode specification.
    /// </summary>
    public const sbyte ParagraphSeparator = 14;

    /// <summary>
    /// General category "Cc" in the Unicode specification.
    /// </summary>
    public const sbyte Control = 15;

    /// <summary>
    /// General category "Cf" in the Unicode specification.
    /// </summary>
    public const sbyte Format = 16;

    /// <summary>
    /// General category "Co" in the Unicode specification.
    /// </summary>
    public const sbyte PrivateUse = 18;

    /// <summary>
    /// General category "Cs" in the Unicode specification.
    /// </summary>
    public const sbyte Surrogate = 19;

    /// <summary>
    /// General category "Pd" in the Unicode specification.
    /// </summary>
    public const sbyte DashPunctuation = 20;

    /// <summary>
    /// General category "Ps" in the Unicode specification.
    /// </summary>
    public const sbyte StartPunctuation = 21;

    /// <summary>
    /// General category "Pe" in the Unicode specification.
    /// </summary>
    public const sbyte EndPunctuation = 22;

    /// <summary>
    /// General category "Pc" in the Unicode specification.
    /// </summary>
    public const sbyte ConnectorPunctuation = 23;

    /// <summary>
    /// General category "Po" in the Unicode specification.
    /// </summary>
    public const sbyte OtherPunctuation = 24;

    /// <summary>
    /// General category "Sm" in the Unicode specification.
    /// </summary>
    public const sbyte MathSymbol = 25;

    /// <summary>
    /// General category "Sc" in the Unicode specification.
    /// </summary>
    public const sbyte CurrencySymbol = 26;

    /// <summary>
    /// General category "Sk" in the Unicode specification.
    /// </summary>
    public const sbyte ModifierSymbol = 27;

    /// <summary>
    /// General category "So" in the Unicode specification.
    /// </summary>
    public const sbyte OtherSymbol = 28;

    /// <summary>
    /// General category "Pi" in the Unicode specification.
    /// </summary>
    public const sbyte InitialQuotePunctuation = 29;

    /// <summary>
    /// General category "Pf" in the Unicode specification.
    /// </summary>
    public const sbyte FinalQuotePunctuation = 30;

    /// <summary>
    /// Error flag. Use int (code point) to avoid confusion with U+FFFF.
    /// </summary>
    public const int Error = unchecked( ( int )0xFFFFFFFF );

    /// <summary>
    /// Undefined bidirectional character type. Undefined {@code char}
    /// values have undefined directionality in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityUndefined = -1;

    /// <summary>
    /// Strong bidirectional character type "L" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityLeftToRight = 0;

    /// <summary>
    /// Strong bidirectional character type "R" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityRightToLeft = 1;

    /// <summary>
    /// Strong bidirectional character type "AL" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityRightToLeftArabic = 2;

    /// <summary>
    /// Weak bidirectional character type "EN" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityEuropeanNumber = 3;

    /// <summary>
    /// Weak bidirectional character type "ES" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityEuropeanNumberSeparator = 4;

    /// <summary>
    /// Weak bidirectional character type "ET" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityEuropeanNumberTerminator = 5;

    /// <summary>
    /// Weak bidirectional character type "AN" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityArabicNumber = 6;

    /// <summary>
    /// Weak bidirectional character type "CS" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityCommonNumberSeparator = 7;

    /// <summary>
    /// Weak bidirectional character type "NSM" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityNonspacingMark = 8;

    /// <summary>
    /// Weak bidirectional character type "BN" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityBoundaryNeutral = 9;

    /// <summary>
    /// Neutral bidirectional character type "B" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityParagraphSeparator = 10;

    /// <summary>
    /// Neutral bidirectional character type "S" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalitySegmentSeparator = 11;

    /// <summary>
    /// Neutral bidirectional character type "WS" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityWhitespace = 12;

    /// <summary>
    /// Neutral bidirectional character type "ON" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityOtherNeutrals = 13;

    /// <summary>
    /// Strong bidirectional character type "LRE" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityLeftToRightEmbedding = 14;

    /// <summary>
    /// Strong bidirectional character type "LRO" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityLeftToRightOverride = 15;

    /// <summary>
    /// Strong bidirectional character type "RLE" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityRightToLeftEmbedding = 16;

    /// <summary>
    /// Strong bidirectional character type "RLO" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityRightToLeftOverride = 17;

    /// <summary>
    /// Weak bidirectional character type "PDF" in the Unicode specification.
    /// </summary>
    public const sbyte DirectionalityPopDirectionalFormat = 18;

    /// <summary>
    /// The minimum value of a <a href="http://www.unicode.org/glossary/#code_point">
    /// Unicode code point</a> constant {@code U+0000}.
    /// </summary>
    public const int MinCodePoint = 0x000000;

    /// <summary>
    /// The maximum value of a <a href="http://www.unicode.org/glossary/#code_point">
    /// Unicode code point </a> constant {@code U+10FFFF}.
    /// </summary>
    public const int MaxCodePoint = 0X10FFFF;

    // ========================================================================
    // ========================================================================

    private readonly char _value;

    // ========================================================================

    /// <summary>
    /// Constructs a newly allocated <c>Character</c> object that represents the
    /// specified <c>char</c> value.
    /// </summary>
    /// <param name="value"> The value to be represented by the <c>Character</c> object. </param>
    public CharacterUtils( char value )
    {
        _value = value;
    }

    /// <summary>
    /// Determines the number of char values needed to represent the specified character
    /// (Unicode code point). If the specified character is equal to or greater than 0x10000,
    /// then the method returns 2. Otherwise, the method returns 1.
    /// <para>
    /// This method doesn't validate the specified character to be a valid Unicode code point.
    /// The caller must validate the character value using IsValidCodePoint if necessary.
    /// </para>
    /// </summary>
    /// <param name="codePoint"></param>
    /// <returns></returns>
    public static int CharCount( int codePoint )
    {
        return codePoint >= 0x010000 ? 2 : 1;
    }

    /// <summary>
    /// Returns the code point at the given index in the supplied string.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="position">The position.</param>
    /// <returns></returns>
    public static int CodePointAt( string str, int position )
    {
        return char.ConvertToUtf32( str, position );
    }

    /// <summary>
    /// Compares two <tt>char</tt> values numerically. The value returned is identical to
    /// what would be returned by: <c>CharHelper.ValueOf(x).CompareTo(CharHelper.ValueOf(y))</c>
    /// </summary>
    /// <param name="x"> the first <tt>char</tt> to compare </param>
    /// <param name="y"> the second <tt>char</tt> to compare </param>
    /// <returns>
    /// the value <c>0</c> if <c>x == y</c>; a value less than <c>0</c> if <c>x &lt; y</c>;
    /// and a value greater than <c>0</c> if <c>x > y</c>
    /// </returns>
    public static int Compare( char x, char y )
    {
        return x - y;
    }

    /// <summary>
    /// Returns the value obtained by reversing the order of the bytes in the
    /// specified <c>char</c> value.
    /// </summary>
    /// <param name="ch"> The <c>char</c> of which to reverse the byte order. </param>
    /// <returns>
    /// The value obtained by reversing (or, equivalently, swapping) the bytes in
    /// the specified <tt>char</tt> value.
    /// </returns>
    public static char ReverseBytes( char ch )
    {
        return ( char )( ( ( ch & 0xFF00 ) >> 8 ) | ( ch << 8 ) );
    }

    /// <summary>
    /// Determines if the specified character is a Unicode space character.
    /// A character is considered to be a space character if and only if
    /// it is specified to be a space character by the Unicode Standard. This
    /// method returns true if the character's general category type is any of
    /// the following:
    /// <list type="bullet">
    /// <item> <see cref="UnicodeCategory.SpaceSeparator"/> </item>
    /// <item> <see cref="UnicodeCategory.LineSeparator"/> </item>
    /// <item> <see cref="UnicodeCategory.ParagraphSeparator"/> </item>
    /// </list>
    /// </summary>
    /// <param name="ch">The character to be tested.</param>
    /// <returns><c>true</c> if the character is a space character; <c>false</c> otherwise.</returns>
    public static bool IsSpaceChar( char ch )
    {
        return IsSpaceChar( ( int )ch );
    }

    /// <summary>
    /// Determines if the specified character (Unicode code point) is a
    /// Unicode space character.  A character is considered to be a
    /// space character if and only if it is specified to be a space
    /// character by the Unicode Standard. This method returns true if
    /// the character's general category type is any of the following:
    /// <list type="bullet">
    /// <item> <see cref="UnicodeCategory.SpaceSeparator"/> </item>
    /// <item> <see cref="UnicodeCategory.LineSeparator"/> </item>
    /// <item> <see cref="UnicodeCategory.ParagraphSeparator"/> </item>
    /// </list>
    /// </summary>
    /// <param name="codePoint">The character (Unicode code point) to be tested.</param>
    /// <returns><c>true</c> if the character is a space character; <c>false</c> otherwise.</returns>
    public static bool IsSpaceChar( int codePoint )
    {
        UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory( codePoint );

        return ( category == UnicodeCategory.SpaceSeparator ) ||
               ( category == UnicodeCategory.LineSeparator ) ||
               ( category == UnicodeCategory.ParagraphSeparator );
    }

    /// <summary>
    /// Determines whether the specified code point is a valid
    /// <a href="http://www.unicode.org/glossary/#code_point">
    /// Unicode code point value</a>.
    /// </summary>
    /// <param name="codePoint">The Unicode code point to be tested.</param>
    /// <returns>
    /// <c>true</c> if the specified code point value is between
    /// <see cref="MinCodePoint"/> and
    /// <see cref="MaxCodePoint"/> inclusive;
    /// <c>false</c> otherwise.
    /// </returns>
    public static bool IsValidCodePoint( int codePoint )
    {
        int plane = codePoint >> 16;

        return plane < ( ( MaxCodePoint + 1 ) >> 16 );
    }

    /// <summary>
    /// Determines whether the specified character (Unicode code point)
    /// is in the <a href="#BMP">Basic Multilingual Plane (BMP)</a>.
    /// Such code points can be represented using a single <c>char</c>.
    /// </summary>
    /// <param name="codePoint">The character (Unicode code point) to be tested.</param>
    /// <returns>
    /// <c>true</c> if the specified code point is between <see cref="MinValue"/>
    /// and <see cref="MaxValue"/> inclusive; <c>false</c> otherwise.
    /// </returns>
    public static bool IsBmpCodePoint( int codePoint )
    {
        return ( codePoint >> 16 ) == 0;
    }
}

// ============================================================================
// ============================================================================



