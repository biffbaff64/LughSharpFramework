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

using JetBrains.Annotations;

namespace LughSharp.Core.Graphics.Text;

[PublicAPI]
public partial class RegexUtils
{
    public const string MatchFinalFolderPattern         = @"[\\/]([^\\/]+)$";
    public const string WordBoundaryPattern              = @"\b\w+\b";
    public const string MinimalNamePattern               = "^[^\":,}/ ][^:]*$";
    public const string FilenameWithoutExtensionPattern = "(.*)\\..*";
    public const string NumberSuffixPattern              = "(.*?)(\\d+)$";
    public const string VersionNumberPattern             = "\\d+";
    public const string IdentifierPattern                 = "^[a-zA-Z_$][a-zA-Z_$0-9]*$";
    public const string UnquotedValuePattern             = "^[^\":,{\\[\\]/ ][^}\\],]*$";
    public const string UnquotedKeyorvaluePattern        = "^[^\":,}/ ][^:]*$";
    public const string MinimalValuePatter               = "^[^\":,{\\[\\]/ ][^}\\],]*$";
    public const string ItemWithUnderscorePattern       = "(.+)_(\\d+)$";
    public const string JavascriptPattern                 = "^[a-zA-Z_$][a-zA-Z_$0-9]*$";

    // ========================================================================

    [GeneratedRegex( FilenameWithoutExtensionPattern, RegexOptions.Compiled )]
    public static partial Regex FileNameWithoutExtensionRegex();

    [GeneratedRegex( NumberSuffixPattern, RegexOptions.Compiled )]
    public static partial Regex NumberSuffixRegex();

    [GeneratedRegex( WordBoundaryPattern, RegexOptions.Compiled )]
    public static partial Regex WordBoundaryRegex();

    [GeneratedRegex( VersionNumberPattern, RegexOptions.Compiled )]
    public static partial Regex VersionNumberRegex();

    [GeneratedRegex( IdentifierPattern, RegexOptions.Compiled )]
    public static partial Regex IdentifierRegex();

    [GeneratedRegex( UnquotedValuePattern, RegexOptions.Compiled )]
    public static partial Regex UnquotedValueRegex();

    [GeneratedRegex( UnquotedKeyorvaluePattern, RegexOptions.Compiled )]
    public static partial Regex UnquotedKeyOrValueRegex();

    [GeneratedRegex( ItemWithUnderscorePattern, RegexOptions.Compiled )]
    public static partial Regex ItemWithUnderscoreSuffixRegex();

    [GeneratedRegex( JavascriptPattern )]
    public static partial Regex JavascriptPatternRegex();

    [GeneratedRegex( MinimalValuePatter, RegexOptions.Compiled )]
    public static partial Regex MinimalValuePatternRegex();

    [GeneratedRegex( MinimalNamePattern, RegexOptions.Compiled )]
    public static partial Regex MinimalNamePatternRegex();

    [GeneratedRegex( MatchFinalFolderPattern, RegexOptions.Compiled )]
    public static partial Regex MatchFinalFolderPatternRegex();
}

// ============================================================================
// ============================================================================