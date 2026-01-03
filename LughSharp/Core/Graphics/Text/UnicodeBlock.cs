// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

using System.Globalization;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Graphics.Text;

/// <summary>
/// A family of character subsets representing the character blocks in the
/// Unicode specification. Character blocks generally define characters
/// used for a specific script or purpose. A character is contained by
/// at most one Unicode block.
/// </summary>
[PublicAPI]
public class UnicodeBlock : Subset
{
    private static Dictionary< string, UnicodeBlock > _map = new( 256 );

    // ========================================================================

    /// <summary>
    /// Creates a UnicodeBlock with the given identifier name.
    /// This name must be the same as the block identifier.
    /// </summary>
    private UnicodeBlock( string idName ) : base( idName )
    {
        _map[ idName ] = this;
    }

    /// <summary>
    /// Creates a UnicodeBlock with the given identifier name and
    /// alias name.
    /// </summary>
    private UnicodeBlock( string idName, string alias ) : this( idName )
    {
        _map[ alias ] = this;
    }

    /// <summary>
    /// Creates a UnicodeBlock with the given identifier name and
    /// alias names.
    /// </summary>
    private UnicodeBlock( string idName, params string[] aliases ) : this( idName )
    {
        foreach ( var alias in aliases )
        {
            _map[ alias ] = this;
        }
    }

    // ========================================================================

    /// <summary>
    /// Constant for the "Basic Latin" Unicode character block.
    /// </summary>
    public static UnicodeBlock BasicLatin { get; } = new( "Basic_Latin",
                                                          "Basic Latin",
                                                          "BasicLatin" );

    /// <summary>
    /// Constant for the "Latin-1 Supplement" Unicode character block.
    /// </summary>
    public static UnicodeBlock Latin1Supplement { get; } = new( "Latin_1_Supplement",
                                                                "Latin-1 Supplement",
                                                                "Latin1Supplement" );

    /// <summary>
    /// Constant for the "Latin Extended-A" Unicode character block.
    /// </summary>
    public static UnicodeBlock LatinExtendedA { get; } = new( "Latin_Extended_A",
                                                              "Latin Extended-A",
                                                              "LatinExtendedA" );

    /// <summary>
    /// Constant for the "Latin Extended-B" Unicode character block.
    /// </summary>
    public static UnicodeBlock LatinExtendedB { get; } = new( "Latin_Extended_B",
                                                              "Latin Extended-B",
                                                              "LatinExtendedB" );

    /// <summary>
    /// Constant for the "IPA Extensions" Unicode character block.
    /// </summary>
    public static UnicodeBlock IpaExtensions { get; } = new( "Ipa_Extensions",
                                                             "Ipa Extensions",
                                                             "IpaExtensions" );

    /// <summary>
    /// Constant for the "Spacing Modifier Letters" Unicode character block.
    /// </summary>
    public static UnicodeBlock SpacingModifierLetters { get; } = new( "Spacing_Modifier_Letters",
                                                                      "Spacing Modifier Letters",
                                                                      "SpacingModifierLetters" );

    /// <summary>
    /// Constant for the "Combining Diacritical Marks" Unicode character block.
    /// </summary>
    public static UnicodeBlock CombiningDiacriticalMarks { get; } =
        new( "COMBINING_DIACRITICAL_MARKS",
             "COMBINING DIACRITICAL MARKS",
             "COMBININGDIACRITICALMARKS" );

    /// <summary>
    /// Constant for the "Greek and Coptic" Unicode character block.
    /// This block was previously known as the "Greek" block.
    /// </summary>
    public static UnicodeBlock Greek { get; } =
        new( "GREEK",
             "GREEK AND COPTIC",
             "GREEKANDCOPTIC" );

    /// <summary>
    /// Constant for the "Cyrillic" Unicode character block.
    /// </summary>
    public static UnicodeBlock Cyrillic { get; } = new( "Cyrillic" );

    /// <summary>
    /// Constant for the "Armenian" Unicode character block.
    /// </summary>
    public static UnicodeBlock Armenian { get; } = new( "ARMENIAN" );

    /// <summary>
    /// Constant for the "Hebrew" Unicode character block.
    /// </summary>
    public static UnicodeBlock Hebrew { get; } = new( "HEBREW" );

    /// <summary>
    /// Constant for the "Arabic" Unicode character block.
    /// </summary>
    public static UnicodeBlock Arabic { get; } = new( "ARABIC" );

    /// <summary>
    /// Constant for the "Devanagari" Unicode character block.
    /// </summary>
    public static UnicodeBlock Devanagari { get; } = new( "DEVANAGARI" );

    /// <summary>
    /// Constant for the "Bengali" Unicode character block.
    /// </summary>
    public static UnicodeBlock Bengali { get; } = new( "BENGALI" );

    /// <summary>
    /// Constant for the "Gurmukhi" Unicode character block.
    /// </summary>
    public static UnicodeBlock Gurmukhi { get; } = new( "GURMUKHI" );

    /// <summary>
    /// Constant for the "Gujarati" Unicode character block.
    /// </summary>
    public static UnicodeBlock Gujarati { get; } = new( "GUJARATI" );

    /// <summary>
    /// Constant for the "Oriya" Unicode character block.
    /// </summary>
    public static UnicodeBlock Oriya { get; } = new( "ORIYA" );

    /// <summary>
    /// Constant for the "Tamil" Unicode character block.
    /// </summary>
    public static UnicodeBlock Tamil { get; } = new( "TAMIL" );

    /// <summary>
    /// Constant for the "Telugu" Unicode character block.
    /// </summary>
    public static UnicodeBlock Telugu { get; } = new( "TELUGU" );

    /// <summary>
    /// Constant for the "Kannada" Unicode character block.
    /// </summary>
    public static UnicodeBlock Kannada { get; } = new( "KANNADA" );

    /// <summary>
    /// Constant for the "Malayalam" Unicode character block.
    /// </summary>
    public static UnicodeBlock Malayalam { get; } = new( "MALAYALAM" );

    /// <summary>
    /// Constant for the "Thai" Unicode character block.
    /// </summary>
    public static UnicodeBlock Thai { get; } = new( "THAI" );

    /// <summary>
    /// Constant for the "Lao" Unicode character block.
    /// </summary>
    public static UnicodeBlock Lao { get; } = new( "LAO" );

    /// <summary>
    /// Constant for the "Tibetan" Unicode character block.
    /// </summary>
    public static UnicodeBlock Tibetan { get; } = new( "TIBETAN" );

    /// <summary>
    /// Constant for the "Georgian" Unicode character block.
    /// </summary>
    public static UnicodeBlock Georgian { get; } = new( "GEORGIAN" );

    /// <summary>
    /// Constant for the "Hangul Jamo" Unicode character block.
    /// </summary>
    public static UnicodeBlock HangulJamo { get; } = new( "HANGUL_JAMO",
                                                          "HANGUL JAMO",
                                                          "HANGULJAMO" );

    /// <summary>
    /// Constant for the "Latin Extended Additional" Unicode character block.
    /// </summary>
    public static UnicodeBlock LatinExtendedAdditional { get; } = new( "Latin_Extended_ADDITIONAL",
                                                                       "Latin Extended ADDITIONAL",
                                                                       "LatinExtendedADDITIONAL" );

    /// <summary>
    /// Constant for the "Greek Extended" Unicode character block.
    /// </summary>
    public static UnicodeBlock GreekExtended { get; } = new( "GREEK_Extended",
                                                             "GREEK Extended",
                                                             "GREEKExtended" );

    /// <summary>
    /// Constant for the "General Punctuation" Unicode character block.
    /// </summary>
    public static UnicodeBlock GeneralPunctuation { get; } = new( "GENERAL_PUNCTUATION",
                                                                  "GENERAL PUNCTUATION",
                                                                  "GENERALPUNCTUATION" );

    /// <summary>
    /// Constant for the "Superscripts and Subscripts" Unicode character
    /// block.
    /// </summary>
    public static UnicodeBlock SuperscriptsAndSubscripts { get; } = new( "SUPERSCRIPTS_AND_SUBSCRIPTS",
                                                                         "SUPERSCRIPTS AND SUBSCRIPTS",
                                                                         "SUPERSCRIPTSANDSUBSCRIPTS" );

    /// <summary>
    /// Constant for the "Currency Symbols" Unicode character block.
    /// </summary>
    public static UnicodeBlock CurrencySymbols { get; } = new( "CURRENCY_SYMBOLS",
                                                               "CURRENCY SYMBOLS",
                                                               "CURRENCYSYMBOLS" );

    /// <summary>
    /// Constant for the "Combining Diacritical Marks for Symbols" Unicode
    /// character block.
    /// <p>
    /// This block was previously known as "Combining Marks for Symbols".
    /// </p>
    /// </summary>
    public static UnicodeBlock CombiningMarksForSymbols { get; } = new( "COMBINING_MARKS_FOR_SYMBOLS",
                                                                        "COMBINING DIACRITICAL MARKS FOR SYMBOLS",
                                                                        "COMBININGDIACRITICALMARKSFORSYMBOLS",
                                                                        "COMBINING MARKS FOR SYMBOLS",
                                                                        "COMBININGMARKSFORSYMBOLS" );

    /// <summary>
    /// Constant for the "Letterlike Symbols" Unicode character block.
    /// </summary>
    public static UnicodeBlock LetterlikeSymbols { get; } =
        new( "LETTERLIKE_SYMBOLS",
             "LETTERLIKE SYMBOLS",
             "LETTERLIKESYMBOLS" );

    /// <summary>
    /// Constant for the "Number Forms" Unicode character block.
    /// </summary>
    public static UnicodeBlock NumberForms { get; } =
        new( "NUMBER_FORMS",
             "NUMBER FORMS",
             "NUMBERFORMS" );

    /// <summary>
    /// Constant for the "Arrows" Unicode character block.
    /// </summary>
    public static UnicodeBlock Arrows { get; } = new( "ARROWS" );

    /// <summary>
    /// Constant for the "Mathematical Operators" Unicode character block.
    /// </summary>
    public static UnicodeBlock MathematicalOperators { get; } =
        new( "MATHEMATICAL_OPERATORS",
             "MATHEMATICAL OPERATORS",
             "MATHEMATICALOPERATORS" );

    /// <summary>
    /// Constant for the "Miscellaneous Technical" Unicode character block.
    /// </summary>
    public static UnicodeBlock MiscellaneousTechnical { get; } =
        new( "MISCELLANEOUS_TECHNICAL",
             "MISCELLANEOUS TECHNICAL",
             "MISCELLANEOUSTECHNICAL" );

    /// <summary>
    /// Constant for the "Control Pictures" Unicode character block.
    /// </summary>
    public static UnicodeBlock ControlPictures { get; } =
        new( "CONTROL_PICTURES",
             "CONTROL PICTURES",
             "CONTROLPICTURES" );

    /// <summary>
    /// Constant for the "Optical Character Recognition" Unicode character block.
    /// </summary>
    public static UnicodeBlock OpticalCharacterRecognition { get; } =
        new( "OPTICAL_CHARACTER_RECOGNITION",
             "OPTICAL CHARACTER RECOGNITION",
             "OPTICALCHARACTERRECOGNITION" );

    /// <summary>
    /// Constant for the "Enclosed Alphanumerics" Unicode character block.
    /// </summary>
    public static UnicodeBlock EnclosedAlphanumerics { get; } =
        new( "ENCLOSED_ALPHANUMERICS",
             "ENCLOSED ALPHANUMERICS",
             "ENCLOSEDALPHANUMERICS" );

    /// <summary>
    /// Constant for the "Box Drawing" Unicode character block.
    /// </summary>
    public static UnicodeBlock BoxDrawing { get; } =
        new( "BOX_DRAWING",
             "BOX DRAWING",
             "BOXDRAWING" );

    /// <summary>
    /// Constant for the "Block Elements" Unicode character block.
    /// </summary>
    public static UnicodeBlock BlockElements { get; } =
        new( "BLOCK_ELEMENTS",
             "BLOCK ELEMENTS",
             "BLOCKELEMENTS" );

    /// <summary>
    /// Constant for the "Geometric Shapes" Unicode character block.
    /// </summary>
    public static UnicodeBlock GeometricShapes { get; } =
        new( "GEOMETRIC_SHAPES",
             "GEOMETRIC SHAPES",
             "GEOMETRICSHAPES" );

    /// <summary>
    /// Constant for the "Miscellaneous Symbols" Unicode character block.
    /// </summary>
    public static UnicodeBlock MiscellaneousSymbols { get; } =
        new( "MISCELLANEOUS_SYMBOLS",
             "MISCELLANEOUS SYMBOLS",
             "MISCELLANEOUSSYMBOLS" );

    /// <summary>
    /// Constant for the "Dingbats" Unicode character block.
    /// </summary>
    public static UnicodeBlock Dingbats { get; } = new( "DINGBATS" );

    /// <summary>
    /// Constant for the "CJK Symbols and Punctuation" Unicode character block.
    /// </summary>
    public static UnicodeBlock CjkSymbolsAndPunctuation { get; } =
        new( "CJK_SYMBOLS_AND_PUNCTUATION",
             "CJK SYMBOLS AND PUNCTUATION",
             "CJKSYMBOLSANDPUNCTUATION" );

    /// <summary>
    /// Constant for the "Hiragana" Unicode character block.
    /// </summary>
    public static UnicodeBlock Hiragana { get; } = new( "HIRAGANA" );

    /// <summary>
    /// Constant for the "Katakana" Unicode character block.
    /// </summary>
    public static UnicodeBlock Katakana { get; } = new( "KATAKANA" );

    /// <summary>
    /// Constant for the "Bopomofo" Unicode character block.
    /// </summary>
    public static UnicodeBlock Bopomofo { get; } = new( "BOPOMOFO" );

    /// <summary>
    /// Constant for the "Hangul Compatibility Jamo" Unicode character block.
    /// </summary>
    public static UnicodeBlock HangulCompatibilityJamo { get; } =
        new( "HANGUL_COMPATIBILITY_JAMO",
             "HANGUL COMPATIBILITY JAMO",
             "HANGULCOMPATIBILITYJAMO" );

    /// <summary>
    /// Constant for the "Kanbun" Unicode character block.
    /// </summary>
    public static UnicodeBlock Kanbun { get; } = new( "KANBUN" );

    /// <summary>
    /// Constant for the "Enclosed CJK Letters and Months" Unicode character block.
    /// </summary>
    public static UnicodeBlock EnclosedCjkLettersAndMonths { get; } =
        new( "ENCLOSED_CJK_LETTERS_AND_MONTHS",
             "ENCLOSED CJK LETTERS AND MONTHS",
             "ENCLOSEDCJKLETTERSANDMONTHS" );

    /// <summary>
    /// Constant for the "CJK Compatibility" Unicode character block.
    /// </summary>
    public static UnicodeBlock CjkCompatibility { get; } =
        new( "CJK_COMPATIBILITY",
             "CJK COMPATIBILITY",
             "CJKCOMPATIBILITY" );

    /// <summary>
    /// Constant for the "CJK Unified Ideographs" Unicode character block.
    /// </summary>
    public static UnicodeBlock CjkUnifiedIdeographs { get; } =
        new( "CJK_UNIFIED_IDEOGRAPHS",
             "CJK UNIFIED IDEOGRAPHS",
             "CJKUNIFIEDIDEOGRAPHS" );

    /// <summary>
    /// Constant for the "Hangul Syllables" Unicode character block.
    /// </summary>
    public static UnicodeBlock HangulSyllables { get; } =
        new( "HANGUL_SYLLABLES",
             "HANGUL SYLLABLES",
             "HANGULSYLLABLES" );

    /// <summary>
    /// Constant for the "Private Use Area" Unicode character block.
    /// </summary>
    public static UnicodeBlock PrivateUseArea { get; } =
        new( "PRIVATE_USE_AREA",
             "PRIVATE USE AREA",
             "PRIVATEUSEAREA" );

    /// <summary>
    /// Constant for the "CJK Compatibility Ideographs" Unicode character
    /// block.
    /// </summary>
    public static UnicodeBlock CjkCompatibilityIdeographs { get; } =
        new( "CJK_COMPATIBILITY_IDEOGRAPHS",
             "CJK COMPATIBILITY IDEOGRAPHS",
             "CJKCOMPATIBILITYIDEOGRAPHS" );

    /// <summary>
    /// Constant for the "Alphabetic Presentation Forms" Unicode character block.
    /// </summary>
    public static UnicodeBlock AlphabeticPresentationForms { get; } =
        new( "ALPHABETIC_PRESENTATION_FORMS",
             "ALPHABETIC PRESENTATION FORMS",
             "ALPHABETICPRESENTATIONFORMS" );

    /// <summary>
    /// Constant for the "Arabic Presentation Forms-A" Unicode character
    /// block.
    /// </summary>
    public static UnicodeBlock ArabicPresentationFormsA { get; } =
        new( "ARABIC_PRESENTATION_FORMS_A",
             "ARABIC PRESENTATION FORMS-A",
             "ARABICPRESENTATIONFORMS-A" );

    /// <summary>
    /// Constant for the "Combining Half Marks" Unicode character block.
    /// </summary>
    public static UnicodeBlock CombiningHalfMarks { get; } =
        new( "COMBINING_HALF_MARKS",
             "COMBINING HALF MARKS",
             "COMBININGHALFMARKS" );

    /// <summary>
    /// Constant for the "CJK Compatibility Forms" Unicode character block.
    /// </summary>
    public static UnicodeBlock CjkCompatibilityForms { get; } =
        new( "CJK_COMPATIBILITY_FORMS",
             "CJK COMPATIBILITY FORMS",
             "CJKCOMPATIBILITYFORMS" );

    /// <summary>
    /// Constant for the "Small Form Variants" Unicode character block.
    /// </summary>
    public static UnicodeBlock SmallFormVariants { get; } =
        new( "SMALL_FORM_VARIANTS",
             "SMALL FORM VARIANTS",
             "SMALLFORMVARIANTS" );

    /// <summary>
    /// Constant for the "Arabic Presentation Forms-B" Unicode character block.
    /// </summary>
    public static UnicodeBlock ArabicPresentationFormsB { get; } =
        new( "ARABIC_PRESENTATION_FORMS_B",
             "ARABIC PRESENTATION FORMS-B",
             "ARABICPRESENTATIONFORMS-B" );

    /// <summary>
    /// Constant for the "Halfwidth and Fullwidth Forms" Unicode character
    /// block.
    /// </summary>
    public static UnicodeBlock HalfwidthAndFullwidthForms { get; } =
        new( "HALFWIDTH_AND_FULLWIDTH_FORMS",
             "HALFWIDTH AND FULLWIDTH FORMS",
             "HALFWIDTHANDFULLWIDTHFORMS" );

    /// <summary>
    /// Constant for the "Specials" Unicode character block.
    /// </summary>
    public static UnicodeBlock Specials { get; } = new( "SPECIALS" );

    /// <summary>
    /// @deprecated As of J2SE 5, use {@link #HIGH_SURROGATES},
    ///             {@link #HIGH_PRIVATE_USE_SURROGATES}, and
    ///             {@link #LOW_SURROGATES}. These new constants match
    ///             the block definitions of the Unicode Standard.
    ///             The {@link #of(char)} and {@link #of(int)} methods
    ///             return the new constants, not SURROGATES_AREA.
    /// </summary>
    [Obsolete]
    public static UnicodeBlock SurrogatesArea { get; } = new( "SURROGATES_AREA" );

    /// <summary>
    /// Constant for the "Syriac" Unicode character block.
    /// </summary>
    public static UnicodeBlock Syriac { get; } = new( "SYRIAC" );

    /// <summary>
    /// Constant for the "Thaana" Unicode character block.
    /// </summary>
    public static UnicodeBlock Thaana { get; } = new( "THAANA" );

    /// <summary>
    /// Constant for the "Sinhala" Unicode character block.
    /// </summary>
    public static UnicodeBlock Sinhala { get; } = new( "SINHALA" );

    /// <summary>
    /// Constant for the "Myanmar" Unicode character block.
    /// </summary>
    public static UnicodeBlock Myanmar { get; } = new( "MYANMAR" );

    /// <summary>
    /// Constant for the "Ethiopic" Unicode character block.
    /// </summary>
    public static UnicodeBlock Ethiopic { get; } = new( "ETHIOPIC" );

    /// <summary>
    /// Constant for the "Cherokee" Unicode character block.
    /// </summary>
    public static UnicodeBlock Cherokee { get; } = new( "CHEROKEE" );

    /// <summary>
    /// Constant for the "Unified Canadian Aboriginal Syllabics" Unicode character block.
    /// </summary>
    public static UnicodeBlock UnifiedCanadianAboriginalSyllabics { get; } =
        new( "UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS",
             "UNIFIED CANADIAN ABORIGINAL SYLLABICS",
             "UNIFIEDCANADIANABORIGINALSYLLABICS" );

    /// <summary>
    /// Constant for the "Ogham" Unicode character block.
    /// </summary>
    public static UnicodeBlock Ogham { get; } = new( "OGHAM" );

    /// <summary>
    /// Constant for the "Runic" Unicode character block.
    /// </summary>
    public static UnicodeBlock Runic { get; } = new( "RUNIC" );

    /// <summary>
    /// Constant for the "Khmer" Unicode character block.
    /// </summary>
    public static UnicodeBlock Khmer { get; } = new( "KHMER" );

    /// <summary>
    /// Constant for the "Mongolian" Unicode character block.
    /// </summary>
    public static UnicodeBlock Mongolian { get; } = new( "MONGOLIAN" );

    /// <summary>
    /// Constant for the "Braille Patterns" Unicode character block.
    /// </summary>
    public static UnicodeBlock BraillePatterns { get; } =
        new( "BRAILLE_PATTERNS",
             "BRAILLE PATTERNS",
             "BRAILLEPATTERNS" );

    /// <summary>
    /// Constant for the "CJK Radicals Supplement" Unicode character block.
    /// </summary>
    public static UnicodeBlock CjkRadicalsSupplement { get; } =
        new( "CJK_RADICALS_Supplement",
             "CJK RADICALS Supplement",
             "CJKRADICALSSupplement" );

    /// <summary>
    /// Constant for the "Kangxi Radicals" Unicode character block.
    /// </summary>
    public static UnicodeBlock KangxiRadicals { get; } =
        new( "KANGXI_RADICALS",
             "KANGXI RADICALS",
             "KANGXIRADICALS" );

    /// <summary>
    /// Constant for the "Ideographic Description Characters" Unicode character block.
    /// </summary>
    public static UnicodeBlock IdeographicDescriptionCharacters { get; } =
        new( "IDEOGRAPHIC_DESCRIPTION_CHARACTERS",
             "IDEOGRAPHIC DESCRIPTION CHARACTERS",
             "IDEOGRAPHICDESCRIPTIONCHARACTERS" );

    /// <summary>
    /// Constant for the "Bopomofo Extended" Unicode character block.
    /// </summary>
    public static UnicodeBlock BopomofoExtended { get; } =
        new( "BOPOMOFO_Extended",
             "BOPOMOFO Extended",
             "BOPOMOFOExtended" );

    /// <summary>
    /// Constant for the "CJK Unified Ideographs Extension A" Unicode character block.
    /// </summary>
    public static UnicodeBlock CjkUnifiedIdeographsExtensionA { get; } =
        new( "CJK_UNIFIED_IDEOGRAPHS_EXTENSION_A",
             "CJK UNIFIED IDEOGRAPHS EXTENSION A",
             "CJKUNIFIEDIDEOGRAPHSEXTENSIONA" );

    /// <summary>
    /// Constant for the "Yi Syllables" Unicode character block.
    /// </summary>
    public static UnicodeBlock YiSyllables { get; } =
        new( "YI_SYLLABLES",
             "YI SYLLABLES",
             "YISYLLABLES" );

    /// <summary>
    /// Constant for the "Yi Radicals" Unicode character block.
    /// </summary>
    public static UnicodeBlock YiRadicals { get; } =
        new( "YI_RADICALS",
             "YI RADICALS",
             "YIRADICALS" );

    /// <summary>
    /// Constant for the "Cyrillic Supplementary" Unicode character block.
    /// </summary>
    public static UnicodeBlock CyrillicSupplementary { get; } =
        new( "Cyrillic_SupplementARY",
             "Cyrillic SupplementARY",
             "CyrillicSupplementARY",
             "Cyrillic Supplement",
             "CyrillicSupplement" );

    /// <summary>
    /// Constant for the "Tagalog" Unicode character block.
    /// </summary>
    public static UnicodeBlock Tagalog { get; } = new( "Tagalog" );

    /// <summary>
    /// Constant for the "Hanunoo" Unicode character block.
    /// </summary>
    public static UnicodeBlock Hanunoo { get; } = new( "Hanunoo" );

    /// <summary>
    /// Constant for the "Buhid" Unicode character block.
    /// </summary>
    public static UnicodeBlock Buhid { get; } = new( "Buhid" );

    /// <summary>
    /// Constant for the "Tagbanwa" Unicode character block.
    /// </summary>
    public static UnicodeBlock Tagbanwa { get; } = new( "Tagbanwa" );

    /// <summary>
    /// Constant for the "Limbu" Unicode character block.
    /// </summary>
    public static UnicodeBlock Limbu { get; } = new( "Limbu" );

    /// <summary>
    /// Constant for the "Tai Le" Unicode character block.
    /// </summary>
    public static UnicodeBlock TaiLE { get; } =
        new( "TAI_LE",
             "TAI LE",
             "TAILE" );

    /// <summary>
    /// Constant for the "Khmer Symbols" Unicode character block.
    /// </summary>
    public static UnicodeBlock KhmerSymbols { get; } =
        new( "KHMER_SYMBOLS",
             "KHMER SYMBOLS",
             "KHMERSYMBOLS" );

    /// <summary>
    /// Constant for the "Phonetic Extensions" Unicode character block.
    /// </summary>
    public static UnicodeBlock PhoneticExtensions { get; } =
        new( "PHONETIC_EXTENSIONS",
             "PHONETIC EXTENSIONS",
             "PHONETICEXTENSIONS" );

    /// <summary>
    /// Constant for the "Miscellaneous Mathematical Symbols-A" Unicode character block.
    /// </summary>
    public static UnicodeBlock MiscellaneousMathematicalSymbolsA { get; } =
        new( "MISCELLANEOUS_MATHEMATICAL_SYMBOLS_A",
             "MISCELLANEOUS MATHEMATICAL SYMBOLS-A",
             "MISCELLANEOUSMATHEMATICALSYMBOLS-A" );

    /// <summary>
    /// Constant for the "Supplemental Arrows-A" Unicode character block.
    /// </summary>
    public static UnicodeBlock SupplementalArrowsA { get; } =
        new( "SupplementAL_ARROWS_A",
             "SupplementAL ARROWS-A",
             "SupplementALARROWS-A" );

    /// <summary>
    /// Constant for the "Supplemental Arrows-B" Unicode character block.
    /// </summary>
    public static UnicodeBlock SupplementalArrowsB { get; } =
        new( "SupplementAL_ARROWS_B",
             "SupplementAL ARROWS-B",
             "SupplementALARROWS-B" );

    /// <summary>
    /// Constant for the "Miscellaneous Mathematical Symbols-B" Unicode
    /// character block.
    /// </summary>
    public static UnicodeBlock MiscellaneousMathematicalSymbolsB { get; } = new( "MISCELLANEOUS_MATHEMATICAL_SYMBOLS_B",
                                                                                 "MISCELLANEOUS MATHEMATICAL SYMBOLS-B",
                                                                                 "MISCELLANEOUSMATHEMATICALSYMBOLS-B" );

    /// <summary>
    /// Constant for the "Supplemental Mathematical Operators" Unicode
    /// character block.
    /// </summary>
    public static UnicodeBlock SupplementalMathematicalOperators { get; } = new( "SupplementAL_MATHEMATICAL_OPERATORS",
                                                                                 "SupplementAL MATHEMATICAL OPERATORS",
                                                                                 "SupplementALMATHEMATICALOPERATORS" );

    /// <summary>
    /// Constant for the "Miscellaneous Symbols and Arrows" Unicode character
    /// block.
    /// </summary>
    public static UnicodeBlock MiscellaneousSymbolsAndArrows { get; } = new( "MISCELLANEOUS_SYMBOLS_AND_ARROWS",
                                                                             "MISCELLANEOUS SYMBOLS AND ARROWS",
                                                                             "MISCELLANEOUSSYMBOLSANDARROWS" );

    /// <summary>
    /// Constant for the "Katakana Phonetic Extensions" Unicode character
    /// block.
    /// </summary>
    public static UnicodeBlock KatakanaPhoneticExtensions { get; } = new( "KATAKANA_PHONETIC_EXTENSIONS",
                                                                          "KATAKANA PHONETIC EXTENSIONS",
                                                                          "KATAKANAPHONETICEXTENSIONS" );

    /// <summary>
    /// Constant for the "Yijing Hexagram Symbols" Unicode character block.
    /// </summary>
    public static UnicodeBlock YijingHexagramSymbols { get; } = new( "YIJING_HEXAGRAM_SYMBOLS",
                                                                     "YIJING HEXAGRAM SYMBOLS",
                                                                     "YIJINGHEXAGRAMSYMBOLS" );

    /// <summary>
    /// Constant for the "Variation Selectors" Unicode character block.
    /// </summary>
    public static UnicodeBlock VariationSelectors { get; } = new( "VARIATION_SELECTORS",
                                                                  "VARIATION SELECTORS",
                                                                  "VARIATIONSELECTORS" );

    /// <summary>
    /// Constant for the "Linear B Syllabary" Unicode character block.
    /// </summary>
    public static UnicodeBlock LinearBSyllabary { get; } = new( "LINEAR_B_SYLLABARY",
                                                                "LINEAR B SYLLABARY",
                                                                "LINEARBSYLLABARY" );

    /// <summary>
    /// Constant for the "Linear B Ideograms" Unicode character block.
    /// </summary>
    public static UnicodeBlock LinearBIdeograms { get; } = new( "LINEAR_B_IDEOGRAMS",
                                                                "LINEAR B IDEOGRAMS",
                                                                "LINEARBIDEOGRAMS" );

    /// <summary>
    /// Constant for the "Aegean Numbers" Unicode character block.
    /// </summary>
    public static UnicodeBlock AegeanNumbers { get; } = new( "AEGEAN_NUMBERS",
                                                             "AEGEAN NUMBERS",
                                                             "AEGEANNUMBERS" );

    /// <summary>
    /// Constant for the "Old Italic" Unicode character block.
    /// </summary>
    public static UnicodeBlock OldItalic { get; } = new( "OLD_ITALIC",
                                                         "OLD ITALIC",
                                                         "OLDITALIC" );

    /// <summary>
    /// Constant for the "Gothic" Unicode character block.
    /// </summary>
    public static UnicodeBlock Gothic { get; } = new( "GOTHIC" );

    /// <summary>
    /// Constant for the "Ugaritic" Unicode character block.
    /// </summary>
    public static UnicodeBlock Ugaritic { get; } = new( "UGARITIC" );

    /// <summary>
    /// Constant for the "Deseret" Unicode character block.
    /// </summary>
    public static UnicodeBlock Deseret { get; } = new( "DESERET" );

    /// <summary>
    /// Constant for the "Shavian" Unicode character block.
    /// </summary>
    public static UnicodeBlock Shavian { get; } = new( "SHAVIAN" );

    /// <summary>
    /// Constant for the "Osmanya" Unicode character block.
    /// </summary>
    public static UnicodeBlock Osmanya { get; } = new( "OSMANYA" );

    /// <summary>
    /// Constant for the "Cypriot Syllabary" Unicode character block.
    /// </summary>
    public static UnicodeBlock CypriotSyllabary { get; } =
        new( "CYPRIOT_SYLLABARY",
             "CYPRIOT SYLLABARY",
             "CYPRIOTSYLLABARY" );

    /// <summary>
    /// Constant for the "Byzantine Musical Symbols" Unicode character block.
    /// </summary>
    public static UnicodeBlock ByzantineMusicalSymbols { get; } =
        new( "BYZANTINE_MUSICAL_SYMBOLS",
             "BYZANTINE MUSICAL SYMBOLS",
             "BYZANTINEMUSICALSYMBOLS" );

    /// <summary>
    /// Constant for the "Musical Symbols" Unicode character block.
    /// </summary>
    public static UnicodeBlock MusicalSymbols { get; } =
        new( "MUSICAL_SYMBOLS",
             "MUSICAL SYMBOLS",
             "MUSICALSYMBOLS" );

    /// <summary>
    /// Constant for the "Tai Xuan Jing Symbols" Unicode character block.
    /// </summary>
    public static UnicodeBlock TaiXuanJingSymbols { get; } =
        new( "TAI_XUAN_JING_SYMBOLS",
             "TAI XUAN JING SYMBOLS",
             "TAIXUANJINGSYMBOLS" );

    /// <summary>
    /// Constant for the "Mathematical Alphanumeric Symbols" Unicode
    /// character block.
    /// </summary>
    public static UnicodeBlock MathematicalAlphanumericSymbols { get; } =
        new( "MATHEMATICAL_ALPHANUMERIC_SYMBOLS",
             "MATHEMATICAL ALPHANUMERIC SYMBOLS",
             "MATHEMATICALALPHANUMERICSYMBOLS" );

    /// <summary>
    /// Constant for the "CJK Unified Ideographs Extension B" Unicode
    /// character block.
    /// </summary>
    public static UnicodeBlock CjkUnifiedIdeographsExtensionB { get; } =
        new( "CJK_UNIFIED_IDEOGRAPHS_EXTENSION_B",
             "CJK UNIFIED IDEOGRAPHS EXTENSION B",
             "CJKUNIFIEDIDEOGRAPHSEXTENSIONB" );

    /// <summary>
    /// Constant for the "CJK Compatibility Ideographs Supplement" Unicode character block.
    /// </summary>
    public static UnicodeBlock CjkCompatibilityIdeographsSupplement { get; } =
        new( "CJK_COMPATIBILITY_IDEOGRAPHS_Supplement",
             "CJK COMPATIBILITY IDEOGRAPHS Supplement",
             "CJKCOMPATIBILITYIDEOGRAPHSSupplement" );

    /// <summary>
    /// Constant for the "Tags" Unicode character block.
    /// </summary>
    public static UnicodeBlock Tags { get; } = new( "TAGS" );

    /// <summary>
    /// Constant for the "Variation Selectors Supplement" Unicode character
    /// block.
    /// </summary>
    public static UnicodeBlock VariationSelectorsSupplement { get; } =
        new( "VARIATION_SELECTORS_Supplement",
             "VARIATION SELECTORS Supplement",
             "VARIATIONSELECTORSSupplement" );

    /// <summary>
    /// Constant for the "Supplementary Private Use Area-A" Unicode character
    /// block.
    /// </summary>
    public static UnicodeBlock SupplementaryPrivateUseAreaA { get; } =
        new( "SupplementARY_PRIVATE_USE_AREA_A",
             "SupplementARY PRIVATE USE AREA-A",
             "SupplementARYPRIVATEUSEAREA-A" );

    /// <summary>
    /// Constant for the "Supplementary Private Use Area-B" Unicode character
    /// block.
    /// </summary>
    public static UnicodeBlock SupplementaryPrivateUseAreaB { get; } =
        new( "SupplementARY_PRIVATE_USE_AREA_B",
             "SupplementARY PRIVATE USE AREA-B",
             "SupplementARYPRIVATEUSEAREA-B" );

    /// <summary>
    /// Constant for the "High Surrogates" Unicode character block.
    /// This block represents codepoint values in the high surrogate
    /// range: U+D800 through U+DB7F
    /// </summary>
    public static UnicodeBlock HighSurrogates { get; } =
        new( "HIGH_SURROGATES",
             "HIGH SURROGATES",
             "HIGHSURROGATES" );

    /// <summary>
    /// Constant for the "High Private Use Surrogates" Unicode character
    /// block.
    /// This block represents codepoint values in the private use high
    /// surrogate range: U+DB80 through U+DBFF
    /// </summary>
    public static UnicodeBlock HighPrivateUseSurrogates { get; } =
        new( "HIGH_PRIVATE_USE_SURROGATES",
             "HIGH PRIVATE USE SURROGATES",
             "HIGHPRIVATEUSESURROGATES" );

    /// <summary>
    /// Constant for the "Low Surrogates" Unicode character block.
    /// This block represents codepoint values in the low surrogate
    /// range: U+DC00 through U+DFFF
    /// </summary>
    public static UnicodeBlock LowSurrogates { get; } =
        new( "LOW_SURROGATES",
             "LOW SURROGATES",
             "LOWSURROGATES" );

    /// <summary>
    /// Constant for the "Arabic Supplement" Unicode character block.
    /// </summary>
    public static UnicodeBlock ArabicSupplement { get; } =
        new( "ARABIC_Supplement",
             "ARABIC Supplement",
             "ARABICSupplement" );

    /// <summary>
    /// Constant for the "NKo" Unicode character block.
    /// </summary>
    public static UnicodeBlock Nko { get; } = new( "NKO" );

    /// <summary>
    /// Constant for the "Samaritan" Unicode character block.
    /// </summary>
    public static UnicodeBlock Samaritan { get; } = new( "SAMARITAN" );

    /// <summary>
    /// Constant for the "Mandaic" Unicode character block.
    /// </summary>
    public static UnicodeBlock Mandaic { get; } = new( "MANDAIC" );

    /// <summary>
    /// Constant for the "Ethiopic Supplement" Unicode character block.
    /// </summary>
    public static UnicodeBlock EthiopicSupplement { get; } =
        new( "ETHIOPIC_Supplement",
             "ETHIOPIC Supplement",
             "ETHIOPICSupplement" );

    /// <summary>
    /// Constant for the "Unified Canadian Aboriginal Syllabics Extended"
    /// Unicode character block.
    /// </summary>
    public static UnicodeBlock UnifiedCanadianAboriginalSyllabicsExtended { get; } =
        new( "UNIFIED_CANADIAN_ABORIGINAL_SYLLABICS_Extended",
             "UNIFIED CANADIAN ABORIGINAL SYLLABICS Extended",
             "UNIFIEDCANADIANABORIGINALSYLLABICSExtended" );

    /// <summary>
    /// Constant for the "New Tai Lue" Unicode character block.
    /// </summary>
    public static UnicodeBlock NewTaiLue { get; } =
        new( "NEW_TAI_LUE",
             "NEW TAI LUE",
             "NEWTAILUE" );

    /// <summary>
    /// Constant for the "Buginese" Unicode character block.
    /// </summary>
    public static UnicodeBlock Buginese { get; } = new( "BUGINESE" );

    /// <summary>
    /// Constant for the "Tai Tham" Unicode character block.
    /// </summary>
    public static UnicodeBlock TaiTham { get; } =
        new( "TAI_THAM",
             "TAI THAM",
             "TAITHAM" );

    /// <summary>
    /// Constant for the "Balinese" Unicode character block.
    /// </summary>
    public static UnicodeBlock Balinese { get; } = new( "BALINESE" );

    /// <summary>
    /// Constant for the "Sundanese" Unicode character block.
    /// </summary>
    public static UnicodeBlock Sundanese { get; } = new( "SUNDANESE" );

    /// <summary>
    /// Constant for the "Batak" Unicode character block.
    /// </summary>
    public static UnicodeBlock Batak { get; } = new( "BATAK" );

    /// <summary>
    /// Constant for the "Lepcha" Unicode character block.
    /// </summary>
    public static UnicodeBlock Lepcha { get; } = new( "LEPCHA" );

    /// <summary>
    /// Constant for the "Ol Chiki" Unicode character block.
    /// </summary>
    public static UnicodeBlock OlChiki { get; } =
        new( "OL_CHIKI",
             "OL CHIKI",
             "OLCHIKI" );

    /// <summary>
    /// Constant for the "Vedic Extensions" Unicode character block.
    /// </summary>
    public static UnicodeBlock VedicExtensions { get; } =
        new( "VEDIC_EXTENSIONS",
             "VEDIC EXTENSIONS",
             "VEDICEXTENSIONS" );

    /// <summary>
    /// Constant for the "Phonetic Extensions Supplement" Unicode character
    /// block.
    /// </summary>
    public static UnicodeBlock PhoneticExtensionsSupplement { get; } =
        new( "PHONETIC_EXTENSIONS_Supplement",
             "PHONETIC EXTENSIONS Supplement",
             "PHONETICEXTENSIONSSupplement" );

    /// <summary>
    /// Constant for the "Combining Diacritical Marks Supplement" Unicode
    /// character block.
    /// </summary>
    public static UnicodeBlock CombiningDiacriticalMarksSupplement { get; } =
        new( "COMBINING_DIACRITICAL_MARKS_Supplement",
             "COMBINING DIACRITICAL MARKS Supplement",
             "COMBININGDIACRITICALMARKSSupplement" );

    /// <summary>
    /// Constant for the "Glagolitic" Unicode character block.
    /// </summary>
    public static UnicodeBlock Glagolitic { get; } = new( "GLAGOLITIC" );

    /// <summary>
    /// Constant for the "Latin Extended-C" Unicode character block.
    /// </summary>
    public static UnicodeBlock LatinExtendedC { get; } =
        new( "Latin_Extended_C",
             "Latin Extended-C",
             "LatinExtended-C" );

    /// <summary>
    /// Constant for the "Coptic" Unicode character block.
    /// </summary>
    public static UnicodeBlock Coptic { get; } = new( "COPTIC" );

    /// <summary>
    /// Constant for the "Georgian Supplement" Unicode character block.
    /// </summary>
    public static UnicodeBlock GeorgianSupplement { get; } =
        new( "GEORGIAN_Supplement",
             "GEORGIAN Supplement",
             "GEORGIANSupplement" );

    /// <summary>
    /// Constant for the "Tifinagh" Unicode character block.
    /// </summary>
    public static UnicodeBlock Tifinagh { get; } = new( "TIFINAGH" );

    /// <summary>
    /// Constant for the "Ethiopic Extended" Unicode character block.
    /// </summary>
    public static UnicodeBlock EthiopicExtended { get; } =
        new( "ETHIOPIC_Extended",
             "ETHIOPIC Extended",
             "ETHIOPICExtended" );

    /// <summary>
    /// Constant for the "Cyrillic Extended-A" Unicode character block.
    /// </summary>
    public static UnicodeBlock CyrillicExtendedA { get; } =
        new( "Cyrillic_Extended_A",
             "Cyrillic Extended-A",
             "CyrillicExtended-A" );

    /// <summary>
    /// Constant for the "Supplemental Punctuation" Unicode character block.
    /// </summary>
    public static UnicodeBlock SupplementalPunctuation { get; } =
        new( "SupplementAL_PUNCTUATION",
             "SupplementAL PUNCTUATION",
             "SupplementALPUNCTUATION" );

    /// <summary>
    /// Constant for the "CJK Strokes" Unicode character block.
    /// </summary>
    public static UnicodeBlock CjkStrokes { get; } =
        new( "CJK_STROKES",
             "CJK STROKES",
             "CJKSTROKES" );

    /// <summary>
    /// Constant for the "Lisu" Unicode character block.
    /// </summary>
    public static UnicodeBlock Lisu { get; } = new( "LISU" );

    /// <summary>
    /// Constant for the "Vai" Unicode character block.
    /// </summary>
    public static UnicodeBlock Vai { get; } = new( "VAI" );

    /// <summary>
    /// Constant for the "Cyrillic Extended-B" Unicode character block.
    /// </summary>
    public static UnicodeBlock CyrillicExtendedB { get; } =
        new( "Cyrillic_Extended_B",
             "Cyrillic Extended-B",
             "CyrillicExtended-B" );

    /// <summary>
    /// Constant for the "Bamum" Unicode character block.
    /// </summary>
    public static UnicodeBlock Bamum { get; } = new( "BAMUM" );

    /// <summary>
    /// Constant for the "Modifier Tone Letters" Unicode character block.
    /// </summary>
    public static UnicodeBlock ModifierToneLetters { get; } =
        new( "MODIFIER_TONE_LETTERS",
             "MODIFIER TONE LETTERS",
             "MODIFIERTONELETTERS" );

    /// <summary>
    /// Constant for the "Latin Extended-D" Unicode character block.
    /// </summary>
    public static UnicodeBlock LatinExtendedD { get; } =
        new( "Latin_Extended_D",
             "Latin Extended-D",
             "LatinExtended-D" );

    /// <summary>
    /// Constant for the "Syloti Nagri" Unicode character block.
    /// </summary>
    public static UnicodeBlock SylotiNagri { get; } =
        new( "SYLOTI_NAGRI",
             "SYLOTI NAGRI",
             "SYLOTINAGRI" );

    /// <summary>
    /// Constant for the "Common Indic Number Forms" Unicode character block.
    /// </summary>
    public static UnicodeBlock CommonIndicNumberForms { get; } =
        new( "COMMON_INDIC_NUMBER_FORMS",
             "COMMON INDIC NUMBER FORMS",
             "COMMONINDICNUMBERFORMS" );

    /// <summary>
    /// Constant for the "Phags-pa" Unicode character block.
    /// </summary>
    public static UnicodeBlock PhagsPa { get; } =
        new( "PHAGS_PA",
             "PHAGS-PA" );

    /// <summary>
    /// Constant for the "Saurashtra" Unicode character block.
    /// </summary>
    public static UnicodeBlock Saurashtra { get; } = new( "SAURASHTRA" );

    /// <summary>
    /// Constant for the "Devanagari Extended" Unicode character block.
    /// </summary>
    public static UnicodeBlock DevanagariExtended { get; } =
        new( "DEVANAGARI_Extended",
             "DEVANAGARI Extended",
             "DEVANAGARIExtended" );

    /// <summary>
    /// Constant for the "Kayah Li" Unicode character block.
    /// </summary>
    public static UnicodeBlock KayahLi { get; } =
        new( "KAYAH_LI",
             "KAYAH LI",
             "KAYAHLI" );

    /// <summary>
    /// Constant for the "Rejang" Unicode character block.
    /// </summary>
    public static UnicodeBlock Rejang { get; } = new( "REJANG" );

    /// <summary>
    /// Constant for the "Hangul Jamo Extended-A" Unicode character block.
    /// </summary>
    public static UnicodeBlock HangulJamoExtendedA { get; } =
        new( "HANGUL_JAMO_Extended_A",
             "HANGUL JAMO Extended-A",
             "HANGULJAMOExtended-A" );

    /// <summary>
    /// Constant for the "Javanese" Unicode character block.
    /// </summary>
    public static UnicodeBlock Javanese { get; } = new( "JAVANESE" );

    /// <summary>
    /// Constant for the "Cham" Unicode character block.
    /// </summary>
    public static UnicodeBlock Cham { get; } = new( "CHAM" );

    /// <summary>
    /// Constant for the "Myanmar Extended-A" Unicode character block.
    /// </summary>
    public static UnicodeBlock MyanmarExtendedA { get; } =
        new( "MYANMAR_Extended_A",
             "MYANMAR Extended-A",
             "MYANMARExtended-A" );

    /// <summary>
    /// Constant for the "Tai Viet" Unicode character block.
    /// </summary>
    public static UnicodeBlock TaiViet { get; } =
        new( "TAI_VIET",
             "TAI VIET",
             "TAIVIET" );

    /// <summary>
    /// Constant for the "Ethiopic Extended-A" Unicode character block.
    /// </summary>
    public static UnicodeBlock EthiopicExtendedA { get; } =
        new( "ETHIOPIC_Extended_A",
             "ETHIOPIC Extended-A",
             "ETHIOPICExtended-A" );

    /// <summary>
    /// Constant for the "Meetei Mayek" Unicode character block.
    /// </summary>
    public static UnicodeBlock MeeteiMayek { get; } =
        new( "MEETEI_MAYEK",
             "MEETEI MAYEK",
             "MEETEIMAYEK" );

    /// <summary>
    /// Constant for the "Hangul Jamo Extended-B" Unicode character block.
    /// </summary>
    public static UnicodeBlock HangulJamoExtendedB { get; } =
        new( "HANGUL_JAMO_Extended_B",
             "HANGUL JAMO Extended-B",
             "HANGULJAMOExtended-B" );

    /// <summary>
    /// Constant for the "Vertical Forms" Unicode character block.
    /// </summary>
    public static UnicodeBlock VerticalForms { get; } =
        new( "VERTICAL_FORMS",
             "VERTICAL FORMS",
             "VERTICALFORMS" );

    /// <summary>
    /// Constant for the "Ancient Greek Numbers" Unicode character block.
    /// </summary>
    public static UnicodeBlock AncientGreekNumbers { get; } =
        new( "ANCIENT_GREEK_NUMBERS",
             "ANCIENT GREEK NUMBERS",
             "ANCIENTGREEKNUMBERS" );

    /// <summary>
    /// Constant for the "Ancient Symbols" Unicode character block.
    /// </summary>
    public static UnicodeBlock AncientSymbols { get; } =
        new( "ANCIENT_SYMBOLS",
             "ANCIENT SYMBOLS",
             "ANCIENTSYMBOLS" );

    /// <summary>
    /// Constant for the "Phaistos Disc" Unicode character block.
    /// </summary>
    public static UnicodeBlock PhaistosDisc { get; } =
        new( "PHAISTOS_DISC",
             "PHAISTOS DISC",
             "PHAISTOSDISC" );

    /// <summary>
    /// Constant for the "Lycian" Unicode character block.
    /// </summary>
    public static UnicodeBlock Lycian { get; } = new( "LYCIAN" );

    /// <summary>
    /// Constant for the "Carian" Unicode character block.
    /// </summary>
    public static UnicodeBlock Carian { get; } = new( "CARIAN" );

    /// <summary>
    /// Constant for the "Old Persian" Unicode character block.
    /// </summary>
    public static UnicodeBlock OldPersian { get; } =
        new( "OLD_PERSIAN",
             "OLD PERSIAN",
             "OLDPERSIAN" );

    /// <summary>
    /// Constant for the "Imperial Aramaic" Unicode character block.
    /// </summary>
    public static UnicodeBlock ImperialAramaic { get; } =
        new( "IMPERIAL_ARAMAIC",
             "IMPERIAL ARAMAIC",
             "IMPERIALARAMAIC" );

    /// <summary>
    /// Constant for the "Phoenician" Unicode character block.
    /// </summary>
    public static UnicodeBlock Phoenician { get; } = new( "PHOENICIAN" );

    /// <summary>
    /// Constant for the "Lydian" Unicode character block.
    /// </summary>
    public static UnicodeBlock Lydian { get; } = new( "LYDIAN" );

    /// <summary>
    /// Constant for the "Kharoshthi" Unicode character block.
    /// </summary>
    public static UnicodeBlock Kharoshthi { get; } = new( "KHAROSHTHI" );

    /// <summary>
    /// Constant for the "Old South Arabian" Unicode character block.
    /// </summary>
    public static UnicodeBlock OldSouthArabian { get; } =
        new( "OLD_SOUTH_ARABIAN",
             "OLD SOUTH ARABIAN",
             "OLDSOUTHARABIAN" );

    /// <summary>
    /// Constant for the "Avestan" Unicode character block.
    /// </summary>
    public static UnicodeBlock Avestan { get; } = new( "AVESTAN" );

    /// <summary>
    /// Constant for the "Inscriptional Parthian" Unicode character block.
    /// </summary>
    public static UnicodeBlock InscriptionalParthian { get; } =
        new( "INSCRIPTIONAL_PARTHIAN",
             "INSCRIPTIONAL PARTHIAN",
             "INSCRIPTIONALPARTHIAN" );

    /// <summary>
    /// Constant for the "Inscriptional Pahlavi" Unicode character block.
    /// </summary>
    public static UnicodeBlock InscriptionalPahlavi { get; } =
        new( "INSCRIPTIONAL_PAHLAVI",
             "INSCRIPTIONAL PAHLAVI",
             "INSCRIPTIONALPAHLAVI" );

    /// <summary>
    /// Constant for the "Old Turkic" Unicode character block.
    /// </summary>
    public static UnicodeBlock OldTurkic { get; } =
        new( "OLD_TURKIC",
             "OLD TURKIC",
             "OLDTURKIC" );

    /// <summary>
    /// Constant for the "Rumi Numeral Symbols" Unicode character block.
    /// </summary>
    public static UnicodeBlock RumiNumeralSymbols { get; } =
        new( "RUMI_NUMERAL_SYMBOLS",
             "RUMI NUMERAL SYMBOLS",
             "RUMINUMERALSYMBOLS" );

    /// <summary>
    /// Constant for the "Brahmi" Unicode character block.
    /// </summary>
    public static UnicodeBlock Brahmi { get; } = new( "BRAHMI" );

    /// <summary>
    /// Constant for the "Kaithi" Unicode character block.
    /// </summary>
    public static UnicodeBlock Kaithi { get; } = new( "KAITHI" );

    /// <summary>
    /// Constant for the "Cuneiform" Unicode character block.
    /// </summary>
    public static UnicodeBlock Cuneiform { get; } = new( "CUNEIFORM" );

    /// <summary>
    /// Constant for the "Cuneiform Numbers and Punctuation" Unicode
    /// character block.
    /// </summary>
    public static UnicodeBlock CuneiformNumbersAndPunctuation { get; } =
        new( "CUNEIFORM_NUMBERS_AND_PUNCTUATION",
             "CUNEIFORM NUMBERS AND PUNCTUATION",
             "CUNEIFORMNUMBERSANDPUNCTUATION" );

    /// <summary>
    /// Constant for the "Egyptian Hieroglyphs" Unicode character block.
    /// </summary>
    public static UnicodeBlock EgyptianHieroglyphs { get; } =
        new( "EGYPTIAN_HIEROGLYPHS",
             "EGYPTIAN HIEROGLYPHS",
             "EGYPTIANHIEROGLYPHS" );

    /// <summary>
    /// Constant for the "Bamum Supplement" Unicode character block.
    /// </summary>
    public static UnicodeBlock BamumSupplement { get; } =
        new( "BAMUM_Supplement",
             "BAMUM Supplement",
             "BAMUMSupplement" );

    /// <summary>
    /// Constant for the "Kana Supplement" Unicode character block.
    /// </summary>
    public static UnicodeBlock KanaSupplement { get; } =
        new( "KANA_Supplement",
             "KANA Supplement",
             "KANASupplement" );

    /// <summary>
    /// Constant for the "Ancient Greek Musical Notation" Unicode character
    /// block.
    /// </summary>
    public static UnicodeBlock AncientGreekMusicalNotation { get; } =
        new( "ANCIENT_GREEK_MUSICAL_NOTATION",
             "ANCIENT GREEK MUSICAL NOTATION",
             "ANCIENTGREEKMUSICALNOTATION" );

    /// <summary>
    /// Constant for the "Counting Rod Numerals" Unicode character block.
    /// </summary>
    public static UnicodeBlock CountingRodNumerals { get; } =
        new( "COUNTING_ROD_NUMERALS",
             "COUNTING ROD NUMERALS",
             "COUNTINGRODNUMERALS" );

    /// <summary>
    /// Constant for the "Mahjong Tiles" Unicode character block.
    /// </summary>
    public static UnicodeBlock MahjongTiles { get; } =
        new( "MAHJONG_TILES",
             "MAHJONG TILES",
             "MAHJONGTILES" );

    /// <summary>
    /// Constant for the "Domino Tiles" Unicode character block.
    /// </summary>
    public static UnicodeBlock DominoTiles { get; } =
        new( "DOMINO_TILES",
             "DOMINO TILES",
             "DOMINOTILES" );

    /// <summary>
    /// Constant for the "Playing Cards" Unicode character block.
    /// </summary>
    public static UnicodeBlock PlayingCards { get; } =
        new( "PLAYING_CARDS",
             "PLAYING CARDS",
             "PLAYINGCARDS" );

    /// <summary>
    /// Constant for the "Enclosed Alphanumeric Supplement" Unicode character
    /// block.
    /// </summary>
    public static UnicodeBlock EnclosedAlphanumericSupplement { get; } =
        new( "ENCLOSED_ALPHANUMERIC_Supplement",
             "ENCLOSED ALPHANUMERIC Supplement",
             "ENCLOSEDALPHANUMERICSupplement" );

    /// <summary>
    /// Constant for the "Enclosed Ideographic Supplement" Unicode character
    /// block.
    /// </summary>
    public static UnicodeBlock EnclosedIdeographicSupplement { get; } =
        new( "ENCLOSED_IDEOGRAPHIC_Supplement",
             "ENCLOSED IDEOGRAPHIC Supplement",
             "ENCLOSEDIDEOGRAPHICSupplement" );

    /// <summary>
    /// Constant for the "Miscellaneous Symbols And Pictographs" Unicode
    /// character block.
    /// </summary>
    public static UnicodeBlock MiscellaneousSymbolsAndPictographs { get; } =
        new( "MISCELLANEOUS_SYMBOLS_AND_PICTOGRAPHS",
             "MISCELLANEOUS SYMBOLS AND PICTOGRAPHS",
             "MISCELLANEOUSSYMBOLSANDPICTOGRAPHS" );

    /// <summary>
    /// Constant for the "Emoticons" Unicode character block.
    /// </summary>
    public static UnicodeBlock Emoticons { get; } = new( "EMOTICONS" );

    /// <summary>
    /// Constant for the "Transport And Map Symbols" Unicode character block.
    /// </summary>
    public static UnicodeBlock TransportAndMapSymbols { get; } =
        new( "TRANSPORT_AND_MAP_SYMBOLS",
             "TRANSPORT AND MAP SYMBOLS",
             "TRANSPORTANDMAPSYMBOLS" );

    /// <summary>
    /// Constant for the "Alchemical Symbols" Unicode character block.
    /// </summary>
    public static UnicodeBlock AlchemicalSymbols { get; } =
        new( "ALCHEMICAL_SYMBOLS",
             "ALCHEMICAL SYMBOLS",
             "ALCHEMICALSYMBOLS" );

    /// <summary>
    /// Constant for the "CJK Unified Ideographs Extension C" Unicode
    /// character block.
    /// </summary>
    public static UnicodeBlock CjkUnifiedIdeographsExtensionC { get; } = new( "CJK_UNIFIED_IDEOGRAPHS_EXTENSION_C",
                                                                              "CJK UNIFIED IDEOGRAPHS EXTENSION C",
                                                                              "CJKUNIFIEDIDEOGRAPHSEXTENSIONC" );

    /// <summary>
    /// Constant for the "CJK Unified Ideographs Extension D" Unicode character block.
    /// </summary>
    public static UnicodeBlock CjkUnifiedIdeographsExtensionD { get; } = new( "CJK_UNIFIED_IDEOGRAPHS_EXTENSION_D",
                                                                              "CJK UNIFIED IDEOGRAPHS EXTENSION D",
                                                                              "CJKUNIFIEDIDEOGRAPHSEXTENSIOND" );

    /// <summary>
    /// Constant for the "Arabic Extended-A" Unicode character block.
    /// </summary>
    public static UnicodeBlock ArabicExtendedA { get; } = new( "ARABIC_Extended_A",
                                                               "ARABIC Extended-A",
                                                               "ARABICExtended-A" );

    /// <summary>
    /// Constant for the "Sundanese Supplement" Unicode character block.
    /// </summary>
    public static UnicodeBlock SundaneseSupplement { get; } = new( "SUNDANESE_Supplement",
                                                                   "SUNDANESE Supplement",
                                                                   "SUNDANESESupplement" );

    /// <summary>
    /// Constant for the "Meetei Mayek Extensions" Unicode character block.
    /// </summary>
    public static UnicodeBlock MeeteiMayekExtensions { get; } = new( "MEETEI_MAYEK_EXTENSIONS",
                                                                     "MEETEI MAYEK EXTENSIONS",
                                                                     "MEETEIMAYEKEXTENSIONS" );

    /// <summary>
    /// Constant for the "Meroitic Hieroglyphs" Unicode character block.
    /// </summary>
    public static UnicodeBlock MeroiticHieroglyphs { get; } = new( "MEROITIC_HIEROGLYPHS",
                                                                   "MEROITIC HIEROGLYPHS",
                                                                   "MEROITICHIEROGLYPHS" );

    /// <summary>
    /// Constant for the "Meroitic Cursive" Unicode character block.
    /// </summary>
    public static UnicodeBlock MeroiticCursive { get; } = new( "MEROITIC_CURSIVE",
                                                               "MEROITIC CURSIVE",
                                                               "MEROITICCURSIVE" );

    /// <summary>
    /// Constant for the "Sora Sompeng" Unicode character block.
    /// </summary>
    public static UnicodeBlock SoraSompeng { get; } = new( "SORA_SOMPENG",
                                                           "SORA SOMPENG",
                                                           "SORASOMPENG" );

    /// <summary>
    /// Constant for the "Chakma" Unicode character block.
    /// </summary>
    public static UnicodeBlock Chakma { get; } = new( "CHAKMA" );

    /// <summary>
    /// Constant for the "Sharada" Unicode character block.
    /// </summary>
    public static UnicodeBlock Sharada { get; } = new( "SHARADA" );

    /// <summary>
    /// Constant for the "Takri" Unicode character block.
    /// </summary>
    public static UnicodeBlock Takri { get; } = new( "TAKRI" );

    /// <summary>
    /// Constant for the "Miao" Unicode character block.
    /// </summary>
    public static UnicodeBlock Miao { get; } = new( "MIAO" );

    /// <summary>
    /// Constant for the "Arabic Mathematical Alphabetic Symbols" Unicode
    /// character block.
    /// </summary>
    public static UnicodeBlock ArabicMathematicalAlphabeticSymbols { get; } =
        new( "ARABIC_MATHEMATICAL_ALPHABETIC_SYMBOLS",
             "ARABIC MATHEMATICAL ALPHABETIC SYMBOLS",
             "ARABICMATHEMATICALALPHABETICSYMBOLS" );

    // ========================================================================

    private static int[] _blockStarts =
    [
        0x0000,   // 0000..007F; Basic Latin
        0x0080,   // 0080..00FF; Latin-1 Supplement
        0x0100,   // 0100..017F; Latin Extended-A
        0x0180,   // 0180..024F; Latin Extended-B
        0x0250,   // 0250..02AF; IPA Extensions
        0x02B0,   // 02B0..02FF; Spacing Modifier Letters
        0x0300,   // 0300..036F; Combining Diacritical Marks
        0x0370,   // 0370..03FF; Greek and Coptic
        0x0400,   // 0400..04FF; Cyrillic
        0x0500,   // 0500..052F; Cyrillic Supplement
        0x0530,   // 0530..058F; Armenian
        0x0590,   // 0590..05FF; Hebrew
        0x0600,   // 0600..06FF; Arabic
        0x0700,   // 0700..074F; Syriac
        0x0750,   // 0750..077F; Arabic Supplement
        0x0780,   // 0780..07BF; Thaana
        0x07C0,   // 07C0..07FF; NKo
        0x0800,   // 0800..083F; Samaritan
        0x0840,   // 0840..085F; Mandaic
        0x0860,   //             unassigned
        0x08A0,   // 08A0..08FF; Arabic Extended-A
        0x0900,   // 0900..097F; Devanagari
        0x0980,   // 0980..09FF; Bengali
        0x0A00,   // 0A00..0A7F; Gurmukhi
        0x0A80,   // 0A80..0AFF; Gujarati
        0x0B00,   // 0B00..0B7F; Oriya
        0x0B80,   // 0B80..0BFF; Tamil
        0x0C00,   // 0C00..0C7F; Telugu
        0x0C80,   // 0C80..0CFF; Kannada
        0x0D00,   // 0D00..0D7F; Malayalam
        0x0D80,   // 0D80..0DFF; Sinhala
        0x0E00,   // 0E00..0E7F; Thai
        0x0E80,   // 0E80..0EFF; Lao
        0x0F00,   // 0F00..0FFF; Tibetan
        0x1000,   // 1000..109F; Myanmar
        0x10A0,   // 10A0..10FF; Georgian
        0x1100,   // 1100..11FF; Hangul Jamo
        0x1200,   // 1200..137F; Ethiopic
        0x1380,   // 1380..139F; Ethiopic Supplement
        0x13A0,   // 13A0..13FF; Cherokee
        0x1400,   // 1400..167F; Unified Canadian Aboriginal Syllabics
        0x1680,   // 1680..169F; Ogham
        0x16A0,   // 16A0..16FF; Runic
        0x1700,   // 1700..171F; Tagalog
        0x1720,   // 1720..173F; Hanunoo
        0x1740,   // 1740..175F; Buhid
        0x1760,   // 1760..177F; Tagbanwa
        0x1780,   // 1780..17FF; Khmer
        0x1800,   // 1800..18AF; Mongolian
        0x18B0,   // 18B0..18FF; Unified Canadian Aboriginal Syllabics Extended
        0x1900,   // 1900..194F; Limbu
        0x1950,   // 1950..197F; Tai Le
        0x1980,   // 1980..19DF; New Tai Lue
        0x19E0,   // 19E0..19FF; Khmer Symbols
        0x1A00,   // 1A00..1A1F; Buginese
        0x1A20,   // 1A20..1AAF; Tai Tham
        0x1AB0,   //             unassigned
        0x1B00,   // 1B00..1B7F; Balinese
        0x1B80,   // 1B80..1BBF; Sundanese
        0x1BC0,   // 1BC0..1BFF; Batak
        0x1C00,   // 1C00..1C4F; Lepcha
        0x1C50,   // 1C50..1C7F; Ol Chiki
        0x1C80,   //             unassigned
        0x1CC0,   // 1CC0..1CCF; Sundanese Supplement
        0x1CD0,   // 1CD0..1CFF; Vedic Extensions
        0x1D00,   // 1D00..1D7F; Phonetic Extensions
        0x1D80,   // 1D80..1DBF; Phonetic Extensions Supplement
        0x1DC0,   // 1DC0..1DFF; Combining Diacritical Marks Supplement
        0x1E00,   // 1E00..1EFF; Latin Extended Additional
        0x1F00,   // 1F00..1FFF; Greek Extended
        0x2000,   // 2000..206F; General Punctuation
        0x2070,   // 2070..209F; Superscripts and Subscripts
        0x20A0,   // 20A0..20CF; Currency Symbols
        0x20D0,   // 20D0..20FF; Combining Diacritical Marks for Symbols
        0x2100,   // 2100..214F; Letterlike Symbols
        0x2150,   // 2150..218F; Number Forms
        0x2190,   // 2190..21FF; Arrows
        0x2200,   // 2200..22FF; Mathematical Operators
        0x2300,   // 2300..23FF; Miscellaneous Technical
        0x2400,   // 2400..243F; Control Pictures
        0x2440,   // 2440..245F; Optical Character Recognition
        0x2460,   // 2460..24FF; Enclosed Alphanumerics
        0x2500,   // 2500..257F; Box Drawing
        0x2580,   // 2580..259F; Block Elements
        0x25A0,   // 25A0..25FF; Geometric Shapes
        0x2600,   // 2600..26FF; Miscellaneous Symbols
        0x2700,   // 2700..27BF; Dingbats
        0x27C0,   // 27C0..27EF; Miscellaneous Mathematical Symbols-A
        0x27F0,   // 27F0..27FF; Supplemental Arrows-A
        0x2800,   // 2800..28FF; Braille Patterns
        0x2900,   // 2900..297F; Supplemental Arrows-B
        0x2980,   // 2980..29FF; Miscellaneous Mathematical Symbols-B
        0x2A00,   // 2A00..2AFF; Supplemental Mathematical Operators
        0x2B00,   // 2B00..2BFF; Miscellaneous Symbols and Arrows
        0x2C00,   // 2C00..2C5F; Glagolitic
        0x2C60,   // 2C60..2C7F; Latin Extended-C
        0x2C80,   // 2C80..2CFF; Coptic
        0x2D00,   // 2D00..2D2F; Georgian Supplement
        0x2D30,   // 2D30..2D7F; Tifinagh
        0x2D80,   // 2D80..2DDF; Ethiopic Extended
        0x2DE0,   // 2DE0..2DFF; Cyrillic Extended-A
        0x2E00,   // 2E00..2E7F; Supplemental Punctuation
        0x2E80,   // 2E80..2EFF; CJK Radicals Supplement
        0x2F00,   // 2F00..2FDF; Kangxi Radicals
        0x2FE0,   //             unassigned
        0x2FF0,   // 2FF0..2FFF; Ideographic Description Characters
        0x3000,   // 3000..303F; CJK Symbols and Punctuation
        0x3040,   // 3040..309F; Hiragana
        0x30A0,   // 30A0..30FF; Katakana
        0x3100,   // 3100..312F; Bopomofo
        0x3130,   // 3130..318F; Hangul Compatibility Jamo
        0x3190,   // 3190..319F; Kanbun
        0x31A0,   // 31A0..31BF; Bopomofo Extended
        0x31C0,   // 31C0..31EF; CJK Strokes
        0x31F0,   // 31F0..31FF; Katakana Phonetic Extensions
        0x3200,   // 3200..32FF; Enclosed CJK Letters and Months
        0x3300,   // 3300..33FF; CJK Compatibility
        0x3400,   // 3400..4DBF; CJK Unified Ideographs Extension A
        0x4DC0,   // 4DC0..4DFF; Yijing Hexagram Symbols
        0x4E00,   // 4E00..9FFF; CJK Unified Ideographs
        0xA000,   // A000..A48F; Yi Syllables
        0xA490,   // A490..A4CF; Yi Radicals
        0xA4D0,   // A4D0..A4FF; Lisu
        0xA500,   // A500..A63F; Vai
        0xA640,   // A640..A69F; Cyrillic Extended-B
        0xA6A0,   // A6A0..A6FF; Bamum
        0xA700,   // A700..A71F; Modifier Tone Letters
        0xA720,   // A720..A7FF; Latin Extended-D
        0xA800,   // A800..A82F; Syloti Nagri
        0xA830,   // A830..A83F; Common Indic Number Forms
        0xA840,   // A840..A87F; Phags-pa
        0xA880,   // A880..A8DF; Saurashtra
        0xA8E0,   // A8E0..A8FF; Devanagari Extended
        0xA900,   // A900..A92F; Kayah Li
        0xA930,   // A930..A95F; Rejang
        0xA960,   // A960..A97F; Hangul Jamo Extended-A
        0xA980,   // A980..A9DF; Javanese
        0xA9E0,   //             unassigned
        0xAA00,   // AA00..AA5F; Cham
        0xAA60,   // AA60..AA7F; Myanmar Extended-A
        0xAA80,   // AA80..AADF; Tai Viet
        0xAAE0,   // AAE0..AAFF; Meetei Mayek Extensions
        0xAB00,   // AB00..AB2F; Ethiopic Extended-A
        0xAB30,   //             unassigned
        0xABC0,   // ABC0..ABFF; Meetei Mayek
        0xAC00,   // AC00..D7AF; Hangul Syllables
        0xD7B0,   // D7B0..D7FF; Hangul Jamo Extended-B
        0xD800,   // D800..DB7F; High Surrogates
        0xDB80,   // DB80..DBFF; High Private Use Surrogates
        0xDC00,   // DC00..DFFF; Low Surrogates
        0xE000,   // E000..F8FF; Private Use Area
        0xF900,   // F900..FAFF; CJK Compatibility Ideographs
        0xFB00,   // FB00..FB4F; Alphabetic Presentation Forms
        0xFB50,   // FB50..FDFF; Arabic Presentation Forms-A
        0xFE00,   // FE00..FE0F; Variation Selectors
        0xFE10,   // FE10..FE1F; Vertical Forms
        0xFE20,   // FE20..FE2F; Combining Half Marks
        0xFE30,   // FE30..FE4F; CJK Compatibility Forms
        0xFE50,   // FE50..FE6F; Small Form Variants
        0xFE70,   // FE70..FEFF; Arabic Presentation Forms-B
        0xFF00,   // FF00..FFEF; Halfwidth and Fullwidth Forms
        0xFFF0,   // FFF0..FFFF; Specials
        0x10000,  // 10000..1007F; Linear B Syllabary
        0x10080,  // 10080..100FF; Linear B Ideograms
        0x10100,  // 10100..1013F; Aegean Numbers
        0x10140,  // 10140..1018F; Ancient Greek Numbers
        0x10190,  // 10190..101CF; Ancient Symbols
        0x101D0,  // 101D0..101FF; Phaistos Disc
        0x10200,  //               unassigned
        0x10280,  // 10280..1029F; Lycian
        0x102A0,  // 102A0..102DF; Carian
        0x102E0,  //               unassigned
        0x10300,  // 10300..1032F; Old Italic
        0x10330,  // 10330..1034F; Gothic
        0x10350,  //               unassigned
        0x10380,  // 10380..1039F; Ugaritic
        0x103A0,  // 103A0..103DF; Old Persian
        0x103E0,  //               unassigned
        0x10400,  // 10400..1044F; Deseret
        0x10450,  // 10450..1047F; Shavian
        0x10480,  // 10480..104AF; Osmanya
        0x104B0,  //               unassigned
        0x10800,  // 10800..1083F; Cypriot Syllabary
        0x10840,  // 10840..1085F; Imperial Aramaic
        0x10860,  //               unassigned
        0x10900,  // 10900..1091F; Phoenician
        0x10920,  // 10920..1093F; Lydian
        0x10940,  //               unassigned
        0x10980,  // 10980..1099F; Meroitic Hieroglyphs
        0x109A0,  // 109A0..109FF; Meroitic Cursive
        0x10A00,  // 10A00..10A5F; Kharoshthi
        0x10A60,  // 10A60..10A7F; Old South Arabian
        0x10A80,  //               unassigned
        0x10B00,  // 10B00..10B3F; Avestan
        0x10B40,  // 10B40..10B5F; Inscriptional Parthian
        0x10B60,  // 10B60..10B7F; Inscriptional Pahlavi
        0x10B80,  //               unassigned
        0x10C00,  // 10C00..10C4F; Old Turkic
        0x10C50,  //               unassigned
        0x10E60,  // 10E60..10E7F; Rumi Numeral Symbols
        0x10E80,  //               unassigned
        0x11000,  // 11000..1107F; Brahmi
        0x11080,  // 11080..110CF; Kaithi
        0x110D0,  // 110D0..110FF; Sora Sompeng
        0x11100,  // 11100..1114F; Chakma
        0x11150,  //               unassigned
        0x11180,  // 11180..111DF; Sharada
        0x111E0,  //               unassigned
        0x11680,  // 11680..116CF; Takri
        0x116D0,  //               unassigned
        0x12000,  // 12000..123FF; Cuneiform
        0x12400,  // 12400..1247F; Cuneiform Numbers and Punctuation
        0x12480,  //               unassigned
        0x13000,  // 13000..1342F; Egyptian Hieroglyphs
        0x13430,  //               unassigned
        0x16800,  // 16800..16A3F; Bamum Supplement
        0x16A40,  //               unassigned
        0x16F00,  // 16F00..16F9F; Miao
        0x16FA0,  //               unassigned
        0x1B000,  // 1B000..1B0FF; Kana Supplement
        0x1B100,  //               unassigned
        0x1D000,  // 1D000..1D0FF; Byzantine Musical Symbols
        0x1D100,  // 1D100..1D1FF; Musical Symbols
        0x1D200,  // 1D200..1D24F; Ancient Greek Musical Notation
        0x1D250,  //               unassigned
        0x1D300,  // 1D300..1D35F; Tai Xuan Jing Symbols
        0x1D360,  // 1D360..1D37F; Counting Rod Numerals
        0x1D380,  //               unassigned
        0x1D400,  // 1D400..1D7FF; Mathematical Alphanumeric Symbols
        0x1D800,  //               unassigned
        0x1EE00,  // 1EE00..1EEFF; Arabic Mathematical Alphabetic Symbols
        0x1EF00,  //               unassigned
        0x1F000,  // 1F000..1F02F; Mahjong Tiles
        0x1F030,  // 1F030..1F09F; Domino Tiles
        0x1F0A0,  // 1F0A0..1F0FF; Playing Cards
        0x1F100,  // 1F100..1F1FF; Enclosed Alphanumeric Supplement
        0x1F200,  // 1F200..1F2FF; Enclosed Ideographic Supplement
        0x1F300,  // 1F300..1F5FF; Miscellaneous Symbols And Pictographs
        0x1F600,  // 1F600..1F64F; Emoticons
        0x1F650,  //               unassigned
        0x1F680,  // 1F680..1F6FF; Transport And Map Symbols
        0x1F700,  // 1F700..1F77F; Alchemical Symbols
        0x1F780,  //               unassigned
        0x20000,  // 20000..2A6DF; CJK Unified Ideographs Extension B
        0x2A6E0,  //               unassigned
        0x2A700,  // 2A700..2B73F; CJK Unified Ideographs Extension C
        0x2B740,  // 2B740..2B81F; CJK Unified Ideographs Extension D
        0x2B820,  //               unassigned
        0x2F800,  // 2F800..2FA1F; CJK Compatibility Ideographs Supplement
        0x2FA20,  //               unassigned
        0xE0000,  // E0000..E007F; Tags
        0xE0080,  //               unassigned
        0xE0100,  // E0100..E01EF; Variation Selectors Supplement
        0xE01F0,  //               unassigned
        0xF0000,  // F0000..FFFFF; Supplementary Private Use Area-A
        0x100000, // 100000..10FFFF; Supplementary Private Use Area-B
    ];

    // ========================================================================

    private static UnicodeBlock?[] _blocks =
    [
        BasicLatin,
        Latin1Supplement,
        LatinExtendedA,
        LatinExtendedB,
        IpaExtensions,
        SpacingModifierLetters,
        CombiningDiacriticalMarks,
        Greek,
        Cyrillic,
        CyrillicSupplementary,
        Armenian,
        Hebrew,
        Arabic,
        Syriac,
        ArabicSupplement,
        Thaana,
        Nko,
        Samaritan,
        Mandaic,
        null,
        ArabicExtendedA,
        Devanagari,
        Bengali,
        Gurmukhi,
        Gujarati,
        Oriya,
        Tamil,
        Telugu,
        Kannada,
        Malayalam,
        Sinhala,
        Thai,
        Lao,
        Tibetan,
        Myanmar,
        Georgian,
        HangulJamo,
        Ethiopic,
        EthiopicSupplement,
        Cherokee,
        UnifiedCanadianAboriginalSyllabics,
        Ogham,
        Runic,
        Tagalog,
        Hanunoo,
        Buhid,
        Tagbanwa,
        Khmer,
        Mongolian,
        UnifiedCanadianAboriginalSyllabicsExtended,
        Limbu,
        TaiLE,
        NewTaiLue,
        KhmerSymbols,
        Buginese,
        TaiTham,
        null,
        Balinese,
        Sundanese,
        Batak,
        Lepcha,
        OlChiki,
        null,
        SundaneseSupplement,
        VedicExtensions,
        PhoneticExtensions,
        PhoneticExtensionsSupplement,
        CombiningDiacriticalMarksSupplement,
        LatinExtendedAdditional,
        GreekExtended,
        GeneralPunctuation,
        SuperscriptsAndSubscripts,
        CurrencySymbols,
        CombiningMarksForSymbols,
        LetterlikeSymbols,
        NumberForms,
        Arrows,
        MathematicalOperators,
        MiscellaneousTechnical,
        ControlPictures,
        OpticalCharacterRecognition,
        EnclosedAlphanumerics,
        BoxDrawing,
        BlockElements,
        GeometricShapes,
        MiscellaneousSymbols,
        Dingbats,
        MiscellaneousMathematicalSymbolsA,
        SupplementalArrowsA,
        BraillePatterns,
        SupplementalArrowsB,
        MiscellaneousMathematicalSymbolsB,
        SupplementalMathematicalOperators,
        MiscellaneousSymbolsAndArrows,
        Glagolitic,
        LatinExtendedC,
        Coptic,
        GeorgianSupplement,
        Tifinagh,
        EthiopicExtended,
        CyrillicExtendedA,
        SupplementalPunctuation,
        CjkRadicalsSupplement,
        KangxiRadicals,
        null,
        IdeographicDescriptionCharacters,
        CjkSymbolsAndPunctuation,
        Hiragana,
        Katakana,
        Bopomofo,
        HangulCompatibilityJamo,
        Kanbun,
        BopomofoExtended,
        CjkStrokes,
        KatakanaPhoneticExtensions,
        EnclosedCjkLettersAndMonths,
        CjkCompatibility,
        CjkUnifiedIdeographsExtensionA,
        YijingHexagramSymbols,
        CjkUnifiedIdeographs,
        YiSyllables,
        YiRadicals,
        Lisu,
        Vai,
        CyrillicExtendedB,
        Bamum,
        ModifierToneLetters,
        LatinExtendedD,
        SylotiNagri,
        CommonIndicNumberForms,
        PhagsPa,
        Saurashtra,
        DevanagariExtended,
        KayahLi,
        Rejang,
        HangulJamoExtendedA,
        Javanese,
        null,
        Cham,
        MyanmarExtendedA,
        TaiViet,
        MeeteiMayekExtensions,
        EthiopicExtendedA,
        null,
        MeeteiMayek,
        HangulSyllables,
        HangulJamoExtendedB,
        HighSurrogates,
        HighPrivateUseSurrogates,
        LowSurrogates,
        PrivateUseArea,
        CjkCompatibilityIdeographs,
        AlphabeticPresentationForms,
        ArabicPresentationFormsA,
        VariationSelectors,
        VerticalForms,
        CombiningHalfMarks,
        CjkCompatibilityForms,
        SmallFormVariants,
        ArabicPresentationFormsB,
        HalfwidthAndFullwidthForms,
        Specials,
        LinearBSyllabary,
        LinearBIdeograms,
        AegeanNumbers,
        AncientGreekNumbers,
        AncientSymbols,
        PhaistosDisc,
        null,
        Lycian,
        Carian,
        null,
        OldItalic,
        Gothic,
        null,
        Ugaritic,
        OldPersian,
        null,
        Deseret,
        Shavian,
        Osmanya,
        null,
        CypriotSyllabary,
        ImperialAramaic,
        null,
        Phoenician,
        Lydian,
        null,
        MeroiticHieroglyphs,
        MeroiticCursive,
        Kharoshthi,
        OldSouthArabian,
        null,
        Avestan,
        InscriptionalParthian,
        InscriptionalPahlavi,
        null,
        OldTurkic,
        null,
        RumiNumeralSymbols,
        null,
        Brahmi,
        Kaithi,
        SoraSompeng,
        Chakma,
        null,
        Sharada,
        null,
        Takri,
        null,
        Cuneiform,
        CuneiformNumbersAndPunctuation,
        null,
        EgyptianHieroglyphs,
        null,
        BamumSupplement,
        null,
        Miao,
        null,
        KanaSupplement,
        null,
        ByzantineMusicalSymbols,
        MusicalSymbols,
        AncientGreekMusicalNotation,
        null,
        TaiXuanJingSymbols,
        CountingRodNumerals,
        null,
        MathematicalAlphanumericSymbols,
        null,
        ArabicMathematicalAlphabeticSymbols,
        null,
        MahjongTiles,
        DominoTiles,
        PlayingCards,
        EnclosedAlphanumericSupplement,
        EnclosedIdeographicSupplement,
        MiscellaneousSymbolsAndPictographs,
        Emoticons,
        null,
        TransportAndMapSymbols,
        AlchemicalSymbols,
        null,
        CjkUnifiedIdeographsExtensionB,
        null,
        CjkUnifiedIdeographsExtensionC,
        CjkUnifiedIdeographsExtensionD,
        null,
        CjkCompatibilityIdeographsSupplement,
        null,
        Tags,
        null,
        VariationSelectorsSupplement,
        null,
        SupplementaryPrivateUseAreaA,
        SupplementaryPrivateUseAreaB,
    ];

    // ========================================================================

    /// <summary>
    /// Returns the object representing the Unicode block containing the given character,
    /// or <c>null</c> if the character is not a member of a defined block.
    /// <para>
    /// <b>Note:</b> This method cannot handle supplementary characters. To support all
    /// Unicode characters, including supplementary characters, use the <see cref="Of(int)"/>
    /// method.
    /// </para>
    /// </summary>
    /// <param name="character"> The character in question. </param>
    /// <returns>
    /// The <c>UnicodeBlock</c> instance representing the Unicode block of which this character
    /// is a member, or <c>null</c> if the character is not a member of any Unicode block.
    /// </returns>
    public static UnicodeBlock? Of( char character )
    {
        return Of( ( int )character );
    }

    /// <summary>
    /// Returns the object representing the Unicode block containing the given character
    /// (Unicode code point), or <c>null</c> if the character is not a member of a
    /// defined block.
    /// </summary>
    /// <param name="codePoint"> the character (Unicode code point) in question. </param>
    /// <returns>
    /// The <c>UnicodeBlock</c> instance representing the Unicode block of which this character
    /// is a member, or <c>null</c> if the character is not a member of any Unicode block
    /// </returns>
    /// <exception cref="ArgumentException">
    /// if the specified <c>codePoint</c> is an invalid Unicode code point.
    /// </exception>
    public static UnicodeBlock? Of( int codePoint )
    {
        if ( !CharacterUtils.IsValidCodePoint( codePoint ) )
        {
            throw new ArgumentException( "Supplied codepoint is not valid" );
        }

        var bottom  = 0;
        var top     = _blockStarts.Length;
        var current = top / 2;

        // invariant: top > current >= bottom && codePoint >= unicodeBlockStarts[bottom]
        while ( ( top - bottom ) > 1 )
        {
            if ( codePoint >= _blockStarts[ current ] )
            {
                bottom = current;
            }
            else
            {
                top = current;
            }

            current = ( top + bottom ) / 2;
        }

        return _blocks[ current ];
    }

    /// <summary>
    /// Returns the UnicodeBlock with the given name. Block names are determined by The
    /// Unicode Standard. The file Blocks-&lt;version&gt;.txt defines blocks for a particular
    /// version of the standard. The <see cref="CharacterUtils"/> class specifies the version of
    /// the standard that it supports.
    /// <para>
    /// This method accepts block names in the following forms:
    /// <li>
    /// Canonical block names as defined by the Unicode Standard. For example, the standard
    /// defines a "Basic Latin" block. Therefore, this method accepts "Basic Latin" as a valid
    /// block name. The documentation of each UnicodeBlock provides the canonical name.
    /// </li>
    /// <li>
    /// Canonical block names with all spaces removed. For example, "BasicLatin" is a valid block
    /// name for the "Basic Latin" block.
    /// </li>
    /// <li>
    /// The text representation of each constant UnicodeBlock identifier. For example, this method
    /// will return the <see cref="BasicLatin"/> block if provided with the <c>"BasicLatin"</c> name.
    /// This form replaces all spaces and hyphens in the canonical name with underscores.
    /// </li>
    /// </para>
    /// <para>
    /// Finally, character case is ignored for all of the valid block name forms. For example,
    /// "BasicLatin" and "basiclatin" are both valid block names.
    /// </para>
    /// <para>
    /// If the Unicode Standard changes block names, both the previous and current names will be accepted.
    /// </para>
    /// </summary>
    /// <param name="blockName"> A <c>UnicodeBlock</c> name. </param>
    /// <returns> The <c>UnicodeBlock</c> instance identified by <c>blockName</c> </returns>
    /// <exception cref="ArgumentException"> if <c>blockName</c> is an invalid name </exception>
    /// <exception cref="NullReferenceException"> if <c>blockName</c> is null. </exception>
    public static UnicodeBlock ForName( string blockName )
    {
        var block = _map[ blockName.ToUpper( CultureInfo.InvariantCulture ) ];

        Guard.ThrowIfNull( block );

        return block;
    }
}