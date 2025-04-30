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
    [GeneratedRegex( "(.*)\\..*", RegexOptions.Compiled )]
    public static partial Regex FileNameWithoutExtensionRegex();

    [GeneratedRegex( "(.*?)(\\d+)$", RegexOptions.Compiled )]
    public static partial Regex NumberSuffixRegex();

    [GeneratedRegex( @"\b\w+\b", RegexOptions.Compiled )]
    public static partial Regex WordBoundaryRegex();

    [GeneratedRegex( "\\d+", RegexOptions.Compiled )]
    public static partial Regex VersionNumberRegex();

    [GeneratedRegex( "^[a-zA-Z_$][a-zA-Z_$0-9]*$", RegexOptions.Compiled )]
    public static partial Regex IdentifierRegex();

    [GeneratedRegex( "^[^\":,{\\[\\]/ ][^}\\],]*$", RegexOptions.Compiled )]
    public static partial Regex UnquotedValueRegex();

    [GeneratedRegex( "^[^\":,}/ ][^:]*$", RegexOptions.Compiled )]
    public static partial Regex UnquotedKeyOrValueRegex();

    [GeneratedRegex( "(.+)_(\\d+)$", RegexOptions.Compiled )]
    public static partial Regex ItemWithUnderscoreSuffixRegex();

    [GeneratedRegex( "^[a-zA-Z_$][a-zA-Z_$0-9]*$" )]
    public static partial Regex JavascriptPatternRegex();
    
    [GeneratedRegex( "^[^\":,{\\[\\]/ ][^}\\],]*$" )]
    public static partial Regex MinimalValuePatternRegex();

    [GeneratedRegex( "^[^\":,}/ ][^:]*$" )]
    public static partial Regex MinimalNamePatternRegex();
}