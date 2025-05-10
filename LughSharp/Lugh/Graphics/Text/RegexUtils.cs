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

using System.Text.RegularExpressions;

namespace LughSharp.Lugh.Graphics.Text;

[PublicAPI]
public partial class RegexUtils
{
    public const string MATCH_FINAL_FOLDER_PATTERN         = @"[\\/]([^\\/]+)$";
    public const string WORD_BOUNDARY_PATTERN              = @"\b\w+\b";
    public const string MINIMAL_NAME_PATTERN               = "^[^\":,}/ ][^:]*$";
    public const string FILENAME_WITHOUT_EXTENSION_PATTERN = "(.*)\\..*";
    public const string NUMMBER_SUFFIX_PATTERN             = "(.*?)(\\d+)$";
    public const string VERSION_NUMBER_PATTERN             = "\\d+";
    public const string IDENTIFIER_PATTERN                 = "^[a-zA-Z_$][a-zA-Z_$0-9]*$";
    public const string UNQUOTED_VALUE_PATTERN             = "^[^\":,{\\[\\]/ ][^}\\],]*$";
    public const string UNQUOTED_KEYORVALUE_PATTERN        = "^[^\":,}/ ][^:]*$";
    public const string MINIMAL_VALUE_PATTER               = "^[^\":,{\\[\\]/ ][^}\\],]*$";
    public const string ITEM_WITH_UNDERSCORE_PATTERN       = "(.+)_(\\d+)$";
    public const string JAVASCRIPT_PATTERN                 = "^[a-zA-Z_$][a-zA-Z_$0-9]*$";
    
    // ========================================================================

    [GeneratedRegex( FILENAME_WITHOUT_EXTENSION_PATTERN, RegexOptions.Compiled )]
    public static partial Regex FileNameWithoutExtensionRegex();

    [GeneratedRegex( NUMMBER_SUFFIX_PATTERN, RegexOptions.Compiled )]
    public static partial Regex NumberSuffixRegex();

    [GeneratedRegex( WORD_BOUNDARY_PATTERN, RegexOptions.Compiled )]
    public static partial Regex WordBoundaryRegex();

    [GeneratedRegex( VERSION_NUMBER_PATTERN, RegexOptions.Compiled )]
    public static partial Regex VersionNumberRegex();

    [GeneratedRegex( IDENTIFIER_PATTERN, RegexOptions.Compiled )]
    public static partial Regex IdentifierRegex();

    [GeneratedRegex( UNQUOTED_VALUE_PATTERN, RegexOptions.Compiled )]
    public static partial Regex UnquotedValueRegex();
    
    [GeneratedRegex( UNQUOTED_KEYORVALUE_PATTERN, RegexOptions.Compiled )]
    public static partial Regex UnquotedKeyOrValueRegex();

    [GeneratedRegex( ITEM_WITH_UNDERSCORE_PATTERN, RegexOptions.Compiled )]
    public static partial Regex ItemWithUnderscoreSuffixRegex();

    [GeneratedRegex( JAVASCRIPT_PATTERN )]
    public static partial Regex JavascriptPatternRegex();

    [GeneratedRegex( MINIMAL_VALUE_PATTER, RegexOptions.Compiled )]
    public static partial Regex MinimalValuePatternRegex();

    [GeneratedRegex( MINIMAL_NAME_PATTERN, RegexOptions.Compiled )]
    public static partial Regex MinimalNamePatternRegex();

    [GeneratedRegex( MATCH_FINAL_FOLDER_PATTERN, RegexOptions.Compiled )]
    public static partial Regex MatchFinalFolderPatternRegex();
}