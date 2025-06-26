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

using System.Globalization;

namespace LughSharp.Lugh.Graphics.Text;

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
    public const char MAX_HIGH_SURROGATE = '\uDBFF';

    /// <summary>
    /// The minimum value of a Unicode high-surrogate code unit in the UTF-16 encoding,
    /// constant '\uD800'. A high-surrogate is also known as a leading-surrogate.
    /// </summary>
    public const char MIN_HIGH_SURROGATE = '\uD800';

    /// <summary>
    /// The maximum value of a Unicode low-surrogate code unit in the UTF-16 encoding,
    /// constant '\uDFFF'. A low-surrogate is also known as a trailing-surrogate.
    /// </summary>
    public const char MAX_LOW_SURROGATE = '\uDFFF';

    /// <summary>
    /// The minimum value of a Unicode low-surrogate code unit in the UTF-16 encoding,
    /// constant '\uDC00'. A low-surrogate is also known as a trailing-surrogate.
    /// </summary>
    public const char MIN_LOW_SURROGATE = '\uDC00';

    /// <summary>
    /// The minimum value of a
    /// <a href="http://www.unicode.org/glossary/#supplementary_code_point">
    /// Unicode supplementary code point
    /// </a>
    /// , constant {@code U+10000}.
    /// </summary>
    public const int MIN_SUPPLEMENTARY_CODE_POINT = 0x010000;

    /// <summary>
    /// The minimum radix available for conversion to and from strings. The constant value
    /// of this field is the smallest value permitted for the radix argument in radix-conversion
    /// methods such as the <tt>digit</tt> method, the <tt>forDigit</tt> method, and the
    /// <tt>toString</tt> method of class <tt>Integer</tt>.
    /// </summary>
    public const int MIN_RADIX = 2;

    /// <summary>
    /// The maximum radix available for conversion to and from strings. The constant value of
    /// this field is the largest value permitted for the radix argument in radix-conversion
    /// methods such as the <tt>digit</tt> method, the <tt>forDigit</tt> method, and the
    /// <tt>toString</tt> method of class <tt>Integer</tt>.
    /// </summary>
    public const int MAX_RADIX = 36;

    /// <summary>
    /// The constant value of this field is the smallest value of type char, <tt>'\u005Cu0000'</tt>.
    /// </summary>
    public const char MIN_VALUE = '\u0000';

    /// <summary>
    /// The constant value of this field is the largest value of type char, <tt>'\u005CuFFFF'</tt>.
    /// </summary>
    public const char MAX_VALUE = '\uFFFF';

    /// <summary>
    /// General category "Cn" in the Unicode specification.
    /// </summary>
    public const sbyte UNASSIGNED = 0;

    /// <summary>
    /// General category "Lu" in the Unicode specification.
    /// </summary>
    public const sbyte UPPERCASE_LETTER = 1;

    /// <summary>
    /// General category "Ll" in the Unicode specification.
    /// </summary>
    public const sbyte LOWERCASE_LETTER = 2;

    /// <summary>
    /// General category "Lt" in the Unicode specification.
    /// </summary>
    public const sbyte TITLECASE_LETTER = 3;

    /// <summary>
    /// General category "Lm" in the Unicode specification.
    /// </summary>
    public const sbyte MODIFIER_LETTER = 4;

    /// <summary>
    /// General category "Lo" in the Unicode specification.
    /// </summary>
    public const sbyte OTHER_LETTER = 5;

    /// <summary>
    /// General category "Mn" in the Unicode specification.
    /// </summary>
    public const sbyte NON_SPACING_MARK = 6;

    /// <summary>
    /// General category "Me" in the Unicode specification.
    /// </summary>
    public const sbyte ENCLOSING_MARK = 7;

    /// <summary>
    /// General category "Mc" in the Unicode specification.
    /// </summary>
    public const sbyte COMBINING_SPACING_MARK = 8;

    /// <summary>
    /// General category "Nd" in the Unicode specification.
    /// </summary>
    public const sbyte DECIMAL_DIGIT_NUMBER = 9;

    /// <summary>
    /// General category "Nl" in the Unicode specification.
    /// </summary>
    public const sbyte LETTER_NUMBER = 10;

    /// <summary>
    /// General category "No" in the Unicode specification.
    /// </summary>
    public const sbyte OTHER_NUMBER = 11;

    /// <summary>
    /// General category "Zs" in the Unicode specification.
    /// </summary>
    public const sbyte SPACE_SEPARATOR = 12;

    /// <summary>
    /// General category "Zl" in the Unicode specification.
    /// </summary>
    public const sbyte LINE_SEPARATOR = 13;

    /// <summary>
    /// General category "Zp" in the Unicode specification.
    /// </summary>
    public const sbyte PARAGRAPH_SEPARATOR = 14;

    /// <summary>
    /// General category "Cc" in the Unicode specification.
    /// </summary>
    public const sbyte CONTROL = 15;

    /// <summary>
    /// General category "Cf" in the Unicode specification.
    /// </summary>
    public const sbyte FORMAT = 16;

    /// <summary>
    /// General category "Co" in the Unicode specification.
    /// </summary>
    public const sbyte PRIVATE_USE = 18;

    /// <summary>
    /// General category "Cs" in the Unicode specification.
    /// </summary>
    public const sbyte SURROGATE = 19;

    /// <summary>
    /// General category "Pd" in the Unicode specification.
    /// </summary>
    public const sbyte DASH_PUNCTUATION = 20;

    /// <summary>
    /// General category "Ps" in the Unicode specification.
    /// </summary>
    public const sbyte START_PUNCTUATION = 21;

    /// <summary>
    /// General category "Pe" in the Unicode specification.
    /// </summary>
    public const sbyte END_PUNCTUATION = 22;

    /// <summary>
    /// General category "Pc" in the Unicode specification.
    /// </summary>
    public const sbyte CONNECTOR_PUNCTUATION = 23;

    /// <summary>
    /// General category "Po" in the Unicode specification.
    /// </summary>
    public const sbyte OTHER_PUNCTUATION = 24;

    /// <summary>
    /// General category "Sm" in the Unicode specification.
    /// </summary>
    public const sbyte MATH_SYMBOL = 25;

    /// <summary>
    /// General category "Sc" in the Unicode specification.
    /// </summary>
    public const sbyte CURRENCY_SYMBOL = 26;

    /// <summary>
    /// General category "Sk" in the Unicode specification.
    /// </summary>
    public const sbyte MODIFIER_SYMBOL = 27;

    /// <summary>
    /// General category "So" in the Unicode specification.
    /// </summary>
    public const sbyte OTHER_SYMBOL = 28;

    /// <summary>
    /// General category "Pi" in the Unicode specification.
    /// </summary>
    public const sbyte INITIAL_QUOTE_PUNCTUATION = 29;

    /// <summary>
    /// General category "Pf" in the Unicode specification.
    /// </summary>
    public const sbyte FINAL_QUOTE_PUNCTUATION = 30;

    /// <summary>
    /// Error flag. Use int (code point) to avoid confusion with U+FFFF.
    /// </summary>
    public const int ERROR = unchecked( ( int )0xFFFFFFFF );

    /// <summary>
    /// Undefined bidirectional character type. Undefined {@code char}
    /// values have undefined directionality in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_UNDEFINED = -1;

    /// <summary>
    /// Strong bidirectional character type "L" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_LEFT_TO_RIGHT = 0;

    /// <summary>
    /// Strong bidirectional character type "R" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_RIGHT_TO_LEFT = 1;

    /// <summary>
    /// Strong bidirectional character type "AL" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_RIGHT_TO_LEFT_ARABIC = 2;

    /// <summary>
    /// Weak bidirectional character type "EN" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_EUROPEAN_NUMBER = 3;

    /// <summary>
    /// Weak bidirectional character type "ES" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_EUROPEAN_NUMBER_SEPARATOR = 4;

    /// <summary>
    /// Weak bidirectional character type "ET" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_EUROPEAN_NUMBER_TERMINATOR = 5;

    /// <summary>
    /// Weak bidirectional character type "AN" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_ARABIC_NUMBER = 6;

    /// <summary>
    /// Weak bidirectional character type "CS" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_COMMON_NUMBER_SEPARATOR = 7;

    /// <summary>
    /// Weak bidirectional character type "NSM" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_NONSPACING_MARK = 8;

    /// <summary>
    /// Weak bidirectional character type "BN" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_BOUNDARY_NEUTRAL = 9;

    /// <summary>
    /// Neutral bidirectional character type "B" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_PARAGRAPH_SEPARATOR = 10;

    /// <summary>
    /// Neutral bidirectional character type "S" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_SEGMENT_SEPARATOR = 11;

    /// <summary>
    /// Neutral bidirectional character type "WS" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_WHITESPACE = 12;

    /// <summary>
    /// Neutral bidirectional character type "ON" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_OTHER_NEUTRALS = 13;

    /// <summary>
    /// Strong bidirectional character type "LRE" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_LEFT_TO_RIGHT_EMBEDDING = 14;

    /// <summary>
    /// Strong bidirectional character type "LRO" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_LEFT_TO_RIGHT_OVERRIDE = 15;

    /// <summary>
    /// Strong bidirectional character type "RLE" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_RIGHT_TO_LEFT_EMBEDDING = 16;

    /// <summary>
    /// Strong bidirectional character type "RLO" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_RIGHT_TO_LEFT_OVERRIDE = 17;

    /// <summary>
    /// Weak bidirectional character type "PDF" in the Unicode specification.
    /// </summary>
    public const sbyte DIRECTIONALITY_POP_DIRECTIONAL_FORMAT = 18;

    /// <summary>
    /// The minimum value of a <a href="http://www.unicode.org/glossary/#code_point">
    /// Unicode code point</a> constant {@code U+0000}.
    /// </summary>
    public const int MIN_CODE_POINT = 0x000000;

    /// <summary>
    /// The maximum value of a <a href="http://www.unicode.org/glossary/#code_point">
    /// Unicode code point </a> constant {@code U+10FFFF}.
    /// </summary>
    public const int MAX_CODE_POINT = 0X10FFFF;

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
        var category = CharUnicodeInfo.GetUnicodeCategory( codePoint );

        return ( category == UnicodeCategory.SpaceSeparator ) ||
               ( category == UnicodeCategory.LineSeparator ) ||
               ( category == UnicodeCategory.ParagraphSeparator );
    }

//    /// <summary>
//    /// Returns the Unicode name of the specified character <c>codePoint</c>, or null if the
//    /// code point is <see cref="UNASSIGNED"/>.
//    /// <para>
//    /// Note: if the specified character is not assigned a name by the <i>UnicodeData</i> file
//    /// (part of the Unicode Character Database maintained by the Unicode Consortium), the returned
//    /// name is the same as the result of expression.
//    /// </para>
//    /// <br/>
//    /// <c>
//    ///     Character.UnicodeBlock.of(codePoint).toString().replace('_', ' ')
//    ///     + " " + codePoint.ToString( "X" ).ToUpper();
//    /// </c>
//    /// </summary>
//    /// <param name="codePoint"> the character (Unicode code point). </param>
//    /// <returns>
//    /// the Unicode name of the specified character, or null if the code point is unassigned. </returns>
//    /// <exception cref="ArgumentException">
//    /// if the specified <c>codePoint</c> is not a valid Unicode code point.
//    /// </exception>
//    public static string GetName( int codePoint )
//    {
//        if ( !IsValidCodePoint( codePoint ) )
//        {
//            throw new ArgumentException( "Invalid codepoint submitted" );
//        }
//
//        var name = CharacterName.Get( codePoint );
//
//        if ( name != null )
//        {
//            return name;
//        }
//
//        if ( GetCharacterType( codePoint ) == UNASSIGNED )
//        {
//            return string.Empty;
//        }
//
//        var block = UnicodeBlock.Of( codePoint );
//
//        if ( block != null )
//        {
//            return block.ToString().Replace( '_', ' ' ) + " " + StringUtils.ToHexString( codePoint );
//        }
//
//        // should never come here
//        return StringUtils.ToHexString( codePoint );
//    }

//    public static int GetCharacterType( int codePoint )
//    {
//        return CharacterData.of( codePoint ).getType( codePoint );
//    }

    /// <summary>
    /// Determines whether the specified code point is a valid
    /// <a href="http://www.unicode.org/glossary/#code_point">
    /// Unicode code point value</a>.
    /// </summary>
    /// <param name="codePoint">The Unicode code point to be tested.</param>
    /// <returns>
    /// <c>true</c> if the specified code point value is between
    /// <see cref="CharacterUtils.MIN_CODE_POINT"/> and
    /// <see cref="CharacterUtils.MAX_CODE_POINT"/> inclusive;
    /// <c>false</c> otherwise.
    /// </returns>
    public static bool IsValidCodePoint( int codePoint )
    {
        // Optimized form of:
        //     codePoint >= MIN_CODE_POINT && codePoint <= MAX_CODE_POINT
        var plane = codePoint >> 16;

        return plane < ( ( MAX_CODE_POINT + 1 ) >> 16 );
    }

    /// <summary>
    /// Determines whether the specified character (Unicode code point)
    /// is in the <a href="#BMP">Basic Multilingual Plane (BMP)</a>.
    /// Such code points can be represented using a single <c>char</c>.
    /// </summary>
    /// <param name="codePoint">The character (Unicode code point) to be tested.</param>
    /// <returns>
    /// <c>true</c> if the specified code point is between
    /// <see cref="CharacterUtils.MIN_VALUE"/> and <see cref="CharacterUtils.MAX_VALUE"/> inclusive;
    /// <c>false</c> otherwise.
    /// </returns>
    public static bool IsBmpCodePoint( int codePoint )
    {
        // Optimized form of:
        //     codePoint >= MIN_VALUE && codePoint <= MAX_VALUE
        // We consistently use logical shift (>>) to facilitate
        // additional runtime optimizations.
        return ( codePoint >> 16 ) == 0; // C# uses >> for unsigned right shift for ints.
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class CharacterCache
    {
        public static readonly CharacterUtils[] Cache = new CharacterUtils[ 127 + 1 ];

        static CharacterCache()
        {
            for ( var i = 0; i < Cache.Length; i++ )
            {
                Cache[ i ] = new CharacterUtils( ( char )i );
            }
        }

        private CharacterCache()
        {
        }
    }
}